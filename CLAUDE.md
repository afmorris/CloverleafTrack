# CloverleafTrack — Codebase Guide

CloverleafTrack is an ASP.NET Core MVC web application (.NET 8+) that tracks high school track & field results — athletes, meets, performances, and all-time leaderboards.

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
└── CloverleafTrack.Tests/        # xUnit unit tests (Services/ subdirectory)
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
| `AthletesController` | `/athletes/{slug}` | Athlete career detail |
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
- Relay athlete names for display: `STRING_AGG(CONCAT(LastName, ', ', FirstName), '; ')` grouped by `PerformanceId`.
- After any Performance insert/update/delete: `EXEC sp_RebuildLeaderboards` is called to recalculate the `Leaderboards` table.
- `GetMeetsForSeasonAsync` returns meets `ORDER BY m.Date ASC` (ascending) from SQL.

### DTOs (DataAccess/Dtos/)
Dtos are used for complex query results that span multiple tables and don't map directly to a model. Examples: `MeetPerformanceDto`, `TopPerformanceDto`, `LeaderboardDto`, `HomePageStatsDto`.

---

## Services (CloverleafTrack.Services/)

| Service | Key methods |
|---|---|
| `SeasonService` | `GetSeasonCardsAsync`, `GetSeasonDetailsAsync` |
| `MeetService` | `GetMeetsIndexAsync`, `GetMeetDetailsAsync` |
| `AthleteService` | Athlete career pages |
| `LeaderboardService` | All-time top 10 queries |
| `HomeService` | Homepage aggregated data |

`MeetService.GetMeetsIndexAsync` groups all meets by season (seasons ordered descending by start date), then orders meets within each season **ascending by date**.

`MeetService.BuildOrderedEventGroups` orders meet results: Sprints → Distance → Hurdles → Running Relays → Jumps → Jump Relays → Throws → Throw Relays.

`SeasonService.GetSeasonDetailsAsync` maps top performances split by `Environment.Indoor` / `Environment.Outdoor`.

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

**ModelState clearing for RelayAthleteIds:** The POST controller unconditionally removes all `RelayAthleteIds.*` model state keys before checking `ModelState.IsValid`, because hidden slots produce type-conversion errors (`"" → int`). Then non-positive IDs are filtered from the list before relay athlete insert.

---

## Database Tables (inferred from SQL)

| Table | Notes |
|---|---|
| `Seasons` | |
| `Meets` | FK to Seasons, Locations |
| `Locations` | |
| `Events` | Includes SortOrder, EventCategorySortOrder, AthleteCount |
| `Athletes` | |
| `Performances` | AthleteId nullable (null = relay); SortedAthleteHash nullable |
| `PerformanceAthletes` | Junction: PerformanceId, AthleteId |
| `Leaderboards` | All-time rankings, rebuilt by `sp_RebuildLeaderboards` |

`sp_RebuildLeaderboards` is a stored procedure that recalculates all leaderboard rankings. It is called after every performance insert, update, or delete.

---

## UI / Theming

- **Tailwind CSS v4** with dark mode support via `dark:` variants throughout.
- Dark mode toggled by `_DarkModeToggle.cshtml` partial.
- Season Details page uses tab UI (OUTDOOR tab first / active by default, INDOOR second) implemented with vanilla JS in the `@section Scripts` block. Tab IDs: `all-time-outdoor`, `all-time-indoor`. Active tab class: `bg-gradient-to-r from-amber-600 to-yellow-500` (outdoor) or `from-blue-600 to-blue-500` (indoor).
- Meets are displayed oldest-first (ascending by date) on both the Season Details page and the Meets index page.

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
