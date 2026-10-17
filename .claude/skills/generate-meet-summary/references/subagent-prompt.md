# Writer subagent prompt (Opus)

Spawn the writer with `model: "opus"` (owner decision: summaries are written with Opus, not Sonnet) and `run_in_background: false`.
Fill the three placeholders. The rules live in `MeetSummaryPrompt.cs`, never copied here, so there is one source of truth.

```
You are writing short recaps of high school track & field meets for the Cloverleaf track & field results website. Do your best work and follow the rules exactly.

Inputs (read-only; do not touch any database, network, or other files):
- The rules you must follow are the SYSTEM PROMPT text inside the C# raw string in <REPO>/CloverleafTrack.Services/MeetSummaryPrompt.cs (the `SystemPrompt` constant). Read it and treat it as your system prompt verbatim. Pay particular attention to naming events exactly as the "event" value appears in the fact sheet ("800M", "100M Hurdles", "4x400M Relay", "Long Jump") and putting "boys" or "girls" before an event name; the website turns those names into links, so any other wording ("800 meters", "the 800", "100 hurdles", "4x400 relay") breaks the links.
- One JSON fact sheet per meet in {{FACTS_DIR}}: {{MEET_LIST}}   (format: "<id>.json (<meet name>, <format>)")

Task: for each meet, read its fact sheet (the whole file) and write the recap exactly as the system prompt specifies (single paragraph, 70 to 120 words, only facts present in the JSON). Write each meet independently from its own fact sheet. Notes: relay teams are arrays of names; "places" are the athlete's or relay's placing in the meet (invitationals) or against a specific opponent (duals, e.g. "1st vs. Ravenna"); if a meet has no "teamResults" you must say nothing about team standings or who won the team competition; counts of personal bests may be stated only if you count the "pr": true rows yourself and are certain. Very small meets may be shorter than 70 words if that is all the facts support. Do not add context that is not in the JSON (where the meet falls in the season, what comes next, whether a team advanced). Do not claim a personal best, school record, or all-time rank unless the specific result carries that flag.

Output: write a single JSON file to {{OUT_FILE}} mapping meet id (string) to the recap text, e.g. {"279": "...", ...}. Use only the Read and Write tools. In your final reply, just say the file is written; do not repeat the texts.
```

Batching: one agent handles up to about 11 meets. For more, start several writers in the same message so they run in parallel, each with its own `{{OUT_FILE}}`, then merge the JSON objects.
A subagent's hand-back message is a status note, not evidence: always run the validator and read the drafts yourself.
