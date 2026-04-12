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

enum MeetType : short
{
    Dual = 1,       // two teams (us vs. one opponent)
    DoubleDual = 2, // three teams (us vs. two opponents, scored independently per pair)
    Invitational = 3 // multi-team; configurable scoring template
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
| `Season` | Id, Name, StartDate, EndDate, IsCurrentSeason, Status, ScoringEnabled, Meets |
| `Meet` | Id, Name, Date, Environment, HandTimed, LocationId, SeasonId, EntryStatus, MeetType, ScoringTemplateId?, Participants, Slug (computed via Slugify), ResultsUrl |
| `ScoringTemplate` | Id, Name, IsBuiltIn, Places |
| `ScoringTemplatePlace` | Id, ScoringTemplateId, Place, Points |
| `MeetParticipant` | Id, MeetId, SchoolName, SortOrder |
| `MeetEventScoringOverride` | Id, MeetId, EventId, ScoringTemplateId |
| `MeetEntry` | Id, MeetId, EventId, AthleteId? (null for relay), PerformanceId? (null until result entered), Athletes (relay junction) |
| `MeetEntryAthlete` | Id, MeetEntryId, AthleteId |
| `MeetPlacing` | Id, MeetId, PerformanceId, MeetParticipantId? (null for invitational), Place, FullPoints, SplitPoints |
| `Location` | Id, Name, City, State, ZipCode, Country |
| `Event` | Id, Name, EventKey, EventType, EventCategory, Gender, Environment, AthleteCount, SortOrder, EventCategorySortOrder |
| `Athlete` | Id, FirstName, LastName, Gender, GraduationYear, IsActive |
| `Performance` | Id, AthleteId (null for relay), EventId, MeetId, TimeSeconds, DistanceInches, SortedAthleteHash, SchoolRecord, SeasonBest, PersonalBest, AllTimeRank (nullable int, not a DB column — populated by queries that join Leaderboards) |
| `PerformanceAthlete` | Id, PerformanceId, AthleteId — relay junction table |

**Relay performances** set `AthleteId = null` on `Performance` and link athletes via `PerformanceAthletes` table. `SortedAthleteHash` is a nullable string on `Performance` (no NOT NULL constraint enforced at app layer).

**Important caveat about performance flags:** The `PersonalBest` and `SchoolRecord` flags on relay `Performance` rows are **not reliably set** by the admin entry flow. `sp_RebuildLeaderboards` does reset and recalculate `SchoolRecord` for **individual** performances (`AthleteId IS NOT NULL`), but relay rows keep `SchoolRecord = 0` always. Do not depend on `p.SchoolRecord` anywhere in application code — use `AllTimeRank == 1` universally for both individual and relay. Specifically:
- Relay "PR" per event = best time (MIN TimeSeconds) or best distance (MAX DistanceInches) across all performances for that event
- School record (individual or relay) = `AllTimeRank == 1` on the Leaderboards table

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
| `SeasonsController` | `/seasons` | Season list, season detail, and season scoring (`/seasons/{name}/scoring`) |
| `MeetsController` | `/meets` | Meet list + meet detail page |
| `RosterController` | `/roster` | Active + former athlete list |
| `AthletesController` | `/athletes/{slug}` | Athlete career detail — NOTE: route is `/roster/{slug}` via `RosterController`, not `AthletesController` |
| `LeaderboardController` | `/leaderboard` | All-time top 10 lists |

### Admin area (Areas/Admin/)
| Controller | Route | Purpose |
|---|---|---|
| `DashboardController` | `/Admin/Dashboard` | Stats overview, data quality issues |
| `AthletesController` | `/Admin/Athletes` | CRUD athletes |
| `MeetsController` | `/Admin/Meets` | CRUD meets (now includes MeetType, participants) |
| `PerformancesController` | `/Admin/Performances` | Performance entry form + CRUD |
| `EventsController` | `/Admin/Events` | CRUD events |
| `LocationsController` | `/Admin/Locations` | CRUD locations |
| `SeasonsController` | `/Admin/Seasons` | CRUD seasons (now includes ScoringEnabled) |
| `ScoringTemplatesController` | `/Admin/ScoringTemplates` | CRUD scoring templates + places; built-in templates cannot be deleted |
| `MeetEntriesController` | `/Admin/MeetEntries` | Pre-meet entry tracking + post-meet result/placing entry |

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

`LeaderboardService.GetLeaderboardDetailsAsync` fetches all performances for an event, then computes the school record progression entirely in C# (no extra SQL query):
1. Sort all performances chronologically (same-day ties broken by best mark first)
2. Walk through tracking the running best — each new best is a record-setting moment
3. Collect into `SchoolRecordProgression`, sorted best-first for display (distance desc / time asc)
4. Track record-setting `PerformanceId`s in a `HashSet<int>` to set `WasRecordAtTime` on each row in `AllPerformances` and `PersonalRecordsOnly`

`AthleteService.GetAthleteDetailsAsync`:
- **Personal Records table**: individual PRs use `PersonalBest = true` flag; relay PRs use best-per-event (min time / max distance) regardless of flag
- **Hero TotalPRs**: individual performances only (`PersonalBest = true && RelayAthletes == null`)
- **Hero TotalSchoolRecords**: all performances (individual and relay) where `AllTimeRank == 1`, distinct by EventId. Does NOT use the `SchoolRecord` DB flag.
- **Season grouping**: ordered by `SeasonStartDate DESC` using the DTO field, not season name string
- **`IsSchoolRecord` everywhere** (PersonalRecordViewModel, IndividualPerformanceViewModel, SeasonPerformanceViewModel.SchoolRecordCount): all use `AllTimeRank == 1` — never the `SchoolRecord` flag

`AthleteService.GetActiveAthletesGroupedByEventCategoryAsync`:
- Relay events are mapped to individual sport categories via `GetRosterCategory(Event)` so relay-only athletes appear in the correct section (Sprints, Distance, Jumps, or Throws) rather than being invisible
- `RunningRelay` → `Sprints` unless the event name contains distance keywords ("distance medley", "dmr", "800", "1500", "1600", "mile", "2000", "3200") → `Distance`
- `JumpRelay` → `Jumps`; `ThrowsRelay`/`FieldRelay` → `Throws`

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
    bool IsSchoolRecord;        // set by service: AllTimeRank == 1 for both individual and relay
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

`LeaderboardDetailsViewModel` (event detail page) key fields:
```csharp
bool IsFieldEvent;                                  // drives Y-axis direction and improvement formatting
bool IsRelayEvent;
List<LeaderboardPerformanceViewModel> AllPerformances;
List<LeaderboardPerformanceViewModel> PersonalRecordsOnly;
List<SchoolRecordMomentViewModel> SchoolRecordProgression; // sorted best-first; empty for relay events with no data
```

`LeaderboardPerformanceViewModel` key fields:
```csharp
bool IsSchoolRecord;     // AllTimeRank == 1 — is the current all-time school record
bool WasRecordAtTime;    // was the school record at the moment it was performed (may no longer be #1)
```

`SchoolRecordMomentViewModel` — one entry in the record progression:
```csharp
string AthleteName, AthleteSlug;
int? GraduationYear;
string Performance;          // formatted display string
double RawValue;             // TimeSeconds or DistanceInches — for Chart.js data arrays
string MeetName, MeetSlug;
DateTime MeetDate;
string? ImprovementFormatted; // "+2' 6.25\"" or "-0.43s" delta from previous record; null for first record
bool IsCurrentRecord;         // true only for the last (best) entry
```

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
| `Seasons` | Added `ScoringEnabled BIT` |
| `Meets` | FK to Seasons, Locations; added `MeetType SMALLINT`, `ScoringTemplateId INT NULL` |
| `Locations` | |
| `Events` | Includes SortOrder, EventCategorySortOrder, AthleteCount, Gender (1/2/3), EventKey |
| `Athletes` | |
| `Performances` | AthleteId nullable (null = relay); SortedAthleteHash nullable |
| `PerformanceAthletes` | Junction: PerformanceId, AthleteId |
| `Leaderboards` | All-time rankings, rebuilt by `sp_RebuildLeaderboards` |
| `RunningRelayEvents` | Separate table for running relay event definitions: Id (UNIQUEIDENTIFIER), Name, Gender (INT), SortOrder, Environment (INT), Deleted (BIT), DateCreated, DateUpdated, DateDeleted |
| `ScoringTemplates` | Id, Name, IsBuiltIn; soft-deleted. Built-in template "Dual Meet (5-3-1)" seeded |
| `ScoringTemplatePlaces` | Id, ScoringTemplateId, Place, Points; unique per (template, place) |
| `MeetParticipants` | Id, MeetId, SchoolName, SortOrder; soft-deleted |
| `MeetEventScoringOverrides` | Id, MeetId, EventId, ScoringTemplateId; per-event template override |
| `MeetEntries` | Id, MeetId, EventId, AthleteId? (null=relay), PerformanceId? (null until result); soft-deleted |
| `MeetEntryAthletes` | Junction: MeetEntryId, AthleteId (relay team members) |
| `MeetPlacings` | Id, MeetId, PerformanceId, MeetParticipantId? (null=invitational), Place, FullPoints, SplitPoints; two filtered UNIQUE indexes |

`sp_RebuildLeaderboards` is a stored procedure that recalculates all leaderboard rankings and resets/recalculates `PersonalBest`, `SeasonBest`, and `SchoolRecord` flags on `Performances`. It is called after every performance insert, update, or delete. It does **not** filter by gender, so Mixed relay performances are ranked alongside Boys/Girls relay performances within their own event. `SchoolRecord` and `PersonalBest`/`SeasonBest` flags are only set for individual performances (`AthleteId IS NOT NULL`); relay rows keep these flags at 0 and use `AllTimeRank = 1` as the SR proxy instead.

---

## UI / Theming

- **Tailwind CSS v4** with dark mode support via `dark:` variants throughout.
- Dark mode toggled by `_DarkModeToggle.cshtml` partial.
- **Tab UI pattern (Outdoor/Indoor):** OUTDOOR tab is always first (leftmost) and active by default. INDOOR is second. This applies to: Season Details (`all-time-outdoor`/`all-time-indoor`), Home Page highlights, Home Page season leaders, and the Leaderboard (`env-outdoor`/`env-indoor`). Active tab class: `bg-gradient-to-r from-amber-600 to-yellow-500` (outdoor) or `from-blue-600 to-blue-500` (indoor). Default is triggered by `.click()` on the outdoor tab in `DOMContentLoaded`.
- Meets are displayed **oldest-first (ascending by date)** on both the Season Details page and the Meets index page.
- Relay athlete names are displayed separated by ` / ` (with spaces) in inline lists — not bullet points.

---

## Meet Participants, Entries, Placings & Season Scoring

### Pre-meet entry flow (admin)
1. Admin creates meets with `MeetType` (Dual/DoubleDual/Invitational) and optional opponent school names.
2. At `/Admin/MeetEntries?meetId=X`: admin adds `MeetEntry` rows (athlete + event) before the meet. Athletes who exceed the 4-event limit are flagged but not blocked.
3. After the meet: admin clicks "Enter Result" per entry → creates `Performance` + `PerformanceAthletes` + links `PerformanceId` on `MeetEntry` + creates `MeetPlacing` row(s) (one per opponent for Dual/DoubleDual, one overall for Invitational).

### Scoring templates
- Dual and DoubleDual meets are **always auto-assigned** the built-in "Dual Meet (5-3-1)" template. The template picker is only shown for Invitational meets in the Create/Edit form.
- `GetTemplatePointsAsync` lookup order: event-level override → meet default → 0 (out of range, silently).

### Relay points
Two pre-computed columns on `MeetPlacings`:
- `FullPoints` — each relay member gets the full template points (e.g., 5 pts for 1st, times 4 members = 20 total across the team)
- `SplitPoints` — `FullPoints / AthleteCount` (so the team gets 5 pts total)
Both are stored; the season scoring page provides a toggle to choose which to display.

### MeetPlacings uniqueness
- Per-opponent (Dual/DoubleDual): `UNIQUE (PerformanceId, MeetParticipantId) WHERE MeetParticipantId IS NOT NULL`
- Invitational overall: `UNIQUE (PerformanceId) WHERE MeetParticipantId IS NULL`
SQL Server's NULL != NULL behavior in unique indexes means these two filtered indexes correctly cover both cases.

### Season scoring page (`/seasons/{name}/scoring`)
- Gated by `Season.ScoringEnabled = true`. Returns 404 otherwise.
- Served by `SeasonsController.Scoring` → `IScoringService.GetSeasonScoringAsync`.
- `ScoringService` iterates `ScoringDataDto` rows (one per athlete per placing, relay expanded per member). Accumulates separate Full/Split totals for Running/Field, Individual/Relay, and per-`EventCategory`. Result is two sorted lists: `Boys` and `Girls`.
- View: `Views/Seasons/Scoring.cshtml` + `Views/Seasons/_ScoringGenderPanel.cshtml` partial. 4 sub-tabs per gender: Total, Running vs Field, Individual vs Relay, By Category. Full/Split toggle uses `data-full` / `data-split` attributes driven by JavaScript.

### Meet details page badges
`Views/Meets/Details.cshtml` Notes column now shows placing badges (🥇🥈🥉 or ordinal "4th", "5th", etc.) when `perf.HasPlacing` is true. One badge per `PerformancePlacingViewModel` in `perf.Placings`. Badge includes opponent school name for Dual/DoubleDual (`vs. School`), bare emoji for Invitational.

---

## Roster Details Page — Key Behaviors

(`Views/Roster/Details.cshtml` + `AthleteService.GetAthleteDetailsAsync`)

- **Hero stats**: TopSprintEvent and TopFieldEvent show an Indoor/Outdoor badge (`☀️ Outdoor` / `🏢 Indoor`) below the event name. These are individual events only (not relay).
- **Personal Records table**: includes both individual PRs and relay bests. Individual PRs use `PersonalBest = true` flag. Relay entries show the best time/distance per relay event regardless of flag. Relay rows show the team members inline (` / ` separated, each linked to their roster page) below the event name.
- **School Records column**: shows `SR` badge whenever `AllTimeRank == 1` (applies to both individual and relay). The Rank column is shown whenever any PR row has either a top-10 rank OR a school record.
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
- **Never use `p.SchoolRecord` in service, ViewModel, or view code to determine whether a performance is the school record.** Always use `AllTimeRank == 1` from the Leaderboards table. The `SchoolRecord` DB column is maintained by `sp_RebuildLeaderboards` for individual performances only — relay rows always have it as `0`. Even for individuals it should not be trusted in application code; `AllTimeRank == 1` is the single authoritative test for both.
- Every query that needs to surface `IsSchoolRecord` must either join `Leaderboards` and project `AllTimeRank`, or use a correlated subquery `(SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id)`. When using Dapper multi-mapping, place this subquery **after `p.*` and before the next model's `Id` column** so Dapper maps it to `Performance.AllTimeRank`.
- Relay athlete name display: always join with ` / ` separator in inline contexts, never individual bullet spans.
- For placing/scoring: use `FullPoints` / `SplitPoints` from `MeetPlacings` — do not recompute points in application code. `GetTemplatePointsAsync` handles the resolution chain.
- `MeetEntry.AthleteId` is NULL for relay entries. Relay athletes are in `MeetEntryAthletes`. The 4-event limit check uses `GetAthleteEventCountForMeetAsync` which counts both individual entries and relay team memberships via UNION ALL.
- When adding new DI registrations to `Program.cs`, follow the existing grouping: public repos first, then admin repos, then services.
