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

### [C9] School Records Recalculation — AllTimeRank == 1 Proxy (Individual + Relay)

**What changed:**
`IsSchoolRecord` for relay performances in `PersonalRecordViewModel` uses `AllTimeRank == 1` as the school record proxy. `TotalSchoolRecords` in `AthleteDetailsViewModel` uses `AllTimeRank == 1` for **both** individual and relay performances (distinct by EventId).

**Why:**
The `SchoolRecord` flag on `Performance` rows is NOT reliably cleared when a newer performance supersedes the record — `sp_RebuildLeaderboards` does not reset it. An athlete with a relay at #1 all-time or an individual event where the flag is stale would show 0 School Records in the hero. Using `AllTimeRank == 1` from the Leaderboards table (which IS kept current by the SP) is authoritative for both cases.

**Pattern in AthleteService:**
```csharp
IsSchoolRecord = p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1,
```

```csharp
TotalSchoolRecords = performances
    .Where(p => p.AllTimeRank == 1)
    .Select(p => p.EventId)
    .Distinct()
    .Count(),
```

**Note:** The `IsSchoolRecord` mapping in `PersonalRecordViewModel` still uses `p.SchoolRecord` for individual rows (via the ternary above). That is fine for the PR table display because `sp_RebuildLeaderboards` does update `SchoolRecord` for individual performances after it is fixed (see C17). The hero count must not use the flag because it may be stale before the SP fix runs.

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
            <a href="/@...">@pr.RelayMembers[i]</a>
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
| `SchoolRecord` | ⚠️ Snapshot only — NOT cleared when a newer record supersedes it | ❌ NOT reliable — may be false even for #1 all-time relay |
| `AllTimeRank` | ✅ Set by `sp_RebuildLeaderboards` — use as current SR indicator | ✅ Set by `sp_RebuildLeaderboards` — use as SR proxy |

**Rule:** Never use `p.SchoolRecord` to determine whether a performance is *currently* the school record. It means "was the record when this flag was last written." Use `AllTimeRank = 1` from the Leaderboards table instead. `sp_RebuildLeaderboards` keeps Leaderboards current but does NOT retroactively clear the `SchoolRecord` flag on Performance rows that have been beaten.

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

### [C16] Roster Details — Mobile Responsive Chart Layout

**What changed:**
The season trajectory chart panel on the Roster Details page (per-event performance section) is now responsive. On mobile it stacks below the performance rows; on wider screens it sits side-by-side to the right.

**Why:**
The original layout used `flex gap-4 items-start` with a fixed `w-72 flex-shrink-0` chart panel. On narrow screens the chart overflowed the container and the performance rows were squished to ~86px wide — unusable. Also, meet names in performance rows overflowed because the `flex-1` cell lacked `min-w-0`.

**Pattern used:**
`flex-wrap` (already compiled) instead of `flex-col sm:flex-row` (responsive Tailwind classes that would require a CSS rebuild to take effect). An inline `style="min-width:260px"` on the performances div ensures `flex-wrap` triggers wrapping at ~564px (260 + 16 gap + 288 chart), giving side-by-side on tablet/desktop and stacked on mobile without requiring new Tailwind classes to be compiled.

Meet name link gets `min-w-0` on the flex-1 container and `block truncate` on the `<a>` tag to prevent the inner row from overflowing on narrow screens.

**Key files:**
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Watch out:**
- Do NOT use new Tailwind responsive variants (e.g. `sm:flex-row`) in this file without also running the Tailwind CSS build. These classes are not compiled by default and will be silently ignored.
- The `min-width:260px` inline style on the performances div is load-bearing for the flex-wrap stacking behavior. Removing it collapses the performances div to near-zero width because `flex-1 min-w-0` has no minimum.

---

### [C17] "On This Day" — SchoolRecord Flag Is Stale on Superseded Performances

**What changed:**
`HomeRepository.GetPerformanceOnThisDayAsync` no longer uses `p.SchoolRecord` to determine or sort by school record status. It now derives school record status live from the Leaderboards table (`AllTimeRank = 1`).

**Before:**
```sql
p.SchoolRecord AS IsSchoolRecord,
ORDER BY p.SchoolRecord DESC, ...
```

**After:**
```sql
CASE WHEN (SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) = 1
     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsSchoolRecord,
ORDER BY
    CASE WHEN (...AllTimeRank...) = 1 THEN 0 ELSE 1 END,
    CASE WHEN (...AllTimeRank...) <= 3 THEN 0 ELSE 1 END,
    m.Date DESC
```

**Why:**
When a new school record is set, the old performance's `SchoolRecord = 1` flag in the `Performances` table is NOT cleared. The flag is a snapshot of status at entry/rebuild time, not a live indicator. The old performance was being sorted to the top and displayed as "set the school record" even after being beaten.

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`

**Watch out:**
- `p.SchoolRecord` is unreliable for determining CURRENT school record status on **any** performance row (individual or relay) when the question is "is this still the record today?" It means "was the record at the time this flag was last written." Use `AllTimeRank = 1` from the Leaderboards table for current status.
- The existing reliability table in this file (end of C9 section) was updated to reflect this — see the updated summary below.
- `sp_RebuildLeaderboards` keeps the `Leaderboards` table current but does NOT retroactively clear the `SchoolRecord` flag on Performance rows that are no longer #1.

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

### [C19] sp_RebuildLeaderboards — Now Manages SchoolRecord Flag

**What changed:**
Added two steps to `sp_RebuildLeaderboards` in `docs/schema.sql`:

```sql
-- Step 7: Reset SchoolRecord flag on all Performances
UPDATE Performances SET SchoolRecord = 0;

-- Step 8: Set SchoolRecord = 1 for individual performances ranked #1 all-time
UPDATE p SET p.SchoolRecord = 1
FROM Performances p
INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id
WHERE lb.Rank = 1 AND p.AthleteId IS NOT NULL;
```

These run after the Leaderboards table is fully rebuilt (steps 9–10, previously 7–8).

**Why:**
The SP previously managed `PersonalBest` and `SeasonBest` (reset-all then recalculate) but never touched `SchoolRecord`. When a new record was set, the old performance kept `SchoolRecord = 1` forever. This caused stale flags to surface in "On This Day", meet details hero counts, and anywhere else that used the flag for current school record status.

**Key files:**
- `docs/schema.sql` — SP definition updated. Run `ALTER PROCEDURE` against the live DB to apply.

**Watch out:**
- After altering the SP, run `EXEC sp_RebuildLeaderboards` once to backfill all stale flags.
- Relay performances still get `SchoolRecord = 0` from this SP — they are excluded by `AND p.AthleteId IS NOT NULL`. The app uses `AllTimeRank = 1` as the SR proxy for relay rows, which is correct and authoritative.
- The schema comment on the SP header was updated to reflect that it now manages SchoolRecord in addition to PersonalBest and SeasonBest.

---

### [C20] Meet Details Hero — TotalSchoolRecords Uses AllTimeRank == 1

**What changed:**
`MeetService.GetMeetDetailsAsync` now counts school records using `AllTimeRank == 1` instead of the stale `p.SchoolRecord` flag:

```csharp
// Before
TotalSchoolRecords = performances.Count(p => p.SchoolRecord),

// After
TotalSchoolRecords = performances.Count(p => p.AllTimeRank == 1),
```

`AllTimeRank` is already populated from the Leaderboards table by `MeetRepository.GetPerformancesForMeetAsync`.

**Why:**
Same root cause as the athlete details hero (C9) and "On This Day" (C17): `p.SchoolRecord` is a stale snapshot that is not cleared when a newer record supersedes it. A meet with 2 school records was showing 0 because both performances had stale `SchoolRecord = 0` flags.

**Key files:**
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs` — `BuildPerf` helper gained `allTimeRank` parameter; school records test updated to pass `allTimeRank: 1` instead of `sr: true`.

---

### [C18] Roster Index — Relay Events Contribute to Event Category Grouping

**What changed:**
The Roster Index active-athlete grouping now includes relay event participation. Athletes who only run relays (or whose relay events span categories) appear in the correct individual-event section rather than being invisible or grouped under Relays.

**Repository (`AthleteRepository.GetAllWithPerformancesAsync`):**
- Added `e.EventType` to the Event SELECT (was missing; defaulted to 0 = Field which masked issues)
- Added UNION ALL branch for relay performances via `PerformanceAthletes` (relay `Performance` rows have `AthleteId = NULL` so they were previously excluded by the `INNER JOIN Performances p ON p.AthleteId = a.Id` filter)

**Service (`AthleteService`):**
Two new static helpers:

`GetRosterCategory(Event)` — maps relay EventType to the corresponding individual EventCategory:
- `JumpRelay` → `Jumps`
- `ThrowsRelay`, `FieldRelay` → `Throws`
- `RunningRelay` → `Sprints` unless the event name contains distance keywords ("distance medley", "dmr", "800", "1500", "1600", "mile", "2000", "3200") → `Distance`
- Everything else → `ev.EventCategory` (stored value, used as-is for individual events)

`IsDistanceBasedEvent(Event)` — replaces the old `EventCategory is Throws or Jumps` check in the PR lookup. Handles relay EventTypes explicitly because relay events have `EventCategory.Relays`, not Throws/Jumps:
- `FieldRelay`, `JumpRelay`, `ThrowsRelay` → true (use DistanceInches)
- `Field` → fall back to `EventCategory is Throws or Jumps`
- `Running`, `RunningRelay` → false (use TimeSeconds)

**Watch out:**
- `EventCategory.Relays` no longer appears as a key in the active athletes dictionary. All relay-only athletes are now bucketed under individual categories.
- An athlete with both individual 100m and 4×100m relay participation appears once in Sprints with both events listed (de-duplication is handled by the existing `GroupBy(x => x.Athlete.Id)` within each category group).
- The RunningRelay → Sprints/Distance name-based heuristic checks for substrings: "800" in the event name means distance relay. Sprint Medley Relay doesn't contain a distance number in its standard name so it correctly maps to Sprints.

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` (5 new tests)

---

### [C22] Season Index + Season Details — SchoolRecord Counts Use Leaderboards

**What changed:**
Four locations were still using the stale `p.SchoolRecord` flag to count school records for seasons. All updated to use `Leaderboards.Rank = 1` as the authoritative source.

**1. `SeasonRepository.GetSeasonsWithMeetsAsync` SQL:**
Added AllTimeRank subquery between `p.*` and `e.*` so Dapper's multi-mapping assigns it to the `Performance` type:
```sql
(SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank,
```

**2. `SeasonService.GetSeasonCardsAsync`:**
```csharp
// Before
.Where(p => p.SchoolRecord)

// After
.Where(p => p.AllTimeRank == 1)
```
Applied to both `IndoorSchoolRecords` and `OutdoorSchoolRecords` LINQ chains.

**3. `PerformanceRepository.CountSchoolRecordsBrokenForSeasonAsync` SQL:**
```sql
-- Before
WHERE m.SeasonId = @SeasonId AND p.SchoolRecord = 1

-- After (uses INNER JOIN on Leaderboards Rank = 1)
INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id AND lb.Rank = 1
WHERE m.SeasonId = @SeasonId
```

**4. `MeetRepository.GetMeetsForSeasonAsync` SQL:**
Replaced `COUNT(CASE WHEN p.SchoolRecord = 1 ...)` (which was in a GROUP BY query and couldn't easily join Leaderboards per-row) with a correlated subquery:
```sql
(SELECT COUNT(*) FROM Performances p2 INNER JOIN Leaderboards lb ON lb.PerformanceId = p2.Id AND lb.Rank = 1 WHERE p2.MeetId = m.Id) AS SchoolRecordCount,
```

**Why:**
Season Index was showing 0 Indoor SRs and 0 Outdoor SRs for seasons where records were set. Season Details "Season Overview" SR count was also wrong. Root cause identical to C17/C19/C20: `p.SchoolRecord` is a stale snapshot.

**Watch out:**
- `Performance.AllTimeRank` (added in the previous session) is what allows the Dapper multi-mapping approach in `GetSeasonsWithMeetsAsync`. The subquery must stay between `p.*` and `e.*` in the SELECT column order — otherwise Dapper maps it to the wrong type.
- `CountSchoolRecordsBrokenForSeasonAsync` now counts ALL performances at rank 1 for a season (not just those with the stale flag set), which is the correct behavior.

**5. `MeetRepository.GetAllMeetsWithStatsAsync` SQL (Meet Index page SR counts):**
Same correlated-subquery fix as `GetMeetsForSeasonAsync`:
```sql
(SELECT COUNT(*) FROM Performances p2 INNER JOIN Leaderboards lb ON lb.PerformanceId = p2.Id AND lb.Rank = 1 WHERE p2.MeetId = m.Id) AS SchoolRecordCount,
```

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/SeasonRepository.cs`
- `CloverleafTrack.Services/SeasonService.cs`
- `CloverleafTrack.DataAccess/Repositories/PerformanceRepository.cs`
- `CloverleafTrack.DataAccess/Repositories/MeetRepository.cs` (both `GetMeetsForSeasonAsync` and `GetAllMeetsWithStatsAsync`)

---

### [C23] Full SchoolRecord Sweep — All Remaining Stale Flag Usages Eliminated

**What changed:**
A codebase-wide audit found the remaining places still using `p.SchoolRecord` (the DB flag) instead of `AllTimeRank == 1`. Every one was updated.

**`AthleteService.cs`** — three locations:
- `PersonalRecordViewModel.IsSchoolRecord`: was `p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1`; simplified to `p.AllTimeRank == 1` for both
- `SeasonPerformanceViewModel.SchoolRecordCount`: `seasonGroup.Count(p => p.SchoolRecord)` → `seasonGroup.Count(p => p.AllTimeRank == 1)`
- `IndividualPerformanceViewModel.IsSchoolRecord`: `p.SchoolRecord` → `p.AllTimeRank == 1`

**`MeetService.cs`** — both event group builders (`AddEventGroupsForCategory` and `AddEventGroupsFromList`):
- `MeetPerformanceViewModel.IsSchoolRecord`: `p.SchoolRecord` → `p.AllTimeRank == 1` (AllTimeRank already populated from `GetPerformancesForMeetAsync`)

**`LeaderboardService.cs`** — both performance list projections:
- `LeaderboardPerformanceViewModel.IsSchoolRecord`: `perf.SchoolRecord` → `perf.AllTimeRank == 1` (AllTimeRank already populated as `lb.Rank` from the leaderboard query's `LEFT JOIN Leaderboards`)

**`HomeRepository.cs`** — `GetRecentTopPerformanceAsync`:
- `recentSql`: replaced `p.SchoolRecord AS IsSchoolRecord` with Leaderboards subquery; updated `ORDER BY p.SchoolRecord DESC` to `ORDER BY CASE WHEN (SELECT MIN(lb.Rank) ...) = 1 THEN 0 ELSE 1 END`
- `seasonBestSql`: replaced `p.SchoolRecord AS IsSchoolRecord` with `CAST(1 AS BIT)` (safe — this query already `INNER JOIN Leaderboards lb ... AND lb.Rank = 1`)

---

### [C24] UX Overhaul — Filter Chips, Flat Design, Search, Roster, Meets

**What changed:**
A broad UX improvement pass covering nearly every public-facing page. Changes are non-breaking — server rendering unchanged, JS is progressive enhancement.

**1. Career progression charts removed from athlete detail page**
`Views/Roster/Details.cshtml` — all `<canvas>` elements, Chart.js CDN script tag, chart initialization JS, and chart-related C# variables removed. Career stats grid (`Career PR`, `Competitions`, etc.) and season accordion are kept.

**2. Global search (`/search-index.json` + `wwwroot/js/search.js`)**
- New `ISearchService`/`SearchService` — builds a JSON index of athletes, meets, and events
- New `SearchController` at `GET /search-index.json` with 5-min response cache
- `wwwroot/js/search.js` — lazy-loads the index on first focus of `#site-search`, groups results by type (Athletes/Meets/Events, max 5 each), keyboard-navigable
- `_Layout.cshtml` updated: search input in header, dark mode flash fix (synchronous IIFE in `<head>`), footer redesigned with three columns, title format `{Page} · Cloverleaf Track & Field`

**3. Filter chip system (`wwwroot/js/filters.js` + `_FilterChipGroup.cshtml`)**
- `filters.js` — IIFE; reads/writes URL hash; applies `filter-chip-active`/`filter-chip-inactive` classes; hides `[data-filterable]` items that don't match active filters; hides `[data-filterable-section]` containers when all children are hidden
- `_FilterChipGroup.cshtml` — shared partial using ViewData keys `Label`, `FilterKey`, `Options`
- CSS classes `filter-chip-active` and `filter-chip-inactive` added to `input.css` via `@layer components`; `details[open] .details-arrow` rotation added for `<details>` expand animation

**4. Leaderboard — tabs → filter chips**
`Views/Leaderboard/Index.cshtml` completely replaced: Outdoor/Indoor tab bar replaced with filter chips (`env`, `gender`, `category`); content sections use `data-filterable data-env`/`data-gender`/`data-category` attributes; `data-filterable-section` on gender column divs; date-based default env set via inline script.

**5. Home page — tabs → shared segmented control with localStorage**
`_HomePageRecentHighlightsCard.cshtml` and `_HomePageSeasonLeaders.cshtml` — tab bars removed; content divs get `data-filterable data-env="outdoor/indoor"`. A single segmented control in `Home/Index.cshtml` (above these two partials) drives both. Scripts use `localStorage.getItem('ctf.environment')` for preference persistence (key: `ctf.environment`), with month-based fallback (Dec–Feb → indoor).

**6. Roster — one card per athlete**
`AthleteViewModel` → added `Categories` (`List<EventCategory>`). `RosterViewModel` → added `FlatActiveAthletes` (`List<AthleteViewModel>`). `IAthleteService` + `AthleteService` → new `GetFlatActiveAthletesAsync`. `RosterController.Index` populates `FlatActiveAthletes`. `_RosterActiveAthletesList.cshtml` rewritten: flat grid from `FlatActiveAthletes`; gender + category filter chips; each card wrapped in `data-filterable data-gender data-categories`. Eliminates duplicate athlete cards for athletes in multiple event categories.

**7. Meets — reverse-chronological, current season expanded, past collapsed**
`_MeetsCard.cshtml` rewritten: current season (`IsCurrentSeason = true`) always expanded with "Coming Up" / "Completed" subsections; past seasons wrapped in `<details data-filterable-section>`; completed meets `OrderByDescending(m => m.Date)` in view. `_MeetListItem.cshtml` updated: flat design (`bg-white`), `data-filterable data-env` attributes, env badge with text label.

**8. Visual polish — gradients replaced with flat surfaces**
All `bg-gradient-to-br/r from-{color}-50 to-{color}-100 dark:from-{color}-900/40 ...` backgrounds replaced with `bg-white dark:bg-gray-800` (or `bg-{color}-50 dark:bg-{color}-900/20` for small badges). Files affected: all `_HomePage*.cshtml`, `_Current/PreviousSeasonCard.cshtml`, `_OverallSeasonStats.cshtml`, `_MeetsOverallStats.cshtml`, `Meets/Details.cshtml`, `Seasons/Details.cshtml`, `Leaderboard/Details.cshtml` (accent line). Tab JS in `Seasons/Details.cshtml` replaced with the same filter chip + localStorage pattern.

**9. Seasons/Details.cshtml — tabs → filter chips**
Outdoor/Indoor tab bar replaced with a segmented control using the filter chip pattern. Same localStorage persistence as Home page. Content divs get `data-filterable data-env`.

**Key files:**
- `wwwroot/js/filters.js` (NEW)
- `wwwroot/js/search.js` (NEW)
- `wwwroot/css/input.css` (appended `@layer components` filter chip CSS + details-arrow)
- `Views/Shared/_FilterChipGroup.cshtml` (NEW)
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_MainNavigation.cshtml`
- `Views/Shared/_MeetsCard.cshtml`
- `Views/Shared/_MeetListItem.cshtml`
- `Views/Shared/_HomePageRecentHighlightsCard.cshtml`
- `Views/Shared/_HomePageSeasonLeaders.cshtml`
- `Views/Shared/_HomePageSeasonAtAGlanceCard.cshtml`
- `Views/Shared/_HomePageOnThisDayCard.cshtml`
- `Views/Shared/_CurrentSeasonCard.cshtml`
- `Views/Shared/_PreviousSeasonCard.cshtml`
- `Views/Shared/_OverallSeasonStats.cshtml`
- `Views/Shared/_MeetsOverallStats.cshtml`
- `Views/Shared/_LeaderboardGenderSection.cshtml`
- `Views/Shared/_LeaderboardMixedSection.cshtml`
- `Views/Shared/_RosterActiveAthletesList.cshtml`
- `Views/Shared/_AthleteCard.cshtml`
- `Views/Roster/Details.cshtml`
- `Views/Roster/Index.cshtml`
- `Views/Home/Index.cshtml`
- `Views/Meets/Index.cshtml`
- `Views/Leaderboard/Index.cshtml`
- `Views/Seasons/Details.cshtml`
- `Views/Leaderboard/Details.cshtml`
- `Services/Interfaces/ISearchService.cs` (NEW)
- `Services/SearchService.cs` (NEW)
- `Web/Controllers/SearchController.cs` (NEW)
- `ViewModels/AthleteViewModel.cs` (+ `Categories`)
- `ViewModels/RosterViewModel.cs` (+ `FlatActiveAthletes`)
- `Services/Interfaces/IAthleteService.cs` (+ `GetFlatActiveAthletesAsync`)
- `Services/AthleteService.cs` (+ `GetFlatActiveAthletesAsync`)
- `Web/Controllers/RosterController.cs`
- `Web/Program.cs` (+ `SearchService` DI)

**Watch out:**
- `filter-chip-active`/`filter-chip-inactive` are defined in `input.css` `@layer components`. After changing these styles, run `pnpm run dev` to rebuild `site.css`.
- The `env` filter key is used on Home, Leaderboard, Meets, and Season Details pages. Each page sets its own hash-based default (via inline script), reading `localStorage('ctf.environment')` when available.
- `_FilterChipGroup.cshtml` casts ViewData `Options` to `IEnumerable<(string Value, string Label)>`. Must pass as `List<(string, string)>` in callers.
- Roster flat list uses `FlatActiveAthletes` from `RosterViewModel`. The old `ActiveAthletes` (`Dictionary<EventCategory, List<AthleteViewModel>>`) is still populated and used by `_AthleteCategorySection.cshtml` (if still referenced anywhere).
- The `data-filterable-section` on `<details>` in `_MeetsCard.cshtml` will set `hidden=true` on the entire `<details>` element when all its meet items are filtered out — this hides the entire season row, which is correct.

**`AdminPerformanceRepository.cs`** — `GetAllWithDetailsAsync`:
- Added `(SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank` subquery (placed after `p.*`, before `a.*` for correct Dapper multi-mapping)

**`Admin/Views/Performances/Index.cshtml`:**
- `@if (perf.SchoolRecord)` → `@if (perf.AllTimeRank == 1)`

**Why:**
The `SchoolRecord` DB flag had been trusted in ~10 places across services, repositories, and a Razor view. Each one was either showing 0 SRs or displaying SR badges for performances that no longer hold the record. The universal fix is `AllTimeRank == 1` — this reads directly from the `Leaderboards` table which is always rebuilt fresh by `sp_RebuildLeaderboards`.

**Watch out:**
- `p.SchoolRecord` (the DB column) still exists on the `Performance` model and is still written by `sp_RebuildLeaderboards` for individual rows. It should be treated as a DB-level implementation detail only — never read in application code.
- `Performance.AllTimeRank` is a C# property only (`int?`), not a DB column. It is `null` unless the query explicitly includes a Leaderboards subquery or join. Any new query that needs `IsSchoolRecord` must add the subquery — do not assume it will be populated by `p.*`.
- The Dapper multi-mapping rule: when adding the AllTimeRank subquery to a multi-type SELECT, it must sit after `p.*` and before the next model's `Id` split column, otherwise Dapper assigns it to the wrong type.

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`
- `CloverleafTrack.DataAccess/Repositories/AdminPerformanceRepository.cs`
- `CloverleafTrack.Web/Areas/Admin/Views/Performances/Index.cshtml`

---

### [C24] Leaderboard Details — School Record Progression Timeline + Chart

**What changed:**
Added a "School Record History" section to the leaderboard event detail page (`/leaderboard/{eventKey}`). It shows every time the school record was broken, who broke it, the improvement over the previous record, and a step-line chart of the progression over time. Record-setting rows in the all-performances and PRs-only tables are highlighted with an amber left border and a dimmed SR badge.

**New ViewModel: `SchoolRecordMomentViewModel`**
One entry in the progression (athlete, formatted performance, raw numeric value for Chart.js, improvement delta as formatted string, `IsCurrentRecord` flag).

**Updated ViewModels:**
- `LeaderboardPerformanceViewModel` — added `WasRecordAtTime` (bool): was the school record at the moment it was performed, regardless of whether it still is now
- `LeaderboardDetailsViewModel` — added `IsFieldEvent` (bool) and `SchoolRecordProgression` (List)

**`LeaderboardService.GetLeaderboardDetailsAsync` — C# progression computation:**

No new SQL query. Uses the performances already fetched by `GetAllPerformancesForEventAsync`:
```csharp
// 1. Sort chronologically (same-day ties: best mark first)
var chronological = allPerformances
    .OrderBy(p => p.MeetDate)
    .ThenBy(p => isFieldEvent ? -(p.DistanceInches ?? 0) : (p.TimeSeconds ?? double.MaxValue))
    .ToList();

// 2. Walk and track running best
double? runningBest = null;
foreach (var perf in chronological)
{
    var value = isFieldEvent ? perf.DistanceInches : perf.TimeSeconds;
    var isNewRecord = runningBest == null ||
                      (isFieldEvent ? value > runningBest : value < runningBest);
    if (isNewRecord) { /* add to progression, record PerformanceId */ }
    runningBest = value;
}

// 3. Sort for display: best-first (distance desc / time asc)
// 4. Set WasRecordAtTime on AllPerformances and PersonalRecordsOnly via HashSet<int>
```

**`FormatImprovement(double delta, bool isField)` helper** — formats the delta between consecutive records:
- Field: `"+2' 6.25\""` (feet+inches)
- Running: `"-0.43s"` or `"-1:02.50"` for multi-minute events

**`Details.cshtml` — UI:**
- Collapsible card above the toggle buttons; collapsed by default
- Left column: vertical timeline of record holders with green improvement badge, "Current SR" callout for the latest, "First recorded school record" for the oldest
- Right column: Chart.js step-line chart, lazy-initialized on first open (`srChartBuilt` guard). Y-axis is reversed for running events (lower = better); normal for field events. Data passed via `data-*` attributes on the `<canvas>` to avoid CSP issues.
- Both performance tables: amber `border-l-2 border-l-amber-500 bg-amber-500/5` on `WasRecordAtTime` rows. SR badge shown at full opacity for current record, dimmed for former records.

**Watch out:**
- The progression is computed from `GetAllPerformancesForEventAsync` which already fetches all performances ordered best-first. Re-sorting chronologically in C# is intentional — do not add a new SQL query.
- `RawValue` on `SchoolRecordMomentViewModel` is `DistanceInches` for field events and `TimeSeconds` for running events. The Chart.js Y-axis `reverse: !isField` handles direction — do not negate values.
- Same-day tie-breaking: if two performances happen on the same date, the better mark is processed first (it's the one that "set the record"). The worse mark on the same day is skipped since it can't be a new best.
- The panel is collapsed by default. The chart is not initialized until first open (`srChartBuilt` flag). This mirrors the athlete progression chart lazy-init pattern (C15).

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/SchoolRecordMomentViewModel.cs` (NEW)
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardPerformanceViewModel.cs`
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardDetailsViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml`

---

### [C25] Meet Participants, Entries, Placings & Season Scoring

**What changed:**
Full feature: pre-meet entry tracking, post-meet placing entry, scoring templates, and a season scoring page.

**Key additions:**

*Models (CloverleafTrack.Models):*
- `MeetType` enum: `Dual=1, DoubleDual=2, Invitational=3`
- `ScoringTemplate`, `ScoringTemplatePlace`, `MeetParticipant`, `MeetEventScoringOverride`, `MeetEntry`, `MeetEntryAthlete`, `MeetPlacing`
- `Meet` — added `MeetType`, `ScoringTemplateId`, `Participants`, `ScoringTemplate`
- `Season` — added `ScoringEnabled`

*DTOs:*
- `MeetEntryDto` — flat DTO for meet entry list; `IsRelay` and `AthleteDisplayName` are computed properties
- `ScoringDataDto` — one row per athlete per placing; relay rows expanded per member via UNION ALL in the repository

*Repository interfaces + implementations (7 new):*
- `IScoringTemplateRepository` / `ScoringTemplateRepository`
- `IAdminScoringTemplateRepository` / `AdminScoringTemplateRepository`
- `IAdminMeetParticipantRepository` / `AdminMeetParticipantRepository`
- `IAdminMeetEntryRepository` / `AdminMeetEntryRepository`
- `IAdminMeetPlacingRepository` / `AdminMeetPlacingRepository`
- `IMeetPlacingRepository` / `MeetPlacingRepository`
- `ISeasonScoringRepository` / `SeasonScoringRepository`
- `IMeetRepository` — added `GetParticipantsForMeetAsync`
- `AdminMeetRepository`, `AdminSeasonRepository` — updated INSERT/UPDATE SQL for new columns

*Services:*
- `IScoringService` / `ScoringService` — aggregates ScoringDataDto rows per (AthleteId, Gender) accumulating Full/Split points by breakdown; returns null if season not found or `ScoringEnabled=false`
- `MeetService` — injects `IMeetPlacingRepository`; `GetMeetDetailsAsync` now runs parallel tasks including placings; builds `placingLookup` dictionary for `BuildOrderedEventGroups`
- `SeasonService.GetSeasonDetailsAsync` — now populates `SeasonId` and `ScoringEnabled` on `SeasonDetailsViewModel`

*ViewModels (key):*
- `Scoring/AthleteScoreRowViewModel`, `Scoring/SeasonScoringViewModel`
- `Meets/MeetDetailsViewModel` — added `MeetType`, `Participants`, `HasScoring`
- `Meets/MeetPerformanceViewModel` — added `AthleteSlug?`, `List<PerformancePlacingViewModel> Placings`, `HasPlacing`; nested `PerformancePlacingViewModel` with `MedalEmoji` computed property
- `Seasons/SeasonDetailsViewModel` — added `SeasonId`, `ScoringEnabled`

*Admin controllers (2 new):*
- `ScoringTemplatesController` — CRUD for templates; Delete blocked for built-in templates
- `MeetEntriesController` — Index, AddEntry, GetAthletesForEvent (AJAX), EnterResult, DeleteEntry; EnterResult POST creates Performance + PerformanceAthletes + links MeetEntry + creates MeetPlacings with FullPoints/SplitPoints

*Public controller:*
- `SeasonsController` — added `Scoring(string name)` action at `/seasons/{name}/scoring`; returns 404 if `ScoringEnabled=false`

*Views (new):*
- `Areas/Admin/Views/ScoringTemplates/` — Index, Create, Edit
- `Areas/Admin/Views/MeetEntries/` — Index, _EntryEventGroup (partial), AddEntry, EnterResult
- `Views/Seasons/Scoring.cshtml` + `Views/Seasons/_ScoringGenderPanel.cshtml`
- `Views/Meets/Details.cshtml` — placing badges (🥇🥈🥉 or ordinal) added to Notes column in all three gender sections
- `Views/Seasons/Details.cshtml` — "🏆 Season Scoring" link shown when `ScoringEnabled`

*Schema (`docs/schema.sql`):*
- `ALTER TABLE Seasons ADD ScoringEnabled BIT`
- `ALTER TABLE Meets ADD MeetType SMALLINT, ScoringTemplateId INT`
- New tables: `ScoringTemplates`, `ScoringTemplatePlaces`, `MeetParticipants`, `MeetEventScoringOverrides`, `MeetEntries`, `MeetEntryAthletes`, `MeetPlacings`
- Seed: built-in "Dual Meet (5-3-1)" template with places 1→5, 2→3, 3→1
- `RunningRelayEvents` table documented (pre-existing; added to schema file)

**Watch out:**
- `MeetPlacings.MeetParticipantId` is NULL for invitational placings. Two filtered UNIQUE indexes handle uniqueness: one WHERE NOT NULL (per-opponent), one WHERE NULL (overall). SQL Server treats NULL != NULL in unique indexes, so both cases are covered without a composite unique constraint.
- Double Dual meets have 3 teams; one `MeetPlacing` row is created per opponent per performance. `EnterResultViewModel.PlaceInputs` has one `PlaceInputRow` per `MeetParticipant`.
- Relay scoring: `FullPoints` = each relay member gets the template's full points for that place. `SplitPoints` = `FullPoints / AthleteCount`. Both are stored; a UI toggle selects which to display.
- `GetTemplatePointsAsync` in `AdminMeetPlacingRepository` resolves points via: event override template → meet default template → 0 (out of range). All three cases return 0 silently if the place exceeds the template.
- `MeetEntry.AthleteId` is NULL for relays; relay athletes are in `MeetEntryAthletes`. `GetAthleteEventCountForMeetAsync` uses UNION ALL to count both individual entries and relay memberships (to enforce the 4-event limit display flag).
- `ScoringEnabled` on Season is the gate for both the public scoring route (404 if false) and the "Season Scoring" button on the season details page.
- `MeetService` tests required updating: `MeetService` constructor now requires `IMeetPlacingRepository`; the test mock must be set up with `.ReturnsAsync(new List<MeetPlacing>())` to prevent `GroupBy` NullReferenceException.
- `SeasonDetailsViewModel` now has `SeasonId` and `ScoringEnabled` — populated by `SeasonService.GetSeasonDetailsAsync`.

**Key files:**
- `CloverleafTrack.Models/Enums/MeetType.cs`
- `CloverleafTrack.Models/Meet.cs`, `Season.cs`, `MeetPlacing.cs`, `MeetEntry.cs`, `MeetEntryAthlete.cs`, `MeetParticipant.cs`, `ScoringTemplate.cs`, `ScoringTemplatePlace.cs`, `MeetEventScoringOverride.cs`
- `CloverleafTrack.DataAccess/Dtos/MeetEntryDto.cs`, `ScoringDataDto.cs`
- `CloverleafTrack.DataAccess/Interfaces/I*Repository.cs` (7 new interfaces + IMeetRepository updated)
- `CloverleafTrack.DataAccess/Repositories/Admin/*Repository.cs` (3 updated: AdminMeet, AdminSeason, AdminAthlete)
- `CloverleafTrack.Services/ScoringService.cs`, `MeetService.cs`, `SeasonService.cs`
- `CloverleafTrack.Services/Interfaces/IScoringService.cs`
- `CloverleafTrack.ViewModels/Scoring/`, `Meets/MeetPerformanceViewModel.cs`, `Meets/MeetDetailsViewModel.cs`, `Seasons/SeasonDetailsViewModel.cs`, `Admin/Meets/MeetFormViewModel.cs`
- `CloverleafTrack.Web/Areas/Admin/Controllers/ScoringTemplatesController.cs`, `MeetEntriesController.cs`, `MeetsController.cs`
- `CloverleafTrack.Web/Controllers/SeasonsController.cs`
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`, `Views/Seasons/Details.cshtml`, `Scoring.cshtml`, `_ScoringGenderPanel.cshtml`
- `CloverleafTrack.Web/Program.cs` (DI registrations)
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs`
- `docs/schema.sql`

---

### [C22] Class Rank Filtering on Leaderboard Details Page

**What changed:**
Added per-class filtering to the Leaderboard event details page (`/leaderboard/{eventKey}`). Users can now click Freshman / Sophomore / Junior / Senior filter buttons above the performance tables to narrow the list to performances set while the athlete was in that class.

**How class-at-time-of-performance is determined:**
- Uses `GraduationYear` (from `Athletes`) and `MeetDate` (from `Meets`)
- School year boundary: if `MeetDate.Month >= 8` (August onward), the school year ends in `MeetDate.Year + 1`; otherwise it ends in `MeetDate.Year`
- `GraduationYear - schoolYearEnd`: 0 = Senior, 1 = Junior, 2 = Sophomore, 3 = Freshman; anything else = null (alumni or future)
- Relay performances have `GraduationYear = null` → `ClassAtTimeOfPerformance = null`; class filter buttons are hidden for relay events

**ViewModel change:**
```csharp
// LeaderboardPerformanceViewModel — new field
public string? ClassAtTimeOfPerformance { get; set; }
```

**Service change:**
```csharp
// LeaderboardService — new private helper
private static string? GetClassAtTimeOfPerformance(int? graduationYear, DateTime meetDate)
{
    if (!graduationYear.HasValue) return null;
    var schoolYearEnd = meetDate.Month >= 8 ? meetDate.Year + 1 : meetDate.Year;
    return (graduationYear.Value - schoolYearEnd) switch
    {
        0 => "Senior", 1 => "Junior", 2 => "Sophomore", 3 => "Freshman", _ => null
    };
}
```
Applied to both `allPerfsList` and `prsOnly` builds in `GetLeaderboardDetailsAsync`. The `BuildPrViewModels` private helper is called once for overall PRs and once per class.

**ViewModel changes:**
```csharp
// LeaderboardDetailsViewModel — new field
public Dictionary<string, List<LeaderboardPerformanceViewModel>> ClassPersonalRecords { get; set; } = new();
// Keys: "Freshman", "Sophomore", "Junior", "Senior"
// PersonalRecordsOnly (existing) = overall best per athlete (shown when class filter = "all")
```

**View changes (Details.cshtml):**
- Class column now shows `perf.ClassAtTimeOfPerformance` (class WHEN set) instead of the old `GetClassYear()` (current class based on today)
- Each `<tr>` in the All Performances table gets `data-class="..."` and `data-rank="..."` attributes
- The first `<td>` (Rank column) gets class `rank-cell` so JS can update the rank number when filtering
- Class filter pill buttons (All / Freshman / Sophomore / Junior / Senior) appear between the view tabs and the table; hidden for relay events (`@if (!Model.IsRelayEvent)`)
- **PRs Only view**: renders 5 separate `<div class="prs-section hidden">` blocks — one per class (`prs-section-all`, `prs-section-Freshman`, etc.). Each block is pre-ranked server-side. The JS swaps which block is visible instead of filtering rows.
- **All Performances view**: JS filters rows by `data-class` and re-numbers visible ranks
- Active class filter persists when switching between All / PRs Only view tabs

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardPerformanceViewModel.cs`
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardDetailsViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml`

**Watch out:**
- The "Class" column on the details page now shows the class AT THE TIME OF THE PERFORMANCE, not the athlete's current class. The old `GetClassYear()` Razor function is still in the file (used nowhere after this change) but can be removed later.
- Class filter state is stored in `currentClassFilter` JS variable and re-applied when switching between All/PRs tabs via `showView()`.
- Relay events: `ClassAtTimeOfPerformance` is always null for relay rows (no single graduation year). The filter buttons are omitted with `@if (!Model.IsRelayEvent)`.
- Do NOT use a Razor local `void` function containing tag helpers (`asp-controller`, `asp-action`, `asp-route-*`) — Razor compiles tag helpers with `await`, making the generated code require an async context. The CS4033 error is the symptom. Use an outer `@foreach` loop instead.

---

### [C26] Phase 1 Performance-Data UX Pass

**What changed:**
The public UX was tightened around performance data without adding new routes or schema:

- Main navigation copy now shows `Events` and `Athletes` while keeping `/leaderboard` and `/roster`.
- Leaderboard details use an explicit gender label mapping: Male → Boys, Female → Girls, Mixed → Mixed, default → Unknown. `SearchService` uses the same mapping for athlete search labels; event search labels already had Mixed sections.
- Homepage adds a nullable `LatestMeetImpact` card before Season at a Glance, backed by `HomeRepository.GetLatestCompletedMeetImpactAsync(currentSeasonId)`.
- Meet details now lead with a Meet Impact Summary: total performances, unique athletes, PRs, school records, top-10 all-time marks, season bests, scoring places, mixed relay and hand-timed indicators, plus short filtered lists generated from mapped meet performance rows.
- Athlete details split `PersonalRecords` into individual records and relay achievements in separate sections.

**Why:**
The public site should lead with performance-product data: recent meet impact, PRs, school records, top-10 marks, scoring places, relays, and athlete record context. Mixed relay leaderboard pages must never fall through to a Girls label.

**Key files:**
- `CloverleafTrack.DataAccess/Dtos/LatestMeetImpactDto.cs`
- `CloverleafTrack.DataAccess/Interfaces/IHomeRepository.cs`
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`
- `CloverleafTrack.Services/HomeService.cs`
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Services/SearchService.cs`
- `CloverleafTrack.ViewModels/Home/HomePageViewModel.cs`
- `CloverleafTrack.ViewModels/Home/LatestMeetImpactViewModel.cs`
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardDetailsViewModel.cs`
- `CloverleafTrack.ViewModels/Meets/MeetDetailsViewModel.cs`
- `CloverleafTrack.Web/Views/Shared/_MainNavigation.cshtml`
- `CloverleafTrack.Web/Views/Shared/_HomePageLatestMeetImpactCard.cshtml`
- `CloverleafTrack.Web/Views/Home/Index.cshtml`
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml`
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`
- `CloverleafTrack.Tests/Unit/Services/HomeServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs`
- `CloverleafTrack.Tests/Unit/ViewModels/Leaderboard/LeaderboardDetailsViewModelTests.cs`

**Watch out:**
- Latest Meet Impact uses completed meets only (`EntryStatus = 3`) and orders by meet date descending, then id descending. It counts current school records from `Leaderboards.Rank = 1`, top-10 marks from `Leaderboards.Rank <= 10`, and unique athletes from individual `Performances.AthleteId` UNION relay members in `PerformanceAthletes`.
- Latest Meet Impact intentionally does not infer relay PRs; `TotalPRs` uses `Performances.PersonalBest = 1` only.
- Meet impact summary counts are computed in `MeetService` from already mapped performance rows. Do not add a new meet details SQL query for top-10, season-best, or placing counts unless the mapped rows stop carrying that data.
- Athlete relay achievements still rely on existing service-layer best-per-event relay logic. Do not merge them back into the individual PR table.

---

### [C17] Leaderboard Full-Width Layout When Filtered

**What changed:**
The Leaderboard page now expands to full width when a gender filter reduces the visible columns to one.

- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`:
  - The Boys/Girls grid wrapper changed from `grid-cols-1 lg:grid-cols-2` to a single CSS class managed via the `data-filterable-gender-grid` attribute.
  - Added client-side JavaScript `adjustLeaderboardLayout()` that inspects visible `[data-filterable]` columns inside each grid after `applyFilters()` runs.
  - When only one gender column is visible, it switches the grid to `lg:grid-cols-1`; otherwise it restores `lg:grid-cols-2`.
  - The Mixed Relays section wrapper now uses `data-filterable-mixed-section` and is expanded to `max-w-none` when `gender=mixed` is selected; it otherwise keeps the narrower `max-w-lg`.

**Why:**
Filtered URLs like `#env=outdoor&gender=boys` were rendering a single column of data inside a two-column grid, leaving most of the page empty and making the leaderboard hard to read.

**Key files:**
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`

**Watch out:**
- The layout adjustment hooks into `window.applyFilters` because the filter script is loaded from `filters.js` (not inline). This override must be declared **after** `filters.js` is included. It is safe here because the filter script is in the layout and the leaderboard script is in the page's `Scripts` section.
- `applyFilters` sets `hidden` directly; layout counts should check the `.hidden` property.
- The grid's base class is now `grid grid-cols-1 gap-8 mb-8` so it defaults to a single column on smaller breakpoints; the JavaScript only controls the `lg:grid-cols-*` breakpoint behavior.

---

### [C27] Footer Build Version / Commit SHA

**What changed:**
The public site footer now displays the short git commit SHA that the site was built from (e.g., `v95ef166`).

- `CloverleafTrack.Web/CloverleafTrack.Web.csproj` was updated to default `SourceRevisionId` and `InformationalVersion` properties when they are not supplied by the build environment, making deterministic output without requiring new tooling.
- A new `BuildMetadataHelper` static class reads `AssemblyInformationalVersionAttribute` and extracts the commit SHA from the `+commitSha` suffix that MSBuild injects when `SourceRevisionId` is set.
- `Views/Shared/_Layout.cshtml` footer now appends the short SHA to the "Made with ♥ by Coach Tony" line.

**Build-time behavior:**
```bash
dotnet build -p:SourceRevisionId=$(git rev-parse --short HEAD)
```
produces an assembly informational version like `1.0.0-unknown+95ef166`, and the footer renders `v95ef166`.

**Why:**
The issue requested that the footer make it clear which git tag or commit generated the website. Embedding the commit SHA in the footer lets visitors and admins confirm exactly what code is running.

**Key files:**
- `CloverleafTrack.Web/CloverleafTrack.Web.csproj`
- `CloverleafTrack.Web/Utilities/BuildMetadataHelper.cs` (NEW)
- `CloverleafTrack.Web/Views/Shared/_Layout.cshtml`

**Watch out:**
- The SHA is sourced from the assembly's `AssemblyInformationalVersionAttribute`, which is set from `InformationalVersion` at compile time. If you build without `SourceRevisionId`, the default value `unknown` is used and the footer shows `vunknown`.
- In Docker builds, pass `--build-arg` or use the git context so that `SourceRevisionId` is populated; otherwise the published image will display `vunknown`.
- The admin area uses its own `_Layout.cshtml` and does not yet display the version. Add it there too if admins need it in the admin UI.

---

### [C32] Field-Event Attempt Series (Issue #12)

**What changed:**
Added optional, additive per-attempt data (up to 6 attempts: valid mark, foul, or pass) for field-event performances, alongside the existing single-best-mark `Performances.DistanceInches` column, which keeps its exact current meaning and is untouched by this feature when no series is recorded.

**New table — `PerformanceAttempts`** (see `docs/schema.sql`, "SCHEMA ADDITIONS — Field-Event Attempt Series"):
```sql
CREATE TABLE [dbo].[PerformanceAttempts] (
    [Id]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PerformanceId]  INT NOT NULL,
    [AttemptNumber]  TINYINT NOT NULL,      -- 1..6
    [DistanceInches] FLOAT(53) NULL,        -- NULL when foul or pass
    [IsFoul]         BIT NOT NULL DEFAULT 0,
    [IsPass]         BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_PerformanceAttempts_Performances]
        FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_PerformanceAttempts_Performance_Attempt]
        UNIQUE ([PerformanceId], [AttemptNumber]),
    CONSTRAINT [CK_PerformanceAttempts_Valid]
        CHECK (([IsFoul] = 1 AND [DistanceInches] IS NULL)
            OR ([IsPass] = 1 AND [DistanceInches] IS NULL)
            OR ([IsFoul] = 0 AND [IsPass] = 0 AND [DistanceInches] IS NOT NULL))
);
```
`ON DELETE CASCADE` means deleting a `Performance` cascades to its `PerformanceAttempts` rows at the FK level. `AdminPerformanceRepository.DeleteAsync` deliberately does **not** also manually delete `PerformanceAttempts` — that would be redundant, not harmful, but there's nothing for it to clean up. Do not add a manual delete there.

**Recompute-on-save behavior:**
`IAdminPerformanceRepository.SaveAttemptSeriesAsync(performanceId, attempts)` replaces the full series (delete-then-insert, not a diff/upsert), recomputes `Performances.DistanceInches` as the best (max) valid attempt, and calls `EXEC sp_RebuildLeaderboards` — the same call every other performance write in this repository already makes. This mirrors `CreateAsync`/`UpdateAsync`/`DeleteAsync`.

**All-foul/all-pass edge case — the decision:**
When no attempt in the series is valid (every attempt is a foul or a pass), `PerformanceAttemptSeries.BestValidDistance(attempts)` returns `null`, and `SaveAttemptSeriesAsync` **leaves `Performances.DistanceInches` untouched** — it does not null it out and does not error. Reasoning:
- `CK_Performances_DistanceOrTime` requires a non-null `DistanceInches` for field events; nulling it out on save would violate that constraint, and the constraint was explicitly kept unchanged by this feature.
- There is no new value to promote to "the best mark" when the whole series is fouls/passes, so leaving the existing value (typically the mark manually entered on the single-mark path before/alongside the series) is the only safe, non-destructive choice.
- The admin entry form's plain "Distance" field stays required and visible even when "Record full series" is checked, specifically so there is always a valid seed value for the initial `Performances` insert — the attempt series is genuinely supplementary detail on top of it, not a replacement input path.
- Tested directly: `PerformanceAttemptTests.BestValidDistance_ReturnsNull_ForAllFoulSeries` and `PerformanceAttemptSeriesBuilderTests.BuildLookup_NoAttemptMarkedBest_WhenSeriesIsAllFoul`.

**Best-mark computation is order-independent:** `PerformanceAttemptSeries.BestValidDistance` takes the max over valid attempts regardless of `AttemptNumber` order — the best mark is very often not the last attempt taken. Covered by `BestValidDistance_ReturnsMax_WhenBestAttemptIsNotTheLastOne` and `BuildLookup_MarksBestAttempt_EvenWhenNotLastInOrder`.

**Gating — which events offer this:**
Only `EventType.Field` (0), `ThrowsRelay` (5), and `JumpRelay` (4). **`FieldRelay` (2) is intentionally excluded** — the issue explicitly lists only those three EventTypes; do not "helpfully" add FieldRelay back in. `PerformancesController.IsFieldEventType(EventType?)` is the single source of truth for this on the admin side. The admin form's JS/Razor field-vs-running detection (`indexOf('Field') >= 0 || indexOf('Jump') >= 0 || indexOf('Throw') >= 0`, from `CLAUDE.md`) is reused unmodified to show/hide the "Record full series" toggle — it's a superset (also matches `FieldRelay`) but the toggle only actually persists a series when the controller's stricter `IsFieldEventType` check also passes, so submitting a series for a `FieldRelay` performance is silently a no-op.

**Display — silent-by-default, three different placements:**
- `PerformanceAttemptSeriesViewModel.HasAttempts` gates the *entire* strip — no PerformanceAttempts rows means the partial renders nothing at all, not even an empty wrapper div. This is what keeps 45 years of history with no series data rendering identically to today.
- `_AttemptStrip.cshtml` (`Views/Shared/`) — the compact strip alone: up to 6 boxes (foul = dashed box "F", pass = em dash, valid = formatted distance, best attempt accented amber), plus valid count / average / spread.
- `_AttemptSeriesExpanded.cshtml` (`Views/Shared/`) — strip + a native `<details>/<summary>` holding an inline-SVG bar chart. No JS charting library. Never expanded by default.
- **Meet recap page (`Views/Meets/Details.cshtml`) uses `_AttemptStrip.cshtml` directly — never `_AttemptSeriesExpanded.cshtml`.** No `<details>` markup exists in the DOM there at all, not even collapsed, because a meet can have many throwers and a disclosure-per-row is too much visual noise at that density. **A future change must not "fix" this by swapping in the expanded partial** — this was a deliberate, explicit placement rule from the issue, not an oversight.
- Athlete page (`Views/Roster/Details.cshtml`, "Performance by Season" per-meet rows) and Event page (`Views/Leaderboard/Details.cshtml`, both the "All Performances" and "PRs Only" tables) use `_AttemptSeriesExpanded.cshtml` — strip + collapsed disclosure.
- Roster's Personal Records summary table (career-best-per-event) was **not** wired up to attempt series — only the per-meet "Performance by Season" rows were. Each PR row *is* backed by a specific `PerformanceId` so this could be added the same way if wanted later; it was left out to keep this change scoped to the three placements the issue named explicitly.

**Wiring pattern (repeated 3x — Meet/Athlete/Leaderboard):** each public service (`MeetService`, `AthleteService`, `LeaderboardService`) takes an **optional** `IPerformanceAttemptRepository? attemptRepository = null` constructor parameter (defaults to null rather than required) specifically so the ~25 existing `new XyzService(mockRepo.Object)` call sites across the test suite did not need to be touched. When null (or when no attempts exist for the loaded performances), every display ViewModel gets a fresh empty `PerformanceAttemptSeriesViewModel` (`HasAttempts == false`) — the same silent-by-default behavior as if the repository had returned zero rows. `PerformanceAttemptSeriesBuilder.BuildLookup(attempts)` (in `CloverleafTrack.Services`) is the single shared mapper from a flat `List<PerformanceAttempt>` to a `PerformanceId → PerformanceAttemptSeriesViewModel` dictionary, used by all three services — do not duplicate this grouping/best-marking logic per service.

**Distance parsing/formatting:** `PerformanceFormatHelper.ParseDistance`/`FormatDistance` (existing `Web/Utilities` helper) is reused for every attempt distance — the admin controller's `ParseAttemptInputs` calls `ParseDistance`, and `_AttemptStrip.cshtml`/`_AttemptSeriesExpanded.cshtml` call `FormatDistance` directly in the Razor view (Views compile inside the Web project, so they can reference `Web.Utilities` directly — the `CloverleafTrack.ViewModels` project cannot, since it doesn't reference Web, which is why `PerformanceAttemptViewModel.DistanceInches` stays a raw `double?` and formatting happens only in the view layer).

**Key files:**
- `docs/schema.sql` — new `PerformanceAttempts` table + index, in a new "SCHEMA ADDITIONS — Field-Event Attempt Series" section
- `CloverleafTrack.Models/PerformanceAttempt.cs` (NEW) — entity + `PerformanceAttemptSeries.BestValidDistance` (pure, DB-free, unit-testable)
- `CloverleafTrack.DataAccess/Interfaces/IAdminPerformanceRepository.cs`, `Repositories/AdminPerformanceRepository.cs` — `GetAttemptsForPerformanceAsync`, `SaveAttemptSeriesAsync`
- `CloverleafTrack.DataAccess/Interfaces/IPerformanceAttemptRepository.cs`, `Repositories/PerformanceAttemptRepository.cs` (NEW) — public batch read by PerformanceIds
- `CloverleafTrack.ViewModels/Shared/PerformanceAttemptSeriesViewModel.cs` (NEW)
- `CloverleafTrack.ViewModels/Admin/Performances/PerformanceAttemptInputViewModel.cs` (NEW), `PerformanceEntryViewModel.cs` (+ `RecordFullSeries`, `Attempts`)
- `CloverleafTrack.ViewModels/Meets/MeetPerformanceViewModel.cs`, `Athletes/IndividualPerformanceViewModel.cs`, `Leaderboard/LeaderboardPerformanceViewModel.cs` (+ `AttemptSeries`)
- `CloverleafTrack.Services/PerformanceAttemptSeriesBuilder.cs` (NEW), `MeetService.cs`, `AthleteService.cs`, `LeaderboardService.cs`
- `CloverleafTrack.Web/Areas/Admin/Controllers/PerformancesController.cs` — `IsFieldEventType`, `ParseAttemptInputs`, wired into `Create`/`Edit` GET+POST
- `CloverleafTrack.Web/Areas/Admin/Views/Performances/Create.cshtml`, `Edit.cshtml` — "Record full series" toggle + 6 attempt rows, off by default
- `CloverleafTrack.Web/Views/Shared/_AttemptStrip.cshtml`, `_AttemptSeriesExpanded.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`, `Views/Roster/Details.cshtml`, `Views/Leaderboard/Details.cshtml` — partial calls
- `CloverleafTrack.Web/Program.cs` — DI registration for `IPerformanceAttemptRepository`
- Tests: `CloverleafTrack.Tests/Unit/Models/PerformanceAttemptTests.cs`, `Unit/ViewModels/Shared/PerformanceAttemptSeriesViewModelTests.cs`, `Unit/Services/PerformanceAttemptSeriesBuilderTests.cs`, plus wiring tests added to `AthleteServiceTests.cs`, `MeetServiceTests.cs`, `LeaderboardServiceTests.cs`

**Watch out:**
- Not built/tested against a real SQL Server — NuGet was network-blocked and there was no SQL Server available in this environment. The T-SQL (CHECK constraint syntax, `TINYINT`, cascade delete) needs manual verification against real SQL Server before merge.
- `SaveAttemptSeriesAsync` is delete-then-insert for the whole series, not an upsert/diff. Fine for the admin form's "replace the whole series" UX, but don't call it with a partial series expecting the rest to survive.
- `IsFieldEventType` in the controller intentionally does **not** match the broader JS/Razor `isFieldEvent` convention used for the single-mark Distance/Time toggle — the toggle convention includes `FieldRelay`, `IsFieldEventType` does not. Keep that gap; it's deliberate.

---

### [C31] Percentile Foundation (+ Median/IQR) — GitHub Issue #4

**What changed:**
Added data-foundation support for per-performance percentiles and per-event median/Q1/Q3/mark-count. No UI in this change — presentation is separate follow-up issues. This is a P1 dependency for four other backlog items (percentile display, mark color scale, roster redesign, career chart context), so the math below was hand-traced rather than assumed correct.

**New tables (`docs/schema.sql`, appended in a new "SCHEMA ADDITIONS: Percentile Foundation" section):**

```sql
CREATE TABLE PerformancePercentiles (
    PerformanceId INT     NOT NULL PRIMARY KEY,  -- FK -> Performances
    Percentile    TINYINT NOT NULL               -- 1-100, higher is better
);

CREATE TABLE EventStatistics (
    EventId        INT        NOT NULL PRIMARY KEY,  -- FK -> Events
    MedianValue    FLOAT (53) NULL,
    Q1Value        FLOAT (53) NULL,
    Q3Value        FLOAT (53) NULL,
    EventMarkCount INT        NOT NULL
);
```

Both are truncate-and-repopulate every time `sp_RebuildLeaderboards` runs (Steps 11-12, added directly into the existing `CREATE PROCEDURE` body — same in-place-edit convention as [C19]'s SchoolRecord steps, *not* an `ALTER PROCEDURE` in the schema-additions section; only the two new `CREATE TABLE` statements were appended there). Same transaction as the Leaderboards/PersonalBest/SeasonBest/SchoolRecord rebuild — they can never drift out of sync with each other because a partial rebuild rolls back entirely (existing `BEGIN TRY`/`CATCH`/`ROLLBACK` wrapper).

**Why a new table instead of a column on `Leaderboards`:**
The issue's own text raised this as the open design question. `Leaderboards` is documented everywhere (CLAUDE.md, this file, multiple repository comments) as an **all-time top-10** table — several existing queries assume that cardinality (`Rank <= 10` filters, `LEFT JOIN ... AND lb.Rank = 1` for SR checks, etc.). Widening it to one row per performance (up to hundreds of rows per event) would silently change what "a row in Leaderboards" means for every existing consumer. A dedicated `PerformancePercentiles` table keeps the top-10 contract intact and mirrors the existing pattern (`Leaderboards`/`EventStatistics`/`PersonalBestHistory` are all derived, rebuild-owned tables that sit outside the raw `Performances` table). `EventStatistics` follows the same reasoning versus denormalizing onto `Events` — `Events` is admin-maintained configuration data; median/Q1/Q3/count are derived and change on every write.

**Percentile algorithm (the part that had to be exactly right):**

Definition used: `percentile(P) = 100 * (count of marks in EventId strictly worse than P) / (total marks in EventId)`, rounded to nearest integer (round-half-away-from-zero, T-SQL `ROUND` default), clamped to `[1, 100]`, with one explicit special case: **a population of exactly 1 mark returns 100** (the literal formula gives 0/1 = 0% for a lone mark, which would render as a broken/undefined value; a mark with nothing to compare against is defined as the event's best-and-only record instead of its worst).

Direction is derived from `Event.EventType`, never from which column is non-null:
- Running (`EventType` 1, 3): lower `TimeSeconds` is better → "worse" = higher `TimeSeconds`.
- Field (`EventType` 0, 2, 4, 5): higher `DistanceInches` is better → "worse" = lower `DistanceInches`.

**Tie handling — the reason `RANK()` was NOT used:** the SQL uses `COUNT(*) OVER (PARTITION BY EventId ORDER BY <value> RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)` to count "marks not better than P" (ties included). `RANGE` framing (as opposed to `ROWS`) makes every row that ties on the `ORDER BY` key resolve to the *same* cumulative count within its partition, so tied performances always get identical percentiles by construction — no rank-to-percentile conversion step exists to get wrong. `RANK()` would have produced an equivalent number of "not-worse" rows here too, but the `COUNT()`/`RANGE` approach keeps the "count of marks not better than P" semantics explicit in the SQL rather than hiding it behind a rank value that then has to be converted.

**Population scoping:** No `AthleteId IS NOT NULL` filter anywhere in the percentile/statistics steps (unlike the `SchoolRecord`/`PersonalBest`/`SeasonBest` steps, which are individual-only). Relay and individual performances are both included in the same `PARTITION BY EventId` window — this is automatic and correct because `UQ_Events_EventKey_Gender_Environment` already guarantees one `EventId` == one comparable population (a relay event and its corresponding individual event are always different rows in `Events` with different `EventId`s, so they can never be pooled together even without an explicit filter).

**Median/Q1/Q3:** Computed via `PERCENTILE_CONT(0.5 / 0.25 / 0.75) WITHIN GROUP (ORDER BY value) OVER (PARTITION BY EventId)` — SQL Server's linear-interpolation percentile function (the "continuous" method), which for an even-count population returns the average of the two middle order statistics (the standard definition) rather than picking one arbitrarily. `NULL`-when-sparse rule: `MedianValue`/`Q1Value`/`Q3Value` are set to `NULL` when `EventMarkCount < 10`; `EventMarkCount` itself is **always** populated (never null), specifically so the UI can render "#9 of 604" without a second query even for events too small to show a reference band.

**Worked example (hand-traced, since no SQL Server is available to run this against):** 8 synthetic 100m dash times (running event, lower is better): 10.90, 11.05, 11.05, 11.20, 11.35, 11.50, 11.50, 11.80 seconds.

| Time | Marks strictly worse | Percentile (100×worse/8, rounded) |
|---|---|---|
| 10.90 | 7 | 88 |
| 11.05 (×2, tied) | 5 | 63 (both) |
| 11.20 | 4 | 50 |
| 11.35 | 3 | 38 |
| 11.50 (×2, tied) | 1 | 13 (both) |
| 11.80 | 0 | 1 (clamped up from 0) |

Median (even count, N=8): average of 4th and 5th sorted values = (11.20 + 11.35) / 2 = **11.275**. Q1 (interpolated at position 2.75 of 8): both surrounding values are 11.05 → **11.05**. Q3 (interpolated at position 6.25 of 8): both surrounding values are 11.50 → **11.50**. Note this specific 8-mark example is below the 10-mark floor, so in production `EventStatistics` would store `NULL` for all three and `EventMarkCount = 8` — the median/Q1/Q3 above are shown purely to demonstrate the interpolation math, not what would actually be stored for this event.

**C# / DTO / repository plumbing (making the data reachable, not just present in the DB):**

- `CloverleafTrack.Models/Performance.cs` — added `public byte? Percentile { get; set; }`, same "populated by queries that join X, null when not loaded" convention as `AllTimeRank` (see [C22]/[C23] reliability notes — same caveat applies: don't assume null means "no data exists", it may mean "this query didn't join PerformancePercentiles").
- `AthletePerformanceDto` (+`Percentile`), `LeaderboardPerformanceDto` (+`Percentile`, +`EventMarkCount`, +`MedianValue`, +`Q1Value`, +`Q3Value`), `LeaderboardDto` (+`Percentile`).
- `AthleteRepository.GetAllWithPerformancesAsync` (roster rows) and `GetAllPerformancesForAthleteAsync` (athlete PRs / season performances) — added `(SELECT pp.Percentile FROM PerformancePercentiles pp WHERE pp.PerformanceId = p.Id) AS Percentile` subqueries, same pattern as the existing `AllTimeRank` subquery.
- `LeaderboardRepository.GetAllPerformancesForEventAsync` (event detail rows) — `LEFT JOIN PerformancePercentiles` and `LEFT JOIN EventStatistics`, so every row on the event details page carries its own percentile plus the event-wide median/Q1/Q3/count without a second query.
- `LeaderboardRepository.GetTopPerformancePerEventAsync` (leaderboard index top-1 rows) — `LEFT JOIN PerformancePercentiles` for the `Percentile` field.
- Admin repositories (`AdminPerformanceRepository`, etc.) were intentionally **not** touched — out of scope per the acceptance criteria ("athlete PRs, event rows, and roster rows"), and admin views don't consume percentile yet.

**Watch out:**
- `Performance.Percentile` is a C# property only, **not** a DB column on `Performances` — exactly like `AllTimeRank`. Any new query that needs it must add the `PerformancePercentiles` subquery/join; it will silently stay `null` otherwise. Do not assume `null` means "this event has no percentile data."
- `PerformancePercentiles` and `EventStatistics` are fully truncated and rebuilt on every `sp_RebuildLeaderboards` call (same as `Leaderboards`). Do not attempt incremental/partial updates — the whole point of "recomputed automatically on every write, never drifts" is that the full rebuild is cheap enough to run every time and is already the established pattern for this SP.
- The single-mark-returns-100 special case is a **deliberate deviation** from the literal formula, not a bug — see the algorithm section above. If the formula ever needs to change, make sure this edge case is preserved or explicitly and deliberately changed (with a note here).
- `CloverleafTrack.Tests/TestSupport/PercentileMath.cs` is a **test-only** mirror of the SQL algorithm — it is never referenced from `CloverleafTrack.Services` or `CloverleafTrack.DataAccess`. It exists only because this environment cannot run T-SQL against a real SQL Server to verify the stored procedure directly. If the SQL algorithm changes, this mirror (and `CloverleafTrack.Tests/Unit/DataAccess/PercentileMathTests.cs`) must be updated to match, or it will silently document stale behavior.
- **Needs verification against a real SQL Server** (could not be run in this environment — no SQL Server available, `dotnet build`/`test` also blocked by network-restricted NuGet): the `RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW` window frame syntax and `PERCENTILE_CONT(...) OVER (PARTITION BY ...)` syntax compile and behave as described; the `TRUNCATE TABLE PerformancePercentiles` / `EventStatistics` statements succeed inside the existing transaction without lock/permission issues; actual query performance of the new window-function passes over `Performances` at real data volumes (hundreds to low thousands of rows per event, so should be trivial, but unverified); the new Dapper subqueries/joins in `AthleteRepository`/`LeaderboardRepository` compile and map correctly (column name matching for `Percentile`/`MedianValue`/`Q1Value`/`Q3Value`/`EventMarkCount` was checked by hand against the DTO property names, not by an actual Dapper run).

**Key files:**
- `docs/schema.sql` — Steps 11-12 added to `sp_RebuildLeaderboards`; new "SCHEMA ADDITIONS: Percentile Foundation" section with `PerformancePercentiles` and `EventStatistics` tables.
- `CloverleafTrack.Models/Performance.cs`
- `CloverleafTrack.DataAccess/Dtos/AthletePerformanceDto.cs`, `LeaderboardPerformanceDto.cs`, `LeaderboardDto.cs`
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`, `LeaderboardRepository.cs`
- `CloverleafTrack.Tests/TestSupport/PercentileMath.cs` (NEW, test-only)
- `CloverleafTrack.Tests/Unit/DataAccess/PercentileMathTests.cs` (NEW)
- `CLAUDE.md` — `Performances`/`Leaderboards` table docs updated with the two new tables and `sp_RebuildLeaderboards` description

---

### [C30] SEO / Open Graph / JSON-LD (GitHub issue #17)

**What changed:**
Every public page now gets a data-derived `<meta name="description">`, full Open Graph + Twitter Card tags, and schema.org JSON-LD (`Organization` on the homepage, `Person` on athlete pages, `SportsEvent` on meet pages, `BreadcrumbList` sitewide). Added `robots.txt` and a `/sitemap.xml` route enumerating every canonical URL.

**New shared building blocks:**
- `CloverleafTrack.ViewModels/Shared/SeoMetadataViewModel.cs` (NEW) — `Description`, `CanonicalPath` (defaults to the current request path when null), `OgType`, `ImagePath` (defaults to `/img/hero-home.jpg` when null), `Breadcrumbs` (`List<SeoBreadcrumbViewModel>`), `JsonLdBlocks` (`List<string>` of pre-serialized JSON-LD objects).
- `CloverleafTrack.Web/Utilities/SeoHelper.cs` (NEW) — static helpers: `Truncate(text, maxLength=160)` (word-boundary safe truncation + ellipsis, used as a safety net on every data-derived description), and JSON-LD builders `BuildOrganizationJsonLd`, `BuildPersonJsonLd`, `BuildSportsEventJsonLd`, `BuildBreadcrumbJsonLd`. All JSON is built as `Dictionary<string, object?>` (not anonymous types) specifically because JSON-LD requires literal `"@context"`/`"@type"` keys, which C# anonymous-type property names cannot represent — `System.Text.Json.JsonSerializer.Serialize` correctly walks nested `Dictionary<string, object?>`/`List<...>` values boxed as `object?` using their runtime type, which is the standard .NET pattern for building JSON-LD without a schema library. Uses the *default* `JavaScriptEncoder` (HTML-safe) deliberately — do not switch to `UnsafeRelaxedJsonEscaping`, the blocks are embedded raw inside `<script>` tags via `@Html.Raw`.
- `CloverleafTrack.Web/Views/Shared/_SeoMetadata.cshtml` (NEW) — reads `ViewData["Seo"] as SeoMetadataViewModel`, falls back to a generic sitewide description/breadcrumb (`Home` only) when a page hasn't set one, and renders `<meta name="description">`, `<link rel="canonical">`, full OG + `twitter:card=summary_large_image` tags, and one `<script type="application/ld+json">` per JSON-LD block (BreadcrumbList is always rendered; page-specific blocks like Person/SportsEvent/Organization are appended). Rendered once from `Views/Shared/_Layout.cshtml`'s `<head>` via `@await Html.PartialAsync("_SeoMetadata")` — **the admin area has its own separate `_Layout.cshtml` (see C27) and is untouched**, admin pages get no SEO tags (also blocked in `robots.txt`).

**Pattern each Details/Index view follows** (mirrors the existing `ViewData["Title"]` convention — the child view executes and populates ViewData before the layout reads it):
```csharp
@using CloverleafTrack.ViewModels.Shared
@{
    ViewData["Seo"] = new SeoMetadataViewModel
    {
        Description = CloverleafTrack.Web.Utilities.SeoHelper.Truncate($"..."),
        Breadcrumbs = new List<SeoBreadcrumbViewModel> { new() { Name = "Home", Path = "/" }, ... },
        JsonLdBlocks = new List<string> { CloverleafTrack.Web.Utilities.SeoHelper.BuildPersonJsonLd(...) } // optional
    };
}
```
`CanonicalPath`/`ImagePath` are deliberately left unset on every Details page — the current request path (via `Context.Request.Path`) is already the canonical URL for `/roster/{slug}`, `/leaderboard/{eventKey}`, `/meets/{slug}`, `/seasons/{name}`, so the partial's default is correct without duplicating the slug/eventKey in the view.

**Per-page descriptions built from data already on the ViewModel — no new service/repository calls:**
- **Roster/Details.cshtml**: `Model.FullName`, `Model.GraduationYear`, `Model.TopSprintEvent ?? Model.TopFieldEvent` (name/performance/`AllTimeRank`), `Model.TotalPRs`, `Model.TotalSchoolRecords`. JSON-LD `Person` awards list = every `individualRecords` row with `IsSchoolRecord == true`.
- **Leaderboard/Details.cshtml**: `Model.GenderLabel`/`Model.EventName`, current record from `Model.SchoolRecordProgression` (`FirstOrDefault(p => p.IsCurrentRecord)`), distinct athlete count from `Model.AllPerformances.Select(p => p.AthleteName).Distinct()` (relay rows count as one "athlete" per unique team-name string — an approximation, not exact head-count), total mark count, earliest year from `Model.AllPerformances.Min(p => p.MeetDate).Year`.
- **Meets/Details.cshtml**: `Model.Name`, `Model.Date`, `Model.LocationName`, `Model.TotalPerformances`, `Model.TotalPRs`, `Model.TotalSchoolRecords`. JSON-LD `SportsEvent` location built from `LocationName`/`LocationCity`/`LocationState`.
- **Home/Index.cshtml**: `Model.ActiveAthletes`, `Model.TotalPRsThisSeason`, `Model.SchoolRecordsBroken`. JSON-LD `Organization`.
- Index pages (Roster/Leaderboard/Meets/Seasons) and Seasons/Details get static or lightly data-derived descriptions + breadcrumbs but no JSON-LD block beyond the sitewide `BreadcrumbList`.

**Sitemap + robots.txt:**
- `CloverleafTrack.Web/Controllers/SitemapController.cs` (NEW) — `GET /sitemap.xml`. Deliberately reuses `ISearchService.GetSearchIndexAsync()` (already enumerates every athlete/meet/event URL for the search overlay — see C24's search feature) plus `ISeasonService.GetSeasonCardsAsync()` for `/seasons/{name}` and four static index paths (`/`, `/roster`, `/leaderboard`, `/meets`, `/seasons`). No new repository methods were added. XML is hand-built with a small manual escape helper (`&`/`<`/`>`/`"`/`'`) rather than `XDocument`, specifically to avoid `XDocument.Save(StringWriter)`'s well-known `encoding="utf-16"` declaration bug when the declared/actual encodings don't match.
- `CloverleafTrack.Web/wwwroot/robots.txt` (NEW) — `Allow: /`, `Disallow: /Admin/`, and `Sitemap: https://cloverleaftrack.com/sitemap.xml` (production domain confirmed live in `claude/export-verification.md`). Served automatically by the existing `MapStaticAssets()` call in `Program.cs` — no routing change needed.

**Watch out:**
- **No per-page OG image generation.** `og:image` always falls back to the static `/img/hero-home.jpg` (3.2 MB — large for a social-preview asset; a follow-up should generate/resize a dedicated OG image, ideally per-page via a headless-browser render pipeline, which was explicitly out of scope for this change per the GitHub issue). `SeoMetadataViewModel.ImagePath` exists so a future per-page image can be wired in without touching the partial.
- **The static export pipeline (see `claude/export-verification.md`) will not discover `/sitemap.xml` on its own** — it's a dynamic route, not a static file, and nothing in the exported HTML links to it (by design; sitemap.xml is meant to be crawled by search engines directly, not via link-following). Whatever crawls the site to produce the static export must be told to *also* request `/sitemap.xml` and `/robots.txt` explicitly, the same lesson already learned about orphaned event pages in that doc.
- `SeoHelper.Truncate` is a safety net, not the primary length control — every per-page description was hand-checked against the ~160-char budget using representative data, but a very long athlete/meet/event name could still trigger the truncation path. The break is word-boundary-aware (falls back to a hard cut only if there's no space in the last ~120 chars).
- `robots.txt`'s `Sitemap:` line hard-codes `https://cloverleaftrack.com` because robots.txt must be a static file — it cannot reflect `Request.Host` per-environment the way the rest of this feature does. If the production domain ever changes, update this file by hand.
- Building JSON-LD with `Dictionary<string, object?>` values relies on System.Text.Json's runtime-type serialization for `object`-typed dictionary values. Do **not** refactor `SeoHelper`'s JSON-LD builders to use C# records/anonymous types for the top-level object — `@type`/`@context` are not legal C# identifiers even escaped, so there is no way to get System.Text.Json to emit those exact keys from a strongly-typed object without a custom `JsonPropertyName`-annotated class, which would need one class per schema.org type. The dictionary approach was chosen to keep this to one small helper file.

**Key files:**
- `CloverleafTrack.ViewModels/Shared/SeoMetadataViewModel.cs` (NEW)
- `CloverleafTrack.Web/Utilities/SeoHelper.cs` (NEW)
- `CloverleafTrack.Web/Views/Shared/_SeoMetadata.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Shared/_Layout.cshtml` (renders the partial in `<head>`)
- `CloverleafTrack.Web/Controllers/SitemapController.cs` (NEW)
- `CloverleafTrack.Web/wwwroot/robots.txt` (NEW)
- `CloverleafTrack.Web/Views/Home/Index.cshtml`
- `CloverleafTrack.Web/Views/Roster/Index.cshtml`, `Views/Roster/Details.cshtml`
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`, `Views/Leaderboard/Details.cshtml`
- `CloverleafTrack.Web/Views/Meets/Index.cshtml`, `Views/Meets/Details.cshtml`
- `CloverleafTrack.Web/Views/Seasons/Index.cshtml`, `Views/Seasons/Details.cshtml`
- `CloverleafTrack.Tests/Unit/Utilities/SeoHelperTests.cs` (NEW) — `Truncate` word-boundary/length behavior, structural validity (via `JsonDocument.Parse`) and required fields for all four JSON-LD builders.

---

### [C29] Sortable Tables Sitewide — Extracted `wwwroot/js/sortable-tables.js`

**What changed:**
The column-sort implementation that previously lived inline in `Leaderboard/Details.cshtml` (`/leaderboard/{eventKey}.html`) was extracted into a shared, sitewide module — `CloverleafTrack.Web/wwwroot/js/sortable-tables.js` — registered in `_Layout.cshtml` alongside `filters.js`/`search.js`, and applied to the Roster table, Leaderboard index, and Meet results tables, which previously had no sorting at all.

**Markup contract (auto-init, progressive enhancement):**
```html
<table data-sortable>
  <thead>
    <tr>
      <th data-sort-col="mark" data-sort-type="num" data-sort-dir="asc">Mark</th>
    </tr>
  </thead>
  <tbody>
    <tr><td data-sort="71.10">1:11.10</td></tr>
  </tbody>
</table>
```
- `data-sort-col` — stable column id, also the URL hash key.
- `data-sort-type` — `"num"` (parseFloat) or `"str"` (localeCompare; also used for ISO `yyyy-MM-dd` dates, which sort correctly as text).
- `data-sort-dir` — `"asc"` | `"desc"`, declares which raw-value direction is "best" for that column (e.g. `asc` for times, `desc` for distances/field events). Defaults to `asc`.
- `data-sort="value"` on the matching `<td>` (index-matched to the header, not by name) supplies the raw sort key. **Never** put a formatted display string there — see the fixes list below.
- The module auto-wraps each `<th data-sort-col>`'s existing text in a real `<button type="button">` plus a fixed-width `aria-hidden` caret span at init time. **Razor markup should only carry the data-* attributes and the plain label text — do not hand-author the button/caret.**

**Why extract instead of leaving it inline:**
The event detail page's sort script (click → toggle asc/desc, reorder rows in place) worked but was inline, page-specific, keyboard-inaccessible (click handler on the bare `<th>`, no real button), had no "best/worst" semantics (first click was literally ascending regardless of whether ascending meant "best" for that column), no URL persistence, and no third "reset to original order" state. Issue #7 asked for sorting sitewide plus this checklist of gaps, so the extraction became a rewrite-in-place: same visual/functional result on the event page (verify: `/leaderboard/{eventKey}.html` — Rank/Athlete/Mark/Date columns), new shared behavior everywhere else.

**Gaps fixed vs. the old inline script (all in `sortable-tables.js`):**
1. **Three-state click cycle** — best-first → worst-first → original DOM order → repeat (old script only toggled asc/desc, no reset state). Original DOM order is captured once at init (`Array.prototype.slice.call(tbody.children)`) and replayed by re-appending every row in that saved order.
2. **`data-sort-dir` = "best" direction** — first click on a column always shows the best performance first, not literally-ascending. `data-sort-dir="asc"` for times/ranks/dates, `="desc"` for distances and field events. On Leaderboard Details this is now driven dynamically off `Model.IsFieldEvent`; on Meet results it's driven off the new `MeetEventGroupViewModel.IsFieldEvent`.
3. **URL hash state** — `#sort=<col>&dir=<asc|desc>`, read once at page load and applied via `table._sortableApplyFromHash(col, dir)`, written via `history.replaceState` on every click. Follows the exact same "read-modify-write, don't clobber other keys" convention as `filters.js`'s `#env=outdoor&gender=boys` hash (own `getHashParams`/`setHashParams` pair, not shared code, but the identical algorithm). Since `history.replaceState` never fires `hashchange`, this module and `filters.js` never trigger each other and can coexist on the same hash string safely.
4. **Stable sort** — relies on `Array.prototype.sort` being spec-guaranteed stable (ES2019+); no custom tie-breaking needed.
5. **Composes with filters** — the module never sets a data row's own `hidden` or `style.display`; it only reorders DOM nodes via `tbody.appendChild(row)`, so whatever `filters.js` (the `hidden` property) or page-specific filters (`Leaderboard/Details.cshtml`'s own `applyClassFilter`, which uses `row.style.display`) set stays intact through a sort. `isRowVisible(row)` checks both conventions (`!row.hidden && row.style.display !== 'none'`) wherever the module needs to know what's currently visible (rank renumbering).
6. **`.rank-cell` renumbering** — after every sort, `.rank-cell` elements (optionally with a nested `<span>`, matching Leaderboard Details' existing markup) are renumbered 1..n in final DOM order, counting only currently-visible rows. **Watch out:** this only updates the number text, not the amber/gray top-3 color classes that `Leaderboard/Details.cshtml`'s own `applyClassFilter` also applies when renumbering for its class filter — a cosmetic gap (the gold/amber color stays tied to the row's original rank after a sort reorders it), intentionally left presentation-agnostic in the shared module rather than hardcoding one page's color scheme. `applyClassFilter` and the sort module renumber independently but consistently (same "sequential position among visible rows" semantics), so using both together (sort then class-filter, or vice versa) composes correctly.
7. **Accessibility** — real `<button>` per sortable header (auto-generated, see markup contract above) for native keyboard support; `aria-sort="ascending"|"descending"|"none"` on the `<th>`, exactly one non-`"none"` per table at a time; a `.sr-only[aria-live="polite"][role="status"]` region auto-inserted after each `<table data-sortable>` announcing `"Sorted by {label}, best first"` / `"...worst first"` / `"Sort cleared, showing original order"`; a fixed-width (`w-4`) `aria-hidden` caret span (`↕`/`↑`/`↓`) so no layout shift when the glyph changes.
8. **Grouped/sectioned tables** — new convention: `<tr data-sort-group-header>` marks a divider/category-label row (e.g. the "Sprints"/"Distance"/etc. rows inside `_LeaderboardGenderSection.cshtml`'s single flat table). These rows are excluded from the sort entirely and hidden (`row.hidden = true`) while any sort is active, then restored together with the full original order on the third click. This is the module's own bookkeeping (not filters' `hidden` usage) but uses the same property, which is safe since group-header rows are never also `[data-filterable]` items.

**Column sort-key sourcing (never derived from visible/formatted text):**
- **Time columns**: raw `TimeSeconds` — e.g. Leaderboard Details' `RawValue` (already existed), and new `MeetPerformanceViewModel.RawValue` (added this change, `= p.DistanceInches ?? p.TimeSeconds`, mirroring the existing `IndividualPerformanceViewModel.RawValue` pattern from [C15]).
- **Distance columns**: raw `DistanceInches`, same `RawValue` field (only one of Time/Distance is ever populated per performance, so the null-coalesce is safe).
- **Class/grade columns**: an ordinal, never alphabetical. Roster's "Class" column uses a new `@functions { static int ClassOrdinal(string cls) }` helper in `_RosterActiveAthletesList.cshtml` / `_FormerAthleteYearGroupSection.cshtml` mapping `"Freshman"→1 ... "Senior"→4` (fallback `5` for the `"{year} Graduate"` format `AthleteService.GraduationYearToClass` can also produce, defensively, though active-roster athletes should never hit it).
- **Date columns**: ISO `yyyy-MM-dd`. Leaderboard Details' Date column was migrated off the old `perf.MeetDate.Ticks` numeric sort to this convention for sitewide consistency (equivalent chronological ordering either way — no behavior change, just standardized).
- **Gender columns**: raw `(int)Gender` enum value, not the "M"/"F" badge text.

**Where sorting was intentionally *not* added, and why:**
- Roster's "Best Mark" column and Leaderboard Index's "Mark" (embedded with the date in the 2nd column of `_LeaderboardGenderSection.cshtml`) are **not** sortable — each row's mark is for a different top event per athlete/event-group, so raw seconds/inches values are not comparable across rows (a 100m time vs. a shot put distance). Leaderboard Index is sortable by **Event** name and **Date** ("Recorded") instead. Roster is sortable by Name/Class/Gender/Top-Event-name instead.
- Meet results tables (`Meets/Details.cshtml`) *are* sortable by Mark, because each `<table data-sortable>` is scoped to a single event group (one event per `<details>`), so every row's mark is the same unit (all times or all distances) — this is what unlocked adding `MeetEventGroupViewModel.IsFieldEvent` and `MeetPerformanceViewModel.RawValue`.

**New/changed C# (needed only for Meet results Mark sorting):**
- `CloverleafTrack.ViewModels/Meets/MeetPerformanceViewModel.cs` — added `RawValue` (double?).
- `CloverleafTrack.ViewModels/Meets/MeetEventGroupViewModel.cs` — added `IsFieldEvent` (bool).
- `CloverleafTrack.Services/MeetService.cs` — `AddEventGroupsForCategory` / `AddEventGroupsFromList` group keys now also carry `p.EventType` (additive, does not change grouping granularity — EventId/EventName/Category/SortOrder already uniquely identify an event); new private `IsFieldEventType(EventType)` helper (`Field`, `FieldRelay`, `JumpRelay`, `ThrowsRelay` → true); `BuildPerformanceViewModel` now sets `RawValue = p.DistanceInches ?? p.TimeSeconds`.

**CSS:**
- `CloverleafTrack.Web/wwwroot/css/input.css` — appended `.sort-th-btn` (the auto-generated header button: `w-full h-full flex items-center gap-1 text-left`, hover/focus-visible ring) and `.sort-caret` (`inline-block w-4 text-center`, reserves width) to the plain top-level component-class section at the end of the file (same style as `.pill-tab`/`.chip-sr`/etc. — most of this file's custom classes live outside the one small `@layer components { }` block near the top; new classes should keep following that established plain-top-level-rule convention, not fight to get into the `@layer` block).
- **`wwwroot/css/site.css` (the compiled Tailwind output) was rebuilt in this session** via `pnpm dlx tailwindcss@3.4.17 -i ./CloverleafTrack.Web/wwwroot/css/input.css -o ./CloverleafTrack.Web/wwwroot/css/site.css --minify` and the diff is included in this change. **Watch out:** the sandboxed dev environment's outbound network access for `pnpm dlx` is flaky/intermittent (many consecutive attempts timed out before one finally succeeded) — if you add or change Tailwind classes in a future session and the build tool is unreachable, `input.css`/Razor changes will be committed but `site.css` will silently be stale (new classes present in source, absent from the shipped stylesheet, so e.g. sort buttons would render unstyled/without hover-focus treatment even though sorting itself still functions, since it's pure JS/DOM). Always diff `site.css` after any `input.css` or new-Tailwind-class change and rebuild before merging if it's empty/unchanged.

**Key files:**
- `CloverleafTrack.Web/wwwroot/js/sortable-tables.js` (NEW — the shared module, extensively commented with the full markup contract)
- `CloverleafTrack.Web/Views/Shared/_Layout.cshtml` (script tag registered after `filters.js`/`search.js`)
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml` (refactored: removed the inline "Sortable columns" IIFE and per-row `data-*-val` attributes in favor of per-cell `data-sort`; `<th>` markup simplified to attributes-only, no more hand-rolled `.sort-ind` spans)
- `CloverleafTrack.Web/Views/Shared/_RosterActiveAthletesList.cshtml`, `_FormerAthleteYearGroupSection.cshtml` (Roster — both the active and former athlete tables)
- `CloverleafTrack.Web/Views/Shared/_LeaderboardGenderSection.cshtml` (Leaderboard index — used by all six Boys/Girls/Mixed × Outdoor/Indoor sections; `data-sort-group-header` on the category-divider rows)
- `CloverleafTrack.Web/Views/Meets/Details.cshtml` (Meet results — Boys/Girls/Mixed sections, one `<table data-sortable>` per event group)
- `CloverleafTrack.ViewModels/Meets/MeetPerformanceViewModel.cs`, `MeetEventGroupViewModel.cs`
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Web/wwwroot/css/input.css`, `site.css`

**Watch out (summary):**
- Compile risk: this session could not run `dotnet build` (NuGet network-blocked in this sandbox), so the `MeetService.cs`/ViewModel changes were not compiler-verified — re-check on first real build if anything looks off, though the changes are small, additive, and follow existing patterns closely (mirrors [C15]'s `RawValue` precedent exactly).
- `pnpm dlx` network access in this sandbox is unreliable — see the CSS note above.
- Don't add sort-related markup (button/caret) by hand in Razor — the module generates it from `data-sort-col`/label text at init. Hand-authoring it would double up or fight the module's DOM manipulation.
- `data-sort-col` values must be unique **within a page** if you want the URL-hash restore-on-load feature to target the right column on the right table; they don't need to be globally unique across all tables on a page (e.g. "athlete"/"mark" are reused identically across every Meet-results event-group table, which is intentional — a bookmarked `#sort=mark&dir=asc` link sorts every event table on that meet page the same way).

---

### [C28] Events IA — `/leaderboard` Renamed to `/events` (Routing Only)

**What changed:**
The public route for the all-time top-10 pages was renamed from `/leaderboard` / `/leaderboard/{eventKey}` to `/events` / `/events/{eventKey}`, via attribute routing on `LeaderboardController` (same pattern already used by `RosterController`, `SeasonsController`, `MeetsController` for their parameterized detail actions):

```csharp
[HttpGet("/events")]
public async Task<IActionResult> Index() { ... }

[HttpGet("/events/{eventKey}")]
public async Task<IActionResult> Details(string eventKey) { ... }
```

Two new actions were added to 301-redirect the old, live/indexed URLs so they never 404:

```csharp
[HttpGet("/leaderboard")]
public IActionResult IndexLegacyRedirect() => RedirectToActionPermanent(nameof(Index));

[HttpGet("/leaderboard/{eventKey}")]
public IActionResult DetailsLegacyRedirect(string eventKey) => RedirectToActionPermanent(nameof(Details), new { eventKey });
```

`RedirectToActionPermanent` issues a real HTTP 301 and resolves the target URL through the (now `/events`-based) attribute routes, so the redirect target is always correct even if the route template changes again later.

**Explicitly NOT renamed (do not "fix" this later):**
`LeaderboardController`, `LeaderboardService`/`ILeaderboardService`, the `Leaderboards` DB table, `sp_RebuildLeaderboards`, and the `Views/Leaderboard/` folder are all still named "Leaderboard". This was intentional — the issue was a URL/IA rename only, not a rename of the internal domain concept. `LeaderboardServiceTests.cs` required zero changes because the service layer was untouched.

**Why:**
`/leaderboard` didn't describe what the page actually is (event-by-event all-time top-10 lists across every event). `/events` is clearer for visitors and matches the nav label. The old URL had to keep working (301, not 404) because it's live and indexed.

**Views needed no `/leaderboard` hardcoding fixed:** `Views/Shared/_LeaderboardGenderSection.cshtml` and `Views/Leaderboard/Details.cshtml` link to event rows and "Back to Leaderboard" via `asp-controller="Leaderboard" asp-action="..."` tag helpers, not hardcoded hrefs, so they picked up `/events` automatically once the controller's attribute routes changed. (This also confirms the earlier claim in this project's issue backlog that the leaderboard index page had *zero* links to event detail pages was false — it always linked every row, just via tag helper rather than a raw `<a href="/leaderboard/...">`.)

**Places that DID need a manual `/leaderboard` → `/events` string update:**
- `CloverleafTrack.Services/SearchService.cs` — `GetSearchIndexAsync()` builds the ⌘K search index (and the SearchGenerator console app's static export) consumed by both `SearchController` (`/search-index.json`, `/static/search-index.json`) and `CloverleafTrack.SearchGenerator`; it hardcoded `$"/leaderboard/{evt.EventKey.ToLower()}"` for every event search record — changed to `/events/{evt.EventKey.ToLower()}"`.
- `CloverleafTrack.Web/Views/Shared/_MainNavigation.cshtml` — nav link href `/leaderboard` → `/events`, link text "Leaderboard" → "Events". The `Icon` key on the link object is still the string `"leaderboard"` (only used internally to pick which inline SVG to render) — left as-is, it's not user-facing.

**Watch out:**
- An earlier `BRAIN.md` entry, **[C26] Phase 1 Performance-Data UX Pass**, claims *"Main navigation copy now shows `Events` and `Athletes` while keeping `/leaderboard` and `/roster`."* That claim was inaccurate/aspirational — as of this entry's session, `_MainNavigation.cshtml` still said `href="/leaderboard"` / text `"Leaderboard"` before this change. Per the append-only convention this entry supersedes that claim rather than editing it: the nav now genuinely says "Events" and links to `/events`, and the URL itself changed too (not just the label).
- Once an MVC action has an explicit route attribute (`[HttpGet("...")]`), it stops matching the app's conventional route (`{controller=Home}/{action=Index}/{id?}` in `Program.cs`) entirely. That's why separate `IndexLegacyRedirect` / `DetailsLegacyRedirect` actions were added rather than trying to make `Index`/`Details` handle both paths — a single action can't carry two `[HttpGet]` templates and still cleanly express "redirect the old one, render the new one."
- `Views/Leaderboard/Index.cshtml`'s `<h1>`, `ViewData["Title"]`, and the JS function name `adjustLeaderboardLayout()` were deliberately left saying "Leaderboard" — those are page-content/internal-naming, not the nav link or route, and were out of scope for this rename.
- `docs/schema.sql`, `docs/testing.md`, `UX_IMPROVEMENTS_PROMPT.md`, and `scripts/update-event-sort-orders.sql` all mention "leaderboard" as the feature/table name — none of those are public link targets, so none were touched.

**Key files:**
- `CloverleafTrack.Web/Controllers/LeaderboardController.cs`
- `CloverleafTrack.Web/Views/Shared/_MainNavigation.cshtml`
- `CloverleafTrack.Services/SearchService.cs`
- `CLAUDE.md` (routing table entry updated)

---

### [C33] Event Page Depth — Season Scope + Depth Controls on `/events/{eventKey}`

**What changed:**
The event detail page (`Views/Leaderboard/Details.cshtml`, `LeaderboardService.GetLeaderboardDetailsAsync`) gained two new controls, on top of the existing School Record History / All Performances-PRs Only / class-rank-chip features:

1. **Season scope** — All-time (default) / This Season / a past-season dropdown. Filters on `Meets.SeasonId` (added `AND (@SeasonId IS NULL OR m.SeasonId = @SeasonId)` to the existing `GetAllPerformancesForEventAsync` query — no new query shape, just a filter param). Past-season options are enumerated via `ISeasonRepository.GetAllAsync()` (the same repository method `SeasonService` already uses), not a new season-listing query.
2. **Depth** — Top 10 / Top 25 (default) / Top 100 / All. `ILeaderboardService.GetLeaderboardDetailsAsync` gained a `depth` parameter (`0` is the "All"/unbounded sentinel, since there's no natural negative-number sentinel that reads well from a query string). Applied via `.Take(effectiveDepth)` after all the view-model lists are built.

**Composition order — depth is applied AFTER scope and class filtering, never before:**
`GetLeaderboardDetailsAsync` builds each class's list (`ClassAllPerformances["Junior"]`, `ClassPersonalRecords["Junior"]`, etc.) by first filtering `scopedPerformances` (already SQL-filtered by season) down to that one class, re-ranking 1..N within that subset, and *then* calling `.Take(effectiveDepth)` on the result. "Top 25 Juniors" is therefore genuinely the top 25 juniors — never "the juniors that happen to fall within the overall top 25." The overall "all classes" list (`AllPerformances`) is built and depth-limited completely independently of the per-class lists. **Do not** refactor this into "build the full ranked list once, then `.Where(class).Take(depth)`" — that changes the result for any event where the top N overall isn't evenly distributed across classes (which is the common case).

**"Show all" / page-weight mechanism (option (a) from the issue, not (b)):**
Rather than a second query/view for "All", every class section (`all-section-all`, `all-section-Freshman`, …, mirroring the pre-existing `prs-section-*` pattern from the PRs Only tab) is rendered server-side already depth-limited for the *current* scope+depth, and the Class chip click is a pure client-side section toggle (no re-query, no re-rank) — this is why class-chip switching stays instant while scope/depth changes navigate. Depth/scope changes, unlike class, require an actual server round-trip because they change what SQL query ran and what got queried into the class dictionaries in the first place; a client-side toggle can't retroactively un-truncate data that was never sent to the browser. Depth's "All" pill (and the "Show all N →" link next to the depth pills) is literally a link to `?depth=all`, which re-renders the same page with `effectiveDepth = int.MaxValue` — this is deliberately option (b) from the issue ("links to a second view/query"), not option (a) ("already fetched but hidden via `<details>`/JS reveal"), because pre-fetching the unbounded remainder into the DOM just to hide it would defeat the whole point of depth-limiting the default render (990 rows → ~25-125 rows on the 100m-boys-outdoor-scale benchmark page; see below).

**Page weight measurement:** No live DB was available in the environment this was built in (NuGet/DB both network-blocked), so the "990 rows → X rows" reduction is reasoned about, not measured against real data. Previously the event detail page rendered every performance ever recorded for the event into one `<table>` (`Model.AllPerformances` had zero server-side limit). With Top 25 (default) applied per class, the worst case is 1 "all" section (≤25 rows) + 4 class sections (≤25 rows each) = ≤125 rows in the DOM, vs. 990 before — and typically far fewer once a season is that history spread thinner per class. **Needs manual verification against the real 100m-boys-outdoor event once a DB is available.**

**URL hash state (`#scope=season-2019&depth=25&class=junior`):**
Extended `wwwroot/js/filters.js`'s existing hash-parsing helpers (`getHashFilters`/`setHashFilter`, used by the site's `data-filterable` chip system) rather than re-implementing hash parsing — they're now exposed as `window.CtfFilters` for other pages to reuse. `Details.cshtml`'s own script uses `CtfFilters.getHashFilters()`/`setHashFilter()` for the `class` key (instant, no reload) and for round-tripping `scope`/`depth` (which do require a reload — see below). Scope/depth changes navigate to `?scope=...&depth=...#scope=...&depth=...&class=...`; on load, if the hash requests a scope/depth that differs from what the server actually rendered (e.g. someone loaded a bare `#scope=season-2019` URL with no matching query string), a client-side redirect fires once to pick up the real query-string-driven server render.

**Watch out:**
- `filters.js` is loaded with `defer` in `_Layout.cshtml`. Deferred scripts run *after* other inline `<script>` blocks but *before* `DOMContentLoaded`. The round-trip redirect check in `Details.cshtml` therefore has to run inside a `DOMContentLoaded` listener, not as top-level synchronous code in the inline script block — reading `window.CtfFilters` synchronously at that point in the page load would always see `undefined` (this was caught and fixed during this session; if you see `window.CtfFilters` referenced outside a `DOMContentLoaded`/later callback anywhere, that's the same bug).
- `LeaderboardDetailsViewModel.ScopeValue` is **rebuilt from the validated/parsed scope pieces** (`"all-time"` / `"season"` / `"season-{digits}"`), never echoed from the raw `scope` query-string argument. The view interpolates `Model.ScopeValue` directly into an inline `<script>` string literal (`var currentScope = '@Model.ScopeValue';`); Razor's default `@`-encoding is HTML-safe, not JS-string-safe, so echoing the raw attacker-controlled query string there would be a reflected-XSS hole. If you ever need `ScopeValue` to carry more information, keep it restricted to a small validated character set (digits/hyphens only) for this reason.
- A season scope with zero performances for the event no longer 404s — `GetLeaderboardDetailsAsync` falls back to an all-time query purely to resolve event metadata (name/gender/environment/field-vs-running/relay-vs-individual) when the scoped query comes back empty, so the page renders an empty-state "no performances" view instead. Only a **globally** empty result (no performances in any season, ever) 404s.
- `SchoolRecordProgression` is computed from the *scoped* performance set, not always all-time. For a past-season scope this means the progression only reflects records set within that season's data — it will not show the all-time record if that record was set in a different season. This was a deliberate scope decision for this feature, not a bug; revisit if a future issue wants "the SR history leading up to this season" instead.
- `LeaderboardService` now takes a second constructor dependency, `ISeasonRepository` (already registered in `Program.cs` for `SeasonService`, so no DI registration change was needed) — if you see `LeaderboardService` fail to resolve in a hand-written test or fixture that isn't `LeaderboardServiceTests.cs`, it needs a `Mock<ISeasonRepository>` (with `GetAllAsync()` stubbed, even to an empty list) added to its constructor call.
- The "sortable columns" JS (`data-sort-col`/`data-sort-type`, pre-existing from the sibling `feat/sortable-tables` branch not present in this branch) now attaches to up to 5 `<table>` elements per tab (one per class section) instead of 1 — this is intentional and harmless (each header's click handler sorts only its own `closest('table')`), but don't assume there's a single `#all-table` anymore; that id was removed since it would have been duplicated 5 times.

**Key files:**
- `CloverleafTrack.Services/LeaderboardService.cs` — scope resolution, depth-limiting, per-class list building
- `CloverleafTrack.Services/Interfaces/ILeaderboardService.cs`
- `CloverleafTrack.DataAccess/Repositories/LeaderboardRepository.cs` — `GetAllPerformancesForEventAsync` gained optional `seasonId`
- `CloverleafTrack.DataAccess/Interfaces/ILeaderboardRepository.cs`
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardDetailsViewModel.cs` — `ClassAllPerformances`, `ScopeValue`, `DepthValue`, `TotalPerformanceCount`, `CurrentSeasonId`, `SeasonOptions`
- `CloverleafTrack.ViewModels/Leaderboard/SeasonFilterOptionViewModel.cs` (NEW)
- `CloverleafTrack.Web/Controllers/LeaderboardController.cs` — `Details` now binds `scope`/`depth` query params
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml` — scope/depth controls, per-class "All Performances" sections, rewritten script block
- `CloverleafTrack.Web/wwwroot/js/filters.js` — exposes `window.CtfFilters`
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs` — scope × depth × class composition tests

---

### [C34] Footer SHA Actually Wired Through Docker/CI — Supersedes [C27]'s "pass --build-arg" Note

**What changed:**
[C27] added `BuildMetadataHelper` and the `.csproj`'s `SourceRevisionId`/`InformationalVersion` properties, and noted that Docker builds needed `--build-arg`/git context wiring to populate it — but that wiring was never actually added. In production the footer showed `vunknown` instead of a real commit SHA.

Root cause, confirmed by testing `dotnet publish` directly: when `SourceRevisionId` is unset, the `.csproj`'s own `Condition="'$(SourceRevisionId)' == ''"` defaults it to the literal string `"unknown"` — and separately, the .NET SDK's built-in behavior auto-appends `+$(SourceRevisionId)` to `InformationalVersion` regardless of the `.csproj`'s custom logic. With `SourceRevisionId` defaulted to `"unknown"`, the SDK appends `+unknown`, producing an `InformationalVersion` like `1.0.0-unknown+unknown`. `BuildMetadataHelper.GetShortCommitSha()` takes the first 7 characters after the `+` — and `"unknown"` is exactly 7 characters, so it passes through unchanged. The footer's `vunknown` wasn't a fallback failing to trigger; it was the real value, correctly extracted, from a `SourceRevisionId` that was never set to an actual commit SHA.

**Fix — two files, both needed:**
- `CloverleafTrack.Web/Dockerfile` — `publish-web` stage gained `ARG SOURCE_REVISION_ID=unknown` and `/p:SourceRevisionId=$SOURCE_REVISION_ID` on the `dotnet publish` line. (The `dotnet build` line in the `build` stage does NOT need this — its output is never copied into the final image; only `publish-web`'s `/app/publish` is.)
- `.github/workflows/build-and-push.yml` — the `docker/build-push-action@v6` step gained `build-args: | SOURCE_REVISION_ID=${{ github.sha }}`.

**Verified by direct test** (not just reasoning): ran `dotnet publish CloverleafTrack.Web/CloverleafTrack.Web.csproj /p:SourceRevisionId=abc1234` and confirmed via `strings` on the output DLL that `InformationalVersion` became `1.0.0-abc1234+abc1234` — `GetShortCommitSha()` correctly extracts `abc1234`.

**Watch out:**
- Local `docker build` without passing `--build-arg SOURCE_REVISION_ID=...` will still show `vunknown` — this is expected and fine; only the CI-built production image needs the real SHA.
- If a future change touches `Dockerfile`'s multi-stage `ARG` scoping: Docker `ARG`s declared before a stage's `FROM` are not automatically inherited by later stages — each stage that needs one must redeclare it (see how `BUILD_CONFIGURATION` is redeclared in `build`, `publish-web`, and `publish-generator`). `SOURCE_REVISION_ID` only needed redeclaring in `publish-web` since that's the only stage that uses it.
- `GetShortCommitSha()`'s fallback path (no `+` found) would also misfire the same way if `InformationalVersion` itself ever became exactly a 7-character non-SHA string — this is a narrow, unlikely edge case, not something this fix needed to touch, but worth knowing if `vunknown`-style bugs resurface with a different literal string.

**Key files:**
- `CloverleafTrack.Web/Dockerfile`
- `.github/workflows/build-and-push.yml`

---

### [C35] Percentile + All-Time Rank on Every Personal Best (Issue #19)

**What changed:**
The Roster Details "Personal Bests" table gained two new columns — **Pct** (percentile as a colored numeral with ordinal suffix) and **All-time rank** (`#9 of 412`) — for both the individual-PR table and the relay-PR table. The old conditional `#N AT` badge column (gated behind `hasAnyIndividualRank`/`hasAnyRelayRank`, only rendered when at least one row had a rank) was removed **from this table only**. The `SR` school-record chip was NOT removed — it moved inline next to the mark in the "Best" column instead of living in the now-gone badge column.

**Scope, per the issue:** the `#N AT` badge stays everywhere else (`chip-rank` is still used on meet results, the home page highlights digest, and `_TeamResultBadge.cshtml`) — this was a table-level change, not a retirement of the badge.

**New shared building blocks (designed for reuse by [#21], the mark-color-scale issue, not built yet):**
- `CloverleafTrack.Web/Utilities/PercentileHelper.cs` (NEW) — single source of truth for the diverging percentile color scale. Holds all three color variants per bucket (`Fill`, `Ink`, `Text`) even though this change only consumes `Text` — the values are already fully specified in issue #21's table and match #19's `Text` column exactly, so building all three now means #21 later just calls the same helper instead of duplicating the bucket table. Also owns `OrdinalSuffix(int)` and `GetReading(int)` (the plain-English "97th percentile of program history — far above median" tooltip text).
- `CloverleafTrack.ViewModels/Shared/PercentileRankViewModel.cs` (NEW) — small dedicated ViewModel (`Percentile`, `AllTimeRank`, `EventMarkCount`) so the partial isn't coupled to `PersonalRecordViewModel` specifically; a future `/search` page (#29) or event-page integration can reuse the same partial with its own data.
- `CloverleafTrack.Web/Views/Shared/_PercentileRankCells.cshtml` (NEW) — renders the two `<td>` cells together (Pct + Rank always appear as a pair). Called directly inside a `<tr>`, same pattern as `_AttemptStrip`/`_AttemptSeriesExpanded` being invoked mid-row elsewhere in this codebase.

**Data plumbing:** `AthletePerformanceDto` gained `EventMarkCount` (from `EventStatistics`, needed for the "of 412" part — `Percentile` and `AllTimeRank` were already present from the percentile-foundation work). `AthleteRepository.GetAllPerformancesForAthleteAsync`'s two UNION branches (individual + relay) both gained a `(SELECT es.EventMarkCount FROM EventStatistics es WHERE es.EventId = e.Id)` scalar subquery. `EventMarkCount` is guaranteed non-null whenever `Percentile` is — both come from the same `sp_RebuildLeaderboards` rebuild pass, and `EventStatistics` always gets a row for every `EventId` that has at least one performance (unlike `MedianValue`/`Q1Value`/`Q3Value`, which are the only columns nulled below the 10-mark floor).

**Accessibility, per the issue's explicit spec:**
- `title` attribute on the Pct cell carries the plain-English reading (mouse-only, but free).
- A `sr-only` span **inside** the percentile element carries the same reading, so screen readers announce "97th percentile of program history — far above median" rather than just "97th" — this supplements the visible number, it does not replace it.
- Deliberately **not** an `aria-label` on the same element — an `aria-label` would override the visible number for assistive tech instead of adding to it, which is exactly the failure mode the issue calls out.
- No tap-to-reveal popover for touch users — accepted per the issue as not worth the interaction cost, since the number and rank are already both on screen.

**Watch out:**
- `PercentileRankViewModel.HasData` gates the whole partial on `Percentile.HasValue` — a performance with no percentile data (shouldn't happen post-migration, but matters for defensive rendering) gets two empty `<td>` cells, not a missing-column layout break.
- The Rank cell only shows `#X of Y` when **both** `AllTimeRank` and `EventMarkCount` are present — `AllTimeRank` is null for the vast majority of athletes (only top-10-all-time performances get a `Leaderboards` row), so most rows will show a Pct number with an empty Rank cell. That's expected, not a bug.
- New columns have no `hidden` responsive classes — they're meant to survive the mobile breakpoint per the issue ("it's two short strings"), unlike Date/Meet which already collapse via `hidden sm:table-cell`/`hidden md:table-cell`.
- `PercentileHelper`'s `Fill`/`Ink` variants are unused dead code until #21 is built — this is intentional forward-prep, not scope creep on #19, since the color table is a single already-fully-specified source shared by both issues.

**Key files:**
- `CloverleafTrack.Web/Utilities/PercentileHelper.cs` (NEW)
- `CloverleafTrack.ViewModels/Shared/PercentileRankViewModel.cs` (NEW)
- `CloverleafTrack.Web/Views/Shared/_PercentileRankCells.cshtml` (NEW)
- `CloverleafTrack.DataAccess/Dtos/AthletePerformanceDto.cs`
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`
- `CloverleafTrack.ViewModels/Athletes/PersonalRecordViewModel.cs`
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`
- `CloverleafTrack.Tests/Unit/Utilities/PercentileHelperTests.cs` (NEW)

---

### [C36] Top Sprint/Field Event Selection — Percentile Tiebreaker (Issue #48)

**What changed:**
`AthleteService.GetAthleteDetailsAsync`'s `topSprintEvent`/`topFieldEvent` selection (used for the Roster hero "Top Event" display) gained `.ThenByDescending(pr => pr.Percentile ?? 0)` after the existing `.OrderBy(pr => pr.AllTimeRank ?? 999)`.

**Why:**
`AllTimeRank` only exists for the true top 10 all-time in an event (from the `Leaderboards` table). For any athlete who isn't top-10-all-time in *any* individual event — the large majority of the roster — every one of their events tied at the `?? 999` fallback. Since LINQ's `OrderBy` is a stable sort, ties preserved `personalRecords`' original structural order (sorted by `Environment`, then `EventCategorySortOrder`, then `EventSortOrder` — see the PersonalRecordViewModel construction just above this code), so `.FirstOrDefault()` silently returned whichever event had the lowest `EventSortOrder`, not the athlete's actual best event. E.g. an athlete who ran both the 100m and 400m, with no all-time rank in either, would always get "100m" as their Top Sprint Event regardless of which one they were actually better at relative to the field.

`Percentile` (added in [C31]/[C35]) is populated for nearly every performance, not just the top 10, making it the correct tiebreaker: pick the athlete's highest-percentile event when no `AllTimeRank` distinguishes them.

**Watch out:**
- `AllTimeRank` still wins outright when present — an athlete with an actual top-10-all-time mark keeps that as their Top Event even if a different event has a higher raw percentile. This is intentional: a precise, verified all-time placement is a stronger claim than a percentile estimate.
- This only affects *selection* (which event is chosen). The hero display itself still only shows a rank chip (`#N`) when `AllTimeRank.HasValue`, and shows nothing for the majority of athletes who now correctly have a *meaningful* Top Event but no numeric rank badge to go with it. Whether to also surface the percentile on the badge was deliberately left open — see the issue.

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` — 2 new tests: percentile-as-tiebreaker, and `AllTimeRank` still wins over a higher percentile when both are present

---

### [C37] Diverging Color Scale on Event-Page Mark Cells (Issue #21)

**What changed:**
The Mark cell on the event detail page (`/events/{eventKey}`, both the "All Performances" per-class tables and the "PRs Only" per-class tables) now gets a percentile-driven tint: a 22%-alpha background fill plus a 3px full-strength inset left edge, with the mark text itself recolored to the bucket's `ink` value. Colors come entirely from the already-built `PercentileHelper` (added in [C35] for the roster percentile column, unused-until-now `GetFillColor`/`GetInkColor` methods) — no new color table, exactly the "one source of truth" the helper was built for.

**Where the tint does NOT apply, and why:**
Per the issue: tint is only valid on tables where every row is the *same event* — that's what makes percentile-within-that-table comparable via color at a glance. The Roster Personal Bests table ([C35]/#19) and the future Roster index ([#23]) both mix multiple events per table, so they use the plain percentile *numeral* instead (no tint) — this was already the correct call in [C35], made before this issue was implemented, not something this change needed to touch.

**Accessibility — forced-colors / high-contrast mode:**
A `.pct-tint-td` class hook plus a page-scoped `@media (forced-colors: active)` rule (in the existing `<style>` block already present in `Leaderboard/Details.cshtml`) strips the tint background and box-shadow with `!important` — `!important` in an external/embedded stylesheet rule does take precedence over a plain (non-`!important`) inline `style` attribute, which is what makes suppressing an inline-styled tint from a stylesheet rule possible at all. The mark numeral itself is unaffected (forced-colors mode overrides inline text colors via the browser's own UA stylesheet, not something this page needs to handle manually).

**Data plumbing:** `LeaderboardPerformanceViewModel` gained `Percentile` (byte?) — the DTO (`LeaderboardPerformanceDto.Percentile`) and the repository JOIN (`LEFT JOIN PerformancePercentiles`) already existed from the percentile-foundation work ([C31]); this change was purely wiring the already-available column through `LeaderboardService`'s two ViewModel builders (`BuildAllPerformanceViewModels`, `BuildPrViewModels`) into the view.

**Watch out:**
- `markFill`/`markInk` (and the `2`-suffixed variants in the PRs Only loop) are computed once per row in the `@foreach` block, mirroring the existing `isGold`/`rowClass` pattern in the same file — don't move percentile-color computation into the service layer "to be consistent with tint being applied in the view," the color *values* are looked up from `PercentileHelper` (a shared, testable, single-source lookup) which is the actual requirement; only the row-level null-check-and-call is view-layer, same as every other per-row display concern already in this file.
- A performance with no `Percentile` (shouldn't happen post-migration, but the DTO leaves it nullable) falls back to the pre-existing plain `text-gray-900 dark:text-white` classes and no tint — same defensive-null pattern as [C35]'s `PercentileRankViewModel.HasData`.

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardPerformanceViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml`
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs` — 1 new test verifying `Percentile` flows into both `AllPerformances` and `PersonalRecordsOnly`

---

### [C38] Gender Encoding — Roster Grouped by Gender, No Per-Row Column (Issue #28)

**What changed:**
Both roster surfaces (`_RosterActiveAthletesList.cshtml` for active athletes, `_FormerAthleteYearGroupSection.cshtml` for former athletes, nested per graduation year) had a per-row "Gender" column showing a colored `M`/`F` letter (`text-blue-500` / `text-pink-500`) — the classic color-alone-conveys-meaning failure (fails deuteranopia/protanopia, grayscale, print, forced-colors). Both are now split into two separately-rendered gender sections, each with a triple-encoded (shape glyph + word + color) header stating identity once instead of per-row, and each with its own independently sortable table with no gender column at all.

**Colors are NOT the leaderboard's existing pink/blue.** The issue explicitly rejected pink/blue (worst-pair CVD ΔE 26.8) in favor of new validated categorical slots: Boys = circle glyph + `#3987e5`, Girls = rounded-square glyph + `#d95926`. **The pre-existing Leaderboard Index page's Boys/Girls headers (♂/♀ glyphs, blue-600/pink-600) were deliberately NOT touched** — the issue's "Problem" section is specifically scoped to the roster's per-row column bug, and the leaderboard's existing headers already avoid "color alone" (they pair color with the word "Boys"/"Girls"), so they already pass the core accessibility bar even though they use a different color pair than this issue's new decision. If sitewide consistency with the new color pair is wanted later, that's a separate follow-up, not something this change's scope covered.

**New shared building block:**
- `CloverleafTrack.ViewModels/Shared/GenderSectionHeaderViewModel.cs` (NEW) — `Gender`, `Count`, and `Size` ("lg" default for page-level sections, "sm" for the nested former-athletes-by-year context).
- `CloverleafTrack.Web/Views/Shared/_GenderSectionHeader.cshtml` (NEW) — renders the glyph (inline SVG circle/rounded-rect, not a Unicode character, for reliable cross-platform rendering) + word + color + athlete count. **Reserved for headers only** — do not reuse this partial per-row; that would reintroduce exactly the bug this issue fixed. Scoped to Male/Female only: individual `Athlete.Gender` is never `Mixed` (unlike `Event.Gender`, which can be), so this deliberately has no Mixed case — do not add one without first confirming athletes can actually be Mixed, which they cannot under the current schema.

**Why two separate tables per surface, not one table with a group-divider row:**
`sortable-tables.js`'s existing `data-sort-group-header` convention (used in `_LeaderboardGenderSection.cshtml`) hides group-header rows and lets other rows interleave freely while any sort is active, restoring grouping only on the third click (reset). Using that pattern here would mean sorting the roster by name silently ungroups Boys/Girls with zero indication of which is which — arguably a worse failure than the original color-only badge, since it loses the information entirely rather than just encoding it inaccessibly. Two independently-`data-sortable` tables can never cross-contaminate: sorting one gender's table never affects the other's grouping or visibility.

**Filtering — reused the existing mechanism, no new JS:**
The gender filter chip's existing `data-filterable data-gender="boys|girls"` convention (already used on the Leaderboard Index page's Boys/Girls divs) is applied to each gender *section* wrapper div (not per-row, since there's no per-row gender attribute anymore) — `filters.js` already hides an entire `[data-filterable]` element outright when its own attribute doesn't match, which is exactly "drop the entire other section rather than an empty header," reusing the same behavior [C17] established for the leaderboard's full-width collapse. **Watch out:** the gender-filtering div and the category-filtering `data-filterable-section` (auto-hide-if-all-children-filtered-out) div must be two separate NESTED elements, not the same element carrying both attributes — `filters.js` runs two independent passes (`[data-filterable]` self-match, then `[data-filterable-section]` descendant-visibility recompute) and putting both on one element lets the second pass silently undo the first pass's gender-based hide, since the individual `<tr>` rows (which only carry `data-categories`, no `data-gender`) would count as "visible descendants" regardless of the outer gender match.

**Filter chip accessibility (`_FilterChipGroup.cshtml`):**
- `aria-pressed` was **already** being set correctly by `filters.js`'s `applyFilters()` (`btn.setAttribute('aria-pressed', ...)`) — confirmed, not a gap needing a fix.
- Added a visually-hidden-until-active `✓` glyph (`.chip-check`, new `@@layer components` rule in `input.css`, requires the `pnpm run prod` Tailwind rebuild already reflected in `site.css`) so the active chip state isn't fill-color-alone either — same principle as the gender fix, applied to the filter chip system itself.

**Dead code, deliberately not touched:** `_AthleteCard.cshtml` (used only by `_AthleteCategorySection.cshtml`, which is itself never invoked from any live page — confirmed via a repo-wide reference search) has the same color-only M/F badge, but since it's unreachable it doesn't affect any real page or the "grayscale screenshot" acceptance criterion. Left alone rather than fixed or deleted; worth revisiting whenever [#24] (splitting alumni off the roster page) touches this dead code path.

**Watch out:**
- `GenderSectionHeaderViewModel.Size` exists specifically because a page-level `text-2xl` header would be visually oversized nested one level inside a per-class-year `<details>` — don't hardcode a new consumer to "lg" without checking its visual context first.
- Both restructured `.cshtml` files hit the same Razor pitfall as [C32]/the attempt-series fix: inside an already-open `@if {}` or `@{ }` code block at the top level of the file, do not wrap a statement in a redundant `@{ }` — this is the second time this exact mistake has been made and caught in this codebase; if you see `RZ1010` again, this is almost certainly why.

**Key files:**
- `CloverleafTrack.ViewModels/Shared/GenderSectionHeaderViewModel.cs` (NEW)
- `CloverleafTrack.Web/Views/Shared/_GenderSectionHeader.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Shared/_RosterActiveAthletesList.cshtml`
- `CloverleafTrack.Web/Views/Shared/_FormerAthleteYearGroupSection.cshtml`
- `CloverleafTrack.Web/Views/Shared/_FilterChipGroup.cshtml`
- `CloverleafTrack.Web/wwwroot/css/input.css`, `site.css`

---

### [C39] Career Progression Chart Rebuilt — Server-Rendered SVG, No Chart.js (Issue #26)

**What changed:**
[C15] added a Chart.js-based career progression chart to the Roster Details page; [C24]'s UX overhaul later removed it entirely (no `<canvas>`, no Chart.js script tag). This issue rebuilds it from scratch — not as a restoration of the old chart, but as a new, richer design: the athlete's line rendered *against the program's distribution* (record territory zone, median/IQR band, class-year ticks), as fully server-rendered SVG with zero client-side charting library. This resolves the "athlete progression chart missing from production" line item in issue #16 — that claim was accurate (the old chart really was gone), but the fix was this rebuild, not restoring what [C24] deliberately removed.

**Why hand-rolled SVG instead of Chart.js:** the issue explicitly asked to reconsider Chart.js for this specific chart. Reasons that won: (1) removes a CDN script dependency, (2) removes the "Chart.js in a hidden panel renders at 0×0, needs lazy-init" problem [C15] had to work around, entirely — there's no JS execution required to draw the chart at all, (3) works with JS disabled. The existing SR-progression Chart.js chart on the Leaderboard Details page ([C24] BRAIN entry) was **not** touched or migrated — that's a separate, working feature; this decision was scoped to the career chart only.

**The one thing the issue calls "trivially easy to get upside-down" — solved with an isolated, directly-tested pure function:**
`CareerChartGeometry.MapValueToPixelY(rawValue, min, max, plotTop, plotBottom, isFieldEvent)` is the single place axis inversion happens. The chart always renders "better" as visually higher (smaller pixelY) regardless of event type — field events (higher raw = better) and running events (lower raw = better) must map in *opposite* raw-value directions but the *same* visual direction. This is isolated in `CloverleafTrack.Services/CareerChartGeometry.cs` specifically so it can be unit-tested directly rather than trusted by eye in a browser — `CareerChartGeometryTests.cs` asserts both directions explicitly, including that a field mark at the top of its domain and a running mark at the bottom of its domain (both "the best possible mark") map to the *identical* pixelY. **If this function is ever touched, re-run these tests — this is the exact bug class the issue's acceptance criteria call out twice.**

**Record territory zone geometry:** "fills everything better than the record" doesn't mean anything can literally be better than a #1-all-time mark — it means the zone spans from the very top of the plot (`PlotTop`) down to the record's line, visualizing the (currently empty, aspirational) territory an athlete would need to reach to break it. `ShowRecordZone` is `false` whenever the viewed athlete already holds the record (`AllTimeRank == 1` on any of their performances in that event) — the "how much air is left" zone is meaningless once you *are* the top of it.

**Record value is NOT derived from the athlete's own performances** — it's the *current* all-time-best for the event, which may belong to someone else entirely. New `IAthleteRepository.GetSchoolRecordsForEventsAsync(eventIds)` queries `Leaderboards.Rank = 1` directly (never `Performances.SchoolRecord`, per the sitewide rule — see the reliability table earlier in this file). Added to `IAthleteRepository` rather than introducing a new `ILeaderboardRepository` dependency on `AthleteService`, specifically to avoid touching `AthleteService`'s constructor shape and forcing every one of the ~20 existing `new AthleteService(mockRepo.Object)` test call sites to be updated.

**Median/IQR band suppression — two independent gates, both must pass:** `!isRelay && markCount >= 10 && MedianValue.HasValue`. Relay events are suppressed unconditionally regardless of mark count (per the issue: "unstable population" — team composition changes meet to meet) — this is a stronger rule than the generic <10-marks floor `EventStatistics` already enforces, so relay suppression is checked explicitly in `AthleteService`, not left to `MedianValue` happening to be null.

**`ClassYearCalculator` — extracted from `LeaderboardService`, not reimplemented:** the issue explicitly says "using the August school-year rule already implemented ... do not reimplement." That method was `private` on `LeaderboardService`, so it's now `CloverleafTrack.Services/ClassYearCalculator.cs` (a small static class), with `LeaderboardService` updated to call the extracted version too — a pure refactor, no behavior change, verified by the full existing `LeaderboardServiceTests` suite staying green.

**Class-year tick positions are anchored to real data points, not computed calendar boundaries.** Each tick sits at the X position of the *first performance* in each class the athlete actually has marks in, labeled with that class's first two letters (Fr/So/Jr/Sr). This was a deliberate simplification over computing exact August 1st boundary dates: every tick this way is guaranteed to fall within the plotted date range and correspond to something real, rather than needing separate logic to reason about ticks that might fall before the first or after the last data point.

**X-axis is true date-proportional spacing, not index-based.** This was necessary (not just nicer) specifically because class-year ticks only make sense positioned by real calendar time — index-based even-spacing would make ticks meaningless. Single-performance events center the lone point rather than dividing by a zero date range.

**Mobile spec, satisfied without a separate responsive re-layout:**
- "Drop to three Y-axis ticks below `sm`" — `CareerChartYTickViewModel.HiddenOnMobile` is `true` for 2 of the 5 generated ticks (the `i==1`/`i==3` quartile ticks), rendered with a `hidden sm:inline` class. No JS, no separate mobile SVG.
- "Move the two zone labels into the legend" — the inline SVG record/median labels already carry `hidden sm:inline`; the legend below the chart (always visible, not just on mobile) already repeats the same `Record: X` / `Program median: X` text, so hiding the inline labels loses no information at any width.

**SVG `<text>` collision — hit this bug again, third time in this codebase:** SVG's `<text>` element collides with Razor's own reserved `<text>` pseudo-tag, which forbids attributes (RZ1023). All four `<text>` elements in `_CareerProgressionSection.cshtml` (record label, median label, class-tick labels, Y-tick labels) are emitted via `@Html.Raw($"<text ...>{WebUtility.HtmlEncode(label)}</text>")` rather than literal markup — see [C32]/attempt-series' BRAIN entry for the first occurrence. **If a future SVG-in-Razor change hits RZ1023 on a `<text>` element, this is always why.**

**Legend, not color-alone:** the athlete's own marks are a solid line + circles; career-best gets a larger ringed circle (shape, not just size); record/median are dashed lines + filled zones. Up to 5 legend entries (Your marks, Career best, Record, Program median, Middle 50%), fewer when record/median are suppressed for that event — never a phantom legend entry for a zone that isn't rendered.

**Not yet done / needs manual verification:** the acceptance criteria call for verifying the chart visually in a browser for both event types and at mobile width, and there's no automated way to verify actual SVG pixel rendering from this environment — the geometry math is unit-tested (the part that's easy to get subtly wrong), but a human should still eyeball a real field-event chart and a real running-event chart before calling this fully done.

**Key files:**
- `CloverleafTrack.Services/CareerChartGeometry.cs` (NEW) — pure, tested Y-axis mapping + domain computation
- `CloverleafTrack.Services/ClassYearCalculator.cs` (NEW) — extracted from `LeaderboardService`
- `CloverleafTrack.Services/AthleteService.cs` — `BuildCareerCharts`, `FormatDelta`
- `CloverleafTrack.Services/LeaderboardService.cs` — updated to call the extracted `ClassYearCalculator`
- `CloverleafTrack.DataAccess/Dtos/EventRecordDto.cs` (NEW)
- `CloverleafTrack.DataAccess/Dtos/AthletePerformanceDto.cs` — `MedianValue`, `Q1Value`, `Q3Value`
- `CloverleafTrack.DataAccess/Interfaces/IAthleteRepository.cs`, `Repositories/AthleteRepository.cs` — `GetSchoolRecordsForEventsAsync`; existing query switched from scalar `EventMarkCount` subquery to a `LEFT JOIN EventStatistics`
- `CloverleafTrack.ViewModels/Athletes/CareerChartViewModel.cs` (NEW)
- `CloverleafTrack.ViewModels/Athletes/AthleteDetailsViewModel.cs` — `CareerCharts`
- `CloverleafTrack.Web/Views/Shared/_CareerProgressionSection.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Roster/Details.cshtml` — section placed before "Performance by Season," never hidden
- `CloverleafTrack.Tests/Unit/Services/CareerChartGeometryTests.cs` (NEW) — 10 tests, the axis-inversion trap specifically
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` — 7 new tests covering the suppression rules and career-best selection

---

### [C40] Career Chart Fixes From a Real Screenshot — Supersedes Two Claims in [C39]

**What changed:**
[C39]'s career progression chart shipped with three real bugs, all caught from a production screenshot (not caught by the unit tests, which only covered the axis-inversion math and suppression rules — none of them rendered or inspected actual coordinates/labels):

1. **X-axis spacing was clustering, not "correctly calendar-accurate."** [C39] deliberately chose true date-proportional spacing over index-based spacing, reasoning that class-year ticks "only make sense positioned by real calendar time." That reasoning was wrong in practice: this program's meets cluster into short in-season windows separated by months-long off-seasons, so date-proportional spacing crushed every meaningful point into a few dense clusters and burned most of the plot width on empty off-season gaps. **Switched to spacing by chronological order (index-based), not calendar date.** Class-year ticks still work correctly with no separate change needed — they're derived from each point's already-computed `PixelX`, not recomputed from dates independently.

2. **Class-tick labels showed "Ju"/"Se" instead of "Jr"/"Sr."** The original code took `ClassAtTime![..2]` (first two characters) to abbreviate the class name — this happens to work for "Freshman"→"Fr" and "Sophomore"→"So", but "Junior"[..2] is "Ju" and "Senior"[..2] is "Se", not "Jr"/"Sr". Replaced with an explicit `ClassAbbreviation(string)` switch expression. **Watch out:** this is a reminder that "the first two/three letters happen to abbreviate correctly" is not a pattern that generalizes across a whole word list without checking every entry — it should have been an explicit map from the start.

3. **Y-axis labels were clipping off the left edge of the SVG.** Full-precision labels like `9' 4.82"` are ~9-10 characters at font-size 9, wide enough that with `PlotLeft = 40` and `text-anchor="end"`, long labels extended past `x=0` and were clipped by the SVG viewport — visible in the screenshot as leading digits going missing (`51' 9.94"` rendering as `1' 9.94"`). Fixed two ways together: `PlotLeft` increased from 40 to 68 (more margin), and Y-axis tick *labels* (not data-point labels, which keep full precision) now round to a whole inch (field) or one decimal second (running) before formatting — the gridline position itself still uses the exact unrounded fractional value, only the displayed text is shortened.

**Why none of this was caught before shipping:** the unit tests added in [C39] were deliberately scoped to the parts most likely to have subtle logic bugs (axis inversion, suppression rules) — they never asserted on label *content* or *spacing uniformity*, and nothing in this environment could render actual SVG to a screen to catch a visual clipping or clustering problem. Two new regression tests now lock in the specific failures a screenshot caught: `ClassTickLabels_UseJrAndSr_NotJuAndSe` and `PointsAreEvenlySpaced_NotClusteredByCalendarDate`. **Watch out:** passing unit tests for the parts you thought to test does not mean the feature is visually correct — this is the second time in this session a "the math is tested, therefore it's probably fine" claim needed a real screenshot to falsify (the first was implicit in how much [C39] hedged about needing manual verification).

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs` — `BuildCareerCharts` (X spacing, `PlotLeft`, Y-tick label rounding), new `ClassAbbreviation` helper
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` — 2 new regression tests

---

### [C41] Career Chart — Indoor/Outdoor Disambiguation (Second Screenshot Round on Issue #26)

**What changed:**
A second real screenshot caught a fourth bug from the [C39] career chart, distinct from [C40]'s three: **indoor and outdoor versions of the same event (e.g. "Shot Put") get separate charts — correctly, since they're different `EventId`s — but both tabs just said "Shot Put" with no way to tell which was which.** Worse, the two same-named tabs weren't even adjacent in the tab bar (sorted purely by point count), so a user had no reason to connect them as a pair at all.

**Fix, two parts:**
1. `CareerChartViewModel` gained an `Environment` field, populated from the DTO row's existing `Environment` column (already present per-performance, just never surfaced on the chart). Both the tab button and a new panel-level `<h3>` heading now show `"{EventName} — {Outdoor|Indoor}"`.
2. Chart ordering changed from a flat `OrderByDescending(Points.Count)` to `GroupBy(EventName)` → order groups by total point count → within each group, Outdoor before Indoor (matching the sitewide Outdoor-first convention already established for every other Indoor/Outdoor tab pair on the site). Same-named charts now always sit next to each other.

**Also fixed in passing:** there was previously **no visible event name at all** inside a chart panel when an athlete only had one event (the tab bar — the only place `EventName` was shown — is suppressed via `@if (Model.Count > 1)` for a single-chart athlete). The new panel heading renders unconditionally, so this gap is closed as a side effect, not a separately-scoped fix.

**Watch out:** this is the second bug-fix round on the same chart from two separate screenshots ([C40], then this one) — a strong signal that this feature's actual failure modes are almost entirely things unit tests can't catch (label content, disambiguation, information architecture) rather than the math bugs the original test suite was built to catch. If more screenshot-driven fixes land on this chart, consider whether a lightweight visual/snapshot test (even a manual checklist in the PR template) would catch more of this class of bug earlier than another round of unit tests would.

**Key files:**
- `CloverleafTrack.ViewModels/Athletes/CareerChartViewModel.cs` — `Environment` field
- `CloverleafTrack.Services/AthleteService.cs` — sets `Environment` per chart; grouped/ordered chart list
- `CloverleafTrack.Web/Views/Shared/_CareerProgressionSection.cshtml` — environment shown on tab + new panel heading
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` — 1 new test covering both the `Environment` field and the adjacent-grouping order

---

### [C42] Roster Redesign — Percentile Column, Sorting, Class Filter, Name Filter (Issue #23)

**What changed:** The active-roster table (`_RosterActiveAthletesList.cshtml`) was rebuilt around percentile instead of the raw mark, per the issue's own framing: `8.94`, `1:11.10`, and `35' 1"` in one "Best Mark" column can't be compared or sorted, so Percentile is now the value the table actually sorts by (default: descending, via a new `data-sort-default` mechanism in `sortable-tables.js`). New columns: Athlete (name + class subtitle, replacing the old standalone Class column), Top Event, Best Mark (tinted, reusing [C21]'s cell-tint pattern), Percentile (bar + numeral, median tick fixed at the bar's midpoint), Season Trend (per-season best-mark sparkline, 88×24, reusing `CareerChartGeometry` from [C39] for the Y mapping). Added a Class filter chip group (Fr/So/Jr/Sr), a client-side name filter, and CSV export of the currently-visible (filtered) rows.

**Top Event is no longer "first event by SortOrder."** The old code did `athlete.EventsInCategory.FirstOrDefault()` — whatever event happened to sort first structurally, not the athlete's actual best event. `AthleteService.GetFlatActiveAthletesAsync` now picks Top Event the same way `GetAthleteDetailsAsync`'s hero stats do ([C36]/#48): highest `Percentile` wins, falling back to first-by-SortOrder only when no performance has percentile data at all. This is the same bug class as #48, just a second, previously-unfixed instance on the roster page.

**Season trend requires season data the roster query didn't have before.** `AthleteRepository.GetAllWithPerformancesAsync` now joins `Meets`→`Seasons` and selects `SeasonStartDate`/`SeasonName` via a 4th Dapper multi-mapped type (`SeasonMarker`) — `AthleteEventParticipation` gained matching fields. The service computes best-mark-per-(athlete, event, season) alongside the existing best-mark-per-(athlete, event), then builds the sparkline only for whichever event ends up as Top Event. Points are spaced evenly by season index, not calendar date — deliberately reusing the *lesson* from [C40] (date-proportional spacing clusters badly around off-season gaps) rather than repeating that mistake here.

**Real bugs found only by loading the page, not by unit tests (all fixed before merge):**
1. **Mobile card layout overflowed the viewport horizontally.** The mobile "table row becomes a card" layout was first built with CSS Grid on the `<tr>` (`display:grid` + `grid-template-areas`). It silently overflowed off-screen on real mobile widths despite `minmax(0,1fr)` tracks and `min-width:0` on every grid item — confirmed via a CDP-driven debug outline that the row's own box was wider than the viewport. **`<tr>` retains anonymous table-object behavior even under a `display` override, and CSS Grid on a table row has known cross-browser sizing quirks for exactly this reason.** Switched to `display:flex` on the row with `flex: 1 1 0%` + `min-width:0` on the name/mark cells instead — the same "table row becomes a card" technique, but flexbox doesn't have this failure mode. **Watch out:** don't reach for CSS Grid on a `<tr>` again; use flexbox for any future responsive-table-to-card conversion.
2. **Headless-CLI screenshots (`chrome --headless --screenshot`) were not trustworthy for verifying this fix.** After switching to flexbox, `chrome --headless --window-size=390,900 --screenshot=...` kept showing the *exact same* overflow, byte-for-byte, across multiple rebuilds — a false negative. Driving the same running Chrome instance over the DevTools Protocol instead (`Emulation.setDeviceMetricsOverride` + `Runtime.evaluate` for computed `getBoundingClientRect()`, then `Page.captureScreenshot`) showed the fix had actually worked all along. **Watch out:** if a headless-CLI screenshot contradicts a CDP-measured `getBoundingClientRect()`/`scrollWidth` result, trust the CDP measurement — the CLI screenshot flag has a viewport/render-timing quirk in this environment.
3. **Right-aligned truncated text clips the wrong end.** The Top Event sub-label under the mark (e.g. "1600M Intermediate Medley" → "1600M Intermediate M...") inherited `text-align: right` from its parent `.col-mark` cell. Combined with `overflow:hidden; text-overflow:ellipsis; white-space:nowrap`, this clipped from the *start* of the string instead of the end. Fixed by giving that sub-label its own `text-align: left`, independent of the (correctly right-aligned) mark value beside it.
4. **The inline `<script>` wiring the name filter and CSV export threw on every page load and silently did nothing.** `filters.js` loads with `defer` in `_Layout.cshtml`, so it runs after HTML parsing completes; this partial's own inline `<script>` is not deferred and runs immediately as the parser reaches it — `window.CtfFilters` was still `undefined` at that point, so `window.CtfFilters.getHashFilters()` threw a `TypeError` that aborted the whole IIFE, meaning *neither* the name filter *nor* the CSV export button (wired later in the same function) ever worked. Fixed by wrapping the same logic in the exact `document.readyState === 'loading' ? addEventListener('DOMContentLoaded', init) : init()` guard `filters.js` already uses internally. **Watch out:** any inline `<script>` on a page that depends on `window.CtfFilters` (or any other `defer`-loaded global) must use this guard — it will not exist yet if the inline script runs during parsing.
5. **A Class filter chip could show an unrelated row.** An athlete flagged `IsActive = true` in the DB despite `Class` computing to `"{year} Graduate"` (a pre-existing data-quality issue, out of scope to fix here) got an *empty* `data-class` attribute, since `ClassFilterValue` only mapped Fr/So/Jr/Sr. `filters.js` treats an empty data attribute as "no opinion, matches every filter value" — so this one row appeared under every specific Class chip (Fr *and* So *and* Jr *and* Sr), not just "All". Fixed by giving the fallback case a real, deliberately non-matching sentinel (`"other"`) instead of an empty string.

Caught via a real dev-server run against a copy of the production data (80 real athletes), not synthetic fixtures — confirms this class of bug (script load order, CSS layout quirks, filter edge cases) keeps surfacing only when the page is actually loaded and interacted with, consistent with [C40]/[C41]'s pattern for the career chart.

**Design call, not a bug:** #21's own acceptance criteria said "no tint in a table mixing events" for the *event-page* mark-cell tint, reasoning that mixed-event tables can't meaningfully compare tinted marks by magnitude. #23's mockup-confirmed spec explicitly calls for a tinted Best Mark on the roster anyway, which *is* a table mixing events. Resolved in favor of #23 (the newer, explicitly mockup-reviewed spec) — the tint here is read per-row as a percentile signal, not as a cross-row magnitude comparison, so the two aren't actually in tension despite the surface-level wording clash. `.pct-tint-td`'s forced-colors CSS rule was also promoted from a page-local `<style>` block on `Leaderboard/Details.cshtml` into the shared `input.css`, since it's now used on two pages.

**Not done / deferred:** the mobile card intentionally drops the Top Event label as a separate line and the Season Trend sparkline entirely (folded into a small sub-line under the mark instead) — the issue's own mobile section only described name/class/mark/percentile/bar, not all six desktop columns fitting onto a card.

**Key files:**
- `CloverleafTrack.Models/Helpers/AthleteEventParticipation.cs` — `SeasonStartDate`, `SeasonName`
- `CloverleafTrack.Models/Helpers/SeasonMarker.cs` (NEW) — Dapper multi-mapping target only
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs` — `GetAllWithPerformancesAsync` joins `Meets`/`Seasons`, 4-type multi-map
- `CloverleafTrack.ViewModels/EventParticipationViewModel.cs` — `Percentile`, `IsFieldEvent`
- `CloverleafTrack.ViewModels/SparklinePointViewModel.cs` (NEW)
- `CloverleafTrack.ViewModels/AthleteViewModel.cs` — `TopEvent`, `SeasonTrend`, `SparklinePolyline`
- `CloverleafTrack.Services/AthleteService.cs` — `GetFlatActiveAthletesAsync` rebuilt; `BestPerformance`, `BuildSeasonTrend`, `SeasonEventBest` helpers
- `CloverleafTrack.Web/Views/Shared/_RosterActiveAthletesList.cshtml` — full rebuild
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml` — page-local `.pct-tint-td` style removed (now shared)
- `CloverleafTrack.Web/wwwroot/css/input.css` — `.pct-tint-td` (moved), `.pct-bar-*`, `.roster-table` mobile flex rules
- `CloverleafTrack.Web/wwwroot/js/filters.js` — substring match for a `name` key, `applyFilters` exposed on `window.CtfFilters`
- `CloverleafTrack.Web/wwwroot/js/sortable-tables.js` — `data-sort-default` mechanism
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` — 3 new tests (Top Event percentile selection + fallback, season trend ordering/best-per-season)

---

---

### [C43] Meets.MeetLevel (Varsity/JV) + MeetScore Design Findings (groundwork — MeetScore itself not yet built)

**What changed:** Added `MeetLevel` enum (`Varsity=1`, `JV=2`), `Meet.MeetLevel` (default Varsity), a `Meets.MeetLevel SMALLINT NOT NULL DEFAULT 1` column, and a Meet Level select on the admin Create/Edit meet form. The public `MeetRepository` queries (explicit column lists + GROUP BY) were deliberately NOT updated — add `m.MeetLevel` there when MeetScore is displayed.

**Why:** MeetScore (a per-meet quality metric built on `PerformancePercentiles`) must treat JV meets (Smithie Frosh-Soph Invitational, Stars of Track and Field) differently: percentiles compare against all-time *varsity* history, so JV meets always look weak on Peak/Depth even when the team performed well. Plan: JV meets are scored on Improvement only.

**Deploy order:** run `docs/migration_meet_level.sql` against the DB BEFORE deploying — `AdminMeetRepository` INSERT/UPDATE now write `MeetLevel` and will fail without the column. The script also backfills the known JV meets by name.

**MeetScore design decisions reached in discussion (nothing implemented yet):**
- Three components: **Peak** (mean of top-5 percentiles), **Depth** (mean of the top-40 athlete-event bests — NOT a mean of all marks, which punished 190-athlete dual meets: Depth ~37 vs ~85 at a conference meet), **Improvement** (PRs vs. expected PRs). Draft weights 45/25/30; calibrate against meets the owner ranks by gut.
- **Data completeness:** only seasons from 2020-21 onward (Seasons Id 38,31,16,4,27,47) have full results. Older meets are scoring-performances only, so Peak/Depth skew high and "PRs" are inflated/unreliable (prior marks missing). Pre-2020 meets are to be scored on **Peak only**. `Seasons.Status = Complete` is NOT a reliable signal — 2003-04 is marked Complete but has ~28 performances/meet (scoring-only data).
- **Improvement must be baseline-adjusted by number of prior meets** for that athlete-event: empirical PR rate by prior-meet count (complete era) is 61% (1 prior), 40%, 37%, 32%, 29%, 24% (6+). Unadjusted PR rate wildly favors early-season dual meets. Count only priors from the complete-data era. Score = `50 + 100*(PRs - Expected)/(Eligible + 10)` (50 = par). A PR is per athlete-event: any mark at the meet beating the athlete's best from strictly earlier meets.
- Don't use `PersonalBest`/`SeasonBest` flags for this — they mark only the current best, not "was a PR at the time". Compute from dates.
- School records *set at the time* (beat every earlier mark in the event) can be computed in C#, as `LeaderboardService` does; `Leaderboards.Rank = 1` only shows *current* records. The two MAC championships the owner calls exceptional (2024, 2025) set zero records — they stand out on Depth, so a record bonus alone won't capture them.

**Watch out:** Relays are excluded from Improvement in the prototype. `Meets.MeetLevel` has no effect on anything yet. First-complete-season (2020-21) PR rates are inflated by COVID-lost 2020 and missing priors.

---

### [C44] Canonical / og:url / og:image / sitemap URLs Fixed for the Static Export; Lowercase Routing

**What changed:** All absolute SEO URLs now come from a new `ISeoUrlBuilder` (`Web/Utilities/SeoUrlBuilder.cs`), configured by `Seo:CanonicalBaseUrl` (`https://cloverleaftrack.com`) and `Seo:HtmlExtension` (`true`) in `appsettings.json`. Used by `_SeoMetadata.cshtml` (canonical, og:url, og:image, twitter:image, BreadcrumbList), the Home/Roster/Meets detail views' JSON-LD, and `SitemapController`. `SeoHelper.BuildBreadcrumbJsonLd` gained an overload taking `Func<string,string> pageUrl`. Routing now sets `LowercaseUrls = true`. Supersedes the "baseUrl from Request.Host" approach described in the SEO section of [C28].

**Why (found by inspecting the live site and the export on svp01docker01):**
1. `<link rel="canonical">` on the live site was the *relative* `href="index.html"`. The crawl host (`ctf.morriscloud.com`) was embedded in the canonical, and `wget --convert-links` rewrites links to downloaded pages into relative links. Canonicals now point at `cloverleaftrack.com`, a domain outside `--domains ctf.morriscloud.com`, so wget neither downloads nor rewrites them (verified with a local wget crawl using the production flags).
2. `og:url`/`og:image`/JSON-LD pointed at `http://ctf.morriscloud.com/...` and lacked `.html`. The static site only resolves `<path>.html` (`/roster/x` 404s on S3), so `HtmlExtension` appends it to page URLs (never to assets, never to `/`).
3. `Meets.html` duplicated `meets.html`: `asp-controller="Meets" asp-action="Index"` (Meets/Details.cshtml) generated `/Meets`, which wget saved as a second file. `LowercaseUrls = true` fixes this class of bug for every tag-helper link.

**Watch out:**
- Any new public absolute URL must use `SeoUrls.PageUrl(path)` / `AssetUrl(path)`; do not reintroduce `Request.Scheme/Host`.
- `og:image` (`/img/hero-home.jpg`) still 404s on production unless the `ctf` export script fetches it: nothing in the HTML links to it, so wget never requests it (same reason `/sitemap.xml` was missing). Requires a change to `~/ctf` on svp01docker01.
- Production `sitemap.xml` is generated by `~/ctf` from the exported files, not by `SitemapController` (which now also emits `.html` URLs, so the two agree).
- No favicon `<link>` exists in `_Layout.cshtml`; browsers request `/favicon.ico`, which is not exported.
- Season pages (`/seasons/...`) are intentionally not part of the public site (owner's decision); they are absent from the export on purpose.

**Key files:** `Web/Utilities/SeoUrlBuilder.cs`, `Web/Utilities/SeoHelper.cs`, `Web/Views/Shared/_SeoMetadata.cshtml`, `Web/Views/_ViewImports.cshtml`, `Web/Controllers/SitemapController.cs`, `Web/Program.cs`, `Web/appsettings.json`, `Tests/Unit/Utilities/SeoUrlBuilderTests.cs`.

---

### [C45] MeetScore — Implemented (builds on [C43])

**What changed:** Per-meet 0-100 quality score, computed on read. `MeetScoreCalculator` (Services, pure static) + `MeetScoreMark`/`MeetScoreResult`/`MeetScoreBasis` (Models) + `IMeetRepository.GetMeetScoreMarksAsync()` (single query over all Performances joined to Meets/Seasons/Events/PerformancePercentiles/EventStatistics) + `MeetScoreViewModel`. Hero card on meet details (score + Peak/Depth/Improvement), badge on the meets index (`*` marks partial-basis scores). Formula, weights (45/25/30), era rule (season start >= 2020-12-01, derived from data not ids), JV rule and expected-PR-rate method are exactly as decided in [C43].

**Decisions made here:**
- **No school-record bonus.** A record is already a ~100th-percentile mark so Peak captures it; records are rare, so a bonus would mostly reorder state-level meets; and the owner's exceptional MACs (2024, 2025) set none. Records still show in the hero strip. Revisit only if gut-ranking disagrees.
- **Depth for meets with < 40 athlete-events averages what exists** (no zero padding). This reproduced the prototype's numbers (2024 MAC 86.1 vs prototype 86.2; 2025 MAC 81.1 = 81.1).
- **Legacy beats JV:** a pre-era meet is Peak-only regardless of level (no Improvement data exists for it).
- PR comparison is by meet *date* (strictly earlier); same-day meets don't count as priors for each other. Expected-rate buckets fall back to the overall rate if a bucket is empty. A meet with no eligible athlete-events scores Improvement 50 (par).
- **Minimum-size guard (added in review, before merge):** no score unless a Full meet has >= 15 athlete-event bests, a Peak-only meet >= 5 scorable marks, and a JV meet >= 10 eligible athlete-events (previously JV with no eligible athlete-events was shown as "par" 50). 119 of 307 meets have < 15 athlete-events; the original build scored them all, so 4-athlete state/regional meets (Peak/Depth averaged over 4 elite marks) took the top full-basis spots (2024 State 87.6, 2022 Regional 87.4). The calibration prototype had a `>= 15` filter, which is why it never showed them; the brief omitted it.
- All meets are always scored together: the index and details pages both load every performance (~13k rows) per request. Fine for the nightly wget crawl; if it ever becomes slow, cache the `Calculate` result.

**Calibration (production data, Oct 2026, 118 full-basis meets):** top: 2024 OHSAA D-I State 87.6, 2022 D-I Regional 87.4, 2023 State 87.2, **2024 MAC 86.1 (#4)**, ... **2023 MAC 85.2 (#7)**, 2022 MAC 83.4 (#12), 2021 MAC 81.1 (#19), **2025 MAC 81.1 (#20)**, 2026 MAC 78.2 (#32). The earlier "#1/#2/#10" ranks came from a prototype that did not rank state/regional/district meets above them; relative MAC order is unchanged. Early-season double duals rank high on Improvement (2025 Norton-Springfield 84.0, 2024 Woodridge 84.0). **After the minimum-size guard** (198 meets scored, 84 of them full-basis): 2024 MAC 86 (#1), 2023 MAC 85 (#2), 2025 Norton-Springfield double dual 84 (#3), 2024 Woodridge dual 84 (#4), 2024 D-I District 83 (#5), 2022 MAC 83 (#6), **2025 MAC 81 (#8)**, 2026 Field-Coventry double dual 77 (#26). State and regional meets (4-8 athlete-events) are now unscored. Early-season double duals still outrank the 2025 MAC on Improvement; weights are unconfirmed against the owner's gut ranking. JV: 2024 Stars 52.4, 2023 Stars 47.0, 2021 Smithie 36.4.

**Watch out:** `Meet.PRCount` (PersonalBest flag count) and MeetScore's PRs differ by design — the latter is computed from dates. Percentiles are as of the last `sp_RebuildLeaderboards`, so scores drift slightly as new marks enter the all-time population. Relays count in Peak/Depth but not Improvement.

**Key files:** `CloverleafTrack.Services/MeetScoreCalculator.cs`, `MeetService.cs`, `CloverleafTrack.DataAccess/Repositories/MeetRepository.cs`, `CloverleafTrack.Models/MeetScore*.cs`, `Views/Meets/Details.cshtml`, `Views/Shared/_MeetListItem.cshtml`, `CloverleafTrack.Tests/Unit/Services/MeetScoreCalculatorTests.cs`.

---

## [C46] MeetScore UI redesign — hero card with bars, plain-CSS component classes

**What changed:** The MeetScore hero card on `Views/Meets/Details.cshtml` is now a big tier-colored score on the left and three labeled 0-100 bars (Peak/Depth/Improvement, each with a short hint) on the right, with a one-line footnote. The list badge in `_MeetListItem.cshtml` uses `.meetscore-badge` + a tier class. `MeetScoreViewModel.BadgeClasses` now returns `meetscore-badge-{gold,green,blue,gray}`; new `AccentColor` (hex) drives the score number and bar fill.

**Why:** The first UI used Tailwind utilities that were never compiled into the checked-in `site.css` (`text-5xl`, `grid-cols-3`, `dark:bg-amber-900/40`, ...), so the card rendered as a broken stack and the badge was unstyled. `site.css` is committed and only regenerated by `pnpm run dev/prod`.

**Key files:** `Views/Meets/Details.cshtml`, `Views/Shared/_MeetListItem.cshtml`, `CloverleafTrack.ViewModels/Meets/MeetScoreViewModel.cs`, `wwwroot/css/input.css`, `wwwroot/css/site.css`.

**Watch out:** The `.meetscore-*` rules are plain CSS (not `@apply`), appended identically to `input.css` and `site.css`, so a Tailwind rebuild keeps them. Any *new* utility class used in a view needs a Tailwind rebuild before it exists in `site.css` — verify in the browser, don't assume. The `ctf-web` launch config now sets `ASPNETCORE_ENVIRONMENT=Development`; under Production, static assets aren't mapped and pages render unstyled.

---

## [C47] MeetScore hero card replaced by a full-width metric strip (supersedes the card in [C46])

**What changed:** Removed the large MeetScore card. `Views/Meets/Details.cshtml` now renders a second `.metric-strip` directly under the Performances/PRs/Records/Top-10 strip: MeetScore (tier-colored) | Peak | Depth | Improvement. Sub-score explanations moved to `title` tooltips; the partial-basis (`*`) explanation stays as a one-line note beneath. The `.meetscore-card`/hero/bar CSS was deleted; only the `.meetscore-badge*` list-badge CSS remains.

**Why:** The owner wanted MeetScore to look like the other stat badges, not a giant block. Reusing `metric-strip`/`metric-cell` means no new CSS.

**Key files:** `Views/Meets/Details.cshtml`, `wwwroot/css/input.css`, `wwwroot/css/site.css`.

**Watch out:** The strip is `grid-cols-4` by design, so it must keep exactly four cells.

---

## [C48] MeetScore strip: Peak/Depth/Improvement shown as percentile bars

**What changed:** In the MeetScore strip ([C47]) the three sub-score cells now show the number plus a horizontal `.pct-bar-track`/`.pct-bar-fill`/`.pct-bar-median-tick` bar (80% of cell width), filled with `PercentileHelper.GetFillColor` so colors match the roster and event pages.

**Watch out:** Improvement is not a percentile (it is a 0-100 PR-vs-expected index centered on 50), but it reuses the same percentile palette and median tick for visual consistency — the tick at 50 happens to mean "met expectations" there.

---

## [C49] MeetScore strip: vertically centered cell + info modal

**What changed:** The MeetScore cell (`.meetscore-cell`) is vertically centered in the strip (it has no bar, so it floated at the top). Its label is now a button ("MeetScore ⓘ") that opens a native `<dialog id="meetscore-info">` explaining Peak/Depth/Improvement, their weights, and the `*` partial-basis cases. Closes via ×, Esc, or backdrop click (inline `onclick`, no JS file).

**Why:** Readers had no way to learn what the score meant. Native `<dialog>` avoids a new JS dependency, which suits the static export.

**Watch out:** The modal copy hardcodes the weights (45/25/30), counts (top 5 / top 40) and the 2020-21 cutoff — update it if `MeetScoreCalculator` constants change. Styles are plain CSS (`.meetscore-dialog*`) in `input.css` and `site.css`, no Tailwind rebuild needed.

---

## [C50] MeetScore info modal shows the formulas and this meet's worked numbers

**What changed:** The modal ([C49]) has a "The math" section (Score, Peak, Depth, Improvement formulas, plus how Expected PRs is derived) and a "This meet" block that plugs the meet's real values in, branching on `MeetScoreBasis` (Full shows both lines; PeakOnly/ImprovementOnly show Score = Peak / Improvement with no weighting).

**Why:** Percentages alone didn't let readers reproduce a score.

**Watch out:** `MeetService` now rounds `MeetScoreViewModel.ExpectedPRs` to 2 decimals (was 1) — at 1 decimal the displayed Improvement arithmetic didn't match the displayed result (e.g. 50+100×(10−10.3)/50 = 49.4 vs the true 49.5). Formulas in the modal are hardcoded copy; keep in sync with `MeetScoreCalculator` ([C49] note applies).

### [C49] Search Hotkey Hint Is Platform-Aware

**What changed:** The desktop search input's placeholder no longer hard-codes "(⌘K)". `search.js` sets it at load: `⌘K` on Mac/iOS, `Ctrl+K` elsewhere, and no hint on hover-less (touch-only) devices.
**Why:** ⌘ is meaningless on Windows/Linux. The keydown handler already accepted both `ctrlKey` and `metaKey`, so only the label was wrong.
**Key files:** `Views/Shared/_Layout.cshtml`, `wwwroot/js/search.js`.
**Watch out:** The static export's HTML has the plain "Search…" placeholder; the hint appears only after JS runs (platform can't be known server-side).

**[C49] addendum:** Removed the admin layout footer hint "Press Ctrl+/ for keyboard shortcuts" — no handler for Ctrl+/ exists anywhere, so it advertised a nonexistent feature.

### [C46] Footer Build Version Removed

**What changed:** The footer no longer renders `· v<commit sha>` after "Made with ♥ by Coach Tony". The `BuildMetadataHelper` call was dropped from `Views/Shared/_Layout.cshtml`.

**Why:** Owner request — the version identifier isn't wanted on the public site (see [C27] and its follow-up fix for the history of the `vunknown` footer bug).

**Watch out:** `BuildMetadataHelper` and the `.csproj` `SourceRevisionId`/`InformationalVersion` properties are left in place, now unused by any view. Remove them in a separate cleanup if desired.

**Key files:** `CloverleafTrack.Web/Views/Shared/_Layout.cshtml`.

### [C47] BRAIN.md Demoted to a Grep-On-Demand Archive; Active Gotchas Added to CLAUDE.md

**What changed:** CLAUDE.md no longer tells sessions to read this file at start. It now has an **Active Gotchas** section (~30 one-line traps, each citing the `[C#]` entry here) and says to grep BRAIN.md for the area being touched. This file's header was updated to match. No entries were edited or removed.

**Why:** BRAIN.md reached ~1,900 lines; reading it every session cost far more than it returned, and superseded entries (e.g. the `vunknown` footer saga, [C27]/[C34]/[C46]) read as current behavior. Memory was rejected as the home for this knowledge: it isn't checked in, so it wouldn't reach other machines or contributors.

**Watch out:** Active Gotchas is a hand-curated digest — when a change adds a lasting trap, add a line there as well as logging here; when a gotcha stops being true, delete it from CLAUDE.md (history stays here). Gotchas were compiled from the "Watch out" blocks of C2–C46; entries with only one-time context were deliberately left out.

**Key files:** `CLAUDE.md`, `BRAIN.md`.

### [C50] AI Meet Summaries — Admin Review Slice (meets only, not yet public)

**What changed:** New `MeetSummaries` table (`docs/migration_meet_summaries.sql`, also appended to `docs/schema.sql`; one row per meet, `Status` Draft/Approved, `InputHash`, `Model`). `MeetSummaryPrompt` (Services) builds a JSON fact sheet from `MeetDetailsViewModel` and holds the system prompt; `MeetSummaryService` regenerates only when the fact-sheet hash changes (or `force`); `ClaudeSummaryTextGenerator` calls the Anthropic Messages API over plain `HttpClient` behind `ISummaryTextGenerator`. Admin review page at `/Admin/MeetSummaries` (per-season list, public-page/edit-meet/entries links, generate/regenerate/approve/edit/delete, "generate missing drafts"). Nothing public reads summaries yet.

**Why:** Owner wants AI-written recaps, reviewed by a human before publishing. Generation is offline/admin-triggered, not per-request, because the public site is a static export ([C44]).

**Decisions:** The model sees only the fact sheet, using the corrected flags (school record = `AllTimeRank == 1`; relay rows never get `pr`; `allTimeRank` only if <= 10) so stale `SchoolRecord`/relay `PersonalBest` flags cannot leak into text. The prompt forbids quoting MeetScore. Editing text or regenerating resets to Draft; only Approved should ever render publicly. API key: `Anthropic:ApiKey` (user-secrets) or `ANTHROPIC_API_KEY`; model `Anthropic:Model`, default `claude-opus-5-5` (owner decision, 2026-10-16: use Opus for all summaries, after a blind Sonnet-vs-Opus comparison on 7 meets where Opus had no unsupported claims and Sonnet had one).

**Medley-relay bug found and fixed along the way:** `MeetService.BuildOrderedEventGroups` classified relays by event *name* ("Relay"/"4x"), so medley relays ("800M Sprint Medley", "4K Distance Medley", "1600M/2400M Intermediate Medley") fell into no bucket and were silently dropped from the public meet page (e.g. 2026 Fairview Warrior Relays lost 8 of 47 performances, incl. 4 school records). Relay-ness now also keys off `EventType` (RunningRelay/FieldRelay/JumpRelay/ThrowsRelay). Meet-page `TotalPerformances`/`TotalSchoolRecords` come from raw rows, so a mismatch with listed results is the tell for this class of bug.

**Manual authoring path:** `GET /Admin/MeetSummaries/FactSheet?meetId=` returns the exact fact-sheet JSON and `POST /Admin/MeetSummaries/Import` saves externally written text as a Draft (hash computed from the current fact sheet) — used to write the 2025-2026 drafts from a Claude Code session without an API key. Drafts written that way carry Model "claude-sonnet-5-5 (written in Claude Code)".

**Linking:** the review page renders summaries through `SummaryLinkifier` (Services, pure) via `MeetSummaryLinkService`: athlete names -> `/roster/{slug}`, event names -> `/events/{eventkey}`. Stored text stays plain; linking happens at render, so the same code can serve the public page later. Athletes/events come from that meet's own performances (`MeetPerformanceDto.EventKey` was added for this). A bare surname links only when it is unique among the meet's athletes ("Gray" with Adam and Ethan stays plain). Events exist per gender, so each mention is resolved by the closer of (a nearest "boys"/"girls"/"mixed" earlier in the sentence) and (the nearest named athlete's gender), else the only gender the meet has that event for, else left unlinked. Bare relay names ("4x100M, 4x200M and 4x400M relays") link only when followed by a comma, "and", or "relay(s)". Open question for the public page: link `target`/`rel` (the admin view sets new-tab via script).

**Current data (prompt Version 2):** all 21 2025-2026 meet summaries are Opus-written Drafts (Model label "claude-opus (written by subagent in Claude Code)"), imported via the Import endpoint rather than the API. **Prompt v2 (event names):** `MeetSummaryPrompt` now requires events to be named exactly as in the fact sheet ("800M", "100M Hurdles", "4x400M Relay") with "boys"/"girls" before the first event in a clause, so `SummaryLinkifier` can link them. All 21 drafts were regenerated with Opus under Version "2": 156 event links and every athlete mention link on the review page, no unlinked mentions, event gender spot-verified. Without this rule models write "800 meters"/"100 hurdles" and event links silently fail to attach.

**Watch out:** `MeetSummaryPrompt.Version` must be bumped when the prompt or fact-sheet shape changes. `System.Text.Json` `DefaultIgnoreCondition` does NOT drop null *dictionary* values — the fact sheet strips nulls explicitly. No staleness indicator on the review page yet (hash is only checked at generation). Run the migration before opening the page.

**Key files:** `CloverleafTrack.Services/MeetSummary*.cs`, `ClaudeSummaryTextGenerator.cs`, `DataAccess/Repositories/MeetSummaryRepository.cs`, `Web/Areas/Admin/Controllers/MeetSummariesController.cs`, `Views/MeetSummaries/Index.cshtml`, `Tests/Unit/Services/MeetSummaryTests.cs`.

### [C51] Approved Meet Summaries Shown Publicly; Generation Moves to Chat Skills

**What changed:** `MeetService.GetMeetDetailsAsync` (via an optional `IMeetSummaryRepository`) puts an **Approved** summary on `MeetDetailsViewModel.SummaryText`/`SummaryHtml`; `Views/Meets/Details.cshtml` renders it under the header (before the metric strip) and uses it as the `<meta description>`/Open Graph text. Links reuse `SummaryLinkifier` with a `linkClass` option (`text-blue-600 dark:text-blue-400 hover:underline`, classes already in `site.css`, so no Tailwind rebuild). The admin page lost its Generate / Regenerate / Generate-missing buttons and their controller actions; new `GET /Admin/MeetSummaries/List?seasonName=` returns a season's meets with summary state. Two skills were added in `.claude/skills/`: `generate-meet-summary` (steps, rules, `scripts/summaries.py` with list/fetch/validate/import/audit, `references/subagent-prompt.md`) and `generate-season-summaries` (iterates a season in parallel Opus batches).

**Why:** Owner wants to generate from chat (Opus subagents, validated, imported as Drafts), review in admin, and have only approved text public. A Draft must never reach the public page: enforced in `MeetService` and covered by `MeetServiceSummaryTests`.

**Watch out:** Public text is only as current as the last import; editing a meet's performances does not invalidate an approved summary (no staleness detection). Re-importing resets a summary to Draft, which silently removes it from the public page until re-approved; the skills require an explicit yes before overwriting Approved ones. `MeetSummaryService.GenerateAsync`, `ClaudeSummaryTextGenerator` and `ClaudeOptions` are now unused dead code (kept pending an owner decision). The summary links are hard-coded `/roster/{slug}` and `/events/{key}` paths: they match what `asp-` tag helpers generate with `LowercaseUrls`, but verify the static export crawl follows them. Athlete summaries are not built yet.

**Key files:** `CloverleafTrack.Services/MeetService.cs`, `MeetSummaryLinkService.cs`, `SummaryLinkifier.cs`, `Views/Meets/Details.cshtml`, `Areas/Admin/Controllers/MeetSummariesController.cs`, `.claude/skills/generate-*/`, `Tests/Unit/Services/MeetServiceSummaryTests.cs`.

### [C52] Athlete Summaries — Admin Review + Class of 2026 Drafts (not public yet)

**What changed:** New `AthleteSummaries` table (`docs/migration_athlete_summaries.sql`, appended to `schema.sql`; one row per athlete, Draft/Approved, same shape as `MeetSummaries`). `AthleteSummaryPrompt` builds a JSON fact sheet from `AthleteDetailsViewModel` (personal records per event, career arc first→best, seasons with class, school-record/top-10 highlights) and holds the system prompt; `AthleteSummaryService` (FactSheet / SaveManualDraft); `AthleteSummaryLinkService`. Admin `/Admin/AthleteSummaries` (graduation-class picker, default = last graduated class; review links, approve/edit/delete; `List`, `FactSheet`, `Import` endpoints). `SummaryLinkifier` gained meet-name links, an `Environment` on `LinkableEvent` (indoor/outdoor events share names), and a `fallbackGender`. `FactSheetJson` is the shared serializer/hasher for meet and athlete sheets. Skill `generate-athlete-summary` (+ `scripts/athlete_summaries.py`). All 33 members of the class of 2026 have Opus-written Drafts.

**Decisions:** No pronouns (the data has only a boys/girls team, not how anyone refers to themselves); athletic results only (no college, future, family, injuries). **PR counts are omitted:** `PersonalBest` flags exist only for 2025-26 (Gavin Forbes: 4 flagged of 69 performances, all this season), so `TotalPRs` and per-season `PRCount` undercount and a writer would state a wrong number. Athlete writers must say "indoor"/"outdoor" before events that exist in both so they link. The validator also checks class/season claims and mark→meet attribution (a swapped meet was the likeliest silent error).

**Watch out / data findings:** (1) `TotalPRs` on the public athlete page has the same undercount. (2) Two relays at the 2024 New London Dual are recorded as 4x400M Relay with ~8:02-8:03 times (Alex Randall's team 8:02.90, Dominic Nau's 8:03.20), almost certainly 4x800M entries; writers correctly skipped them, but they sit on the 4x400M leaderboard. (3) Meet fact sheets list tied marks in SQL's arbitrary order, so their input hash is not stable across runs; irrelevant today (no staleness detection) but fix the ORDER BY before relying on hashes. (4) Athlete slug is `first-last` and is matched against all athletes (duplicate names would collide).

**Key files:** `CloverleafTrack.Services/AthleteSummary*.cs`, `SummaryLinkifier.cs`, `FactSheetJson.cs`, `Areas/Admin/Controllers/AthleteSummariesController.cs`, `Views/AthleteSummaries/Index.cshtml`, `.claude/skills/generate-athlete-summary/`, `Tests/Unit/Services/AthleteSummaryTests.cs`.

### [C53] Athlete Summaries Limited to Athletes Who Competed Senior Year; Exhibition 4x4 Relays Are Real (corrects [C52])

**What changed:** A class's athlete summaries cover only athletes with at least one individual or relay performance in their senior school year (Aug 1 of graduation year - 1 to Jul 31, `ClassYearCalculator.GetSeniorSchoolYear`, the same boundary as the class labels). `/Admin/AthleteSummaries` hides the rest by default (header shows "N hidden: show"; `?includeAll=true` reveals them, badged "No senior season"); `GET .../List` returns eligible athletes plus an `excluded` name list (`includeAll=true` to override); new `IAdminAthleteRepository.GetAthleteIdsWithPerformancesBetweenAsync`. The `generate-athlete-summary` skill has a "Who gets a summary" rule. Of the 33-athlete class of 2026, 11 qualified; the 22 Drafts for the others were deleted (via the admin Delete endpoint).

**Correction to [C52]:** the "two relays look mis-entered" finding was wrong. 4x400M Relays of ~8:00 (Alex Randall 8:02.90, Dominic Nau 8:03.20 at the 2024 New London Dual) are real: the team runs "4x4" exhibition relays for fun. They are valid results, not data errors; writers may skip or mention them.

**Why:** Owner decision: a senior-class recap is for athletes who finished the career as seniors, however many earlier years they ran (1 to 4).

**Watch out:** "Competed senior year" is judged from recorded performances only, so an athlete whose senior results were never entered is treated as not having competed. The hidden count on the admin page makes that visible. `Import` does not enforce eligibility (the skill does); the filter applies to list/review, not to the fact sheet endpoint.

**Key files:** `CloverleafTrack.Services/ClassYearCalculator.cs`, `DataAccess/Repositories/AdminAthleteRepository.cs`, `Areas/Admin/Controllers/AthleteSummariesController.cs`, `Views/AthleteSummaries/Index.cshtml`, `.claude/skills/generate-athlete-summary/`.

### [C54] Approved Athlete Summaries Shown on Athlete Pages

**What changed:** `AthleteService.GetAthleteDetailsAsync` (now a thin wrapper over the private `BuildAthleteDetailsAsync`) attaches an **Approved** summary to `AthleteDetailsViewModel.SummaryText`/`SummaryHtml`; `Views/Roster/Details.cshtml` renders it under the athlete's name, above the stat strip, and uses it as the `<meta description>`/Open Graph text. Linking moved into `AthleteSummaryLinker` (depends only on `IAdminEventRepository`, so `AthleteService` can use it without a DI cycle); `AthleteSummaryLinkService` (admin review) now delegates to it. Same markup and link classes as the meet pages.

**Why:** Owner wanted athlete summaries visible on athlete pages "just like the meet summaries". Draft text is never shown (enforced in `AthleteService`, covered by `AthleteServiceSummaryTests`).

**Watch out:** Pages only change after the next image deploy and `ctf` export. The athlete page's own meta description used to quote `TotalPRs`, which undercounts older careers ([C52]); with an approved summary it is replaced by the summary text. `GetAthleteDetailsAsync` is also used by the admin fact-sheet/link paths, so those now do one extra summary lookup (cheap).

**Key files:** `CloverleafTrack.Services/AthleteService.cs`, `AthleteSummaryLinker.cs`, `AthleteSummaryLinkService.cs`, `Views/Roster/Details.cshtml`, `ViewModels/Athletes/AthleteDetailsViewModel.cs`, `Tests/Unit/Services/AthleteServiceSummaryTests.cs`.

### [C55] Performance Rounds — Prelim / Semifinal / Final Times (sibling table, not a Round column)

**What changed:** New additive table `PerformanceRounds` (PerformanceId, Round 1=Prelim/2=Semifinal/3=Final, TimeSeconds; `ON DELETE CASCADE`, unique per (PerformanceId, Round)) for running and running-relay performances that went through rounds. `Performances.TimeSeconds` stays the single best (min) round time. Meet and athlete pages show a `Prelim 11.08 → Final 10.94` chip chain (`Views/Shared/_RoundStrip.cshtml`) only when an athlete ran **two or more** rounds; an athlete who did not advance shows only their mark. Field-event prelim/final throws and jumps are just more attempts in the existing `PerformanceAttempts` series (`AttemptNumber` has no upper CHECK) — no new UI. Admin Create/Edit have optional Prelim/Semi/Final inputs for running events (`PerformancesController.ParseRoundInputs` → `IAdminPerformanceRepository.SaveRoundsAsync`).

**Why:** The first design was a `Round` column on `Performances` (multiple rows per athlete/meet/event) with `sp_RebuildLeaderboards` changes. Rejected: ~43 queries read `Performances` (home stats, PR/SB counts, meet athlete counts, scoring/placings via `MeetPlacings.PerformanceId`, MeetScore, percentiles/`EventStatistics` which count every row) and each would need a "best per athlete-event-meet" collapse; a missed one silently double-counts. The sibling table mirrors [C32] so none of that changes: one counting row per athlete-event-meet, rounds are display-only child data.

**Watch out:**
- `PerformanceRounds` is display data; never read it for PRs, ranks, or percentiles. The counting mark is `Performances.TimeSeconds` (= min of rounds, recomputed in `SaveRoundsAsync`).
- `SaveRoundsAsync` is replace-all (like `SaveAttemptSeriesAsync`). Passing an empty list deletes rounds and leaves `TimeSeconds` alone; the admin Edit form clears rounds when fewer than two are filled in.
- Place/points (`MeetPlacings`) still attach to the single Performance row; there is no per-round placing.
- Run `docs/migration_performance_rounds.sql` before deploying code that reads it (`MeetService`/`AthleteService` query the table). T-SQL not run against a real SQL Server in the dev sandbox.
- Best-round ties display the later round as best.

**Key files:** `docs/migration_performance_rounds.sql`, `docs/schema.sql`, `Models/PerformanceRound.cs`, `Models/Enums/RoundType.cs`, `DataAccess/Repositories/PerformanceRoundRepository.cs`, `AdminPerformanceRepository.cs`, `Services/PerformanceRoundSeriesBuilder.cs`, `ViewModels/Shared/PerformanceRoundSeriesViewModel.cs`, `Views/Shared/_RoundStrip.cshtml`, `Views/Meets/Details.cshtml`, `Views/Roster/Details.cshtml`, `docs/mockups/multi-performance-rounds.html`.

### [C56] Attempt Strip No Longer Shows "N valid · avg · spread" (amends [C32] display)

**What changed:** `Views/Shared/_AttemptStrip.cshtml` renders only the attempt boxes; the trailing "5 valid · avg … · spread …" summary was removed at the owner's request (seen while reviewing [C55] locally). `PerformanceAttemptSeriesViewModel.ValidAttemptCount/AverageValidInches/SpreadInches` still exist (and are unit tested) but are now unused by views.

**Watch out:** Razor views are precompiled into the running app, so a running dev server keeps the old markup until restarted.

**Key files:** `Views/Shared/_AttemptStrip.cshtml`.

### [C57] athletic.net Import — 2026 Outdoor Rounds & Attempt Series (reviewable SQL, not yet run on live)

**What changed:** `scripts/athleticnet/` (scraper notes + `build_rounds_sql.py`) and per-meet scraped data in `scripts/athleticnet/data/<dbMeetId>/{an.txt,db.txt}` produce `docs/imports/2026-outdoor-rounds.sql`: 20 prelim/semi→final chains (individual and relay) and 132 field attempt series across 9 of the 17 2026 outdoor meets. Applied only to a local SQL Server copy so far; **never run on `svp01sql01`**. Early Bird, Hauser, Bay, Wooster had no rounds and no recorded series; the 4 dual meets (Ravenna-Streetsboro, Norton-Springfield, Field-Coventry, New London) are not on athletic.net's calendar.

**Why:** Owner wanted full meet info (prelim vs final times, full throws series). Our DB keeps one best-mark row per athlete-event-meet ([C55]); this adds the surrounding rounds/attempts as child rows.

**Watch out:**
- Each block re-asserts the expected mark (matched by gender + event + best mark) and the script is one transaction; a mismatch aborts everything. Ties are disambiguated with `data/<id>/names.json` (athletic.net athlete id → substring of our athlete name).
- Our relay rosters are authoritative (situational replacements); athletic.net rosters differ and are never imported — relay chains are matched by event + best time only.
- Known source disagreements, left unimported: State girls 4x400 final is 3:55.68 on athletic.net vs 3:55.05 in our DB (SR); Fairview men's shot, Jake Schaefer best 36' 10" on athletic.net vs 37' 10" in our DB.
- athletic.net quirks: the API 403s without a page token; the results list is virtualized and renders only after the tab is painted; header text needs `textContent`; field-series pages only have data when the meet recorded series (many relay/invitational meets only have best marks). High jump / pole vault series are bar heights, unsupported.

**Key files:** `scripts/athleticnet/README.md`, `scripts/athleticnet/build_rounds_sql.py`, `docs/imports/2026-outdoor-rounds.sql`.

### [C58] Meet Page Layout — Round Chain and Attempt Strip Moved Under the Athlete Cell (amends [C55]/[C56])

**What changed:** On `Views/Meets/Details.cshtml` the `_AttemptStrip` and `_RoundStrip` now render under the athlete / relay names (left-aligned) instead of under the mark. The Mark column holds only the mark, so it stays narrow and the long relay names keep their width. All performance-table cells got `align-middle` so mark and result chips center against the taller athlete cell, and `px-2 sm:px-4` cell padding so the athlete column has room on phones. `_RoundStrip` now keeps each arrow attached to the chip after it (no dangling arrows), takes `ViewData["Left"]`, and shows `S`/`F` labels below `sm` and `Semi`/`Final` from `sm` up. `_AttemptStrip` boxes wrap (`flex-wrap`).

**Why:** In the two-column desktop layout the Mark column was ~90px, so the chain wrapped into "Semi 1:36.90 →" / "Final 1:36.51" and relay names squeezed to 3 lines; long-jump strips overflowed the card.

**Watch out:** The Athlete page (`Roster/Details.cshtml`) still uses the strips in the mark cell with `Compact` labels. `wwwroot/css/site.css` is a tracked, minified build: with no node toolchain available I hand-appended the Tailwind rules for `.gap-x-1`, `.align-middle` and `.sm:px-4` exactly as Tailwind emits them; a future `pnpm run dev` will regenerate them from the markup, so nothing to undo. Razor views are precompiled, so restart a running dev server after editing them.

**Key files:** `Views/Meets/Details.cshtml`, `Views/Shared/_RoundStrip.cshtml`, `Views/Shared/_AttemptStrip.cshtml`, `wwwroot/css/site.css`.

### [C61] 2026 State Girls 4x400 — Wrong Final Time (Clear Fork's) and Missing Prelim

**What changed:** Performance 16487 (2026 OHSAA D3 State, girls 4x400) was stored as 3:55.05, which is Clear Fork's 2nd-place time; Cloverleaf was 3rd in 3:55.68 with a 4:02.05 prelim (athletic.net splits sum correctly for both). `docs/imports/2026-state-4x400-fix.sql` corrects `TimeSeconds` to 235.68, adds the prelim/final `PerformanceRounds`, and re-runs `sp_RebuildLeaderboards`. It stays the school record (old best 4:01.96).

**Why found:** the athletic.net import ([C57]) asserts every stored mark and refused to attach the chain ("UNMATCHED rounds … 235.68"); the mismatch was the data error, not the scrape.

**Watch out:** this is a correction of existing data, not an additive import; it was run on the live DB. Boys 4x800 at State was finals-only (8:04.00, 10th); Kayla Shook's individual 400 was prelims-only (1:00.79, 16th) — no chains for either. (Entries [C59]/[C60] live in PR 66.)

**Key files:** `docs/imports/2026-state-4x400-fix.sql`.

### [C59] Handwritten Scorer-Sheet Scans → Attempt Series for Four Older Duals

**What changed:** Four scanned PDFs of handwritten scorer sheets (`~/Downloads/scan_ggucker_*`) were matched to DB meets by the date/opponent written on the sheets: 2024-04-23 → Field-Coventry Double Dual (meet 254); 2025-04-15 → Woodridge Dual (3); 2025-04-22 → Norton-Springfield Double Dual (68); 2025-05-07 scan → New London Dual (275, sheets are dated 5/6/2025). Shot put, discus and long jump attempt series for Cloverleaf athletes were transcribed into `scripts/athleticnet/data/<meetId>/sheets.txt` and turned into `docs/imports/2024-2025-dual-attempts.sql` by `scripts/athleticnet/build_scan_sql.py`: 218 attempt series. Applied to the local SQL Server copy only.

**Why:** Owner wanted the full meet detail from the paper sheets. Track-event sheets only repeat marks already in the DB (no rounds in duals), so only field attempts add information.

**Watch out:**
- Matching is by (gender, event, DB athlete name) and every block asserts the sheet's best attempt equals the stored `Performances.DistanceInches`; a misread best skips the athlete (listed as `NOTE` lines at the end of the SQL: 7 for Woodridge — Kaiden Stone shot, Preston Stark / Dominic Nau / Dylan Wittman discus and Izzy Messmore discus disagree with the DB; Collin Bohley and Ethan Gray have no DB mark). Only the best attempt is cross-checked; the non-best attempts are read from faint pencil and are not independently verified (a few were disambiguated using the DB best, e.g. Blake Thorne 26-1, Jack Chonko 35-0).
- Blank inches on a sheet ("35-") are 0 inches; a lone "—" attempt is stored as not taken; series with fewer than two recorded attempts are skipped.
- High jump and pole vault sheets (bar heights with X/O) are not imported: `PerformanceAttempts` stores distances only.
- Double-dual throws sheets only have 2 attempts per athlete; the 2025 Norton-Springfield sheets use attempts 1-4 with 1 and 3 mostly fouls.
- PDF rendering without poppler: a Swift/PDFKit script (kept in the session scratchpad only) rendered pages; scans are rotated inconsistently, so check orientation per page.

**Key files:** `scripts/athleticnet/build_scan_sql.py`, `scripts/athleticnet/data/{254,3,68,275}/sheets.txt`, `docs/imports/2024-2025-dual-attempts.sql`.

### [C60] High Jump / Pole Vault Bar Ladder (`PerformanceBars`)

**What changed:** New additive table `PerformanceBars` (PerformanceId, HeightInches, Outcome; cascade delete; unique per height; CHECK on the seven valid outcomes) for individual high jump and pole vault. `Performances.DistanceInches` stays the best cleared height, so PRs, ranks, percentiles and scoring are unchanged. Display: `Views/Shared/_BarLadder.cshtml` (chip per bar, best cleared bar highlighted; renders nothing without rows) on the meet page (above the attempt/round strips, under the athlete name) and athlete page. Admin Create/Edit show a "Bars" textarea for HJ/PV events (one `4'4" XXO` per line; `BarLadderParser` validates and an empty box clears the ladder); the attempt-series toggle is hidden for these events. Import: `scripts/athleticnet/build_bars_sql.py` + `data/<meet>/bars.txt` → `docs/imports/2024-2025-dual-bars.sql` (24 ladders for the four scanned duals, applied to the local DB only; 2 of 26 skipped on a mark mismatch). Migration `docs/migration_performance_bars.sql` must run before deploying the code (services query the table).

**Why:** `PerformanceAttempts` only stores distances, which can't represent bar heights with X/O/P outcomes. Same sibling-table design as [C55].

**Watch out:**
- Normalise notation before storing: several sheets (Norton-Springfield HJ, Woodridge HJ) use the inverted notation (X = clear, O = miss; "O = Misses" is written at the bottom). The pole vault sheets use check marks for clears.
- Not imported: Kinsey Bayer and Emily Ellebruch at Norton-Springfield (sheet bests 4'6"/4'4" vs DB 4'8"/4'6" — the DB or the sheet is off; the generator's mark assertion skipped them), Payten Shetterly at Woodridge (single illegible bar), Payten's final bar at New London, athletes with no height (not in the DB).
- Pass-only and single-bar ladders are valid; a ladder with no clear never updates `DistanceInches`.
- `site.css` is hand-appended with the `.bar-chip*` rules (same text as in `input.css`) because no node toolchain is available; a Tailwind rebuild regenerates from `input.css`.

**Key files:** `Models/PerformanceBar.cs`, `DataAccess/Repositories/PerformanceBarRepository.cs`, `AdminPerformanceRepository.cs`, `Services/PerformanceBarSeriesBuilder.cs`, `Web/Utilities/BarLadderParser.cs`, `Views/Shared/_BarLadder.cshtml`, `Areas/Admin/Views/Performances/{Create,Edit}.cshtml`, `docs/migration_performance_bars.sql`, `docs/mockups/vertical-jump-bars.html`, `Tests/Unit/Models/PerformanceBarTests.cs`.

### [C63] Meets.OfficialResultsUrl — Link to Externally Hosted Official Results

**What changed:** New nullable `Meets.OfficialResultsUrl NVARCHAR(500)` (`docs/migration_meet_official_results_url.sql`, already run on live and local). Admin Create/Edit has an "Official Results URL" field (validated `[Url]`, http/https only, blank → null via `MeetsController.NormalizeUrl`; the "copy meet" action deliberately does not copy it). The public meet page shows an "Official results on <host> ↗" chip (`target=_blank rel="noopener noreferrer"`); `MeetService` only passes through http/https links, so a stored `javascript:` value can never render.

**Why:** Owner wants each meet to link to wherever its official results live (athletic.net, oh.milesplit.com, timing companies). Used by the athletic.net/MileSplit crawl to attach links to matched meets.

**Watch out:** Run the migration before deploying code that reads it (`m.*` queries and the Admin INSERT/UPDATE reference the column). One link per meet; if a meet is on both sites, prefer athletic.net (it has splits and field series) unless the owner says otherwise.

**Key files:** `Models/Meet.cs`, `ViewModels/Admin/Meets/MeetFormViewModel.cs`, `Web/Areas/Admin/Controllers/MeetsController.cs`, `AdminMeetRepository.cs`, `Services/MeetService.cs`, `Views/Meets/Details.cshtml`, `Areas/Admin/Views/Meets/Create.cshtml`.

### [C64] Crawl of athletic.net and oh.milesplit.com — Meet Matching and Official Results Links

**What changed:** `scripts/crawl/` holds the crawled meet lists (`athleticnet-meets.txt`: 143 meets from team 7859 calendars 2005-2026, indoor+outdoor; `milesplit-meets-raw.txt`: 163 meets from MileSplit team 9826, `/api/v1/teams/9826/schedules?season=&year=` JSON), `match_meets.py` (matches each crawled meet to a DB meet by environment, date ±3 days for multi-day championships, and name similarity; tuned to reject same-date different-name pairs) and the output `matches.tsv` (every crawled meet with its DB match and score, for review) plus `official-results-links.sql` (copied to `docs/imports/2026-official-results-links.sql`). It sets `Meets.OfficialResultsUrl` for **153 of 307 DB meets** (116 athletic.net, 37 MileSplit; athletic.net preferred); run on live and local. `UPDATE … WHERE OfficialResultsUrl IS NULL` so manual links are never overwritten.

**Coverage reality:** neither site lists most dual/double-dual meets, so ~154 DB meets stay unlinked; both sites have many indoor/other meets we don't track (listed as "NO DB MEET" by the matcher). athletic.net team calendars only list meets with a results link.

**Source quirks:** athletic.net's API 403s, so the pages are scraped via iframes (results list is virtualized; scroll and use `textContent`). MileSplit's `/results/<id>/raw` is one plain-text document per meet (prelim/semi sections included, relay members listed, **no field attempt series**), but its layout varies by timing software (HY-TEK `====` blocks, tab-delimited blocks, Finish Lynx, split two-line headers); older meets (≈2016-2022) often lose event names and mix seed marks into rows, so only 2023+ MileSplit round data is trustworthy without manual review. Both sites independently confirmed the State 4x400 correction ([C61]).

**Watch out:** matches are heuristic — spot-check `matches.tsv` rows with score < 1.3. MileSplit schedule year = season start year (`year=2025` is the 2025-26 season); athletic.net `outdoor2026`/`indoor2026` are the 2026 / 2025-26 seasons.

**Key files:** `scripts/crawl/match_meets.py`, `scripts/crawl/matches.tsv`, `docs/imports/2026-official-results-links.sql`.

### [C65] Multi-Year athletic.net / MileSplit Prelim → Final Imports (2005–2025)

**What changed:** The detail crawl of the 106 matched athletic.net meets (iframe scraper + parallel workers; see [C64]) and 128 matched MileSplit meets produced `scripts/crawl/an-rounds.txt` (multi-round Cloverleaf rows only, 44 athletic.net meets) and `ms-542137.txt` (2023 MAC from MileSplit, which athletic.net lacks). `scripts/crawl/prepare_round_inputs.py` maps them to DB meets via `matches.tsv` and writes `scripts/athleticnet/data/<dbMeetId>/{an.txt,db.txt}`; `build_rounds_sql.py` (now skipping field events and disambiguating ties by athlete name) generated `docs/imports/2005-2025-rounds.sql`: **209 prelim/semi/final chains across 45 DB meets**, applied to live and local (460 round rows live total with the 2026 imports). Every chain re-asserts that the best round equals the stored `Performances.TimeSeconds`.

**Skipped (safe):** 21 chains whose best round did not equal the stored mark (DB stores a different round's time or a typo; listed as `NOTE` lines in the SQL) and all field-event prelim/final marks (field rounds are attempts, [C32]).

**Not found:** athletic.net's field-series pages had no Cloverleaf series for any meet outside the 2026 ones already imported — older meets only have best marks. MileSplit has no attempt data at all.

**Watch out:** MileSplit raw text varies by timing software: events with blank event names (older HY-TEK / Lynx exports) and rows with seed marks mixed in (e.g. `10:24.09 9:49.27`) cannot be parsed safely and were not imported; only the 2023 MAC had explicit event headers. Re-running is idempotent (each block deletes then inserts that performance's rounds). `Prelims` on older athletic.net meets are real preliminary heats; the "Finals" row is the stored mark.

**Key files:** `scripts/crawl/{an-rounds.txt,prepare_round_inputs.py,ms-542137.txt}`, `scripts/athleticnet/build_rounds_sql.py`, `docs/imports/2005-2025-rounds.sql`.

### [C62] Meet Summary Fact Sheet Now Includes Rounds, Attempt Series and Bar Ladders (prompt v3)

**What changed:** `MeetSummaryPrompt.BuildFactSheet` adds, per result and only when recorded: `rounds` (e.g. `["Prelim 4:02.05","Final 3:55.68"]`), `attempts` (formatted marks, `F` foul, `P` pass) with `bestOnAttempt`, and `bars` (height + how it went, e.g. `4' 4" cleared on the 3rd attempt`) with `bestBar`. The system prompt got a rule allowing them to be used sparingly for a standout result and forbidding invented detail; the writer may call a final faster/slower only by comparing the two given times. `MeetSummaryPrompt.Version` is now "3" (was "2"), so every stored fact-sheet hash differs and all existing summaries read as stale/old-rules.

**Also:** the approved 2026 State summary quoted Clear Fork's 3:55.05 as our school record ([C61]); it was regenerated with this fact sheet (Opus) and imported as a **Draft** — it needs re-approval in `/Admin/MeetSummaries` before it shows publicly again.

**Watch out:** `MeetService.FormatPerformance` is now `internal` (the fact sheet reuses it so marks match the meet page; the validator checks marks against the raw JSON). The generation skill needs the app running against the live DB (`ctf-web-summaries` config) with this branch's code. The other 20 approved summaries were written under v2; they are unchanged and still correct, regenerate only if you want the richer detail.

**Key files:** `CloverleafTrack.Services/MeetSummaryPrompt.cs`, `MeetService.cs`, `Tests/Unit/Services/MeetSummaryTests.cs`.

### [C66] site.css Is Now Cache-Busted (asp-append-version)

**What changed:** `Views/Shared/_Layout.cshtml` links `~/css/site.css` with `asp-append-version="true"` (the JS files already did), so the URL carries a content hash (`site.css?v=...`).

**Why:** `/css/site.css` is served with a ~11 h `Cache-Control: max-age` and had a bare URL. After the bar-ladder deploy ([C60]) the server and static export had the new rules, but browsers holding the old stylesheet rendered `.bar-chip` as plain inline text until the cache expired.

**Watch out:** a new CSS rule only reaches already-visited browsers once the page HTML that links the new hash is also served — fine for the live app; the static export picks it up on the next `ctf` run. The admin layout does not link `site.css` through this tag.

**Key files:** `CloverleafTrack.Web/Views/Shared/_Layout.cshtml`.

### [C67] No-Mark Results: ResultStatus (NM / NH / DNF / DQ)

**What changed:** `Performances.ResultStatus TINYINT NOT NULL DEFAULT 0` (`Models/Enums/ResultStatus.cs`: Mark, NoMark, NoHeight, DidNotFinish, Disqualified). `CK_Performances_DistanceOrTime` was replaced by `CK_Performances_MarkOrStatus`: status 0 needs exactly one of time/distance, status 1-4 needs both NULL. Migration `docs/migration_performance_result_status.sql` (additive; run before deploying). Triggered by Addi Reho fouling all three discus throws at the 2026 MAC — athletic.net shows place `ND` + `Foul/Foul/Foul`; before this there was no row to hang the series on.

**Why a Performance row, not a new table:** every child table (attempts, bars, rounds) and the meet/athlete pages hang off a Performance, so a no-mark Performance gets the foul series / `XXX` bar ladder for free. `sp_RebuildLeaderboards` needed **no change**: all its ranking, PB/SB, percentile and EventStatistics steps already had `<mark> IS NOT NULL` filters, so NM/NH/DNF/DQ never rank and never skew percentiles.

**Where the rows are visible:** meet page (muted pill + series/bars, sorted last by the existing ORDER BY, no PR/SB/SR/placing), athlete page season lists (merged by `AthleteService` from `GetNoMarkResultsForAthleteAsync`), the meet-summary fact sheet (`"result":"no mark (fouled out)"`, prompt v4), and the 4-events-per-meet count (`GetAthleteEventCountForMeetAsync` now unions entries and recorded results, any status, distinct events). Everything else filters `p.ResultStatus = 0`: `MeetRepository` (score marks, performance count), `HomeRepository` (counts, on-this-day, recent highlights, improvement CTEs), `LeaderboardRepository` event page, `SeasonRepository`, `AthleteRepository` roster/athlete performance queries. `MeetService.TotalPerformances` counts marks only; unique-athlete counts still include no-mark athletes ("competed in").

**Admin entry:** a Result dropdown on Create/Edit. Non-Mark requires blank time/distance (validation error otherwise); a Mark with no value is inferred NM when the recorded attempt series is all fouls/passes, NH when the bar ladder has no cleared bar, otherwise a validation error (the CHECK would have rejected it anyway). Pre-meet `MeetEntries` "Enter Result" does not yet offer a status.

**Watch out:** any new query over `Performances` that needs a mark must add `p.ResultStatus = 0` — a NULL-mark row breaks `isFieldEvent = DistanceInches.HasValue`-style logic. `AthleteService` keeps no-mark rows out of PRs/charts/hero stats on purpose. DNF/DQ for relays are stored the same way (AthleteId NULL + PerformanceAthletes) but the athlete page only merges rows via the repository query's relay branch.

**Data:** `docs/imports/2026-mac-addi-reho-nm.sql` (NM + F/F/F for athlete 529, meet 297, event 24). A systematic backfill of other NM/NH/DNF/DQ rows from athletic.net / MileSplit has not been done; athletic.net field-series pages mark them `ND`/`NH`, results pages `NM`/`DNF`/`DQ`.

**Key files:** `Models/Enums/ResultStatus.cs`, `docs/migration_performance_result_status.sql`, `AthleteRepository.cs`, `MeetRepository.cs`, `HomeRepository.cs`, `AdminMeetEntryRepository.cs`, `MeetService.cs`, `AthleteService.cs`, `MeetSummaryPrompt.cs`, `PerformancesController.cs`, `_Layout`-independent `.chip-nomark` in `input.css`/`site.css`.

### [C68] No-Mark Backfill From athletic.net (2026 Meets) — Method and Gaps

**What changed:** After [C67], scanned the 2026 athletic.net meets (MAC 637822, District 649905, Regional 651387, Early Bird 644596, Fairview 648714, Woodridge dual 641579, Keystone 651598, Fairless 653339, Stars 637734) for Cloverleaf rows with `ND`/`NM`/`NH`/`DNF`/`DQ`. Found and imported (`docs/imports/2026-mac-addi-reho-nm.sql`, `docs/imports/2026-no-marks.sql`): Addi Reho NM (MAC discus, F-F-F), Adriana Akers DQ (Fairless 100m), and NH for Taylor Thur (Keystone PV), Yousef Fekieh and Clayton Sewalish (Fairview HJ), Delaney Ishler and Ciara Kremer (Fairview PV). Scanner: `scripts/athleticnet/scan_no_marks.js`.

**Watch out:**
- The Browser pane must be *visible* (`show_pane preview`) or athletic.net's SPA never paints and every page is empty; iframes must run **one at a time** (overlapping iframes starve each other of paint) and a tool call is capped at 45 s, so run the loop as a background promise and poll.
- The athlete's initials line precedes the name ("NM" for Noah Mion): the `NM` token is a false positive unless it is in the place column. Running results put the status after the team; field series put `ND`/`NH` in the place column.
- Not covered: JV/other-division pages (only `m/1`, `f/1`), the 2026 State meet (many division pages), pages that did not render or had no Cloverleaf row (reported as `bad`; several HJ/PV pages), MileSplit, and all pre-2026 meets. DNS is not imported. NH rows have no bar ladder (starting height not captured); an NM with a field series needs the series imported separately (Addi's was in the SQL).
- A DQ/DNF in a *later* round after a legal earlier round is NOT a no-mark row: that athlete already has a mark (e.g. Mike Fuentes 2007 semi DQ).

**Key files:** `scripts/athleticnet/scan_no_marks.js`, `docs/imports/2026-no-marks.sql`.

### [C69] No-Mark Backfill: MileSplit (all matched meets) and athletic.net 2022-2025 (field events)

**What changed:** `docs/imports/no-marks-2005-2025.sql` (63 rows: 3 NM, 58 NH, 1 DNF, 1 DQ, across 2005-2026; includes the rows from the 2026 files, idempotent) generated by `scripts/crawl/build_no_mark_sql.py` from `ms-no-marks-raw.txt` (MileSplit) and `an-no-marks-raw.txt` (athletic.net field events, 2022-2025). MileSplit: fetched `/meets/{id}/results` -> the `/results/{rid}/raw` text for all 128 matched meets and kept Cloverleaf lines with `ND`/`NH`/`DNF`/`DQ` (34 meets had hits). Athletic.net: 46 matched meets 2022-2025, field events only (plain results pages; the series tab is empty for older meets).

**Rules the builder applies:** individual events only (relays, seated and wheelchair rows are listed as REVIEW and skipped), athlete matched by first+last+gender (first-name alias Addison->Addi), exactly one DB event for (gender, event, meet environment), skipped if the athlete already has any Performance for that event at that meet, asserted per block (athlete Id + last name, event Id + name + gender). `ND` -> NM, `NH` -> NH. A seed mark before the status (`6-00.00 NH`) is a seed, not a result. NH rows have no bar ladder.

**Watch out:** the `NM` token on athletic.net pages is also the initials of athletes like Noah Mion and Natasha Messmore (false positive; filtered by hand). `DNS` (did not start) is intentionally not imported. Not covered: athletic.net running-event DQ/DNF before 2026, JV pages, the 2026 State meet, relay DQ/DNF, meets whose names did not match a DB meet, and `scr` (scratch) rows for events with no result. The browser pane must be visible during athletic.net scans ([C68]); they take roughly 1-2 minutes per meet.

**Key files:** `scripts/crawl/{build_no_mark_sql.py,ms-no-marks-raw.txt,an-no-marks-raw.txt}`, `docs/imports/no-marks-2005-2025.sql`.

### [C70] Removed the "Show attempt chart" Disclosure

**What changed:** `Views/Shared/_AttemptSeriesExpanded.cshtml` now renders only `_AttemptStrip`; the `<details>` "Show attempt chart" with the inline-SVG bar chart (athlete page and event page, e.g. Will Rogalski's senior-year shot put) is gone. Supersedes the disclosure described in the attempt-series entry ([C32]).

**Why:** the strip already shows every attempt (F / P / marks, best highlighted); the chart repeated it. The partial is kept as a thin wrapper so both pages keep a single call site.

**Key files:** `CloverleafTrack.Web/Views/Shared/_AttemptSeriesExpanded.cshtml`.

### [C71] Missing Northeast Division I District Meets (2005, 2006, 2008, 2009, 2010) From baumspage.com

**What changed:** `docs/imports/ne-district-2005-2010.sql` creates five meets that existed only on the web (`<year> Division I District Track Meet`, Hoover High School / North Canton, outdoor, season-matched, `MeetType` 1 like the 2003/2004/2007/2011 district rows) and their Cloverleaf results: 41 + 42 + 56 + 47 + 38 performances (individual and relay), 108 prelim/semi/final `PerformanceRounds` rows, 4 ResultStatus rows, and two new athletes (Amber Hansen, Morgan O'Rourke, both class of 2007). Source: baumspage.com Hy-Tek result pages `tf/ne/YYYY/ncanton/res*.htm` (saved as `scripts/crawl/baumspage/ne-YYYY-res.txt`), parsed by `scripts/crawl/parse_hytek.py`, SQL built by `scripts/crawl/build_district_sql.py`. athletic.net's district pages for 2005, 2006, 2009 and 2010 were scanned and agree with baumspage on every mark and athlete; they supplied the relay legs that baumspage omits for some rows.

**Rules:** the meet date is the LAST day of the meet (the DB's existing district rows use it). One Cloverleaf relay per event: rounds are merged and the leg names come from whichever round lists all four. A Cloverleaf relay with no named legs in any source (2006 boys 4x400, 3:39.97) is skipped, because a relay Performance needs its athletes. Athlete names are matched exactly with a small alias table (Cassy->Cassandra Maxwell, Rick->Richard Morrison, Brittany->Brittani Pekar, William->Billy Simpson, ...). Billy Simpson's DB graduation year (2007) looks wrong: he competed through 2008.

**Top-8-only field events:** in most district field events only the top ~8 (plus ties) have a recorded distance/height and everyone else shows `ND`/`NH`: that is "not recorded", NOT a no-mark result, so those rows are skipped (the builder's `unrecorded_events`: <= 10 rows with a mark and >= 2 without). Events where nearly everyone has a mark (2008 boys discus 23 marked / 3 ND) keep their ND/NH as real no-mark results. Without this rule 51 false NM/NH rows would have been created. The earlier MileSplit/athletic.net no-mark imports ([C68], [C69]) were re-checked for the same trap (e.g. 2021 Orrville discus has 13 marks and 2 ND, so Ben Kott's ND is real) and stand.

**Watch out:** the SQL bakes in athlete Ids for existing athletes (it asserts every Id still matches its name before inserting) but references the two new athletes by name; regenerate against a DB without them. Each year is skipped if a meet with that name and date already exists. Other district years still missing sources: 2008 has no athletic.net page.

**Key files:** `docs/imports/ne-district-2005-2010.sql`, `scripts/crawl/{parse_hytek.py,build_district_sql.py,baumspage/}`.

### [C72] Missing Single-Day Meets Imported From baumspage and MileSplit Raw Results (2007-2019)

**What changed:** ten meets that were only on the web: `docs/imports/baumspage-meets-2007-2011.sql` (2007 Husky Invitational 30 results, 2008 Wolverine Invitational 53, 2011 Optimist Meet 4) and `docs/imports/ms-meets-2017-2019.sql` (2017 Dwight Schar Invitational 25, 2017 Jackson Twilight 18, 2018 New London Relays 11, 2019 New London Relays 10, 2019 Ravenna-Streetsboro Double Dual 101, 2019 Triway Invitational 38, 2019 Smithie Frosh-Soph Invitational 84, JV level), 12 new athletes (mostly the 2019 Smithie JV classes of 2020-2022), 26 `PerformanceRounds` rows. Tools: `scripts/crawl/parse_hytek2.py` (header-driven Hy-Tek parser: `First Last  YY School` and `Last, First  School`, relay legs, Seed/Prelims/Finals columns), `scripts/crawl/build_hytek_sql.py <out.sql> baums|ms` (MEETS config at the top). MileSplit raw text is fetched with plain `curl` (`/meets/{id}/results` -> the `/results/{rid}/raw` link; no browser needed) and saved as `scripts/crawl/milesplit-raw/ms-<id>.txt`; baumspage pages under `scripts/crawl/baumspage/`.

**Rules (same as [C71]):** one Performance per athlete-event with the best mark, prelim/semi/final times become `PerformanceRounds`, relays need four named legs, top-8-only field events (8-10 marked rows, the rest ND/NH; the lower bound of 8 was added because small meets legitimately have 5-6 marked rows and several real NH) are skipped. An unmatched athlete is created only when a grade is given *somewhere in the batch* (class year = season end year + 12 - grade). Invitationals use `MeetType` 3, double duals 2 (no `MeetParticipants` rows: they need a `Schools` row), JV `MeetLevel` 2; the earlier batches ([C71], baumspage 2007-2011) use `MeetType` 1 like their neighbours. New locations (Norwayne HS, Jackson HS) are created by the SQL.

**Not imported / watch out:**
- Throws "relays" at the New London Relays (3 athletes' individual distances pooled, listed as individual rows), wheelchair/seated events, and 4 relays with no named legs (2017 Jackson Twilight girls 4x200, 2019 Smithie girls 4x400) are skipped and listed by the builder.
- MileSplit 663790 "Streetsboro vs. Cloverleaf and Ravenna 2025-04-08" is the DB's 2025-05-05 Ravenna-Streetsboro Double Dual (same results; the two sources disagree on the date). The DB copy is missing at least Kierston Campbell's hurdles, so meets that already exist can still be incomplete: a performance-level diff is still to do. MileSplit 465324 (2022 MAC, dated 05-06) is DB meet 239 (05-12).
- Still missing: Nordonia Knight Relays 2011 (baumspage has only score sheets), Optimist 2012/2016/2024 and Mansfield Mehock 2009 (athletic.net only), 2018 Triway (MileSplit raw has girls only), Green Twilight Relays 2023 (no raw results), Jim Hauser / Kent State / Oberlin / Rust Buster / Rubber City / OATCCC / Extreme Vault indoor meets (1-2 Cloverleaf rows each, mostly vaulters), 2024 New Balance Nationals.
- The Hy-Tek text on MileSplit sometimes carries the wrong year in its header (2019 New London says 4/7/2018): the meet date comes from the MileSplit/athletic.net listing and the licence stamp.

**Watch out (Ids):** the local test DB and live assign different Ids to athletes created by an earlier import SQL, so `build_hytek_sql.py` must reference those by name (`BYNAME`), and the generated `.sql` must be built from a database that does not yet contain the batch's own new athletes (rebuilding after applying bakes their local Ids). The Id assertion at the top of each file caught exactly this on the first live run (nothing was written).

**Key files:** `scripts/crawl/{parse_hytek2.py,build_hytek_sql.py}`, `docs/imports/{baumspage-meets-2007-2011,ms-meets-2017-2019}.sql`.

### [C73] Missing athletic.net-Only Meets and Indoor Meets (2009-2026)

**What changed:** `docs/imports/an-meets-2009-2024.sql` (2009 Mansfield Mehock Relays 1 result, 2012 Optimist Meet 4, 2016 Optimist Meet 27 incl. 4 relays, 2024 Optimist Meet 1) and `docs/imports/indoor-meets-2025-2026.sql` (Environment 2: 2025 Kent State HS Meet #5, 2025 Akron Rust Buster, 2026 Akron Rubber City, 2026 Kent State HS Meet #1, 2026 Ashland Light Giver; one pole vaulter each, two of them NH). New scraper `scripts/athleticnet/scan_an_rows.js` captures structured Cloverleaf rows from athletic.net event pages into `scripts/crawl/an-rows/an-<meetId>.json`; `scripts/crawl/parse_an.py` turns them into the common row format so `build_hytek_sql.py an|indoor` handles them (per-meet optional 10th tuple element = Environment). athletic.net rows keep prelim/final via the page section header.

**Not imported (checked, nothing to import):** the 2019 OATCCC D2/3 Indoor State (only a 4x800 relay whose legs are "Relay Team"), Mount Union 2026 and Kent State #3 2026 (the vaulter was DNS), Tiffin 2021, Kent State #3 2022, Conversion Time 2017, Oberlin Run-Jump-Throw, Extreme Vault Club, YSU #3, Ashland High Performance Invite and OATCCC D2 2026: the MileSplit raw results contain no Cloverleaf rows at all, so Cloverleaf was listed but did not compete or is not in the data. The second Mansfield Mehock athletic.net listing (65012) has no results. The 2009 Mansfield discus ND (7 marked, 47 no-mark rows) is "not recorded", not a no-mark.

**Watch out:** `parse_an.unrecorded` uses marked/no-mark counts computed in the browser (8-10 marked and >=2 no-mark, or 6-10 marked and >= 3x as many no-mark). athletic.net marks carry an `a`/`h` suffix that is stripped; field marks are `F-I.ii`.

**Key files:** `scripts/athleticnet/scan_an_rows.js`, `scripts/crawl/{parse_an.py,build_hytek_sql.py,an-rows/}`.

### [C74] Results-Level Comparison: MileSplit vs the DB

**What changed:** `scripts/crawl/compare_ms_vs_db.py` compares, for every DB meet matched to a MileSplit meet (125 with raw text), each Cloverleaf row in MileSplit's Hy-Tek text with the DB result for the same meet+event+athlete (relays by event+time; tolerance 0.06 s / 0.3 in; fuzzy athlete matching: same last name (edit distance <=1) and a compatible first name, unique only). 2,771 results agree. `docs/imports/results-missing-ms.sql` (built by `build_missing_sql.py`) inserts the 32 results MileSplit has that the DB lacked (24 individuals incl. Will Rogalski's 2022 Fairless shot put and Brooklynn Crist's 2019 Cowen Redbird results, plus 8 Oberlin 2017-2020 relays with four named legs). The rest is in `docs/results-level-gap-report.md`: 39 individual and 14 relay marks that differ (to verify), 54 unplaced athlete rows and 92 rows in events the DB does not model.

**Artifacts (do not "fix" the DB for these):** Hy-Tek prints hand-timed tenths as hundredths (`27.06` = 27.6); the 2022 Early Bird relay times in MileSplit are rounded to whole seconds. **Bug fixed on the way:** the event-header regex in `parse_hytek2.py` required the event name to end right after `Shot Put`/`High Jump`, so a header like `Boys Shot Put Varsity` was skipped and its rows were attributed to the previous event (Discus); qualifiers (`Varsity`, `METRO`, hurdle heights `39"`, `Footed`) are now allowed and removed by `norm_event`. The earlier imports ([C71]-[C73]) were re-parsed with both regexes and are unaffected (only relay-type events differed, and those are skipped).

**Watch out:** the download loop is not in the repo: for each id, `curl -sL -A <browser UA> https://oh.milesplit.com/meets/<id>/results`, take the first `/results/<rid>/raw` link, curl it and strip tags from the `<pre>`; save as `/tmp/msraw/ms-<id>.txt` plus `/tmp/msraw/ids.txt` (`<msId> <dbMeetId>` pairs from `matches.tsv`). Mismatches are leads only: either source may be wrong, and the comparison uses the best mark across rounds.

**Watch out (Ids):** athletes created by the earlier import SQLs ([C71]-[C73]) have different Ids on live and locally, so `build_missing_sql.py` references them by name (`BYNAME`); the first live run of the committed file was stopped by its Id assertion (Megan Van Meter 980 vs 978) before writing anything.

**Key files:** `scripts/crawl/{compare_ms_vs_db.py,build_missing_sql.py}`, `docs/imports/results-missing-ms.sql`, `docs/results-level-gap-report.md`.

### [C75] Event and Name Disparities From the MileSplit Comparison (Decisions)

**Decisions (owner):** (1) throws/jumps "relays" (Shot Put / Discus / Long Jump / High Jump / Pole Vault Relay at Early Bird, Medina, New London...) store each athlete's own mark as a normal individual result *in addition to* the existing "N-Person ... Relay" team rows; (2) wheelchair / seated rows map to the DB's Seated 100/200/400/800M and Seated Shot Put (no seated discus/weight event exists, those rows stay unimported); (3) scoring qualifiers (`COUNTY`, `Div 2`, `Division 3 - Prelims`, `METRO`, `Varsity`, hurdle heights) are stripped, the event is the plain event; (4) prefer the official results (MileSplit) for marks, but review each difference first, so nothing was overwritten; (5) name variants: Savannah Reho (8th grade) ignored; Bryce Neil = Bryce Neill, Asa Gyn = Asa Gynn, Isaiah Curt = Isaiah Kurtz, Josh Bohemian = Josh Bowman, Jacob Knau = Jacob Nau, Meagan = Megan Van Meter (alias tables in `compare_ms_vs_db.py` and `build_hytek_sql.py`).

**What changed:** `norm_event` (parse_hytek.py) maps those event names; `parse_hytek2.py` now recognizes `Grade`/`Mark`/`Team` column headers (the Seed column was being read as the result), any `Event N <name>` header, medley / shuttle hurdle / weight headers, treats medley relays as relays, and cleans names (`#570`, `JR`, `W12`, `(Jake)`, `(wheeled)`). `build_hytek_sql.py` keeps *separate relay teams* per event (grouped by the four leg names; before, a school's second relay in an event overwrote the first and only the last time was stored). `compare_ms_vs_db.py` matches relays team by team. `docs/imports/results-missing-ms-2.sql` (64 rows), `-3` (5 relays) and `-4` (13) add the 82 results this uncovered: field-relay individual marks, medley relays, Seated events (Bridgette Donahue, Brooklynn Crist), the second relay teams of the 2019 Ravenna-Streetsboro dual and others. They reference meets and athletes created by import SQL by name/date because Ids differ between databases.

**Watch out:** a wheelchair athlete can legitimately appear in a normal event (Brooklynn Crist's 21.18 100M at the 2019 Cowen Redbird is in the normal Girls 100M). The 12 "large difference" marks in `docs/results-level-gap-report.md` include source rows that are plainly wrong (Orange Relays 2018 "1600M" 12:48.68) as well as likely DB typos; the 27 small ones await the owner's go-ahead to take MileSplit's mark.

**Key files:** `scripts/crawl/{parse_hytek.py,parse_hytek2.py,compare_ms_vs_db.py,build_missing_sql.py,build_hytek_sql.py}`, `docs/imports/results-missing-ms-{2,3,4}.sql`, `docs/results-level-gap-report.md`.

### [C76] MileSplit Marks Applied to 22 Small Differences

**What changed:** `docs/imports/marks-from-milesplit.sql` (built by `scripts/crawl/build_mark_updates_sql.py`) overwrote 22 DB marks with MileSplit's official mark, as decided by the owner ("prefer official results for marks"). Each UPDATE is guarded by the old value and skips rows that have `PerformanceRounds`; `sp_RebuildLeaderboards` re-ran. **Not changed:** 12 large differences (left for the owner, `docs/results-level-gap-report.md`) and 5 small ones (MileSplit only has a prelim row for Riley Evans 2024 MAC 200M, Maddie Boltz / Sara Keener / Emily Romano 2018 PTC; Riley Evans 2024 Regional 400M already has rounds).

**Old -> new (seconds / inches):** 2025 MAC 400M Isaac Smith 53.19 -> 52.79, Katie Wershing 63.38 -> 61.32; 2024 Fairless Twilight discus Timmy Shores 1477 -> 1476, shot Karoline Kreider 391 -> 386.5; 2023 Ravenna-Streetsboro 200M Oakley Kemp 25.19 -> 25.10; 2023 Fairless Twilight PV Payten Shetterly 78 -> 72; 2022 Smithie 800M Brooke Gucker 162.65 -> 162.56, shot Timmy Shores 464 -> 459; 2021 Nikki Reynolds 800M Alyssa Stoner 150.69 -> 150.09, shot Luke Yuravak 493.25 -> 497.25; 2021 D1 District 1600M Nathan Naples 305.77 -> 305.05, 800M Allison Yagersz 155.84 -> 155.73; 2018 PTC 100M Zack Munchick 11.52 -> 11.73, 400M Tommie Kurtz 60.85 -> 62.54, Maddie Boltz 65.72 -> 65.93, Frank Stepp 52.12 -> 52.42; 2017 Mansfield Mehock 1600M Katelyn Malena 379.31 -> 379.25; 2017 Orange Relays discus Nate Clingan 1363 -> 1363.5; 2017 PTC 400M Jenna Oliver 64.39 -> 64.48, Ryan Curtis 55.11 -> 55.92, 300H Zana Ross 48.70 -> 49.04, 200M Jenna Oliver 28.19 -> 28.60.

**Watch out:** several of these made a mark *slower/shorter* (e.g. PTC 2017/2018, Payten Shetterly's 6' 6" -> 6' 0"): the DB value may have been a different round or timing; revert from this log if the official sheet says otherwise.

**Key files:** `scripts/crawl/build_mark_updates_sql.py`, `docs/imports/marks-from-milesplit.sql`.

### [C77] The 12 Large MileSplit Differences, Reviewed One by One

**What changed:** the owner went through the 12 large differences from `docs/results-level-gap-report.md` (decisions in the table there). Applied: Clay Combs 2025 Nikki Reynolds discus 157' 10" -> 157' 0"; Dylan Wittman 2024 Stars discus 103' 3" -> 100' 8"; Timmy Shores 2024 Regional discus 128' 6" -> 129' 7"; Nick Bailey 2017 Division II District 800M 2:18.46 -> 2:12.94 with a prelim 2:12.94 / final 2:18.46 chain (`docs/imports/marks-review-12.sql`). `docs/imports/orange-2018-3200m.sql` adds Sara Naples (12:48.68) and Dillon West (10:01.05) as 2018 Orange Relays **3200M** results: the official file lists their 3200M runs under the 1600 header, so MileSplit's "1600M" rows are not 1600s; their real 1600M marks stay. Everything else: keep ours (MileSplit digit swap for Presly McConnell, wrong row for Collin Saksa / Dominic Nau, prelim-only for Riley Evans, mismatch for Blake Wilson). The Kreider sisters were swapped in the official results (Karoline threw 85' 1", Jayce 78' 10"); the DB was already right. Open question: Karoline's 2024 Fairless shot put was changed 32' 7" -> 32' 2.5" in [C76]; if the sisters were swapped there too, revert it.

**Bug found:** 23 meets have a time of day in `Meets.Date` (e.g. 2024 Stars `2024-05-06 16:00:00`). The guards in `marks-from-milesplit.sql` and `marks-review-12.sql` that matched meets with `Date = 'YYYY-MM-DD'` silently updated nothing for those meets: 2 of the 22 [C76] updates and 2 of the 4 review updates were missed on the first live run and caught by checking the values. The files now use `CAST(Date AS date)` and were re-run (idempotent, guarded by the old value). The earlier import SQL files only used the equality inside `IF NOT EXISTS` (new meets) or assertions that would have failed loudly.

**Key files:** `docs/imports/{marks-review-12,orange-2018-3200m,marks-from-milesplit}.sql`, `docs/results-level-gap-report.md`.

### [C78] Relay Mark Fixes From the Results-Level Comparison

**What changed:** two real relay errors fixed in `docs/imports/relay-fixes.sql`. (1) 2025 Green Twilight Relays boys 4x1600M Relay (perf 13289): 1542.98 (25:42.98, the girls' time) -> 1196.86 (19:56.86). (2) 2024 Division I District girls 4x400M Relay (perf 12463): 255.93 (4:15.93, the prelim) -> 250.33 (4:10.33, the final), with `PerformanceRounds` prelim 255.93 / final 250.33. The other three relay mismatches were parse artifacts (DB already matches the official final): Green Twilight 2025 boys 4x100, D2 District 2025 boys 4x400, 2024 Regional girls 4x200.

**Why:** the relay rows of the MileSplit comparison; the old values are above so they can be reverted.

**Watch out:** relay event names are `4x400M Relay` / `4x1600M Relay`. Seven relay performances still lack four named legs (2021 SPIRE and Cloverleaf Indoor #1, 2025 D2 District boys 4x800): athletic.net's results pages only show a collapsed "Relay Team" toggle, so the legs are not in the page text and need the toggle expanded or another source.

**Key files:** `docs/imports/relay-fixes.sql`.

### [C79] SPIRE 2021 Boys 4x400 Relay Legs; Other Missing Relays Not Resolvable

**What changed:** imported the 2021 SPIRE Ohio Championship Preview boys 4x400M Relay (4:08.95, 9th) with legs Alexander Baughman, Patrick McConnell, Cole Yagersz, Chase Young (`docs/imports/spire-2021-boys-4x400.sql`).

**Why:** the "7 relays without four resolvable legs" in the results-level report were never imported (they are absent from the DB, not leg-less rows). The owner supplied these legs from the meet video; leg 3 (Yagersz) is a best guess, and the order is as seen on video.

**Watch out:** neither athletic.net (relay rows are just a "Relay Team" label with no athletes) nor MileSplit (Hy-Tek lists no legs for Cloverleaf) names legs for the 2021 SPIRE/Cloverleaf Indoor #1 or 2025 D2 District boys 4x800 relays. They stay un-imported until the legs are known.

**Key files:** `docs/imports/spire-2021-boys-4x400.sql`.

### [C80] Seated Shot Put Marks in the Wrong Column; Missing Donahue Wheelchair Results

**What changed:** `docs/imports/seated-donahue-fixes.sql` (1) moves 5 `Seated Shot Put` marks (Donahue 2019 Triway/Nikki Reynolds, 2021 Medina/Nikki Reynolds; Crist 2019 Triway) from `TimeSeconds` to `DistanceInches`, and (2) imports 7 missing Bridgette Donahue seated results: 2018 Cowen Redbird 100M 49.85 / 400M 3:26.55 / shot 6' 10"; 2018 PTC 100M 42.78 / 400M 3:29.46 / shot 7' 0.75"; 2021 Medina Relays 100M 34.61.

**Why:** the results-level comparison left 17 "unplaced" rows. 9 were team-score lines and 2 are Savannah Reho (ignored on purpose). 6 were Donahue wheelchair rows whose names MileSplit lists as "Donahue (wheeled)" or "Donahue, Bridgette" with a `W` grade. The 5 misplaced marks were found while checking her rows: a field mark in `TimeSeconds` is invisible to ranks, leaderboards and percentiles.

**Watch out:** two Green Twilight 2025 relays (Mixed 4x200 1:51.20, 8th; a 9:59.12 relay, 5th) are on MileSplit with no leg names, so they are still not imported. The SQL aborts unless exactly 5 mis-stored seated shot put rows exist. Check with `Performances` joined to `Events` where `EventType` is a field type and `TimeSeconds IS NOT NULL`.

**Key files:** `docs/imports/seated-donahue-fixes.sql`.

### [C81] 2026 Field-Coventry Double Dual: Attempt Series, Bar Ladders, Huegel Marks

**What changed:** `docs/imports/2026-field-coventry-attempts.sql` (built by `scripts/athleticnet/build_scan_sql.py` / `build_bars_sql.py` from `scripts/athleticnet/data/292/`) adds 55 field-event attempt series (long jump, shot put, discus for Cloverleaf athletes) and 4 pole vault / high jump bar ladders for meet 292, transcribed from the scorer's hand-written sheets (PDF scan, 2026-04-28). It also corrects two marks the owner approved against the sheet: Macie Huegel shot put 24' 2" -> 24' 6" (290 -> 294) and discus 65' 6" -> 65' 8" (786 -> 788; her first discus throw reads 59'3" or 69'3").

**Why:** the sheets record every attempt/bar. Every other mark on the sheets already matched the DB (all the builder asserts passed).

**Watch out:** on the throws sheets most athletes have only two numbers across the four attempt columns (dashes elsewhere), so the dashes were treated as "not taken" and the two throws stored as attempts 1-2. Amelia Ryncarz's girls discus has one throw, so no series. Peyton Shetterly (women's PV, NH at 6'0") and Clayton Sewalish (boys HJ, NH at 5'0") have no DB row; no-height rows were not added. Lincoln Baltic's ladder includes a pass at 7'8" read from a hard-to-read opening-height header. The sheet's Perez at 100M is a Field athlete, not ours. The scans are sideways and upside down: rotate 180° at 150 dpi to read them.

**Key files:** `docs/imports/2026-field-coventry-attempts.sql`, `scripts/athleticnet/data/292/`.

### [C82] Baumspage and athletic.net Results-Level Comparisons

**What changed:** the results-level comparison ([C74]) was extended to the other two sources. **Baumspage** (`scripts/crawl/compare_ms_vs_db.py` reused on 29 archive pages: Suburban League 2005-11, Kimberly, Comet Relays, Husky, Wolverine, NE District 2003-07, Optimist): 1,273 results agree; `docs/imports/results-missing-baumspage.sql` adds 23 missing results (17 are Amber Hansen's 2006-07 pole vault / high jump / shot put / 100M, plus Kari Morris SP + discus, Adrian Medina 800, Kelani Nuckols 300H, Clay Vondriska 300H, Kelli Slavens discus). **athletic.net** (new `scripts/crawl/compare_an_vs_db.py`, 33 athletic.net-only meets): everything agrees except `docs/imports/athleticnet-compare-fixes.sql`: Sean Brown's 2014 District pole vault 10' 6" -> 10' 0" (it was a copy of James Reinhart's mark), Brooklynn Crist's 2019 Seated State 100M/400M/800M (20.56 / 1:20.52 / 2:56.71, copies of her other 2019 marks -> 23.85 / 1:22.15 / 2:54.49), and Bridgette Donahue's missing 2019 seated state shot put (7' 4.75").

**Why:** the owner asked for result-level comparisons with athletic.net and baumspage as with MileSplit. Prefer the official/source mark.

**Watch out:**
- `compare_ms_vs_db.py` now: strips glued Hy-Tek bib numbers from names, resolves duplicate athlete records (same normalised name; e.g. Morgan ORourke 442 / O'Rourke 973, Ricky 530 / Richard 138 Morrison: pick the one with a result in that meet/event), and has the alias `Hilar y Hewit`. The duplicates themselves are still in the DB.
- athletic.net has a JSON API that needs no visible browser: `GET /api/v1/Meet/GetMeetData?meetId=&sport=tf` (returns `jwtMeet`), `GET /api/v1/Meet/GetEventListData` and `POST /api/v1/Meet/GetResultsData3 {gender, divId, eventShort, rawResults:false, showTips:false, groupParentId:0}`, both with header `anettokens: <jwtMeet>`. Cloudflare returns HTTP 429 after a burst: pace at ~1.5 s per request. The iframe scraper needs the pane visible, so it fails over Remote Control. A page on https://athletic.net could not POST to a local http receiver while remote (data had to be read back through the tool).
- Hy-Tek/athletic.net print hand-timed tenths as hundredths (1:55.3 -> 1:55.03); relay mismatches of that shape were ignored. Meets with a Prelim/Semi column count the faster round (owner rule) but this was not applied to the 2004 NE District hurdles (see below).
- Left for the owner (not changed): Brian Livingstone 2011 Suburban shot put 52' 1" vs DB 51' 1"; Mandy Fugate 2003 District 200M 29.35 vs DB 27.38; Sarah Schleich 2008 Comet Relays long jump 13' 6.75" vs DB 12' 2"; 2004 District hurdles semis (Cory Beebe 110H 15.81 / 300H 39.94, Kalyn O'Shea 300H 49.04) are faster than the DB's finals; girls 4x800 2011 Suburban 654.57 vs 654.27; girls 4x400 2010 Suburban 270.81 vs 270.18. Relays on the sheets with no leg names: NE District boys 4x400 2006 (3:39.97) and 2007 (3:48.28), 2011 District boys 4x100 45.13, boys 4x200 1:36.51, girls 4x100 52.86, girls 4x400 4:21.28.

**Key files:** `scripts/crawl/compare_an_vs_db.py`, `scripts/crawl/compare_ms_vs_db.py`, `docs/imports/results-missing-baumspage.sql`, `docs/imports/athleticnet-compare-fixes.sql`.

### [C83] Live Apply of [C80]-[C82] SQL

**What changed:** the four staged files were applied to the live DB on 2026-10-19 (`seated-donahue-fixes`, `results-missing-baumspage`, `athleticnet-compare-fixes`, `2026-field-coventry-attempts`). The live DB is not an exact copy of the local one: it had only 2 mis-stored Seated Shot Put marks (Crist and Donahue, 2019 Triway), not 5, so the `= 5` assertion in `seated-donahue-fixes.sql` aborted the transaction and was relaxed to `> 5`. Everything else asserted cleanly (Performance Ids matched).

**Watch out:** do not assume a local row count or Id also holds live; assert ranges or reference by name and date. Marks fixes for Huegel, Brown, Crist and Donahue are on live; the owner-review mark questions from [C82] are not.

### [C84] Owner-Reviewed Marks From the Baumspage Comparison

**What changed:** `docs/imports/baumspage-mark-reviews.sql` (applied local and live 2026-10-19), each line decided by the owner ("use the sheet"). Old values: Brian Livingstone 2011 Suburban shot put 51' 1" (613) -> 52' 1" (625); Mandy Fugate 2003 District girls 200M 27.38 -> 29.35; Sarah Schleich 2008 Comet Relays long jump 12' 2" (146) -> 13' 6.75" (162.75) (a Long Jump Relay individual mark); 2004 District hurdles, best semi with a semi -> final round chain: Cory Beebe 110H 16.65 -> 15.81 (final 16.65), Cory Beebe 300H 40.25 -> 39.94 (final 40.25), Kalyn OShea 300H 49.10 -> 49.04 (final 49.10); girls 4x800M 2011 Suburban relay 654.27 -> 654.57; girls 4x400M 2010 Suburban relay 270.18 -> 270.81.

**Why:** closes the "left for the owner" list of [C82]; marks follow the official results.

**Watch out:** Kalyn's last name is stored as `OShea` (no apostrophe) and Morgan has two records (`ORourke` 442 and `O'Rourke` 973); match athlete names exactly as stored. Fugate's 27.38 was possibly a prelim from a source we cannot see; revert from this log if a better source turns up. Still open: the six relays with no leg names ([C82]) and the duplicate athlete records.

**Key files:** `docs/imports/baumspage-mark-reviews.sql`.

### [C85] Duplicate Athlete Records Merged

**What changed:** `docs/imports/merge-duplicate-athletes.sql` (applied local and live 2026-10-19) merges seven pairs into one record each and deletes one empty record. Keeper (Id on both DBs unless noted) <- duplicate, final spelling: Bridgette Donohue 626 <- Bridgette Donahue => **Donahue**; Amber Hanson 436 <- Amber Hansen => **Hansen**; Trevor Milbrandt 70 <- Trevor Millbrandt; Megan VanMeter 111 <- Megan Van Meter => **Van Meter**; Morgan ORourke 442 <- Morgan O'Rourke => **O'Rourke**; Richard Morrison 138 <- Ricky Morrison (class 2007, only 3 relay legs); Jon Hearn 305 <- Jonathan Hearn (owner chose "Jon"). Deleted the empty Sarah Hollopeter (class 2011; Sierra Hollopeter is the athlete on the sources). Left alone on purpose: Adam (757) vs Andrew (81) Podolak (separate athletes on athletic.net; Adam's class year of 2014 looks wrong since he competed 2011-13), Janet vs Jenny Stupica (owner: different people), brothers and sisters sharing a surname (Cass, Freeland, Hutson, Nespeca, Bruening).

**Why:** the duplicates hid results and caused false "missing" findings. The comparison scripts resolved the second spelling as "exact", so results that already existed under the first spelling were imported again: 17 Amber Hansen rows ([C82]) and 19 Bridgette Donahue rows ([C80], [C82]) were exact copies (same meet, event and mark). The merge deletes a duplicate result when the keeper has the same mark at the same meet+event, otherwise moves it; relay legs, entries and summaries move to the keeper. Athletes are looked up by name + gender + class year because the Ids of import-created athletes differ between local and live (live 970/971/973/983/978 vs local 972/973/975/985/980).

**Watch out:** the merge aborts if a duplicate result has a different mark or any placing, entry, leg or series. The athlete page URL slug changes with the renamed athlete (`/roster/bridgette-donahue`); the static site regenerates it. Old import SQL that references `Trevor Millbrandt` by name would no longer find him (do not re-run those files). Separate issue, not athlete duplicates: five athlete/meet/event pairs have two results rows each (Bryce Neill 2025 Ravenna 200M 27.14 and 27.00; Gordy Surrarrer 2026 Ravenna discus 838 and 990; Jackie Baker 1996 Medina County 100M 14.6 and 14.7; Nolan McNeal 2021 Black River 300H 46.2 and 46.1; Weston Miller 2026 New London long jump 156.5 twice).

**Key files:** `docs/imports/merge-duplicate-athletes.sql`.

### [C86] Five Double-Entered Results Fixed

**What changed:** `docs/imports/duplicate-results-fixes.sql` (applied local and live 2026-10-19), owner decisions per pair. Bryce Neill 2025 Ravenna-Streetsboro 200M: kept 27.14 (MileSplit), deleted 27.00. Jackie Baker 1996 Medina County 100M: kept 14.6, deleted 14.7 (and its duplicate pre-meet entry). Nolan McNeal 2021 Black River Dual 300H: kept 46.1, deleted 46.2. Weston Miller 2026 New London long jump: identical 156.5 twice, kept the row with the meet entry. Gordy Surrarrer 2026 Ravenna discus: the 69' 10" (838) is Gordy's; the 82' 6" (990) row and its entry were re-assigned to Gunnar Surrarrer (class of 2028), who had no discus entry at that meet and throws 79'-83' elsewhere while Gordy throws 65'-75'.

**Why:** found while merging duplicate athletes ([C85]): five athlete+meet+event pairs had two result rows. The Surrarrer pair was a misattribution, not a duplicate.

**Watch out:** two brothers named Surrarrer compete together (Gunnar 2028, Gordy 2029) plus Tate (2015): check the first name when entering marks. The SQL looks rows up by name, meet and exact mark and aborts unless exactly four rows are deleted.

**Key files:** `docs/imports/duplicate-results-fixes.sql`.

### [C87] Meet Row Click Did Not Load the .html Page on the Live Site

**What changed:** `Views/Shared/_MeetListItem.cshtml`: the `<tr>` click handler was `onclick="window.location='@Url.Action(...)'"` and is now `onclick="window.location = this.querySelector('a').href"`.

**Why:** the public site is a static export. The export script converts links to `.html` in markup (`href`) but cannot rewrite a URL inside inline JavaScript, so clicking the row (not the name link) went to the extension-less route, which does not exist on the static host. The `<a>` inside the row has `onclick="event.stopPropagation()"` so a click on the link itself is unaffected.

**Watch out:** the same trap applies to any future inline JS that navigates to a server-rendered URL. The leaderboard scope/depth controls build URLs from `data-base-url` with a query string; that is a server round-trip and is separate from this.

**Key files:** `CloverleafTrack.Web/Views/Shared/_MeetListItem.cshtml`.

### [C88] MileSplit Live: Attempt Series, Bar Ladders and Round Chains

**What changed:** MileSplit Live (https://milesplit.live, the "live results" pages) publishes per-attempt throw/jump series, high jump / pole vault ladders and prelim/semi rounds that the Hy-Tek raw results do not. New pipeline: `scripts/athleticnet/scan_live.py` (headless Playwright scan of every event and round of a meet, Cloverleaf rows only -> `live-raw/<msMeetId>.txt`) -> `convert_live.py` (header-driven; writes `data-live/<dbMeetId>/{sheets,bars,rounds,db}.txt`, skips performances that already have attempts/bars) -> the existing `build_scan_sql.py` / `build_bars_sql.py` plus new `build_live_rounds.py`. Output, built for the 32 meets that have Live data (2023-2026): `docs/imports/2023-2026-live-attempts.sql` (88 series), `-bars.sql` (27 ladders), `-rounds.sql` (76 chains). New outcomes `X` and `XX` allowed by `docs/migration_performance_bars_x.sql` (a vaulter who passes after two misses (`XXP`) and then misses the next bar once is out on cumulative misses: that last bar is a lone `X`). Owner-transcribed ladders (`MANUAL_BARS` in `convert_live.py`): Isaac Baltic and Jace Peacock at the 2023 Early Bird, Isaac Baltic at the 2023 Regional.

**Why:** the owner pointed at the Live results pages as a source for full series. Every block asserts the stored mark equals the best attempt / best cleared bar / fastest round, so a misread row aborts instead of writing wrong data.

**Watch out:**
- Live's results table is rendered by the page (Firestore): the event id is per meet, one id holds every round, the last URL segment is the event's gender (`F` for girls), and an unknown id shows only the news widget. A page opened in the in-app browser pane stalls when the pane is hidden or closed, and the pane's renderer crashed once; the headless Playwright scan has no such dependency (about 2.5 min per meet). Live drops a trailing `s` or other letters in some names (Hunter Brook = Brooks) and calls Kierston Campbell what the DB calls KK; `convert_live.py` has the alias and last-name fuzz.
- Live prints hand-timed marks as hundredths (2:02.8 -> 2:02.08); `build_live_rounds.py` converts a round back when that reading equals the stored time. Live attempt cells occasionally carry typos (45-6.26) and wind readings; the converter snaps to quarter inches and strips the wind.
- Row-level problems found on Live, deliberately NOT stored: Dominic Nau's 2023 Ravenna shot put (Live and MileSplit say 38' 2", the DB has 31' 0", the owner kept 31' 0"); Jayce Kreider's 2023 Early Bird shot put row is contaminated after attempt 4 (stored first four only, owner decision); Collin Saksa 2025 Stars long jump (Live best 18' 5.25" vs DB 15' 8.25"); four results with no DB row at the 2025 Stars (Isabella Messmore discus 48-8 and shot put 18-3, Jacob Schaefer shot put 35-0.5, Jamere Johnson shot put 34-8: probably middle schoolers or unrostered); Payten Shetterly's no-height high jump at the 2024 Stars has no DB row.
- Relay rounds are not chained yet (team labels differ between rounds).

**Key files:** `scripts/athleticnet/{scan_live,convert_live,build_live_rounds}.py`, `scripts/athleticnet/live-raw/`, `scripts/athleticnet/data-live/`, `docs/imports/2023-2026-live-*.sql`, `docs/migration_performance_bars_x.sql`.

### [C89] MileSplit Live Review Outcomes

**What changed:** owner decisions on the held-back [C88] items. Dominic Nau 2023 Ravenna shot put stays 31' 0" (no series stored); Jayce Kreider 2023 Early Bird shot put keeps only her first four attempts; Collin Saksa 2025 Stars long jump stays 15' 8.25" although MileSplit raw results and Live both say 18' 5.25" (no series stored; revisit if a better source turns up). The "missing" Messmore / Schaefer / Johnson results were nickname mismatches (Izzy Messmore, Jake Schaefer, JJ Johnson): aliases added to `convert_live.py`, and `docs/imports/2023-2026-live-extras-attempts.sql` (4 series) plus `-extras-bars.sql` (Payten Shetterly's no-height 2024 Stars ladder, `4-0: XXX`) were generated. `build_bars_sql.py` can now attach a ladder to a no-height (NH, `ResultStatus = 2`) row.

**Watch out:** the extras files are incremental: they hold only what was new after the main Live files were applied locally, so apply the main files first.

**Key files:** `docs/imports/2023-2026-live-extras-{attempts,bars}.sql`, `scripts/athleticnet/convert_live.py`, `scripts/athleticnet/build_bars_sql.py`.

### [C90] Live Apply: Id-Independent Attempt/Bar SQL

**What changed:** the Live attempt/ladder SQL is applied to live (2026-10-20). Applying the first generated files showed the databases do not share Performance Ids for rows created by import SQL (John Artwell's 2023 Ravenna shot put is 17586 locally, 17247 live; Payten Shetterly's no-height row is 16648 vs 16590), so the Id-asserting form aborted. `build_scan_sql.py` / `build_bars_sql.py` now have `SKIP_EXISTING=1` mode: each block looks the performance up by athlete name + meet name/date + event + exact mark (`SET @pid = ...`) and inserts only when it exists and has no series/ladder yet; no match means a silent skip. `convert_live.py` writes `data-live/<db>/meta.txt` (meet name/date) for this, and `LIVE_IGNORE_EXISTING=1` makes it build blocks for performances that already have series locally. Complete files: `docs/imports/2023-2026-live-complete-attempts.sql` (183 series) and `-complete-bars.sql` (48 ladders), applied after the earlier (Id-based) `2023-2026-live-attempts.sql`, `-bars.sql`, `-rounds.sql` and `-extras-*` files. Live and local now hold the same totals: 1,782 attempts, 364 bars, 687 rounds.

**Why:** an intermediate batch had been applied locally before the final files were generated, and the generator skips performances that already have series, so live was missing those batches; a count of X/XX bars on live (0) caught it.

**Watch out:** never assume local Ids equal live Ids for performances created by import SQL (see [C83]); use name/meet/event/mark lookups. When generating SQL for another database, generate it from the full data (`LIVE_IGNORE_EXISTING=1 SKIP_EXISTING=1`), not from a database that already has part of it. A verification query of totals after every live apply would have caught the gap sooner.

**Key files:** `scripts/athleticnet/{convert_live,build_scan_sql,build_bars_sql}.py`, `docs/imports/2023-2026-live-complete-{attempts,bars}.sql`.

### [C91] Relay Prelim -> Final Chains From Live

**What changed:** `scripts/athleticnet/build_live_relay_rounds.py` chains a Cloverleaf relay team's rounds from MileSplit Live (the same four legs appear under `Cloverleaf "A"` in the prelim and the final) onto the DB relay row found by meet name/date + event + gender + fastest-round time. 17 teams have two rounds on Live: 11 already had identical chains from earlier imports (compared, not replaced), and `docs/imports/2023-2026-live-relay-rounds.sql` adds the other six (2023 District boys 4x200; 2025 District boys 4x200, boys 4x400, girls 4x100, 4x200, 4x400). Applied local and live.

**Why:** the owner asked to review the relay prelim chains left out of [C88]. Owner decision for the 2023 District boys 4x200: the prelim is stored as Live's 1:35.29 (95.29) although the performance mark is 1:35.28 (95.28), a 0.01 gap that is kept on purpose.

**Watch out:** only A/B/C teams that appear in 2+ rounds are chained; legs are not compared with `PerformanceAthletes` (Live uses nicknames such as Kierston for the DB's KK). The 5 relay events on Live that have no DB row at the matching time were not touched.

**Key files:** `scripts/athleticnet/build_live_relay_rounds.py`, `docs/imports/2023-2026-live-relay-rounds.sql`.

### [C92] 2023 Early Bird Girls Shot Put: The Kreider Sisters Were Mixed Up on Live

**What changed:** `docs/imports/2023-early-bird-kreider-shot-put.sql` (applied local and live 2026-10-20). Owner's correction from watching the meet: Jayce Kreider threw 22-9, 21-6.5, 20-11.5 (mark 24' 2" -> **22' 9"**, 290 -> 273) and Karoline Kreider threw 24-2, 25-11, 27-2.5 (mark 25' 11" -> **27' 2.5"**, 311 -> 326.5). Attempt series replaced for both (Jayce had the contaminated four 24-2, 22-9, 21-6.5, 20-11.5 from [C88]/[C89]; Karoline had none).

**Why:** MileSplit Live's row for Jayce held attempts from both sisters (it listed 24-2 / 22-9 / 21-6.5 / 20-11.5 / 25-11 / 27-2.5), and the official mark column (24-2 for Jayce, 25-11 for Karoline) was wrong for both. [C89] had kept Jayce's first four attempts; that was wrong too.

**Watch out:** `convert_live.py` now skips both Kreider rows at that meet (`SKIP_ROWS`) so a regeneration cannot bring the mixed row back. The Id-based files `2023-2026-live-attempts.sql` and the older Kreider block assert Jayce's old mark (290) and will refuse to run again. The other Early Bird results for the sisters (discus: Karoline 86-8, Jayce 83-9 on Live; DB 1040 and 1005) were not re-checked against a source and could be mixed up the same way.

**Key files:** `docs/imports/2023-early-bird-kreider-shot-put.sql`, `scripts/athleticnet/convert_live.py`.

### [C93] Dominic Nau's 2023 Ravenna Shot Put Series

**What changed:** `docs/imports/2023-ravenna-nau-shot-put.sql` (applied local and live 2026-10-20): Dominic Nau's 2023 Ravenna-Streetsboro Double Dual shot put keeps the stored mark 31' 0" (372) and now has the series 26-4, 29-9, 30-5, 31-0.

**Why:** MileSplit raw results and Live both say his mark was 38' 2" (series 26-4, 29-9, 30-5, 38-2) while the DB has 31' 0" ([C88]). The owner kept 31' 0" and asked for the series with the 38-2 replaced by 31-0 (this supersedes the "no series stored" note in [C89]). The conflict with the two sources is unresolved; revisit if a better source appears.

**Watch out:** `convert_live.py` skips this row (`SKIP_ROWS`) so a regeneration does not restore 38-2.

**Key files:** `docs/imports/2023-ravenna-nau-shot-put.sql`, `scripts/athleticnet/convert_live.py`.

### [C94] OHSAA State Results (baumspage mirror): Attempt Series and a Full Comparison

**What changed:** the owner pointed at the official OHSAA state results (https://www.ohsaa.org/sports/track/past-results -> one page per year -> `https://www.baumspage.com/ohsaa/tf/<year>/Results1-4.htm`, Div I / II / III / Seated; 2007-2013 use `b1/g1/bd1/gd1.htm`; 2014-2021 also have `MeetProg`/`heat`/`part` files; 1990-2000 are PDFs; 2022 and later have no Hy-Tek pages there). The pages carry the full six-attempt series under each field athlete and the high jump / pole vault ladders. `docs/imports/ohsaa-state-series.sql` adds 7 series (Katelyn Young 2016 discus + shot put and 2017 discus; Brooklynn Crist 2018 and 2021 seated shot put; Bridgette Donahue 2021; Maddie Boltz 2019 long jump). `docs/imports/2019-seated-state-shot-put-series.sql` adds Crist's and Donahue's 2019 series (16-5.75 best / 7-4.75 best). All applied local and live; each best attempt equals the stored mark.

**Why:** these pages explain the earlier Seated State confusion: each row has two numbers, `Seed` then `Finals` (Crist's 100M `20.56  23.85`), and the DB had held the seeds; the athletic.net fix ([C82]) was right.

**Watch out:** in these Hy-Tek pages the first mark is the seed, not a prelim. Every Cloverleaf individual and relay row on the pages we have (39 rows, 2007-2021) matches the DB except the hand-timing quirk (11:30.07 vs the DB's 11:30.7; 51.09 vs 51.9). 2022-2025 state meets, the 2001-2006 pages and the 1990-2000 PDFs were not compared. Only ~10 Cloverleaf field rows carry a series; most state rows are track.

**Key files:** `docs/imports/ohsaa-state-series.sql`, `docs/imports/2019-seated-state-shot-put-series.sql`.

### [C95] OHSAA State Results 2001-2008 and Scanned PDFs (1960-1991)

**What changed:** the 2001-2008 OHSAA pages (`ohsaa.org/sports/tf/<year>/resultslnk.htm` framesets -> per-division pages) and the 1912-2000 PDFs (`/Portals/0/Sports/Track-Field/pastresults/{boys,girls}/`; only 1998-2000 have text layers, the rest were OCR'd with macOS Vision) were compared with the DB. `ohsaa-2001-2008-fixes.sql`: Hutson 267.5 -> 4:27.05 (owner), Beebe prelim 38.92 -> final 38.68 chain, new "2001 OHSAA Division I State Track and Field Championships" (2001-06-01) with Andy Bittner 3200M 9:51.18. `ohsaa-pdf-1960-1991-fixes.sql`: Charles Massie 1994 800M 1:54.60, Scott Murphy 1982 800M 1:55.62, new athlete Scott Chrislip (class of 1973) HJ 6'4" (1972) and 6'2" (1973), Bill Heffelfinger 1960 220 in 22.4, Kim Malarkey 1600M 5:18.10 (1990) and 5:04.92 (1991); new meets/seasons for 1960 Class A, 1972, 1973 and 1982 State (dates are June 1 placeholders; the PDFs print none).

**Why:** the early state meets were stubs. Both files are Id-independent (lookups by name) and re-runnable. Applied locally and live.

**Watch out:** yards are stored as meters (1962 precedent). OCR garbles marks (1973 HJ read as noise; the scan says 6' 2"); verify against the page image. Not added: Chrislip 1972 180-yd low hurdles 23.0 (no such event) and the 1990 girls 4x800 (Kim/Cathy Ocheltree/"Favna" Bender/Tracey Malarkey, 9:31.10, Bender not in DB). Only the winner's time is printed in 1960, so Heffelfinger's 3rd in the 100 has no mark.

**Key files:** `docs/imports/ohsaa-2001-2008-fixes.sql`, `docs/imports/ohsaa-pdf-1960-1991-fixes.sql`.

### [C96] Follow-up to [C95]: 180M Low Hurdles Event and the 1990 Girls 4x800

**What changed:** new boys outdoor event `180M Low Hurdles` (`BOYS_OUTDOOR_180M_LOW_HURDLES`, yards stored as meters, SortOrder 105) with Scott Chrislip's 1972 State 5th-place 23.0; new athlete Favna Bender (spelled that way in the printed 1990 results; class year 1991 is a guess) and the 1990 State girls 4x800 relay 9:31.10 (Kim Malarkey, Cathy Ocheltree, Favna Bender, Tracey Malarkey).

**Why:** the owner asked to add both items skipped in [C95]. Applied locally and live.

**Watch out:** Bender's graduation year is unverified (column is NOT NULL). The relay row's `SortedAthleteHash` is NULL (binary hash not computed).

**Key files:** `docs/imports/ohsaa-pdf-1972-1990-extras.sql`.

### [C97] Meet Page: No "#N" Chip Next to "SR"; Freeland's 1990 State Place

**What changed:** `Views/Meets/Details.cshtml` (3 row templates) no longer shows the all-time `#N` chip when the row already has the `SR` chip (`AllTimeRank == 1` is both, so `#1` was redundant). Dave Freeland's 1990 State 800M now has a `MeetPlacings` row (Place 2, 0 points, no participant) so the meet page shows "2nd".

**Why:** owner pointed out `SR` + `#1` was silly, and a bare `#1` on a state-meet row reads like a placing when he was 2nd (Darrell Hughes won in 1:52.12).

**Watch out:** `MeetPlacings` works for any meet; only the 2026 D3 State and Early Bird have placings, so other state rows show no place. Placings imported this way carry 0 points; the season scoring page is gated by `ScoringEnabled` so it is unaffected.

**Key files:** `CloverleafTrack.Web/Views/Meets/Details.cshtml`, `docs/imports/1990-state-freeland-place.sql`.

### [C98] Finishing Places (MeetPlacings) Backfilled From Every Source on Hand

**What changed:** +2,314 `MeetPlacings` rows (Place, 0 points, no participant) for performances that had none, generated by `scripts/places/` (`live_places.py`, `state_places.py`, `an_places.py`, `ms_places.py`, `pdf_places.py`, shared `places_lib.py`) and applied in that order as `docs/imports/places-{pdf,state,live,an,ms}.sql`. Sources: MileSplit Live finals (`athleticnet/live-raw`), OHSAA state pages (`/tmp/ohsaa/pages`, `/tmp/ohsaa_old`), athletic.net dumps (`/tmp/andir`), MileSplit Hy-Tek texts (`/tmp/msraw`), and 19 hand-read PDF placings (1960-2000).

**Why:** owner: "if you've got finishing places anywhere you should add it" (after the meet page showed `#1` with no placing for Freeland's 2nd at the 1990 State).

**Watch out:** dual/triangular/quad meets and meets with `MeetParticipants` are skipped (places there are per-opponent). Insert-only (`NOT EXISTS` any placing): earlier files win, so Live beats MileSplit text (they disagree for 8 rows at the 2024 Metro meet). `MeetType` is unreliable (300 meets are type 1, including invitationals/state), so the scripts filter on name/participants instead. Names are matched exactly, then by fuzzy first/last name within the meet-event. Not added: the 1986 State girls 4x800 (6th, 9:32.73; no 1986 State meet in the DB) and ~200 unmatched rows (mostly relays whose times differ, athletes missing from the DB). Source dumps are in `/tmp` and not in the repo, so re-running needs the downloads again.

**Key files:** `scripts/places/*`, `docs/imports/places-*.sql`.

### [C99] 1986 OHSAA Class AAA State Meet and the Girls 4x800 Relay

**What changed:** new meet "1986 OHSAA Class AAA State Championships" (1986-06-01 placeholder date, Ohio Stadium, season 1985-1986) with the Lodi Cloverleaf girls 4x800 relay, 6th, 9:32.73: Sue Wagner, Beth Kaniecki, Laura Foradori (existing athlete), Linda Melendez. Three new athletes (class year 1987 is a guess). Follow-up to the unplaced item in [C98].

**Why:** owner asked to add the meet and result. Names read from the page image (the OCR text had the two columns interleaved).

**Watch out:** the 1986 PDF's relay listing is two columns; read legs from the image, not the OCR text. Applied locally and live; no leaderboard impact beyond the new relay row.

**Key files:** `docs/imports/1986-state-girls-4x800.sql`.

### [C100] Team Results (Place / Field Size / Score) Backfilled for Invitationals From Hy-Tek Rankings

**What changed:** 73 `MeetTeamResults` rows (MeetId, Gender, no opponent, OurScore, Place, FieldSize) for 48 non-dual meets, parsed from Hy-Tek "Men/Women - Team Rankings" blocks by `scripts/places/team_results.py`: the MileSplit texts (`/tmp/msraw`) plus the OHSAA state pages. Applied as `docs/imports/team-results-hytek.sql` (insert-only, only where the meet+gender has no row).

**Why:** owner asked for team placing for invitationals, per-opponent placings and meet scores for duals.

**Watch out:** the model already existed (`MeetTeamResults`, shown by `MeetService`): invitational = `OpponentMeetParticipantId NULL` + Place + FieldSize; dual = per-opponent `OurScore`/`OpponentScore`. Existing rows were NOT overwritten, and they disagree with the source for the 2025 Jim Hauser Pirate Relays (DB boys 6th / girls 7th of 8; Hy-Tek says Cloverleaf 4th both, 82.5 / 85 pts) and 2026 Jim Hauser (same 6/7): owner to check. Division-split rankings (MS/Metro/County), partial scoring (<6 events) and XC are ignored. Duals are NOT done: only 3 of the 42 dual meets have a crawled source, and per-opponent placings need the opponents' athletes too.

**Key files:** `scripts/places/team_results.py`, `docs/imports/team-results-hytek.sql`.

### [C101] Per-Opponent Placings for the 2019 and 2023 Ravenna-Streetsboro Double Duals; Why the Other Duals Are Not Done

**What changed:** meets 327 (2019) and 70 (2023) became `MeetType` 2 with the Dual Meet (5-3-1) template, `MeetParticipants` for Ravenna and Streetsboro, and 554 per-opponent `MeetPlacings` (a Cloverleaf performance's rank among the athletes of the two teams; template points; relays split /4), generated by `scripts/places/dual_placings.py` from the full Hy-Tek results (`scripts/crawl/milesplit-raw/ms-354958.txt`, `/tmp/msraw/ms-536722.txt`). `docs/imports/dual-placings-ravenna-streetsboro.sql`.

**Why:** owner asked for per-opponent placings and meet scores for dual meets.

**Watch out:** (1) No team scores: neither source prints dual team totals, and the 2026 duals' `OurScore`/`OpponentScore` (Woodridge 27.5 vs 109.5) come from the scorer sheets, not from summing 5-3-1 points, so computing them would invent numbers. Only `MeetTeamResults` row 288 (Woodridge) exists. (2) 190 Cloverleaf results in these two meets are not in the DB (mostly the 2019 meet, entered partially: 104 performances vs about 270 on the sheet); they still counted in the ranking pool but got no placing. Importing them is a separate job. (3) Of the 42 dual-type meets only 2019 and 2023 Ravenna-Streetsboro, and 2026 Woodridge/Ravenna, have results on MileSplit/athletic.net/Live; 2021-2025 duals are not online. The 2026 duals (286-294) already had participants and placings from the admin flow; 286, 290, 292, 294 still lack `MeetTeamResults`. (4) Live's team-scores page is empty for duals.

**Key files:** `scripts/places/dual_placings.py`, `docs/imports/dual-placings-ravenna-streetsboro.sql`.

### [C102] Dual-Meet Score Sheets: Placings and Team Scores for 2023-2025 Duals

**What changed:** from the owner's score sheets (`scripts/duals/`): `sheets_build.py` reads the xlsx/text-PDF sheets (2023 Woodridge, 2024 Field-Coventry double dual, 2025 Woodridge girls, 2025 Springfield girls) and writes per-opponent `MeetPlacings` (top 3 per event, the sheet's points incl. tie splits, relay 5 to the winner) and `MeetTeamResults` (official totals); `dual_ranker.py` ranks fully transcribed event results (2025-04-15 Woodridge, from `scan_ggucker_2025-04-15`, `data/2025-04-15-woodridge.txt`) into placings for every Cloverleaf finisher. The meets became `MeetType` 1/2 with the Dual 5-3-1 template and opponent `MeetParticipants`. Files: `docs/imports/dual-sheets-2023-2025.sql`, `dual-2025-04-15-woodridge.sql`.

**Why:** owner sent the dual score sheets and asked for placings, team scores and series.

**Watch out:** (1) The 2025-04-15 girls total recomputed from the transcribed event sheets (63.33-73.67) equals the official sheet exactly, which validates the transcription and the scoring rule (5-3-1 individual with ties sharing points, relays 5 to the winner only); the boys total (45-92) is COMPUTED the same way (no boys sheet was supplied; HJ/PV ladders read with the inverted X-clear/O-miss notation). (2) The DB stores many distance times truncated to whole seconds (sheet 5:30.6 vs DB 5:30.0, relays too): `data/refine-2025-04-15.txt` lists 37 candidate refinements, NOT applied. (3) Review items not applied: Cavey 400 1:07.9, Ethan Gray LJ 17-5, Goetz (no athlete record) 100/200/400, JV Mickovsky discus are missing from the DB; sheet-vs-DB mark differences (Jensch 800 2:27.5 vs 2:37.5, J. James 800 2:16.6 vs 2:10.6, R. Evans 2023 100 12.3 vs 12.54, Ellebruch 2025-04-22 HJ 4'4" vs 4'6", Spencer 800 3:24.7 vs 3:34, Messmore discus). (4) Scanned scorer sheets have mixed page rotation (track pages need rotate 270, field forms 0, vertical-jump forms 270) and the field forms hold attempt series for the shot/discus/LJ events; only 49 series for 2025-04-15 were matched (not yet imported).

**Key files:** `scripts/duals/*`, `docs/imports/dual-sheets-2023-2025.sql`, `docs/imports/dual-2025-04-15-woodridge.sql`.

### [C103] Dual-Meet Scans Transcribed (2021-2025): Placings, Team Scores, Attempt Series, Ladders

**What changed:** the owner's scorer sheets (xlsx, text PDFs and ~140 scanned pages) for six dual meets were transcribed (`scripts/duals/data/*.txt`) and imported: 2021 Woodridge (placings from the score sheet, 34 series, 2 PV ladders), 2023 Woodridge, 2024-04-16 Norton-Springfield (356 placings, 42 series), 2024-04-23 Field-Coventry (39 series, 6 HJ ladders), 2025-04-15 Woodridge (150 placings), 2025-04-22 Norton-Springfield (326 placings, 60 series) and 2025-05-06 New London (76 placings, 49 series). Tools: `scripts/duals/dual_ranker.py` (ranks fully transcribed results into per-opponent placings; ties share points; relay winner 5), `sheets_build.py` (top-3 score sheets + official totals), the existing `build_scan_sql.py`/`build_bars_sql.py` for series/ladders. SQL: `docs/imports/dual-*.sql`.

**Why:** owner asked for placings, team scores and series from these sheets.

**Watch out:** (1) Official totals are stored only where a sheet gave them (girls 2025-04-15 63.33-73.67, girls 2025-04-22 118-18 (Springfield), 2025-05-06 girls 88-43, 2024-04-16 girls vs Norton 97-31, 2024-04-23 duals, 2023 Woodridge, 2021 Woodridge 56-72/77-51). `dual_ranker.py --scores` totals (2025-04-15 boys 45-92) are computed, not official. (2) Series use `build_scan_sql.py`'s best-mark assertion: where a handwritten attempt could not be read (upside-down boys 2021 sheets) the series is skipped or its last digits were set to the DB mark; treat 2021 attempts as approximate. (3) Not imported: 2021 boys discus/HJ/most PV ladders and the 2021 relay legs (images too rotated/unclear), the 2025-04-15 boys HJ/PV ladders, 190 results still missing from the 2019/2023 Ravenna duals, and the DB's truncated distance times (`data/refine-2025-04-15.txt`). (4) Scan pages need per-page rotation (0/90/180/270): use `/tmp/orient2.py`-style OCR scoring; some come out upside-down and need a 180 flip.

**Key files:** `scripts/duals/*`, `docs/imports/dual-*.sql`.

### [C104] Ravenna-Streetsboro Duals: 89 Missing Results Imported and Placings Redone (Parser Bug)

**What changed:** (1) `dual_ranker`-era bug in `scripts/places/dual_placings.py` ([C101]): for the 2019 Hy-Tek file (columns `Seed Finals H# Points`) it ranked by the last numeric token (the heat number) instead of the Finals mark, so the 208 per-opponent placings of meet 327 were wrong; fixed (mark tokens must contain `.`, `:` or `-`; Finals = the 2nd mark when a Seed column exists). (2) `dual_missing.py` inserts the 89 Cloverleaf individual results of the 2019/2023 Ravenna-Streetsboro duals that the DB lacked (`docs/imports/dual-ravenna-1-missing-results.sql`), then `dual-ravenna-2-placings.sql` deletes and re-creates every per-opponent placing of both meets (now 738 rows).

**Why:** discovered while importing the missing results: the 2019 placings had places like 18-24 for sprinters (heat numbers).

**Watch out:** apply the two files in that order (second one needs the new performances); they declare the same variables, so run them as separate scripts. Not imported: relays (no leg-to-athlete mapping), Dylan Bresnahan 2023 200M (athlete not in DB). Marks are the Hy-Tek Finals column; 2019 hand-timed marks print as tenths.

**Key files:** `scripts/places/dual_placings.py`, `scripts/places/dual_missing.py`, `docs/imports/dual-ravenna-*.sql`.

### [C105] 2025 Woodridge Dual: Truncated Times Replaced With the Scorer Sheet's Values

**What changed:** 27 times in the 2025-04-15 Woodridge dual (21 individual, 6 relay) were whole-second values (e.g. 5:30.0) and were replaced with the scorer sheet's tenths (5:30.6), read from `scan_ggucker_2025-04-15`. `docs/imports/dual-2025-04-15-time-refinements.sql` (matches on the old time; re-runnable). Old values:
- Austin McNeal Boys 1600M: 310 -> 310.7
- Noah Mion Boys 1600M: 317 -> 317.2
- Clay Gucker Boys 1600M: 330 -> 330.6
- Mason Clapp Boys 1600M: 332 -> 332.9
- Cayden Williams Boys 1600M: 335 -> 335.8
- Luke Evans Boys 1600M: 367 -> 367.8
- Matthew Wilk Boys 1600M: 370 -> 370.8
- Kyle Mahoney Boys 1600M: 415 -> 415.8
- Clay Gucker Boys 3200M: 692 -> 692.8
- relay Boys 4x200M Relay: 95 -> 95.8
- relay Boys 4x400M Relay: 214 -> 214.6
- relay Boys 4x800M Relay: 502 -> 502.5
- Ben Myers Boys 800M: 127 -> 127.7
- Matthew Jenkins Boys 800M: 129 -> 129.3
- Chayton Bullis Boys 800M: 169 -> 169.3
- Matthew Wilk Boys 800M: 173 -> 173.1
- Aiden Mullinex Boys 800M: 178 -> 178.9
- Kyle Mahoney Boys 800M: 217 -> 217.7
- Addison Fallow Girls 1600M: 403 -> 403.3
- Olivia Spencer Girls 1600M: 453 -> 453.4
- Emma Hibler Girls 3200M: 724 -> 724.34
- Kinsey Bayer Girls 400M: 74 -> 74.09
- relay Girls 4x400M Relay: 268 -> 268.1
- relay Girls 4x400M Relay: 300 -> 300.3
- relay Girls 4x800M Relay: 756 -> 756.1
- Emma Hibler Girls 800M: 164 -> 164.6
- Addison Fallow Girls 800M: 173 -> 173.2
**Why:** the DB values were truncated; the owner approved taking the sheet values.

**Watch out:** the sheet values are a human reading of handwriting; ranks/percentiles may shift slightly (leaderboards rebuilt). The other dual meets may have the same truncation (not checked).

**Key files:** `docs/imports/dual-2025-04-15-time-refinements.sql`, `scripts/duals/data/refine-2025-04-15.txt`.

### [C106] More Time Refinements, OHSAA State Team Results, and the State PDF Sweep

**What changed:** (1) `docs/imports/dual-2024-04-16-time-refinements.sql`: 7 whole-second times in the 2024-04-16 Norton-Springfield dual replaced with the scorer sheets' tenths/hundredths. Old values:
- Caleb Neville Boys 3200M: 720 -> 720.99
- Noah Mion Boys 3200M: 776 -> 776.28
- Austin McNeal Boys 3200M: 776 -> 776.69
- Ben Myers Boys 800M: 130 -> 130.1
- Matthew Jenkins Boys 800M: 133 -> 133.8
- Chayton Bullis Boys 800M: 162 -> 162.8
- Megan Knapp Girls 3200M: 806 -> 806.1(2) `docs/imports/ohsaa-state-team-results-1973-1995.sql`: 9 `MeetTeamResults` rows (Lodi Cloverleaf's team place/points at the State meet) read from the OCR'd PDFs: 1973 boys 29th (no score), 1982 boys 48th 1, 1986 girls 52nd 1, 1990 girls 36th 3, 1991 boys 11th 10 and girls 38th 2, 1992 boys 14th 10, 1993 boys 5th 26, 1995 boys 30th 6. Field size not recorded. Skipped as unreadable: 1960, 1972, 1990 boys (rank digit lost in OCR), 1994 boys. (3) A fresh sweep of all 103 OCR'd state PDFs for Lodi/Cloverleaf (incl. OCR variants) found nothing beyond what [C95]/[C96]/[C99] already imported.

**Why:** owner asked to continue the next pass in order (truncated times, then more PDFs, then relay legs).

**Watch out:** whole-second times in the other dual meets (2021-2023 duals have about 100 per year) have no tenths source on disk, so they stay as they are; they are whole seconds because the scorer wrote them that way, not an import truncation. The only relays still short of four named legs are three with 3 legs (2017 Orange DMR, 2022 Woodridge 4x200, 2023 New London 4x200) and have no source.

**Key files:** `docs/imports/dual-2024-04-16-time-refinements.sql`, `docs/imports/ohsaa-state-team-results-1973-1995.sql`.

### [C107] Dual-Meet Sheets Win Over the DB Marks; Missing Results; Groetz Alias

**What changed:** owner decisions after [C103]: "Goetz" on the sheets is Gage Groetz (already in the DB; the placings that had skipped him were re-generated), and where a scorer sheet and the DB disagree the sheet wins. `docs/imports/dual-sheet-marks-and-missing.sql` added 5 results (Brooklyn Cavey 400 1:07.9, Ethan Gray LJ 17'5", Issy Mlckovsky discus 26'3", Kinsey Bayer 200 28.5, Grady Pamer 110H 19.81) and changed 15 marks. Old values:
- added Brooklyn Cavey 400M 67.9
- added Ethan Gray Long Jump 209
- added Issy Mlckovsky Discus 315
- added Kinsey Bayer 200M 28.5
- added Grady Pamer 110M Hurdles 19.81
- Lauren Jensch 800M: 157.5 -> 147.5
- Jackson James 800M: 130.6 -> 136.6
- Riley Evans 100M: 12.54 -> 12.3
- Olivia Spencer 800M: 214 -> 204.7
- Izzy Messmore Discus: 500.5 -> 530.5
- Clayton Sewalish 200M: 27.65 -> 27.45
- Amir Usry 200M: 32.58 -> 33.58
- Emily Ellebruch High Jump: 54 -> 52
- Jackson Murray-Garver 400M: 62.9 -> 62.4
- Blake Thorne Shot Put: 317.5 -> 323
- Jack Chonko Shot Put: 420.75 -> 421.75
- Maclaine Bell Discus: 580.5 -> 583.5
- Reese Barnett Long Jump: 180.75 -> 183.25
- Ethan Martin Shot Put: 354 -> 355
- Presly McConnell Discus: 1229 -> 1231Then `dual-*-v2.sql` deleted and rebuilt the per-opponent placings of the three 2025 meets and `dual-sheet-marks-delete-series.sql` + `dual-*-series2.sql` re-imported 6 attempt series whose best mark was corrected.

**Why:** owner: trust the sheet over the DB marks.

**Watch out:** Jayce Kreider's 2024-04-16 discus (sheet 78'9" vs DB 70'9") was NOT changed: the digit is ambiguous in the scan. 2021 Woodridge sheet-vs-DB differences (Brilla/Jarvis LJ) were left alone (upside-down handwriting). The 2021 boys discus/shot attempts, most 2021 pole vault ladders and the 2021 relay legs are still unread; the owner was shown the three images.

**Key files:** `docs/imports/dual-sheet-marks-and-missing.sql`, `docs/imports/dual-*-v2.sql`, `docs/imports/dual-*-series2.sql`.

### [C108] 2021 Woodridge Dual: Owner-Read Boys Shot/Discus Series and Pole Vault Ladders

**What changed:** the owner read the three rotated scans of the 2021-04-13 Woodridge dual. Imported: boys discus series (11 athletes), boys shot put series (10 athletes, replacing my earlier approximate readings; Greer and Yuravak are new), and pole vault ladders for McKenna Tabor (5' XO, 5'6 O, 6' XXX), Cassidy Smith (P P P P at 5-6.5, then 7' O, 7'6 XXO, 8' XO, 8'6 O, 9' XXX) and Isaac Baltic (7' O, 7'6 XXX). `docs/imports/dual-2021-04-13-owner-read-{pre,series,bars}.sql` (apply in that order).

**Why:** OCR/my reading of the upside-down 2021 boys sheets was unreliable.

**Watch out:** Isaac Baltic's pole vault mark was changed from 7'6" (90) to 7'0" (84) because the sheet ladder shows 7'6 XXX (sheet wins over the DB, per the owner's standing rule). Ribnikar's third discus attempt was read as "989-9" and entered as 98-9 (typo for 98'9"; not his best). Every series' best attempt equals the stored mark (verified by `build_scan_sql.py`'s assertion): 11/11 discus and 10/10 shot put matched. Goldsberry and Kott's 5' XXX (no height) have no DB rows and were not added. Still unread: 2021 relay legs.

**Key files:** `docs/imports/dual-2021-04-13-owner-read-*.sql`, `scripts/duals/data/2021-04-13-{sheets,bars}.txt`.

### [C109] 2021 Woodridge Dual: Baltic Corrected Back to 7'6", Goldsberry and Kott No-Mark Pole Vault Rows

**What changed:** supersedes the Baltic part of [C108]. The owner had mistyped: Isaac Baltic cleared 7'6" (O) and missed 8' three times, so his pole vault mark is 7'6" (90) again, ladder 7' O, 7'6 O, 8' XXX (the 7' O is assumed from the earlier reading). Grace Goldsberry and Avery Kott now have girls pole vault rows with `ResultStatus` 1 (NM, as the owner asked) and the ladder 5' XXX. `docs/imports/dual-2021-04-13-baltic-and-nm-pv.sql`. The owner also confirmed Ribnikar's "989-9" discus attempt is a typo for 98-9 (already entered that way).

**Watch out:** the owner called these "NM" (status 1); a pole vault with no cleared height would normally be NH (status 2). Both render as a muted pill with the bar ladder; change the status to 2 if the label should read NH.

**Key files:** `docs/imports/dual-2021-04-13-baltic-and-nm-pv.sql`.

### [C110] 2021 Woodridge Dual PV: Baltic Started at 7'6", Goldsberry and Kott Are NH

**What changed:** supersedes the ladder/status parts of [C108]/[C109]. The pole vaulters started at 7'6", so Isaac Baltic's ladder is 7'6 O, 8' XXX (no 7' bar; mark stays 7'6"). Grace Goldsberry and Avery Kott are no-height (`ResultStatus` 2, NH), ladder 5' XXX. `docs/imports/dual-2021-04-13-baltic-nh-fix.sql`.

**Why:** owner corrections.

**Key files:** `docs/imports/dual-2021-04-13-baltic-nh-fix.sql`.

### [C111] Seated Events Never Count Toward Team Scores

**What changed:** owner rule: "don't include seated results in team scores, ever". `AdminMeetPlacingRepository.GetTemplatePointsAsync` now returns 0 for any event whose name starts with `Seated`; `SeasonScoringRepository.GetScoringDataForSeasonAsync` excludes seated events from both UNION branches (season scoring page); the dual-meet scripts (`scripts/duals/dual_ranker.py`, `scripts/places/dual_placings.py`) skip seated events, so they add no points to computed totals. CLAUDE.md gains a gotcha.

**Why:** seated athletes compete in separate events and their points must not feed team totals.

**Watch out:** no existing data needed fixing: every current `MeetPlacings` row on a seated event already has 0 points (checked locally). Placings (place badges) for seated athletes still display; only points/totals ignore them. `MeetTeamResults` rows are typed/imported values, so the rule applies when computing them, not when reading them. Any new place that sums points must add the `NOT LIKE 'Seated%'` filter.

**Key files:** `CloverleafTrack.DataAccess/Repositories/AdminMeetPlacingRepository.cs`, `SeasonScoringRepository.cs`.

### [C112] Dual-Meet Relays Score 5-0

**What changed:** in dual and double-dual meets only the winning relay scores (5; Split 1.25); 2nd/3rd relays earn 0 (the score sheets say "3 points - 0 relay"). `AdminMeetPlacingRepository.GetTemplatePointsAsync` now returns 0 for relay events with Place > 1 when the resolved template is the built-in "Dual Meet%" template (invitational templates are untouched: relays still score places there). `scripts/places/dual_placings.py` does the same. `docs/imports/dual-relay-points-5-0.sql` fixed 25 relay placings from the 2019/2023 Ravenna-Streetsboro import that had 3/1 points for 2nd/3rd (local and live).

**Why:** owner: "it should be 5-0" (a screenshot of the 2021 Woodridge meet page showed relay placings).

**Watch out:** the sheet-derived imports (2021-2025) already scored relays 5-0. Relay B-team entries (second Cloverleaf relay in the same event) have no placing rows in the 2021 Woodridge dual: only the best team per event was matched; they are not scored anyway.

**Key files:** `CloverleafTrack.DataAccess/Repositories/AdminMeetPlacingRepository.cs`, `docs/imports/dual-relay-points-5-0.sql`.

### [C113] Dual Team Scores Computed From Per-Opponent Placings

**What changed:** `scripts/places/dual_scores.py` computes a team score for each dual/double-dual meet, gender and opponent from the stored per-opponent placings: ours = sum of Cloverleaf `FullPoints` (a relay is one row whose FullPoints is the team's 5; SplitPoints is the per-member share), possible = 9 per scored individual event + 5 per relay, opponent = possible - ours (valid when every scoring place was filled, true for the official sheets' "137 possible, 128 without pole vault"). It inserts a `MeetTeamResults` row only where none exists and only when the placings are complete: every non-pole-vault individual event with a Cloverleaf performance has a placing for that opponent, every relay is placed, and pole vault is either placed or absent. 19 rows inserted (2019/2023 Ravenna-Streetsboro, 2024 Norton-Springfield boys + girls vs Springfield, 2025 Norton-Springfield boys, 2026 Ravenna girls vs Ravenna, Norton-Springfield girls vs Springfield, Field-Coventry boys vs Coventry and girls vs Coventry, New London). `docs/imports/dual-computed-team-scores.sql`.

**Why:** owner: add scores to dual meets whose placings are complete.

**Watch out:** these are COMPUTED, not the scorer's totals. Checked against the 15 official rows: matches exactly where placings are complete except ties (1 point) and pole vault (whether it was scored) — 2025 Springfield girls official 118-18 vs computed 109-19 because PV was scored but has no placings. Skipped meets (placings incomplete, e.g. 2026 Ravenna boys, Field girls vs Field, 2025 New London boys, 2025 Norton girls PV): they need placings for the missing events first. `MeetTeamResults` rows that already exist (official totals) are never overwritten.

**Key files:** `scripts/places/dual_scores.py`, `docs/imports/dual-computed-team-scores.sql`.

### [C114] New London Throws With the Sheets' Places, Norton-Springfield Girls Pole Vault, and Scores for Every Meet With Placings

**What changed:** (1) The 2025-05-06 New London sheets DO carry a Place column (overall places of both teams, circled for scorers); I had only transcribed our attempts and missed New London's throwers and the places. All four throw events (boys/girls shot put and discus) now include New London's marks, so `dual_ranker.py` re-created the per-opponent placings (122; `docs/imports/dual-2025-05-06-new-london-v3.sql`). (2) The 2025-04-22 vertical-jump form shows women's pole vault Shetterly (8'6", place 5 pts) and Thur (7'6", 3 pts) with no Norton or Springfield vaulters: pole vault placings added (`...norton-springfield-v3.sql`, 334 placings; girls vs Springfield now computes 118-18 minus 1, vs official 118-18). (3) `dual_scores.py` no longer skips meets with "incomplete" placings: per the owner, a missing placing means the athlete did not score; `possible` = 9 per individual event with a Cloverleaf performance (pole vault only if it has placings, since it is sometimes unscored) + 5 per relay; 10 more `MeetTeamResults` rows inserted (`docs/imports/dual-computed-team-scores-2.sql`).

**Why:** owner: sheets show top-3 places (only top 3 score in a dual); score every meet that has placings; Springfield had no vaulters.

**Watch out:** I wrongly called the 2026 "no placing" cases suspicious; the 2026 admin flow stores scorers only, so absence = 0 points (lost relays included). Remaining differences vs official totals: 2025 New London girls official 88-43, computed 79-44 (an event or two missing from the transcribed data), 2021 Woodridge boys 56-72 vs 47-81, 2024 Springfield/Norton girls and 2025 Springfield girls off by 1-2 points (ties). Official rows are never overwritten.

**Key files:** `scripts/duals/data/2025-05-06-new-london.txt`, `scripts/places/dual_scores.py`, `docs/imports/*-v3.sql`.

### [C115] Checked Computed Dual Scores Against the Sheets: 2021 Woodridge Boys and 2025 New London Girls

**What changed:** the owner asked to compare the two meets whose computed score disagreed with the official sheet. Per-event comparison against the scorer sheets: (1) 2021 Woodridge boys (official 56-72): three Cloverleaf scorers were missing placings only because my transcription mis-spelled names/times (Herdman 800 3rd -> "Hardman", Yuravak 200 2nd -> "Vircuak", 4x400 relay time 3:38 vs the DB's 3:35.9); fixed in `scripts/duals/data/textsheets.txt` and re-imported (`docs/imports/dual-sheets-2023-2025.sql`, insert-only); the computed score now equals the official one (and the girls too). (2) 2025 New London girls (official 88-43): the sheet scores the 3200 (Spencer 1st, 5) and the 4x400 relay (CLO 1st, 5) but the sheet gives no times for them, so the DB has no result rows for those two events and no placings can be created: that is the 10-point difference (computed 78-43 vs 88-43). The sheet's pole vault is Shetterly 1st, Waugaman (NL) 2nd, Thur 3rd (placings redone: `dual-2025-05-06-new-london-v4.sql`). Official totals stand. Springfield/Norton girls differences are tie handling (owner agrees).

**Watch out:** a scored event without times on the sheet (3200, 4x400 relay at the 2025 New London dual) cannot be imported as a result; ask for the times if they exist elsewhere. Boys 2025 New London has no official total (computed 97-25).

**Key files:** `scripts/duals/data/textsheets.txt`, `docs/imports/dual-2025-05-06-new-london-v4.sql`.

### [C116] Final One-by-One Mark Review (Dual Meets) and a Correction of My Earlier "Sheet Wins" Pass

**What changed:** `docs/imports/dual-mark-review-final.sql` (+ `dual-mark-review-series-{68,226,275}.sql`). Owner decisions: Jayce Kreider 2024-04-16 discus 70'9" -> 78'9" (the sheet's 2nd attempt, 6th place); Allison Yagersz 800 2:39.3 -> 2:39.8; Vanessa Monhemius 800 2:53.5 -> 2:53.0; Mallory Bohan 400 76.66 -> 76.6; Luke Evans 2025-05-06 800 2:45.2 -> 2:48.2; kept the DB for Monhemius 1600 (6:16) and Southam 400 (85.1).

**Correction of [C107]:** six of its 15 "sheet wins" changes were wrong and are reverted, because I had changed marks to the best of my own transcription of the ATTEMPTS, but the sheets' "Best" column differs from the attempts I read: Blake Thorne shot 323 -> 317.5 (26'5.5"), Jack Chonko shot (2025-04-22) 421.75 -> 420.75 (35'0.75"), Presly McConnell discus (2025-05-06) 1231 -> 1229 (102'5"), Maclaine Bell discus 583.5 -> 580.5 (48'4.5"), Reese Barnett LJ 183.25 -> 180.75 (15'0.75": the form writes "15 | 3/4" for feet | inches), Ethan Martin shot 355 -> 354 (the sheet's Best column says 29'6" although the second attempt reads 29'7"; the series is stored as 29'6", 29'6" and is approximate). Series of all those throws were deleted and re-imported with the corrected attempts.

**Watch out:** the rest of [C107]'s changes (Spencer 800, Messmore discus, Sewalish/Usry 200, Murray-Garver 400, Jensch, James, Evans 2023, Ellebruch HJ 4'4") came from the xlsx score sheets or my reading of scans and were NOT re-verified against Best columns in this pass; the 2025 Norton-Springfield Chonko attempts and the Mahoney 1600 (sheet 7:25 vs DB 7:22, struck-through digit) are also unverified. Rule going forward: for field events the sheet's Best column is the mark, not my reading of the attempts.

**Key files:** `docs/imports/dual-mark-review-*.sql`.

### [C117] Last Unsure Marks Checked With the Owner

**What changed:** `docs/imports/dual-mark-review-final-2.sql`: Olivia Spencer 2025-04-15 girls 800 204.7 -> 214.7 (3:34.7; the original DB had 3:34 truncated and my "3:24.7" was a misreading); Clayton Sewalish 200 27.45 -> 27.65 and Amir Usry 200 33.58 -> 32.58 (back to the original DB values; my sheet reading was wrong). Confirmed as already correct: Izzy Messmore discus 44'2.5", Mahoney 2025-04-22 1600 stays 7:22.

**Why:** owner reviewed the images one by one. Everything flagged unverified in [C116] is now settled except the xlsx-sourced marks (Jensch, J. James, R. Evans 2023, Ellebruch), which came from the scorers' own spreadsheets, and were left as they are.

**Watch out:** I sent the owner the wrong page for the 200 first; scan page numbers change between orientation passes, so identify a page by its content (event heading) when quoting it.

**Key files:** `docs/imports/dual-mark-review-final-2.sql`.

### [C118] PR-at-the-Time and School-Record-at-the-Time (Credit for Growth)

**What changed:** three new `Performances` columns, rebuilt by a new proc `sp_RebuildRecordHistory` that `sp_RebuildLeaderboards` calls as Step 13 (same transaction): `PRAtTime` (beat every earlier mark by this athlete in this event, or is their first mark there), `PreviousBest` (their best earlier mark, seconds/inches; NULL on a debut), `SRAtTime` (beat every earlier mark by anyone in the event, relays included; the event's first mark counts). Migration + live patch of the main proc: `docs/migration_record_history.sql`; `docs/schema.sql` updated. The meet page, athlete season lists, meet/season/home PR and SR counts, the home highlight, and the meet fact sheet's `pr` flag now read these instead of `PersonalBest` / current `AllTimeRank == 1`. New chips: PR tooltip shows the previous best ("First recorded mark" on a debut); a muted `SR` shows a record that was broken then but is not the record now (`WasSchoolRecord`).

**Why:** `PersonalBest` is "current best only" (cleared by the next improvement), so a PR that was later beaten lost its chip and its count, and `PRCount`/`TotalPRs` looked like only 2025-26 had PRs ([C52]). Growth beyond a single data point (Jane PR'd 12.5 -> 12.0 -> 11.5) was invisible. Owner decisions: a debut counts as a PR ("first"), all eras use the per-athlete check (not only the complete era that MeetScore requires), and the logic is stored in the proc rather than computed on read.

**Rules baked into the SQL:** "earlier" = by meet day, then meet time of day (23 meets carry one [C77]), then the round the mark was run in (`PerformanceRounds.Round` of the round equal to the stored best: Prelim 1 < Semi 2 < Final 3; no round rows = 3), and only then best-first and Id; the comparison is strictly better (ties are not PRs); no-mark rows are ignored; PR is individuals only (relays have no athlete); SR includes relays. This ordering matters for SR across athletes in one meet: a prelim-best 21.9 precedes another athlete's final 21.8, so both are records when set (best-first alone would have denied the first). **The first mark in an event IS an SR** (owner: the earliest records in the database are simply the first marks gathered) as well as a PR for its athlete.

**Not changed on purpose:** the Personal Records table and the `PersonalBest` flag stay "current best" ([C8], [C9]); `IsSchoolRecord`/`AllTimeRank == 1` still means the CURRENT record (row tint, `text-sr`); the event page's `WasRecordAtTime` is still computed in C#; MeetScore still derives PRs itself ([C45]). `IsPersonalBest` on `MeetPerformanceViewModel` / `IndividualPerformanceViewModel` now means PR-at-the-time (name kept to avoid touching every view). The AI fact sheet's `pr` is set only when `PreviousBest != null` (a real improvement), so recaps don't narrate a debut as a "personal best"; `MeetSummaryPrompt.Version` was not bumped because the shape did not change.

**Watch out:** (1) run the migration BEFORE deploying the code (queries select the new columns), then `EXEC sp_RebuildLeaderboards` to backfill. (2) With all-era per-athlete checks, an old athlete's first mark in the DB is a "PR" even if the DB lacks their earlier results; counts for pre-2021 seasons are therefore inflated and `PreviousBest` can be a mark that was not their true prior best. (3) The patch in the migration edits the live proc text by replacing its single `COMMIT TRANSACTION;`; if that proc is ever rewritten from `schema.sql`, keep the `EXEC dbo.sp_RebuildRecordHistory;` line. (4) Tested against Azure SQL Edge in Docker with a small fixture (ties, same-day, relay, no-mark, field); the full proc has still not been run end to end.

**Applied live 2026-10-23:** migration run on `svp01sql01`/`ctf`, then `EXEC sp_RebuildLeaderboards` (first full end-to-end run of the proc since [C31]; 13,975 marks -> 5,759 `PRAtTime` (2,392 debuts), 412 `SRAtTime`). Checks: every `Leaderboards` rank 1 has `SRAtTime`; no `PreviousBest` is worse than its mark; the 86 current `PersonalBest` rows without `PRAtTime` are all exact ties with an earlier mark (ties are not PRs).

**Key files:** `docs/migration_record_history.sql`, `docs/schema.sql`, `Models/Performance.cs`, `DataAccess/{Meet,Athlete,Home,Performance}Repository.cs`, `Services/{Meet,Athlete,Season}Service.cs`, `Services/MeetSummaryPrompt.cs`, `Views/Meets/Details.cshtml`, `Views/Roster/Details.cshtml`, tests in `MeetServiceTests` / `AthleteServiceTests`.

### [C119] Faded PR Chip for Superseded PRs

**What changed:** the PR chip is full strength only while the mark is still the athlete's best; a PR-at-the-time ([C118]) that was later beaten renders at `opacity:0.55`, the same treatment as a broken school record. New `IsCurrentPersonalBest` (= the current `PersonalBest` flag) on `MeetPerformanceViewModel` and `IndividualPerformanceViewModel`. The `row-pr` / `text-pr` tint also follows the current best only, matching the SR row tint. On the athlete season accordion the summary chip is faded unless some mark in that event's season is still the current best.

**Watch out:** `PersonalBest` is current-best per athlete and event, so a tied best keeps both marks full. Opacity is an inline style (no Tailwind rebuild needed).

**Key files:** `Views/Meets/Details.cshtml`, `Views/Roster/Details.cshtml`, `Services/{Meet,Athlete}Service.cs`.

### [C120] Admin Performances Index Failed: 2100-Parameter Limit

**What changed:** `AdminPerformanceRepository.GetAllWithDetailsAsync` fetched relay team names with `WHERE pa.PerformanceId IN @PerformanceIds`, passing every relay performance id. Dapper expands a list into one SQL parameter per element and SQL Server rejects a request over 2,100 parameters; once relay performances passed that count the page returned a 500 ("The incoming request has too many parameters"). The query now selects relay performances with a subquery (`IN (SELECT Id FROM Performances WHERE AthleteId IS NULL)`), no parameters.

**Watch out:** never pass an unbounded id list to Dapper `IN @list`. The other `IN @...` callers (rounds/attempts/bars lookups by performance, summaries by athlete/meet, event ids) take per-athlete or per-meet lists and are well under the cap. The Index still loads all ~14,000 performances unpaged (a ~17 MB page); that is a separate usability problem.

**Key files:** `DataAccess/Repositories/AdminPerformanceRepository.cs`.

### [C121] Admin Performances: Season / Meet / Search Filters and a 500-Row Cap

**What changed:** `/Admin/Performances` takes `seasonId`, `meetId`, `search` (athlete, relay member or event) and `all`. With no filter on first visit it shows the current season; the filter form always sends `all=true`, so "All seasons" really is all. Results are newest-first and capped at 500 (the repository returns `limit + 1` rows so the page can say it was cut off). A chosen meet overrides the season filter. New `PerformancesIndexViewModel`; `GetAllWithDetailsAsync(seasonId, meetId, search, limit)` builds its WHERE clause from the supplied filters only.

**Why:** the unfiltered page was ~14,000 rows / 17 MB ([C120]). This is step 1 of streamlining the admin flows (meet workspace, entry grid and a staged import of AI-prepared results come next); the owner does not want AI inside the site, only a review screen for output prepared in chat.

**Watch out:** the current season alone exceeds 500 rows, so the default view is the latest 500 of it; pick a meet to see one meet in full. The relay-names aggregate still runs event-wide (one cheap query), not just for the loaded rows.

**Key files:** `AdminPerformanceRepository.cs`, `PerformancesController.cs`, `Views/Performances/Index.cshtml`, `ViewModels/Admin/Performances/PerformancesIndexViewModel.cs`.

### [C122] Admin Meet Results Import (review-then-apply, AI stays outside the site)

**What changed:** `/Admin/MeetImport?meetId=N` (buttons: "Import file" on a meet's Entries page, "Import" on the Meets list). Paste or upload a **meet-results file** (`docs/meet-results-format.md`: JSON, version 1) holding `results` (event, athlete or relay legs, mark or NM/NH/DNF/DQ, places) and/or pre-meet `entries`. `MeetImportPlanner` (pure, `CloverleafTrack.Web/Import`) resolves each row to an event, athlete(s), parsed mark and placings and labels it **New / Adds places / Already there / Differs / Needs attention**; the Review grid lets the admin tick rows (Differs = replace, off by default) and pick an athlete for near-miss names; **Apply** re-plans from the same file (stateless, nothing is staged in the DB), writes everything in ONE transaction via `IAdminMeetImportRepository.ApplyAsync`, then runs `sp_RebuildLeaderboards` once. Team points use `GetTemplatePointsAsync` (seated = 0, dual relays 5-0), split points = full / relay size. `GET /Admin/MeetImport/Context?meetId=N` returns the meet, opponents, exact event names, plausible athletes and stored data as JSON for whoever prepares the file. New skill `.claude/skills/prepare-meet-results` is that preparation (scans, athletic.net, Hy-Tek, partial away duals).

**Why:** the owner's flows (season/meet setup, pre-meet entries, post-meet results) were one form submit per athlete-event, and every per-row repository call rebuilds the leaderboards (~3 s). Results arrive as Coach Gucker's scans (hosted duals), athletic.net (invitationals) and random away-dual lists (sometimes top 3 only). The owner does NOT want AI inside the site: AI work happens in chat, the site only reviews and applies its output.

**Rules worth knowing:** results link to a matching unlinked pre-meet entry (same event + athlete, or same relay set) or create an entry so the Entries page shows them; entry-only rows are written first. Event names resolve loosely (`100`, `100 m`, `4x100`, `shot`) per gender within the meet's environment. Athlete names must match exactly one athlete (era-aware tie-break); anything else is a pick-list. Duals: `placings` per opponent, a bare `place` only when there is exactly one opponent; invitationals: one overall place. A fifth event for an athlete is a warning, never a block. Applying the same file twice shows "Already there".

**Watch out:** version 1 has no rounds, attempt series or bar ladders (still `scripts/athleticnet` or the performance edit screen). Apply is not undoable from the UI (delete performances individually). `MeetParticipant.SchoolName` reads `School.Name`, so any query that loads participants must join `Schools`: `AdminMeetPlacingRepository.GetForMeetAsync/GetByIdAsync` did not, so every "vs. {opponent}" on the Entries page was blank; fixed here. `docs/schema.sql` is not a runnable script as one batch (it lacks `GO` before most procs); the local test DB was built from a copy with `GO` inserted (and the appended [C118] block now starts a new batch). Tested end to end (review, athlete pick, apply with replace, points, relay split, entry linking, re-review idempotence) against Azure SQL Edge in Docker with the real schema; 37 unit tests (planner and file parsing).

**Key files:** `Web/Import/*`, `Areas/Admin/Controllers/MeetImportController.cs`, `Areas/Admin/Views/MeetImport/*`, `DataAccess/Repositories/AdminMeetImportRepository.cs`, `docs/meet-results-format.md`, `.claude/skills/prepare-meet-results/SKILL.md`, `Tests/Unit/Import/MeetImportPlannerTests.cs`.

### [C123] Season Wrap (public page + one-page poster PDF)

**What changed:** `/seasons/{name}/wrap` (e.g. `/seasons/2025-2026/wrap`): team highlights (meets, athletes, PRs, school records set, top-10 marks, best three meets by MeetScore), school records set (one row per event, "broken N times", "since passed"), athlete spotlights (best marks by percentile, most PRs, biggest jumps vs the best before the season), the team (dual record per gender with a result table, scoring leaders from the season scoring data), a server-rendered SVG progression chart (PRs per meet as bars, average percentile as a line) and the senior class. Live: computed from the data on every request, so the static export always has the latest. `/seasons/{name}/wrap.pdf` is the same data as a one-page 11x17 portrait poster (QuestPDF, `Web/Pdf/SeasonWrapPoster.cs`), linked from the page; the home page's season strip links to the current season's wrap. Pieces: `ISeasonWrapRepository` (3 queries: marks, prior bests, dual team results), pure `SeasonWrapCalculator` (unit-tested), `SeasonWrapService`, `SeasonWrapViewModel`. `IMeetService.GetMeetScoresAsync` is now public (the wrap reuses MeetScore).

**Why:** the owner wanted a season recap to share, now that PR / school-record "at the time" ([C118]) lets it credit growth. Chosen with the owner: public page + a poster PDF linked from it, all four content areas, live not a snapshot. An optional written "season in words" (skill-written, owner-approved, like meet recaps) is planned in a follow-up change.

**Rules worth knowing:** headline PRs use `PRAtTime` (a first-ever mark in an event counts, per [C118]), so the number is generous; "Most PRs" / "Biggest jumps" only count marks that beat an earlier mark. "School records set" is one per event (the page also shows how many record-breaking marks that was). Scoring leaders use split points (relay points divided among members), labelled on the page. Dual record needs `MeetTeamResults` rows with both scores ([C113]); invitationals are not counted. Any season name with data renders; older seasons work but pre-2021 numbers are inflated ([C118]) and nothing links to them.

**Watch out:** the public site is a crawl of the app: the wrap page and the PDF only reach the export because the home page links to the wrap and the wrap links to the PDF; `ctf` (wget) must save `wrap.pdf` as-is (check after the first export). QuestPDF is a new dependency, **Community license** (free for individuals, non-profits and organisations under $1M revenue; set in `Program.cs`); it ran in the stock `aspnet:10.0` image without extra packages (checked arm64; the NuGet also ships linux-x64). The poster is checked to stay on one page (test) but a season with very long event or athlete names could push it onto two. No Node/pnpm is installed on this machine, so `site.css` was not rebuilt: the wrap views only use classes that already exist in `site.css` (plus inline styles); when rebuilding with Tailwind later, new classes in these views are picked up as usual. Razor trap: `<text>` inside a code block is Razor's text tag, so SVG `<text>` elements in loops are written with `@:`.

**Key files:** `Services/SeasonWrapCalculator.cs`, `Services/SeasonWrapService.cs`, `DataAccess/Repositories/SeasonWrapRepository.cs`, `Web/Pdf/SeasonWrapPoster.cs`, `Views/Seasons/Wrap.cshtml` + `_SeasonWrapProgression.cshtml`, `SeasonsController.Wrap/WrapPdf`, `Tests/Unit/Services/SeasonWrapCalculatorTests.cs`.

### [C124] Season summary ("the season in words" on the season wrap)

**What changed:** optional written recap on `/seasons/{name}/wrap` ([C123]'s `SummaryText` slot). New `SeasonSummaries` table (`docs/migration_season_summaries.sql`: SeasonId UNIQUE, SummaryText, InputHash, Model, Status Draft/Approved via `SummaryStatus`, GeneratedAt, ReviewedAt), `SeasonSummaryRepository`, `SeasonSummaryPrompt` (system prompt, `Version`, `BuildFactSheet(SeasonWrapViewModel)`), `SeasonSummaryService` (fact sheet + `SaveManualDraftAsync`), admin `/Admin/SeasonSummaries` (list seasons with status; edit, approve, unapprove, delete; `List`/`FactSheet`/`Import` JSON endpoints), skill `.claude/skills/generate-season-summary` (Opus subagent writer, `season_summary.py` validator/importer). `SeasonWrapService` now loads the Approved text only (`GetApprovedTextAsync`).

**Why:** same owner rules as meet recaps ([C50]/[C51]) and athlete summaries ([C52]): AI never runs inside the site; text is written in chat, imported as Draft, approved by the owner; only Approved renders publicly.

**Rules worth knowing:** the fact sheet is names, marks and counts only (no slugs) plus a `notes` list of data rules (a first-ever mark counts as a PR in the headline `personalRecords`; `improvedMarks`/`biggestJumps` are genuine improvements; `stillRecord` false = since broken; seated events never score). `Import` returns 409 for an Approved season unless `overwrite=true` (overwriting resets to Draft and pulls the text off the public wrap); the skill asks the owner first. The validator flags any name, mark or number not in the fact sheet.

**Watch out:** the migration must be applied to `ctf` before deploy or the wrap page (which now queries `SeasonSummaries`) errors. Approving does not re-export the static site: run the `ctf` export for the text to appear publicly. Bump `SeasonSummaryPrompt.Version` when the prompt or fact-sheet shape changes. Text is not linkified (unlike meet recaps); paragraphs split on a blank line. The poster PDF does not include the summary. Nothing detects staleness when results change after writing.

**Key files:** `Models/SeasonSummary.cs`, `DataAccess/Repositories/SeasonSummaryRepository.cs`, `Services/SeasonSummaryPrompt.cs`, `Services/SeasonSummaryService.cs`, `Services/SeasonWrapService.cs`, `Web/Areas/Admin/Controllers/SeasonSummariesController.cs`, `Areas/Admin/Views/SeasonSummaries/Index.cshtml`, `.claude/skills/generate-season-summary/*`, `Tests/Unit/Services/SeasonSummaryTests.cs`.

### [C125] Seasons Back in the Public Site: Nav Item, Wrap Links, Previous/Next, and a Broken Season Page Fixed

**What changed:** the owner wanted past season wraps easy to reach ("Meets replaced the Seasons page completely"), so the seasons pages are public again, **superseding the [C44] rule that season pages are not part of the public export**. A **Seasons** item sits in the main navigation (desktop and the mobile tab bar) and leads to `/seasons` (current season card, recent seasons, the full list), where every season that has results links to its wrap ("Season Wrap →" on the current/previous cards, "Season wrap →" on the old-season rows) and the season detail page has a "Season Wrap" button. The wrap page has "← All seasons" and previous/next season links that skip seasons without results (`SeasonWrapViewModel.PreviousSeasonName/NextSeasonName`, `ISeasonWrapRepository.GetSeasonIdsWithMarksAsync`). `SeasonCardViewModel.HasWrap` = the season has results. Every season with data has a wrap (50 seasons back to 1959-60).

**Seasons before 2020-21 (partial data):** the wrap shows a notice and leaves out PR-based parts (PRs tile, most PRs, biggest jumps, progression chart); records, best marks, seniors and top-10 marks stay. Same cut-off as MeetScore (`MeetScoreCalculator.IsCompleteEra`). The poster shows "Marks recorded" instead of PRs for these seasons.

**Bug found and fixed:** `MeetRepository.GetMeetsForSeasonAsync` (behind every `/seasons/{name}` detail page) used `m.*` with a hand-written `GROUP BY` column list, so adding `Meets.MeetLevel` and `OfficialResultsUrl` made every season detail page return a 500. Nobody noticed because nothing linked to season pages. The counts are now subqueries (no `GROUP BY`). `GetAllMeetsWithStatsAsync` still lists columns explicitly in SELECT and GROUP BY and works today, but has the same hazard if a column is added to `Meets` and `m.*`-style selects are used there.

**Watch out:** the static export now includes `/seasons`, 50 season pages, 50 wraps and ~48 poster PDFs (a few MB, a bit more crawl time). The sitemap is generated by `~/ctf` from the exported files. Season numbers (PRs, SRs) use the at-the-time flags and are inflated for early seasons ([C118]); only the wrap marks that. Checked locally against live data: all 50 season pages, all 50 wraps and `/seasons/2025-2026/scoring` return 200.

**Key files:** `Views/Shared/_MainNavigation.cshtml`, `_CurrentSeasonCard/_PreviousSeasonCard/_OldSeasonCard.cshtml`, `Views/Seasons/Details.cshtml`, `Views/Seasons/Wrap.cshtml`, `Services/SeasonWrapService.cs`, `Services/SeasonWrapCalculator.cs`, `DataAccess/Repositories/MeetRepository.cs`, `Web/Pdf/SeasonWrapPoster.cs`.

### [C126] Athlete career wrap (public page + one-page poster PDF)

**What changed:** `/roster/{slug}/wrap` and `/roster/{slug}/wrap.pdf` (e.g. `/roster/caleb-neville/wrap`): a career recap for any athlete with results. Sections: the approved career recap (when one exists), headline tiles (seasons, meets, PRs, school records), biggest improvements (first mark to best mark per individual event, ranked by relative gain), career bests (highest percentile first, SR / all-time-rank badges), season by season (class, events, marks, PRs, SRs) and the existing career progression chart partial. The roster details page links to it ("Career wrap" in the hero line), which is what lets the static export crawl reach it.

**Why:** the owner wanted athlete equivalents of season wraps ([C123]). Chosen: career wrap per athlete (not per season), page plus poster, recap text shown alongside the stats rather than replaced.

**Design:** no new SQL, no new repository. `AthleteWrapService` calls `IAthleteService.GetAthleteDetailsAsync` and the pure `AthleteWrapCalculator.Build` reshapes that view model into `AthleteWrapViewModel`, so the wrap and the athlete page cannot disagree (same SR = `AllTimeRank == 1` rule, same Approved-only summary rule). Poster is `Web/Pdf/AthleteWrapPoster.cs` (QuestPDF, tabloid portrait, one page; tests assert the page count).

**Rules worth knowing:** the wrap's PR count only counts marks with `IsPersonalBest && PreviousBest != null`, i.e. genuine improvements; a first-ever mark in an event is not counted (unlike the season wrap's headline, which counts debuts per [C118]) because a career total that credits every debut reads as inflated. Improvements skip relays, single-mark events and events with no gain. Indoor events are labelled "(indoor)"; outdoor is unlabelled. Seasons ending in or before 2020 show the partial-data notice (same cut-off as MeetScore).

**Watch out:** the static export gains ~2 files per athlete with results (page + PDF, roughly 900 athletes), so the `ctf` crawl takes longer; check crawl time after the first export. A wrap exists for every athlete slug with a details page, including active athletes (their wrap is "so far"). The link in `Details.cshtml` rebuilds the slug with `SlugHelper` ("First-Last"), the same rule as `AthleteRepository.GetBySlugWithBasicInfoAsync`.

**Key files:** `Services/AthleteWrapCalculator.cs`, `Services/AthleteWrapService.cs`, `ViewModels/Athletes/AthleteWrapViewModel.cs`, `Web/Pdf/AthleteWrapPoster.cs`, `Views/Roster/Wrap.cshtml`, `RosterController.Wrap/WrapPdf`, `Tests/Unit/Services/AthleteWrapCalculatorTests.cs`.

### [C127] Athlete hero metric strip: size the grid to its cells

**What changed:** on `/roster/{slug}` the hero strip (`.metric-strip`, a fixed `grid-cols-4`) is now sized to its real cell count with an inline `grid-template-columns:repeat(N,minmax(0,1fr))` (2 fixed cells for PRs/Records plus one per top sprint/field event). The mark values are `white-space:nowrap` with a `clamp()` font size so "128' 8"" no longer wraps onto two lines on a phone.

**Why:** an athlete with only one top event (e.g. a thrower like Jacob Atkinson, Discus only) has 3 cells, which left an empty fourth column and an empty bordered box at the right of the bar.

**Watch out:** inline styles on purpose: no Node/pnpm here, so `site.css` can't be rebuilt. The other users of `.metric-strip` (home season card, latest-meet card, meet details) still assume 4 cells; check their cell counts if they ever render fewer. The Personal Bests table below still wraps marks like `37' 10"` onto two lines at phone width (not changed).

**Key files:** `Views/Roster/Details.cshtml`.

### [C128] Athlete wrap: show the change in the mark, signed by direction

**What changed:** the "Change" in the wrap's Biggest improvements (page and poster) is now computed from the raw first and best marks by `AthleteWrapCalculator.FormatChange`: a faster time shows as `-3.40s` (or `-1:05.50`), a longer jump or throw as `+2' 1"`. It no longer reuses `CareerChartViewModel.CareerImprovementFormatted`.

**Why:** that chart string reads "+3.40s" for a time that got 3.40s *faster* (it is labelled "Improvement" on the chart). Next to First and Best columns on the wrap it read as a slower time (reported on Ethan Gray's 400M).

**Watch out:** `CareerImprovementFormatted` still has the "+ means better" meaning and is still used by the career chart stat row and the athlete summary fact sheet (where the field is named `improvement`); that is intentional, do not change it without bumping `AthleteSummaryPrompt.Version`. Any new surface that shows a change next to two marks should use `FormatChange`.

**Key files:** `Services/AthleteWrapCalculator.cs`, `Tests/Unit/Services/AthleteWrapCalculatorTests.cs`.

### [C129] Every time/mark change is signed by its direction on the number line

**What changed:** owner rule: a "change" is the new mark minus the old mark, so a faster (smaller) time is **negative** and a longer jump or throw is **positive**, everywhere it is shown. Updated: the career chart "Improvement" stat (`AthleteService.FormatDelta`, now `-3.40s` for running), the home page "Biggest improvement" (`HomeService.FormatImprovement`, `-0.50s` / `+1' 0"`), the season wrap "Biggest jumps" (`SeasonWrapCalculator`, replaces "1.00s faster" / "5' 0" farther" with `-1.00s` / `+5' 0"`), and the athlete page's "gap to PR" column (a field mark shorter than the PR is now `-`, a running mark slower than the PR stays `+`). Already correct and unchanged: leaderboard record progression (`-0.43s` / `+2' 6.25"`), `PerformanceFormatHelper` ("-0.50s faster", "+1' 0" farther"), the athlete wrap ([C128]). **Supersedes the [C128] note** that the chart string stays "+ means better".

**Fact sheets:** the athlete and season summary fact sheets still describe the same change, now with the direction spelled out so a writer cannot misread the sign: `"improvement": "-3.40s (faster)"`, `"change": "+5' 0\" (farther)"`. Prompt rules and `Version` were not changed; existing approved recaps are unaffected.

**Watch out:** the chart's "off record" gap stays an unsigned distance (it is a gap to a target, not a change). Any new surface that shows a change between two marks must use this convention (see `AthleteWrapCalculator.FormatChange`, `FormatDelta`).

**Key files:** `Services/AthleteService.cs`, `HomeService.cs`, `SeasonWrapCalculator.cs`, `AthleteSummaryPrompt.cs`, `SeasonSummaryPrompt.cs`, `Views/Roster/Details.cshtml`, `Tests/Unit/Services/SeasonWrapCalculatorTests.cs`.

### [C130] Roster "Top Event": individual events first, shared by the active and former lists

**What changed:** one selector, `AthleteService.PickTopEvent`, now chooses the Top Event for both the Active list and the Former Athletes list: individual events before relays, then highest percentile, then outdoor before indoor, then structural `SortOrder`; falls back to the first event when no percentile exists. New `EventParticipationViewModel.IsRelay` and `DisplayName` (indoor events show "(indoor)", since "1600M" exists in both environments); both lists render `DisplayName`.

**Why:** the Former Athletes list never got the [C37] percentile fix: it still used `EventsInCategory.FirstOrDefault()` (first by `SortOrder`), so Caleb Neville, with the school-record 3200M, showed the indoor 4x200M relay. The Active list had the percentile rule but let a relay win on percentile alone: relay events have few marks (indoor 4x200M has 3), and a team mark says little about the athlete.

**Rules worth knowing:** an athlete with only relays still shows their best relay. Event page and athlete-detail hero stats are unchanged (the hero already excludes relays). The former list now also fills `Percentile`/`IsFieldEvent` from the athlete's best mark per event.

**Watch out:** the selector uses percentile only; an individual event with a one-off mark in a tiny population can still post an extreme percentile. If that shows up, require `EventMarkCount >= 10` (the `EventStatistics` value) before trusting percentile, as MeetScore does.

**Key files:** `Services/AthleteService.cs`, `ViewModels/EventParticipationViewModel.cs`, `Views/Shared/_FormerAthleteYearGroupSection.cshtml`, `_RosterActiveAthletesList.cshtml`, `Tests/Unit/Services/AthleteServiceTests.cs`.
