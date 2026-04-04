# CloverleafTrack — Codebase Guide

CloverleafTrack is an ASP.NET Core MVC web application (.NET 8+) that tracks high school track & field results — athletes, meets, performances, and all-time leaderboards.

---

## AI Documentation Convention

This repository uses two files to give AI assistants full context:

| File | Purpose |
|---|---|
| `CLAUDE.md` *(this file)* | Current state of the codebase — architecture, conventions, patterns, and invariants. Describes what the code *is* right now. Keep this up to date when anything structural changes. |
| `BRAIN.md` | Chronological log of every accepted, non-obvious change — what changed, why, which files, and what to watch out for. Describes how the code *got* to where it is. |

### How to use these files

**When starting a new session:** Read `CLAUDE.md` first for the full architectural picture, then read `BRAIN.md` to understand decisions and gotchas that are not obvious from reading the code alone.

**When completing a change:** Every accepted change that is not self-evident from reading the current code **must** be logged in `BRAIN.md` before the session ends. This includes:
- Bug fixes where the root cause was non-obvious
- Behavior that deviates from what you would naively expect (e.g. a flag that is unreliable and must not be trusted)
- Architectural decisions and the reasons behind them
- Patterns established to avoid a class of bugs
- Any "watch out" knowledge a future AI would need to avoid re-introducing a problem

Each `BRAIN.md` entry must include: **what changed**, **why**, **key files**, and **watch out** notes. Entries are never deleted — the log is append-only and cumulative. If a later change supersedes an earlier one, add a new entry that references and overrides it rather than editing the old one.

`CLAUDE.md` should always reflect the *current* state. `BRAIN.md` should always reflect the *full history* of how it got there.

---

## Solution Structure

```
CloverleafTrack.sln
├── CloverleafTrack.Web/          # ASP.NET Core MVC host — controllers, views, wwwroot
├── CloverleafTrack.Services/     # Business logic layer
├── CloverleafTrack.DataAccess/   # Dapper repositories + DTOs + interfaces
├── CloverleafTrack.Models/       # Domain model classes + all enums
├── CloverleafTrack.ViewModels/   # ViewModels scoped to each feature area
├── CloverleafTrack.Infrastructure/  # (placeholder, currently empty)
└── CloverleafTrack.Tests/        # xUnit unit tests (Unit/Services/ and Unit/Utilities/)
```

Frontend: **Tailwind CSS v4** (config at `tailwind.config.js` / `postcss.config.js`, npm/pnpm managed).

---

## Request Lifecycle

```
HTTP Request
  → Controller (CloverleafTrack.Web)
    → Service (CloverleafTrack.Services)
      → Repository (CloverleafTrack.DataAccess)
        → SQL Server (Dapper, raw SQL / stored procs)
      ← DTO or Model
    ← ViewModel built from model/DTO
  ← Razor View rendered with ViewModel
```

- Controllers are thin: call service, pass ViewModel to view.
- Services own business logic and ViewModel mapping.
- Repositories own all SQL. They return domain `Models` or `Dtos`.
- Views never call services or repositories directly.

---

## Key Enums (CloverleafTrack.Models/Enums/)

```csharp
enum EventType : short
{
    Field = 0,        // individual field (jumps, throws, pole vault)
    Running = 1,      // individual running
    FieldRelay = 2,   // relay of a field event (e.g. distance medley throws)
    RunningRelay = 3, // 4x100, 4x400, etc.
    JumpRelay = 4,    // relay of a jump event
    ThrowsRelay = 5   // relay of a throws event
}

enum Environment : short  // NOTE: aliases clash — always use full name or alias
{
    Outdoor = 1,
    Indoor = 2
}

enum EventCategory : short
{
    Sprints = 0, Distance = 1, Throws = 2, Jumps = 3, Relays = 4, Hurdles = 5
}

enum Gender : short  { Male = 1, Female = 2, Mixed = 3 }

enum MeetEntryStatus  // digitizing pipeline status
{
    NotAvailable = 0, Scanned = 1, Placeholder = 2, Entered = 3
}

enum SeasonStatus
{
    Draft = 1, Importing = 2, Partial = 3, RecordOnly = 4, Complete = 5
}
```

`Environment` conflicts with `System.Environment`. All files that use it add:
```csharp
using Environment = CloverleafTrack.Models.Enums.Environment;
```

---

## Core Domain Models (CloverleafTrack.Models/)

| Model | Key fields |
|---|---|
| `Season` | Id, Name, StartDate, EndDate, IsCurrentSeason, Status, Meets |
| `Meet` | Id, Name, Date, Environment, HandTimed, LocationId, SeasonId, EntryStatus, Slug (computed via Slugify), ResultsUrl |
| `Location` | Id, Name, City, State, ZipCode, Country |
| `Event` | Id, Name, EventKey, EventType, EventCategory, Gender, Environment, AthleteCount, SortOrder, EventCategorySortOrder |
| `Athlete` | Id, FirstName, LastName, Gender, GraduationYear, IsActive |
| `Performance` | Id, AthleteId (null for relay), EventId, MeetId, TimeSeconds, DistanceInches, SortedAthleteHash, SchoolRecord, SeasonBest, PersonalBest |
| `PerformanceAthlete` | Id, PerformanceId, AthleteId — relay junction table |

**Relay performances** set `AthleteId = null` on `Performance` and link athletes via `PerformanceAthletes` table. `SortedAthleteHash` is a nullable string on `Performance` (no NOT NULL constraint enforced at app layer).

**Important caveat about performance flags:** The `PersonalBest` and `SchoolRecord` flags on relay `Performance` rows are **not reliably set** by the admin entry flow. Additionally, `SchoolRecord` on **any** Performance row (individual or relay) is a snapshot — it is **not cleared** when a newer performance supersedes the record. `sp_RebuildLeaderboards` keeps the `Leaderboards` table current but does NOT retroactively clear old `SchoolRecord` flags. Do not depend on `p.SchoolRecord` to mean "is currently the school record." Instead:
- Relay "PR" per event = best time (MIN TimeSeconds) or best distance (MAX DistanceInches) across all performances for that event
- School record (individual or relay) = AllTimeRank == 1 on the Leaderboards table

**Field vs. running determination:**
```csharp
// Field events (higher distance is better):
EventType.Field, FieldRelay, JumpRelay, ThrowsRelay  → DistanceInches
// Running events (lower time is better):
EventType.Running, RunningRelay                       → TimeSeconds
```

Slug generation uses the `Slugify` NuGet package (`SlugHelper`). `Meet.Slug` is a computed property, not stored in DB.

---

## Web Project Layout (CloverleafTrack.Web/)

### Public controllers (Controllers/)
| Controller | Route | Purpose |
|---|---|---|
| `HomeController` | `/` | Homepage stats, highlights, upcoming meets |
| `SeasonsController` | `/seasons` | Season list + season detail page |
| `MeetsController` | `/meets` | Meet list + meet detail page |
| `RosterController` | `/roster` | Active + former athlete list |
| `AthletesController` | `/athletes/{slug}` | Athlete career detail — NOTE: route is `/roster/{slug}` via `RosterController`, not `AthletesController` |
| `LeaderboardController` | `/leaderboard` | All-time top 10 lists |

### Admin area (Areas/Admin/)
| Controller | Route | Purpose |
|---|---|---|
| `DashboardController` | `/Admin/Dashboard` | Stats overview, data quality issues |
| `AthletesController` | `/Admin/Athletes` | CRUD athletes |
| `MeetsController` | `/Admin/Meets` | CRUD meets |
| `PerformancesController` | `/Admin/Performances` | Performance entry form + CRUD |
| `EventsController` | `/Admin/Events` | CRUD events |
| `LocationsController` | `/Admin/Locations` | CRUD locations |
| `SeasonsController` | `/Admin/Seasons` | CRUD seasons |

Admin area has **no authentication** — it is expected to be protected at the infrastructure layer.

### Utilities (Web/Utilities/)
- `PerformanceFormatHelper` — static helpers for parsing/formatting time (`ParseTime`, `FormatTime`) and distance (`ParseDistance`, `FormatDistance`). Both `PerformancesController` and `SeasonService` duplicate some of this logic; `Edit` action uses the helper, `Create` action uses inline private methods.
- `SessionExtensions` — helpers for storing int/string in session.

---

## Data Access (CloverleafTrack.DataAccess/)

**ORM:** Dapper with raw SQL. No EF Core.

**Connection:** `SqlConnectionFactory` wraps `Microsoft.Data.SqlClient`. Registered as `IDbConnectionFactory` in DI. Connection string key: `DefaultConnection`.

### Repository split: public vs. admin

Each entity has **two** repository interfaces/implementations:
- `I{Entity}Repository` / `{Entity}Repository` — read-only queries for public views
- `IAdmin{Entity}Repository` / `Admin{Entity}Repository` — full CRUD for admin area

### Key SQL patterns
- Multi-table queries use Dapper multi-mapping (`splitOn: "Id,Id,Id"`).
- Relay athlete names for display: `STRING_AGG(a.FirstName + ' ' + a.LastName, '|~|')` — separator is `|~|`, format is `FirstName LastName`. Split on `|~|` in C# to get individual names.
- After any Performance insert/update/delete: `EXEC sp_RebuildLeaderboards` is called to recalculate the `Leaderboards` table.
- `GetMeetsForSeasonAsync` returns meets `ORDER BY m.Date ASC` (ascending) from SQL.
- `AthleteRepository.GetAllPerformancesForAthleteAsync` uses a **UNION ALL** to combine individual performances (`WHERE p.AthleteId = @AthleteId`) with relay performances (`INNER JOIN PerformanceAthletes pa ON pa.PerformanceId = p.Id WHERE pa.AthleteId = @AthleteId AND p.AthleteId IS NULL`). The relay branch includes `STRING_AGG` of team members as `RelayAthletes`.

### DTOs (DataAccess/Dtos/)
Dtos are used for complex query results that span multiple tables and don't map directly to a model. Examples: `MeetPerformanceDto`, `TopPerformanceDto`, `LeaderboardDto`, `HomePageStatsDto`, `AthletePerformanceDto`.

`AthletePerformanceDto` includes:
- `RelayAthletes` (nullable string, `|~|`-separated) — null for individual performances, populated for relay performances
- `SeasonStartDate` — used for correct descending season ordering in the service layer

---

## Services (CloverleafTrack.Services/)

| Service | Key methods |
|---|---|
| `SeasonService` | `GetSeasonCardsAsync`, `GetSeasonDetailsAsync` |
| `MeetService` | `GetMeetsIndexAsync`, `GetMeetDetailsAsync` |
| `AthleteService` | `GetAthleteDetailsAsync` — athlete career/roster detail page |
| `LeaderboardService` | `GetLeaderboardAsync`, `GetLeaderboardDetailsAsync` |
| `HomeService` | Homepage aggregated data |

`MeetService.GetMeetsIndexAsync` groups all meets by season (seasons ordered descending by start date), then orders meets within each season **ascending by date**.

`MeetService.BuildOrderedEventGroups` orders meet results: Sprints → Distance → Hurdles → Running Relays → Jumps → Jump Relays → Throws → Throw Relays. Mixed-gender relay performances (`EventGender == Gender.Mixed`) are split into `MixedEvents` on `MeetDetailsViewModel`.

`SeasonService.GetSeasonDetailsAsync` maps top performances split by `Environment.Indoor` / `Environment.Outdoor`. **Note:** relay performances do not appear in season top-10 sections because `GetTopPerformancesForSeasonAsync` uses `INNER JOIN Athletes a ON a.Id = p.AthleteId`, which excludes null-AthleteId relay rows.

`LeaderboardService.GetLeaderboardAsync` splits performances into Boys/Girls/Mixed × Outdoor/Indoor, producing six category lists on `LeaderboardViewModel`.

`AthleteService.GetAthleteDetailsAsync`:
- **Personal Records table**: individual PRs use `PersonalBest = true` flag; relay PRs use best-per-event (min time / max distance) regardless of flag
- **Hero TotalPRs**: individual performances only (`PersonalBest = true && RelayAthletes == null`)
- **Hero TotalSchoolRecords**: all performances (individual and relay) where `AllTimeRank == 1`, distinct by EventId. Does NOT use the `SchoolRecord` DB flag — it is a stale snapshot.
- **Season grouping**: ordered by `SeasonStartDate DESC` using the DTO field, not season name string

---

## Mixed Relay Support

Mixed relays (both boys and girls athletes on the same team) use `Gender.Mixed` (= 3) on the `Event` row.

**Display surfaces:**
- **Meet Details**: full-width "Mixed Relays" section below the Boys/Girls two-column grid, only rendered when `Model.MixedEvents.Any()`
- **Leaderboard**: Mixed Relays section below Boys/Girls grid inside each environment tab, rendered via `_LeaderboardMixedSection.cshtml` partial (purple `border-purple-500` accent)
- **Roster Details**: mixed relay appearances show in the athlete's season performance section and personal records table like any other relay

**Admin entry**: `AdminAthleteRepository.GetAthletesForMeetAsync` skips the gender filter when `gender == Gender.Mixed`, so both boys and girls appear in the athlete dropdown for mixed relay events.

**Database**: Mixed relay event rows go in the `RunningRelayEvents` table (UNIQUEIDENTIFIER Id, no EventKey column). The `Events` table (used by app repositories) must also have corresponding rows for mixed relay events to appear in performance entry and leaderboards.

---

## ViewModels (CloverleafTrack.ViewModels/)

Organized by feature area, mirroring the controller structure. Admin ViewModels live in `Admin/` subdirectories.

### Admin Performance entry ViewModel (key fields)
```csharp
class PerformanceEntryViewModel {
    int MeetId, EventId;
    int? AthleteId;              // null for relay
    string? TimeInput;           // user-entered time string
    string? DistanceInput;       // user-entered distance string
    List<int> RelayAthleteIds;   // bound from RelayAthleteIds[0..3]
    EventType EventType;
    int EventAthleteCount;       // drives relay vs. individual UI
    // + dropdown option lists: Meets, Events, Athletes
}
```

### Athlete detail ViewModels (key fields)
```csharp
class AthleteTopEventViewModel {
    string EventName, Performance;
    int? AllTimeRank;
    Environment Environment;    // used to show ☀️ Outdoor / 🏢 Indoor badge in hero
}

class PersonalRecordViewModel {
    // ...standard fields...
    bool IsSchoolRecord;        // set by service: DB flag for individual, AllTimeRank==1 for relay
    string? RelayAthletes;      // null for individual; |~|-separated names for relay
    bool IsRelay;               // => RelayAthletes != null
    string[] RelayMembers;      // => RelayAthletes.Split("|~|")
}

class IndividualPerformanceViewModel {
    // ...standard fields...
    string? RelayAthletes;      // null for individual; |~|-separated names for relay
    bool IsRelay;               // => RelayAthletes != null
    string[] RelayMembers;      // => RelayAthletes.Split("|~|")
}
```

### Leaderboard ViewModels
`LeaderboardViewModel` has six category lists:
- `BoysOutdoorCategories`, `BoysIndoorCategories`
- `GirlsOutdoorCategories`, `GirlsIndoorCategories`
- `MixedOutdoorCategories`, `MixedIndoorCategories`

---

## Performance Entry Form — Important Behavior

The admin performance entry form (`Areas/Admin/Views/Performances/Create.cshtml`) uses **jQuery AJAX** for dynamic UX:

1. **Meet selection** → AJAX `GET /Admin/Performances/GetEventsForMeet?meetId=X` → populates event dropdown
2. **Event selection** → AJAX `GET /Admin/Performances/GetAthletesForMeetAndEvent?meetId=X&eventId=Y` → populates athlete dropdowns, shows relay vs. individual section

**Relay slots:** 4 `<select name="RelayAthleteIds[@i]">` elements are always rendered. Slots beyond `EventAthleteCount` are hidden via `display:none` but still POST (they submit empty string `""`).

**Field vs. running detection** (both Razor and JS):
```js
var isFieldEvent = eventTypeStr.indexOf('Field') >= 0 ||
                   eventTypeStr.indexOf('Jump') >= 0 ||
                   eventTypeStr.indexOf('Throw') >= 0;
```
Field events show a distance input; running events show a time input.

**ModelState clearing for RelayAthleteIds:** The POST controller **unconditionally** removes all `RelayAthleteIds.*` model state keys before checking `ModelState.IsValid`, because hidden slots produce type-conversion errors (`"" → int`) for all relay types (not just individual events). Non-positive IDs are then filtered from the list before relay athlete insert.

---

## Database Tables (inferred from SQL)

| Table | Notes |
|---|---|
| `Seasons` | |
| `Meets` | FK to Seasons, Locations |
| `Locations` | |
| `Events` | Includes SortOrder, EventCategorySortOrder, AthleteCount, Gender (1/2/3), EventKey |
| `Athletes` | |
| `Performances` | AthleteId nullable (null = relay); SortedAthleteHash nullable |
| `PerformanceAthletes` | Junction: PerformanceId, AthleteId |
| `Leaderboards` | All-time rankings, rebuilt by `sp_RebuildLeaderboards` |
| `RunningRelayEvents` | Separate table for running relay event definitions: Id (UNIQUEIDENTIFIER), Name, Gender (INT), SortOrder, Environment (INT), Deleted (BIT), DateCreated, DateUpdated, DateDeleted |

`sp_RebuildLeaderboards` is a stored procedure that recalculates all leaderboard rankings and resets/recalculates `PersonalBest`, `SeasonBest`, and `SchoolRecord` flags on `Performances`. It is called after every performance insert, update, or delete. It does **not** filter by gender, so Mixed relay performances are ranked alongside Boys/Girls relay performances within their own event. `SchoolRecord` and `PersonalBest`/`SeasonBest` flags are only set for individual performances (`AthleteId IS NOT NULL`); relay rows keep these flags at 0 and use `AllTimeRank = 1` as the SR proxy instead.

---

## UI / Theming

- **Tailwind CSS v4** with dark mode support via `dark:` variants throughout.
- Dark mode toggled by `_DarkModeToggle.cshtml` partial.
- **Tab UI pattern (Outdoor/Indoor):** OUTDOOR tab is always first (leftmost) and active by default. INDOOR is second. This applies to: Season Details (`all-time-outdoor`/`all-time-indoor`), Home Page highlights, Home Page season leaders, and the Leaderboard (`env-outdoor`/`env-indoor`). Active tab class: `bg-gradient-to-r from-amber-600 to-yellow-500` (outdoor) or `from-blue-600 to-blue-500` (indoor). Default is triggered by `.click()` on the outdoor tab in `DOMContentLoaded`.
- Meets are displayed **oldest-first (ascending by date)** on both the Season Details page and the Meets index page.
- Relay athlete names are displayed separated by ` / ` (with spaces) in inline lists — not bullet points.

---

## Roster Details Page — Key Behaviors

(`Views/Roster/Details.cshtml` + `AthleteService.GetAthleteDetailsAsync`)

- **Hero stats**: TopSprintEvent and TopFieldEvent show an Indoor/Outdoor badge (`☀️ Outdoor` / `🏢 Indoor`) below the event name. These are individual events only (not relay).
- **Personal Records table**: includes both individual PRs and relay bests. Individual PRs use `PersonalBest = true` flag. Relay entries show the best time/distance per relay event regardless of flag. Relay rows show the team members inline (` / ` separated, each linked to their roster page) below the event name.
- **School Records column**: shows `SR` badge for individual events where `SchoolRecord = true`, and for relay events where `AllTimeRank == 1`. The Rank column is shown whenever any PR row has either a top-10 rank OR a school record.
- **Performance by Season**: shows all events including relays. Relay performance rows show the team members below the mark/date/meet row.
- **Season ordering**: most recent season first, using `SeasonStartDate` from the DTO (not string sort).

---

## Testing

**Framework:** xUnit 2.x + Moq + FluentAssertions. Tests live in `CloverleafTrack.Tests/Unit/`.

```
CloverleafTrack.Tests/Unit/
├── Models/
│   └── MeetTests.cs                — Slug generation, ResultsUrl format
├── Services/
│   ├── AthleteServiceTests.cs      — GetAthleteDetailsAsync, roster grouping, PR/SR logic
│   ├── LeaderboardServiceTests.cs  — gender/env partitioning, category grouping, details page
│   ├── MeetServiceTests.cs         — meet details, event ordering, meets index grouping
│   └── SeasonServiceTests.cs       — current season resolution, missing season exception
├── Utilities/
│   └── PerformanceFormatHelperTests.cs — ParseTime, FormatTime, ParseDistance, FormatDistance, round-trips
└── ViewModels/
    ├── Admin/
    │   ├── AdminPerformanceOptionViewModelTests.cs — AthleteOptionViewModel, EventOptionViewModel, MeetOptionViewModel DisplayText/CategoryName
    │   ├── LocationOptionViewModelTests.cs          — conditional DisplayText (city+state vs. name-only)
    │   └── SeasonProgressViewModelTests.cs          — PercentComplete integer division + zero guard
    ├── Athletes/
    │   └── IndividualPerformanceViewModelTests.cs   — IsRelay, RelayMembers parsing (also covers PersonalRecordViewModel)
    ├── Leaderboard/
    │   └── LeaderboardEventViewModelTests.cs        — RelayMembers (IsNullOrEmpty guard), AthleteFullName
    ├── Meets/
    │   └── MeetListItemViewModelTests.cs            — IsUpcoming date comparison
    └── Seasons/
        └── SeasonCardViewModelTests.cs              — Indoor/OutdoorSchoolRecordCount null coalescing, StatusBadge
```

**Conventions:**
- All service tests mock repository interfaces — no real DB.
- `GetCurrentSeasonAsync()` returns `EndDate.Year`, not the season `Id` — assert accordingly.
- Relay `PersonalBest` and `SchoolRecord` flags are unreliable — tests verify the service uses best-per-event and `AllTimeRank == 1` instead.
- `FormatDistance` always outputs `"F' I\""` with a space after the apostrophe — round-trip test input uses compact format but the formatted output is canonical.
- `SlugHelper` keeps periods but strips apostrophes — tests for slug behavior use apostrophes as the special-char example, not periods.
- `SeasonProgressViewModel.PercentComplete` uses integer division (truncates, not rounds) — tests explicitly cover 1/3 and 2/3 cases.
- Run with: `dotnet test CloverleafTrack.Tests/CloverleafTrack.Tests.csproj`

See `docs/testing.md` for full test strategy and coverage details.

---

## Development Setup

- **Database:** SQL Server. Connection string in `appsettings.json` under key `DefaultConnection`.
- **Docker:** `compose.yaml` builds and runs the web container from `CloverleafTrack.Web/Dockerfile`.
- **CSS build:** `npm` / `pnpm` + PostCSS. Run `pnpm install` then the Tailwind watch/build script before working on styles.
- **Session:** In-memory distributed session, 30-minute timeout. Used by `PerformancesController` to persist selected meet/event across page loads (`PerformanceEntry_MeetId`, `PerformanceEntry_EventId`).

---

## Conventions & Patterns to Follow

- **Never** add EF Core — this repo uses Dapper exclusively.
- **Never** query DB from a Razor view or ViewModel.
- Add ordering logic in the **service or repository layer**, not in views (exception: `Details.cshtml` meet list, which applies `OrderBy` in-view because the ViewModel list is reused).
- All new repositories must implement a corresponding interface in `DataAccess/Interfaces/`.
- Use `PerformanceFormatHelper` (in `Web/Utilities/`) for time/distance parsing and formatting — do not add new inline parse logic.
- Relay performances: always set `AthleteId = null` on the `Performance` row and insert athlete rows into `PerformanceAthletes`.
- Always call `sp_RebuildLeaderboards` after any performance insert/update/delete.
- Slugs are generated at runtime via `SlugHelper` — do not store them in the DB.
- Do not rely on `PersonalBest` or `SchoolRecord` flags on relay performances — use best-per-event logic and `AllTimeRank == 1` respectively.
- Do not use `p.SchoolRecord` to determine whether a performance is *currently* the school record — it is a stale snapshot and is not cleared when a newer record supersedes it. Always use `AllTimeRank = 1` from the Leaderboards table for current school record status.
- Relay athlete name display: always join with ` / ` separator in inline contexts, never individual bullet spans.
