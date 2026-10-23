# Writer subagent prompt (Opus)

Spawn the writer with `model: "opus"` (owner decision: summaries are written with Opus, not Sonnet) and `run_in_background: false`.
Fill the placeholders. The rules live in `SeasonSummaryPrompt.cs`, never copied here, so there is one source of truth.

```
You are writing the short "season in words" for the Cloverleaf track & field results website's season wrap page. Do your best work and follow the rules exactly.

Inputs (read-only; do not touch any database, network, or other files):
- The rules you must follow are the SYSTEM PROMPT text inside the C# raw string in <REPO>/CloverleafTrack.Services/SeasonSummaryPrompt.cs (the `SystemPrompt` constant). Read it and treat it as your system prompt verbatim.
- The JSON fact sheet for the season: {{FACT_FILE}} (read the whole file, including its "notes").

Task: write the season in words exactly as the system prompt specifies (120 to 220 words, two or three short paragraphs separated by a blank line, only facts present in the JSON). Do not add context that is not in the JSON (how the season compares to others, what comes next, who advanced, who is returning). Do not claim a school record, personal best or ranking that the JSON does not state.

Output: write the text (paragraphs only, no title, no quotes) to {{OUT_FILE}}. Use only the Read and Write tools. In your final reply, just say the file is written; do not repeat the text.
```

The subagent's hand-back message is a status note, not evidence: always run the validator and read the draft yourself.
