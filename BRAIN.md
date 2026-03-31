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

## Relay Athlete Name Format

- **Stored in DB:** `STRING_AGG(a.FirstName + ' ' + a.LastName, '|~|')`
- **Format:** `FirstName LastName` (NOT `LastName, FirstName`)
- **Separator:** `|~|` (chosen to be unlikely to appear in real names)
- **C# split:** `RelayAthletes?.Split("|~|") ?? Array.Empty<string>()`
- **Display format:** Names joined by ` / ` inline, each linked to athlete's roster page

---
