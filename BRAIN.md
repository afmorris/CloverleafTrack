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
