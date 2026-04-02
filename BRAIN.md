# BRAIN.md — AI-Readable Accepted Changes Log

This file documents all accepted, shipped changes to CloverleafTrack that are not obvious from reading the current code. It is written for future AI assistants to load alongside CLAUDE.md. Read CLAUDE.md first for architecture context, then use this file to understand what changed from the original implementation and why.

---

## Format

Each entry is:
- **What changed** — the code that was written
- **Why** — the reason / bug being fixed
- **Key files** — exact paths to changed files
- **Watch out** — gotchas or related places to keep in sync

---

## Change Log

---

### [C1] Outdoor-First Tab Ordering (All Pages)

**What changed:**
All pages with Indoor/Outdoor tab pairs now default to showing the OUTDOOR tab first. The tab that renders first in HTML is Outdoor. `DOMContentLoaded` fires `.click()` on the outdoor tab button to ensure it is visually active on load.

**Why:**
Default rendering landed on Indoor. Outdoor is the primary season; it should be the default view.

**Key files:**
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`
- `CloverleafTrack.Web/Views/Seasons/Details.cshtml`
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Pattern:**
```js
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('tab-outdoor').click();
});
```

---

### [C2] Ascending Meet Ordering on Season Detail + Meets Index

**What changed:**
Meets are displayed in ascending date order (oldest meet first) on both the Season Details page and the Meets index page.

**Why:**
Meets were displaying in inconsistent order. Ascending chronological order is the natural reading order for a season's schedule/results.

**Key files:**
- `CloverleafTrack.Services/MeetService.cs` — `GetMeetsIndexAsync`, `GetSeasonDetailsAsync`
- `CloverleafTrack.DataAccess/Repositories/MeetRepository.cs` — `GetMeetsForSeasonAsync` uses `ORDER BY m.Date ASC`

**Watch out:**
`Details.cshtml` for Season Details applies an in-view `OrderBy(m => m.Date)` on the ViewModel list. This is an intentional exception to the "sort in service layer" rule because the ViewModel list is reused in multiple contexts.

---

### [C3] Relay Field Event ModelState Bug Fix

**What changed:**
The admin Performance `Create` action unconditionally removes all `RelayAthleteIds.*` keys from `ModelState` before calling `ModelState.IsValid`. Non-positive IDs are then filtered from the relay athlete list before insert.

**Why:**
Hidden relay slot `<select>` elements submit empty string `""` which fails `int` model binding, producing spurious ModelState errors that blocked valid relay performance entry.

**Key files:**
- `CloverleafTrack.Web/Areas/Admin/Controllers/PerformancesController.cs`

---

### [C4] Mixed Relay Support

**What changed:**
The leaderboard, meet details page, and admin forms now fully support `Gender.Mixed` (= 3) relay events.

**LeaderboardViewModel** — added two new properties:
```csharp
public List<LeaderboardCategoryViewModel> MixedOutdoorCategories { get; set; } = new();
public List<LeaderboardCategoryViewModel> MixedIndoorCategories { get; set; } = new();
```

**LeaderboardService** — `GetLeaderboardAsync` now filters for `Gender.Mixed` and populates both new lists.

**New partial** — `CloverleafTrack.Web/Views/Shared/_LeaderboardMixedSection.cshtml`:
- Model: `Tuple<bool, List<LeaderboardCategoryViewModel>>` (Item1 = isIndoor)
- Uses purple `border-purple-500` accent color
- Renders relay member links via `evt.RelayMembers` and `evt.MeetSlug`

**Leaderboard/Index.cshtml** — after Boys/Girls grid in each environment tab:
```html
@if (Model.MixedOutdoorCategories.Any())
{
    <!-- Mixed Relays section with purple heading -->
    @await Html.PartialAsync("_LeaderboardMixedSection", new Tuple<bool, List<LeaderboardCategoryViewModel>>(false, Model.MixedOutdoorCategories))
}
```

**Meets/Details.cshtml** — full-width Mixed Relays section rendered after Boys/Girls grid when `Model.MixedEvents.Any()`.

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Shared/_LeaderboardMixedSection.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`

---

### [C5] RunningRelayEvents Table — Separate from Events

**What changed:**
Mixed relay events are stored in a separate `RunningRelayEvents` table, NOT in the `Events` table.

**Why:**
The `Events` table schema does not cover relay-specific fields. `RunningRelayEvents` has its own schema.

**RunningRelayEvents schema:**
```sql
CREATE TABLE RunningRelayEvents (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    -- (other columns per actual DDL provided by user)
)
```

**Key watch-out:**
- `Id` is `UNIQUEIDENTIFIER`, NOT an auto-increment `INT`. Use `NEWID()` on insert.
- There is NO `EventKey` column in `RunningRelayEvents`.
- When inserting mixed relay events, target `RunningRelayEvents`, not `Events`.

---

### [C6] AthletePerformanceDto — Added SeasonStartDate and RelayAthletes

**What changed:**
Two fields added to `AthletePerformanceDto`:

```csharp
public DateTime SeasonStartDate { get; set; }
public string? RelayAthletes { get; set; }
```

**Why:**
- `SeasonStartDate`: SQL `UNION ALL` cannot `ORDER BY` a non-selected column. `s.StartDate` must be projected into both branches so the wrapping subquery can order by it.
- `RelayAthletes`: relay team members are passed through from SQL `STRING_AGG` to the service and view layers.

**Key files:**
- `CloverleafTrack.DataAccess/Dtos/AthletePerformanceDto.cs`

---

### [C7] AthleteRepository — UNION ALL for Individual + Relay Performances

**What changed:**
`GetAllPerformancesForAthleteAsync` was rewritten from a single-table query to a UNION ALL query that returns both individual and relay performances for a given athlete.

**Why:**
Relay performances have `AthleteId = NULL` on the `Performance` row; the athlete link is in `PerformanceAthletes`. The original query only returned individual performances.

**SQL pattern:**
```sql
SELECT * FROM (
    -- Branch 1: Individual performances (AthleteId = @AthleteId)
    SELECT p.Id as PerformanceId,
           ...,
           s.StartDate as SeasonStartDate,
           NULL as RelayAthletes
    FROM Performances p
    INNER JOIN Events e ON e.Id = p.EventId
    INNER JOIN Meets m ON m.Id = p.MeetId
    INNER JOIN Seasons s ON s.Id = m.SeasonId
    WHERE p.AthleteId = @AthleteId

    UNION ALL

    -- Branch 2: Relay performances where athlete is in PerformanceAthletes
    SELECT p.Id as PerformanceId,
           ...,
           s.StartDate as SeasonStartDate,
           (SELECT STRING_AGG(a2.FirstName + ' ' + a2.LastName, '|~|')
            FROM PerformanceAthletes pa2
            INNER JOIN Athletes a2 ON a2.Id = pa2.AthleteId
            WHERE pa2.PerformanceId = p.Id) as RelayAthletes
    FROM Performances p
    INNER JOIN PerformanceAthletes pa ON pa.PerformanceId = p.Id
    INNER JOIN Events e ON e.Id = p.EventId
    INNER JOIN Meets m ON m.Id = p.MeetId
    INNER JOIN Seasons s ON s.Id = m.SeasonId
    WHERE pa.AthleteId = @AthleteId AND p.AthleteId IS NULL
) AS combined
ORDER BY SeasonStartDate DESC, EventCategorySortOrder, MeetDate DESC
```

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`

**Watch out:**
- Both UNION branches must select `s.StartDate as SeasonStartDate` — required for outer ORDER BY.
- `STRING_AGG` separator is `|~|`. Split in C# using `RelayAthletes?.Split("|~|")`.
- Relay athlete name format from SQL is `FirstName LastName` (not `LastName, FirstName`).

---

### [C8] Relay PRs in Roster Details — PersonalBest Flag Unreliable

**What changed:**
`AthleteService.GetAthleteDetailsAsync` computes relay Personal Records via best-per-event logic, NOT by relying on the `PersonalBest` flag.

**Why:**
The admin performance entry form does NOT reliably set `PersonalBest = true` on relay `Performance` rows. Relying on the flag caused relay events to appear in the season section but never in the Personal Records table.

**Pattern in AthleteService:**
```csharp
// Individual PRs — rely on the PersonalBest flag (reliable for individuals)
var individualPRs = performances
    .Where(p => p.PersonalBest && p.RelayAthletes == null)
    .GroupBy(p => p.EventId)
    .Select(g => g.OrderByDescending(p => p.MeetDate).First());

// Relay bests — compute best per relay event regardless of flag
var relayBests = performances
    .Where(p => p.RelayAthletes != null)
    .GroupBy(p => p.EventId)
    .Select(g => g.First().TimeSeconds.HasValue
        ? g.OrderBy(p => p.TimeSeconds).First()
        : g.OrderByDescending(p => p.DistanceInches).First());

var personalRecords = individualPRs.Concat(relayBests).Select(...).ToList();
```

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`

---

### [C9] School Records Recalculation — Relay AllTimeRank == 1 Proxy

**What changed:**
`IsSchoolRecord` for relay performances in `PersonalRecordViewModel` uses `AllTimeRank == 1` as the school record proxy. `TotalSchoolRecords` in `AthleteDetailsViewModel` adds relay school records (distinct by EventId) to the individual count.

**Why:**
The `SchoolRecord` flag on `Performance` rows is NOT reliably set by the admin entry form for relay performances. An athlete with a Mixed 4×400M relay ranked #1 all-time correctly has a school record, but the flag was `false`, causing the hero to show 0 School Records.

**Pattern in AthleteService:**
```csharp
IsSchoolRecord = p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1,
```

```csharp
TotalSchoolRecords = performances.Count(p => p.SchoolRecord && p.RelayAthletes == null)
    + performances
        .Where(p => p.RelayAthletes != null && p.AllTimeRank == 1)
        .Select(p => p.EventId)
        .Distinct()
        .Count(),
```

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.ViewModels/Athletes/PersonalRecordViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/AthleteDetailsViewModel.cs`

---

### [C10] ViewModels — Relay Member Fields

**What changed:**
Added relay support fields to three ViewModels:

**`IndividualPerformanceViewModel`:**
```csharp
public string? RelayAthletes { get; set; }
public bool IsRelay => RelayAthletes != null;
public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();
```

**`PersonalRecordViewModel`:**
```csharp
public bool IsSchoolRecord { get; set; }
public string? RelayAthletes { get; set; }
public bool IsRelay => RelayAthletes != null;
public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();
```

**`AthleteTopEventViewModel`:**
```csharp
using Environment = CloverleafTrack.Models.Enums.Environment;
public Environment Environment { get; set; }
```

**Key files:**
- `CloverleafTrack.ViewModels/Athletes/IndividualPerformanceViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/PersonalRecordViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/AthleteTopEventViewModel.cs`

---

### [C11] Roster Details Page — Relay Display and Enhancements

**What changed — hero section:**
TopSprintEvent and TopFieldEvent now show an Indoor/Outdoor badge:
```cshtml
@if (Model.TopSprintEvent.Environment == CloverleafTrack.Models.Enums.Environment.Indoor)
{ <span>🏢 Indoor</span> }
else { <span>☀️ Outdoor</span> }
```

**What changed — Personal Records table:**
Relay team members displayed below the event name as linked names joined by ` / `:
```cshtml
@if (pr.IsRelay)
{
    <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
        @for (int i = 0; i < pr.RelayMembers.Length; i++)
        {
            <a href="/athletes/@...">@pr.RelayMembers[i]</a>
            @if (i < pr.RelayMembers.Length - 1) { <span> / </span> }
        }
    </div>
}
```

SR (School Record) badge in the Rank column shown when `pr.IsSchoolRecord` is true (even when AllTimeRank is null — covers relay records).

**What changed — season performance rows:**
Relay team members shown below the performance mark/date/meet row using the same ` / ` separator pattern.

**Key files:**
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Watch out:**
`hasTopTenRanks` check was expanded to `|| pr.IsSchoolRecord` so the Rank column renders for school records even when the athlete has no top-10 ranked individual performances.

---

### [C12] Season Ordering Fix — SeasonStartDate GroupBy Key

**What changed:**
Season grouping in `GetAthleteDetailsAsync` now groups by both `SeasonName` AND `SeasonStartDate`, and orders by `SeasonStartDate` descending.

**Why:**
Grouping by `SeasonName` string alone gave no reliable ordering. The new `SeasonStartDate` DTO field enables correct descending chronological ordering.

**Pattern:**
```csharp
var seasons = performances
    .GroupBy(p => new { p.SeasonName, p.SeasonStartDate })
    .OrderByDescending(g => g.Key.SeasonStartDate)
    .Select(...)
```

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`

---

## Relay Flag Reliability — Summary Table

This is the most important behavioral invariant to remember:

| Flag | Individual performances | Relay performances |
|---|---|---|
| `PersonalBest` | ✅ Reliable — set by admin form | ❌ NOT reliable — may be false even for best relay |
| `SchoolRecord` | ✅ Reliable — set by leaderboard rebuild | ❌ NOT reliable — may be false even for #1 all-time relay |
| `AllTimeRank` | ✅ Set by `sp_RebuildLeaderboards` | ✅ Set by `sp_RebuildLeaderboards` — use as SR proxy |

**Rule:** For relay performances, always compute PRs via best-per-event and use `AllTimeRank == 1` as the school record proxy. Never trust the `PersonalBest` or `SchoolRecord` flags on relay rows.

---

### [C13] Unit Test Suite — Initial Build-Out

**What changed:**
A full xUnit unit test suite was written and made green across `CloverleafTrack.Tests/Unit/`:

| File | Tests | What is covered |
|---|---|---|
| `Services/SeasonServiceTests.cs` | 2 | `GetCurrentSeasonAsync` returns `EndDate.Year` (not Id); throws `InvalidOperationException` when no current season |
| `Services/MeetServiceTests.cs` | 13 | Null slug, meet info, PR/SR counts, Boys/Girls/Mixed splits, event category ordering, meets index grouping and season ordering |
| `Services/LeaderboardServiceTests.cs` | 11 | Gender/environment partitioning, Mixed isolation, category grouping, relay-type separation, details null-when-empty, IsRelayEvent detection, PRs-only de-duplication |
| `Services/AthleteServiceTests.cs` | 18 | Active/former roster grouping, PR formatting, null slug, relay PR via best-per-event (not flag), TotalPRs counts only individuals, TotalSchoolRecords includes relay AllTimeRank==1, relay member parsing |
| `Utilities/PerformanceFormatHelperTests.cs` | 43 | `ParseTime` / `FormatTime` (sub-minute, colon, suffix, invalid), `ParseDistance` / `FormatDistance` (feet+inches, dash, total-inches, natural language, invalid), `FormatPerformance`, `FormatImprovement`, round-trip theories |

**Why:**
No automated test coverage existed. Tests were needed to lock in the relay flag workarounds (C8, C9) and the service-layer business logic that is not obvious from reading the code.

**Bugs found and fixed during test authoring:**

1. `SeasonServiceTests` constructor — `performanceRepository` and `meetRepository` were declared as fields but never initialized with `new Mock<...>()`, causing `NullReferenceException` on construction.

2. `SeasonServiceTests.GetCurrentSeason_ReturnsCorrectSeasonId` — asserted `result == 3` (the season Id) but `GetCurrentSeasonAsync()` returns `EndDate.Year`. Fixed assertion to `today.AddYears(2).Year`.

3. `PerformanceFormatHelperTests.ParseDistance_RoundTrip_WithFormatDistance` — theory assumed `FormatDistance` round-trips to the same compact input format (`"19'4"`) but the helper always produces `"19' 4\""` (canonical with space). Fixed by adding a third `InlineData` argument for the expected formatted string.

**Key files:**
- `CloverleafTrack.Tests/Unit/Services/SeasonServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Utilities/PerformanceFormatHelperTests.cs`

**Watch out:**
- `GetCurrentSeasonAsync()` returns `EndDate.Year` (an int representing the calendar year), not the season DB row Id. This is intentional — callers use the year as a display/filtering key.
- `FormatDistance` canonical output always includes a space: `"F' I\""`. The `ParseDistance` input parser accepts multiple input formats (compact, dash, natural language) but `FormatDistance` output is always the spaced canonical form.

---

## Relay Athlete Name Format

- **Stored in DB:** `STRING_AGG(a.FirstName + ' ' + a.LastName, '|~|')`
- **Format:** `FirstName LastName` (NOT `LastName, FirstName`)
- **Separator:** `|~|` (chosen to be unlikely to appear in real names)
- **C# split:** `RelayAthletes?.Split("|~|") ?? Array.Empty<string>()`
- **Display format:** Names joined by ` / ` inline, each linked to athlete's roster page

---

### [C14] Unit Test Suite — Models and ViewModels Layer

**What changed:**
Expanded the unit test suite to cover the Models and ViewModels layers. Added 66 new tests across 8 new test files, bringing the total to 153.

| File | Tests | What is covered |
|---|---|---|
| `Unit/Models/MeetTests.cs` | 6 | `Meet.Slug` generation via SlugHelper, `Meet.ResultsUrl` format (`/meets/{slug}`) |
| `Unit/ViewModels/Admin/SeasonProgressViewModelTests.cs` | 6 | `PercentComplete` integer division, zero-guard when `TotalMeets == 0`, truncation (not rounding) |
| `Unit/ViewModels/Admin/LocationOptionViewModelTests.cs` | 4 | `DisplayText` conditional: full `"Name (City, State)"` when both present, falls back to `Name` when either is empty |
| `Unit/ViewModels/Admin/AdminPerformanceOptionViewModelTests.cs` | 9 | `AthleteOptionViewModel.DisplayText` (`"Last, First (Year)"`); `EventOptionViewModel.DisplayText` + `CategoryName` for all categories including null; `MeetOptionViewModel.DisplayText` with date/env formatting |
| `Unit/ViewModels/Athletes/IndividualPerformanceViewModelTests.cs` | 9 | `IsRelay` (null check) and `RelayMembers` (`|~|` split) on both `IndividualPerformanceViewModel` and `PersonalRecordViewModel` |
| `Unit/ViewModels/Leaderboard/LeaderboardEventViewModelTests.cs` | 5 | `RelayMembers` (uses `IsNullOrEmpty` guard, not null check — different from athlete VMs), `AthleteFullName` concat |
| `Unit/ViewModels/Meets/MeetListItemViewModelTests.cs` | 4 | `IsUpcoming` date comparison against `DateTime.Now` |
| `Unit/ViewModels/Seasons/SeasonCardViewModelTests.cs` | 9 | `IndoorSchoolRecordCount` + `OutdoorSchoolRecordCount` null coalescing; `StatusBadge` for all `SeasonStatus` values |

**Bug found during test authoring:**

`MeetTests.Slug_StripsSpecialCharacters` initially asserted that `"St. Mary's Invitational"` would produce a slug without a period. **SlugHelper actually keeps periods** — it produces `"st.-marys-invitational"`. The apostrophe is stripped but the period is not. Fixed by removing the `.NotContain(".")` assertion.

**Watch out:**

- `SlugHelper` (Slugify NuGet package) **keeps periods** and **strips apostrophes**. Do not assume all special characters are removed.
- `LeaderboardEventViewModel.RelayMembers` uses `string.IsNullOrEmpty(RelayName)` as its guard — it handles both null and empty-string `RelayName`. This differs from `IndividualPerformanceViewModel.RelayMembers` which uses a null check on `RelayAthletes`. These are intentionally different because `RelayName` defaults to `string.Empty`, while `RelayAthletes` is nullable.
- `SeasonProgressViewModel.PercentComplete` uses **integer division** (`EnteredMeets * 100 / TotalMeets`), so 1/3 → 33 and 2/3 → 66 (truncates, not rounds). Do not change this to floating-point without understanding downstream display consequences.

**Key files:**
- `CloverleafTrack.Tests/Unit/Models/MeetTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/SeasonProgressViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/LocationOptionViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/AdminPerformanceOptionViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Athletes/IndividualPerformanceViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Leaderboard/LeaderboardEventViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Meets/MeetListItemViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Seasons/SeasonCardViewModelTests.cs` (NEW)

---

### [C15] Athlete Progression Charts — ViewModel + Service + View

**What changed:**

Added per-event progression charts to the Roster Details page, building on two major design decisions:
1. **Season view — Option 1**: table of performance rows on the left, Chart.js line chart on the right, side-by-side per event group.
2. **Career Progression — Mockup B**: dedicated "Career Progression" section below the season accordion with a tabbed event selector and full career-arc chart per event.

Three supporting changes were made before the view rewrite:

- `IndividualPerformanceViewModel` — added `public double? RawValue { get; set; }` — raw numeric value (TimeSeconds for running, DistanceInches for field) needed for Chart.js data arrays. Formatted strings are human-readable but can't be plotted.
- `EventPerformanceGroupViewModel` — added `public bool IsFieldEvent { get; set; }` — drives Chart.js `reverse` axis option (running events: lower = better so Y-axis is inverted; field events: higher = better, normal axis).
- `AthleteService.GetAthleteDetailsAsync` — set `EventId`, `IsFieldEvent`, and `RawValue` in the performance mapping. `EventId` was always 0 before because the `GroupBy` key was used but `EventId` was never assigned.

**Key behaviors in `Details.cshtml` rewrite:**

- **AllTimeRank shown for all athletes**: removed the `<= 10` guard — every athlete sees their rank regardless of value.
- **Chart.js lazy initialization**: charts in hidden accordion panels have zero size when initialized on `DOMContentLoaded`. Season charts are built when the accordion first opens (tracked via `canvas._chart`). Career charts are built when the career tab is first clicked (tracked via `careerCharts[idx]` object).
- **Relay events excluded from charts**: relay team compositions change meet-to-meet, making a progression line meaningless. Chart panel is suppressed for relay event groups; only the existing performance row layout is shown.
- **PR takes precedence over SB**: when a performance is both a PR and SB, the PR badge/color wins. Chart point types: `"pr"` (amber) → `"sb"` (blue) → `"normal"` (green).
- **Data passing**: `data-*` attributes on `<canvas>` elements carry JSON arrays for labels, values, point types, formatted performances, and ranks. Avoids CSP issues with inline scripts holding data.
- **`@functions` block**: `FormatImprovement(double delta, bool isField)` computes career improvement delta server-side for the stats row in the career section.

**New / updated test files:**

- `AthleteServiceTests.cs` — expanded with `GetAthleteDetailsAsync` tests covering: null athlete, no performances, individual PR uses PersonalBest flag, relay PR uses best-per-event regardless of flag, TotalPRs excludes relays, TotalSchoolRecords includes relay events at AllTimeRank==1, relay school record detection, season ordering by date, relay members parsed from `|~|` string.
- `LeaderboardServiceTests.cs` (NEW) — covers `GetLeaderboardAsync` gender/environment partitioning, category grouping, relay category separation, ordering; covers `GetLeaderboardDetailsAsync` null handling, event info, all-performances count, PRs-only per-athlete deduplication, relay event detection.
- `MeetServiceTests.cs` (NEW) — covers `GetMeetDetailsAsync` null slug, name/location, PR/SR counts, boys/girls/mixed split, unique athlete count, event ordering (Sprints→Distance→Hurdles→Running Relays→Jumps→Throws); covers `GetMeetsIndexAsync` total count, season grouping, season ordering descending, meets within season ascending.

**Watch out:**

- `EventPerformanceGroupViewModel.EventId` was always 0 before this change. Tests or code that relied on it being 0 would be wrong — it's now populated from the `GroupBy` key.
- Chart.js with hidden panels: always lazy-initialize charts. Initializing in `DOMContentLoaded` while the container is `display:none` produces a 0×0 canvas and broken charts.
- `RawValue` is `DistanceInches ?? TimeSeconds` — for relay field events this correctly picks distance; for all running events this picks time. Null when neither is set (shouldn't happen in practice but guard in JS with `!= null` filter).

**Key files:**
- `CloverleafTrack.ViewModels/Athletes/IndividualPerformanceViewModel.cs` (MODIFIED — added RawValue)
- `CloverleafTrack.ViewModels/Athletes/EventPerformanceGroupViewModel.cs` (MODIFIED — added IsFieldEvent)
- `CloverleafTrack.Services/AthleteService.cs` (MODIFIED — set EventId, IsFieldEvent, RawValue)
- `CloverleafTrack.Web/Views/Roster/Details.cshtml` (REWRITTEN)
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` (EXPANDED)
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs` (NEW)

---
