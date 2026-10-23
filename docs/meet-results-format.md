# Meet results file (version 1)

Read by **Admin → Meets → Import** (`/Admin/MeetImport?meetId=N`, also an "Import file" button on a meet's Entries page). The site does no AI: the file is prepared outside it (in chat, from a scan, athletic.net, a Hy-Tek export…), the Import screen shows what every row would do, and nothing is written until you tick rows and click **Apply**. See BRAIN.md [C122].

```json
{
  "version": 1,
  "source": "Scan from Coach Gucker, 2026-04-15",
  "notes": "Page 2 is cut off; Smith's 400 is hard to read (52.4 or 53.4)",
  "meet": { "id": 292, "name": "2026 Field-Coventry Double Dual" },
  "results": [
    { "gender": "Boys", "event": "100M", "athlete": "Isaac Smith", "mark": "11.23",
      "placings": [ { "opponent": "Field", "place": 1 }, { "opponent": "Coventry", "place": 2 } ] },
    { "gender": "Boys", "event": "Shot Put", "athlete": "Tim Shores", "mark": "38' 8\"", "place": 2 },
    { "gender": "Boys", "event": "Long Jump", "athlete": "Cal Quick", "status": "NM" },
    { "gender": "Boys", "event": "4x100M Relay", "athletes": ["Isaac Smith", "Sam Runner", "Ben Fast", "Cal Quick"], "mark": "46.10" }
  ],
  "entries": [
    { "gender": "Girls", "event": "100M", "athlete": "Riley Evans" }
  ]
}
```

| Field | Meaning |
|---|---|
| `meet.id` | Optional safety check. If present it must equal the meet you are importing into. |
| `results[]` / `entries[]` | Either list may be empty. `entries` are pre-meet entries (no mark). |
| `gender` | `Boys`, `Girls`, `Mixed` (also `M`/`F`/`Male`/`Female`). |
| `event` | The site's event name for that gender and the meet's environment: `100M`, `4x100M Relay`, `Shot Put`, `Pole Vault`. Loose spellings (`100`, `100 m`, `4x100`, `shot`) are accepted. `GET /Admin/MeetImport/Context?meetId=N` lists the exact names. |
| `athlete` | Individual events: `First Last` (or `Last, First`). Must be an exact match for one athlete; near misses become a pick-list on the review screen. |
| `athletes` | Relays: one name per leg, exactly the event's athlete count. |
| `mark` | Times: `11.23`, `2:05.40` (a bare JSON number is read as seconds). Field: `38' 8"`, `38-8`, `15' 10.25"`. Leave out when `status` is set. |
| `status` | `NM`, `NH`, `DNF`, `DQ` instead of a mark (a competed-but-no-mark result). |
| `place` | Invitational: overall place. Dual with one opponent: the place against that opponent. |
| `placings` | Dual / double dual: `{ "opponent": "<school>", "place": n }` per opponent (matched to the meet's opponents). Team points come from the meet's scoring template when applied. |
| `note` | Shown on the review screen, not stored. |

## What the review screen does
- **New**: will be created (pre-ticked). **Adds places**: the mark is stored, only missing places are added (pre-ticked). **Already there**: identical, nothing to do. **Differs**: a different mark or place is stored; ticking *replaces* it (unticked by default). **Needs attention**: unknown event, unreadable mark, unknown opponent, or an athlete that is not an exact match (pick one from the list to continue).
- Warnings (never blocking): an athlete in more than four events at this meet.
- Results are linked to a matching pre-meet entry when there is one; otherwise an entry is created so the meet's Entries page shows the result.
- Apply is one transaction, then the leaderboards rebuild once (~3 s, not per row).
- Applying the same file twice is harmless: the second review shows "Already there".

## Not in version 1
Prelim/final rounds, field attempt series and pole-vault bar ladders still go through the existing scripts (`scripts/athleticnet`) or the per-performance edit screen. A file with partial results (an away dual that only reports the top three) is fine: list only what is known.
