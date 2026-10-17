# Writer subagent prompt (Opus)

Spawn the writer with `model: "opus"` (owner decision: summaries are written with Opus, not Sonnet) and `run_in_background: false`.
Fill the placeholders. The rules live in `AthleteSummaryPrompt.cs`, never copied here, so there is one source of truth.

```
You are writing short career recaps of high school track & field athletes for the Cloverleaf track & field results website. Do your best work and follow the rules exactly.

Inputs (read-only; do not touch any database, network, or other files):
- The rules you must follow are the SYSTEM PROMPT text inside the C# raw string in <REPO>/CloverleafTrack.Services/AthleteSummaryPrompt.cs (the `SystemPrompt` constant). Read it and treat it as your system prompt verbatim. Pay particular attention to: naming events exactly as the "event" value appears in the fact sheet ("800M", "100M Hurdles", "4x400M Relay") with "indoor" or "outdoor" directly before an event name when the athlete has that event in both environments; naming meets exactly as written; no gendered pronouns; and never stating how many personal bests an athlete set. The website turns event and meet names into links, so any other wording ("800 meters", "the 800") breaks the links.
- One JSON fact sheet per athlete in {{FACTS_DIR}}: {{ATHLETE_LIST}}   (format: "<id>.json (<name>, <team>)")

Task: for each athlete, read the whole fact sheet and write the recap exactly as the system prompt specifies (single paragraph, 80 to 140 words, only facts in the JSON). Write each athlete independently from their own fact sheet. Notes: "personalRecords" are the athlete's best marks per event; "careerArc" gives the first recorded mark and the best mark per event (with "improvement"); "seasons" run oldest to newest and carry the athlete's class that season; "highlights" are school records and top-10 all-time marks (an "sr" or "allTimeRank" appears only where the fact is true). Relay "relay" arrays list teammates. An athlete with few results should get a shorter, plainer recap rather than padding. Do not add context that is not in the JSON (what the athlete did after high school, what the program did, how others performed).

Output: write a single JSON file to {{OUT_FILE}} mapping athlete id (string) to the recap text, e.g. {"671": "...", ...}. Use only the Read and Write tools. In your final reply, just say the file is written; do not repeat the texts.
```

Batching: one writer per 8 to 9 athletes; for a class of about 30 start four writers in the same message so they run in parallel, each with its own `{{OUT_FILE}}`, then merge the JSON objects.
A subagent's hand-back message is a status note, not evidence: always run the validator and read the drafts yourself.
