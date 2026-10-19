# Results-level comparison: MileSplit vs the database

For every meet the database holds that is matched to a MileSplit meet (125 meets with raw results; two have none), each Cloverleaf row in MileSplit's Hy-Tek text was compared with the DB result for the same meet, event and athlete (relays by event and time). Script: `scripts/crawl/compare_ms_vs_db.py` (raw text fetched by curl, see BRAIN.md [C74]); full row-level output was in `results-compare-ms.tsv`, not committed.

**Summary:** about 2,770 results agree; 24 individual results and 16 relays were missing from the DB (32 of them could be resolved safely and were imported, `docs/imports/results-missing-ms.sql`); 39 individual and 14 relay marks differ and need a human look; 54 rows name an athlete the matcher could not place; 92 rows are events the DB does not model.

## Artifacts to ignore
- Hy-Tek prints hand-timed tenths as hundredths (27.6 appears as `27.06`); the DB's tenths are right. The comparison treats `x.0y` as `x.y`.
- The 2022 Cloverleaf Early Bird Relays raw text rounds relay times to whole seconds (54.0 vs the DB's 53.31): ignore.

## Marks that differ (source vs DB, best round)
| Meet | Event | Athlete | MileSplit | DB |
|---|---|---|---|---|
| 2025-04-26 2025 Green Twilight Relays | Girls Discus | Presly McConnell | 69' 5.00" | 96' 5.00" |
| 2025-05-03 2025 Nikki Reynolds Invitational | Boys Discus | Clay Combs | 157' 0.00" | 157' 10.00" |
| 2025-05-12 2025 Stars of Track and Field | Boys Long Jump | Collin Saksa | 18' 5.25" | 15' 8.25" |
| 2025-05-17 2025 Metro Athletic Conference Championshi | Boys 400M | Isaac Smith | 52.79 | 53.19 |
| 2025-05-17 2025 Metro Athletic Conference Championshi | Girls 400M | Katie Wershing | 1:01.32 | 1:03.38 |
| 2024-05-03 2024 Fairless Twilight Invitational | Girls Discus | Karoline Kreider | 78' 10.00" | 85' 1.00" |
| 2024-05-03 2024 Fairless Twilight Invitational | Boys Discus | Tim Shores | 123' 0.00" | 123' 1.00" |
| 2024-05-03 2024 Fairless Twilight Invitational | Girls Shot Put | Karoline Kreider | 32' 2.50" | 32' 7.00" |
| 2024-05-06 2024 Stars of Track and Field | Boys Discus | Dylan Wittman | 100' 8.00" | 103' 3.00" |
| 2024-05-10 2024 Metro Athletic Conference Championshi | Girls 200M | Riley Evans | 25.82 | 25.55 |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 100M | Riley Evans | 12.78 | 12.25 |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 400M | Riley Evans | 57.88 | 56.44 |
| 2024-05-24 2024 Division I Regional Track Meet | Boys Discus | Tim Shores | 129' 7.00" | 128' 6.00" |
| 2023-04-04 2023 Ravenna-Streetsboro Double Dual | Boys 200M | Oakley Kemp | 25.10 | 25.19 |
| 2023-04-04 2023 Ravenna-Streetsboro Double Dual | Boys Shot Put | Dominic Nau | 38' 2.00" | 31' 0.00" |
| 2023-05-05 2023 Fairless Twilight Invitational | Girls Pole Vault | Payten Shetterly | 6' 0.00" | 6' 6.00" |
| 2022-05-09 2022 Smithie Frosh-Soph Invitational | Girls 800M | Brooke Gucker | 2:42.56 | 2:42.65 |
| 2022-05-09 2022 Smithie Frosh-Soph Invitational | Boys Shot Put | Tim Shores | 38' 3.00" | 38' 8.00" |
| 2021-05-01 2021 Nikki Reynolds Invitational | Girls 800M | Alyssa Stoner | 2:30.09 | 2:30.69 |
| 2021-05-01 2021 Nikki Reynolds Invitational | Boys Shot Put | Luke Yuravak | 41' 5.25" | 41' 1.25" |
| 2021-05-21 2021 Division I District Track Meet	 | Boys 1600M | Nathan Naples | 5:05.05 | 5:05.77 |
| 2021-05-21 2021 Division I District Track Meet	 | Girls 800M | Allison Yagersz | 2:35.73 | 2:35.84 |
| 2018-04-27 2018 Orange Relays | Girls 1600M | Sara Naples | 12:48.68 | 6:17.24 |
| 2018-04-27 2018 Orange Relays | Boys 1600M | Dillon West | 10:01.05 | 5:12.55 |
| 2018-05-09 2018 PTC Track Championships | Girls 100M | Maddie Boltz | 13.59 | 13.53 |
| 2018-05-09 2018 PTC Track Championships | Girls 100M | Sara Keener | 13.69 | 13.40 |
| 2018-05-09 2018 PTC Track Championships | Girls 200M | Emily Romano | 27.61 | 27.51 |
| 2018-05-09 2018 PTC Track Championships | Boys 100M | Zack Munchick | 11.73 | 11.52 |
| 2018-05-09 2018 PTC Track Championships | Girls 400M | Tommie Kurtz | 1:02.54 | 1:00.85 |
| 2018-05-09 2018 PTC Track Championships | Girls 400M | Maddie Boltz | 1:05.93 | 1:05.72 |
| 2018-05-09 2018 PTC Track Championships | Boys 400M | Frank Stepp | 52.42 | 52.12 |
| 2018-05-09 2018 PTC Track Championships | Boys 400M | Blake Wilson | 57.50 | 54.88 |
| 2017-04-15 2017 Mansfield Mehock Relays | Girls 1600M | Katelyn Malena | 6:19.25 | 6:19.31 |
| 2017-04-28 2017 Orange Relays | Boys Discus | Nate Clingan | 113' 7.50" | 113' 7.00" |
| 2017-05-10 2017 PTC Track Championships | Girls 400M | Jenna Oliver | 1:04.48 | 1:04.39 |
| 2017-05-10 2017 PTC Track Championships | Boys 400M | Ryan Curtis | 55.92 | 55.11 |
| 2017-05-10 2017 PTC Track Championships | Girls 300M Hurdles | Zana Ross | 49.04 | 48.70 |
| 2017-05-10 2017 PTC Track Championships | Girls 200M | Jenna Oliver | 28.60 | 28.19 |
| 2017-05-20 2017 Division II District Track Meet | Boys 800M | Nick Bailey | 2:12.94 | 2:18.46 |

Most differences are small (tenths of a second or a few hundredths, possibly a different round or timing system); the large ones (for example Collin Saksa 2025 long jump 18' 5.25" vs 15' 8.25", Presly McConnell 2025 discus, Dominic Nau 2023 shot put) are the ones to verify first.

## Relay times that differ
| Meet | Relay | MileSplit | DB |
|---|---|---|---|
| 2025-04-26 2025 Green Twilight Relays | Boys 4x1600M Relay | 19:56.86 | [1542.98] |
| 2025-04-26 2025 Green Twilight Relays | Boys 4x100M Relay | 44.00 | [48.14] |
| 2025-05-24 2025 Division II District Track Meet | Boys 4x400M Relay | 2:08.50 | [210.96] |
| 2024-05-17 2024 Division I District Track Meet | Girls 4x400M Relay | 4:10.33 | [255.93] |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 4x200M Relay | 1:44.61 | [102.75] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Girls 4x100M Shuttle Hurdles | 1:30.00 | [84.71] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Boys 4x110M Shuttle Hurdles | 1:22.00 | [85.9] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Girls 4x100M Relay | 54.00 | [53.31] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Boys 4x100M Relay | 48.00 | [47.54] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Boys 4x800M Relay | 8:45.00 | [538.44] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Girls 4x200M Relay | 1:55.00 | [116.94] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Boys 4x200M Relay | 1:40.00 | [101.19] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Girls 4x400M Relay | 4:35.00 | [282.15] |
| 2022-03-26 2022 Cloverleaf Early Bird Relays | Boys 4x400M Relay | 3:40.00 | [221.98] |

## Names the matcher could not place (candidate new athletes or spelling variants)
Bryce Neil (3), Savannah Reho (2), Jacob Knau (2), Meagan Van Meter (1), Asa Gyn (1), Jacob (Jake) Schaefer (1), Payten Shetterly 11 f (1), Isaiah Curt (1), Josh Bohemian (1), Brooklynn         W12 Crist (1), Bridgette       W12 Donahue (1), Donahue (wheeled) (1)

## Events the DB does not model (not compared)
Discus Throw Relay (18), Shot Put Relay (18), Long Jump Relay (12), High Jump Relay (9), Shot Put Wheelchair (5), 100 Meter Dash Wheelchair (4), Pole Vault Relay (3), 400 Meter Dash Wheelchair (3), Shot Put Div 2 (2), 100 Meter Wheelchair (2), High Jump Ashley Sigmo (2), 800 Meter Run Wheelchair (2), Shot Put COUNTY (2), 100 Meter Dash COUNTY (2), 400m Division 3 - Prelims (Division 3) (Prelims) (1)

## Not covered
- athletic.net-only content (the comparison used MileSplit raw text; athletic.net pages need a browser scrape per meet).
- DB meets with no MileSplit counterpart (all dual meets, most league meets), see `docs/meet-history-gap-report.md`.
- Prelim/final rounds and attempts are not compared, only the best mark.
