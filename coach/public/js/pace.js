// Pace math for the goal-pace builder. Pure functions, no DOM: the page, the Worker, and the tests all import this
// one module. Times are seconds, distances are meters.

/** Meters in an event name: "1600M" -> 1600, "100M Hurdles" -> 100, "2K Steeple Chase" -> 2000. */
export function parseEventDistance(name) {
  const m = /(\d+(?:\.\d+)?)\s*(K|M)\b/i.exec(name ?? "");
  if (!m) return null;
  const n = Number(m[1]);
  return m[2].toUpperCase() === "K" ? n * 1000 : n;
}

/** "24.3" -> 24.3, "1:05.2" -> 65.2, "10:12" -> 612, "1:02:03" -> 3723. Returns null when it can't read it. */
export function parseTime(text) {
  const s = String(text ?? "").trim();
  if (!/^\d+(:\d{1,2}){0,2}(\.\d+)?$/.test(s)) return null;
  const parts = s.split(":");
  let total = 0;
  for (let i = 0; i < parts.length; i++) {
    const v = Number(parts[i]);
    if (i > 0 && v >= 60) return null;
    total = total * 60 + v;
  }
  return total > 0 ? total : null;
}

/** 24 -> "24.00", 65.234 -> "1:05.23", 612 -> "10:12.00". */
export function formatTime(seconds, decimals = 2) {
  if (seconds == null || !Number.isFinite(seconds)) return "";
  const factor = 10 ** decimals;
  const rounded = Math.round(seconds * factor) / factor;
  const mins = Math.floor(rounded / 60);
  const secs = rounded - mins * 60;
  const secText = secs.toFixed(decimals);
  if (mins === 0) return secText;
  return `${mins}:${secs < 10 ? "0" : ""}${secText}`;
}

/** Signed change from one time to another, new minus old (BRAIN.md [C129]): faster is negative. */
export function formatChange(fromSeconds, toSeconds) {
  const d = Math.round((toSeconds - fromSeconds) * 100) / 100;
  if (d === 0) return "0.00s";
  return `${d > 0 ? "+" : "-"}${formatTime(Math.abs(d))}${Math.abs(d) < 60 ? "s" : ""}`;
}

/** Rest in seconds as "90s", "3:00". */
export function formatRest(seconds) {
  if (!seconds) return "";
  return seconds < 60 ? `${seconds}s` : formatTime(seconds, 0);
}

/**
 * Target time for one rep: the goal's average speed scaled to the rep distance, then slowed by effort.
 * 90% effort means 90% of goal speed, so a 24.00 200m goal gives 26.67 for a 200m rep at 90%.
 */
export function repTarget(goalSeconds, eventMeters, repMeters, effortPct) {
  if (!(goalSeconds > 0 && eventMeters > 0 && repMeters > 0 && effortPct > 0)) return null;
  return (goalSeconds * (repMeters / eventMeters)) / (effortPct / 100);
}

/** The split length that reads well for a rep or race of this length. */
export function splitLength(meters) {
  if (meters <= 400) return 100;
  if (meters <= 1000) return 200;
  return 400;
}

/** Even-pace cumulative splits: [{ at: 100, time: 13.33 }, ...]; the last entry is always the full distance. */
export function evenSplits(totalSeconds, meters, every = splitLength(meters)) {
  if (!(totalSeconds > 0 && meters > 0 && every > 0)) return [];
  const out = [];
  for (let at = every; at < meters; at += every) out.push({ at, time: (totalSeconds * at) / meters });
  out.push({ at: meters, time: totalSeconds });
  return out;
}

/**
 * The whole workout. sets: [{ reps, distance, effort, rest }] (rest in seconds).
 * Returns each set with its rep target and splits, plus total volume in meters.
 */
export function buildWorkout({ goalSeconds, eventMeters, sets }) {
  const rows = (sets ?? []).map((set) => {
    const target = repTarget(goalSeconds, eventMeters, set.distance, set.effort);
    return {
      ...set,
      target,
      splits: target == null ? [] : evenSplits(target, set.distance),
      volume: (set.reps > 0 && set.distance > 0) ? set.reps * set.distance : 0
    };
  });
  return { sets: rows, volume: rows.reduce((sum, r) => sum + r.volume, 0) };
}

/** Best mark across season rows: lowest time for running, longest distance for field. */
export function bestMark(rows, isRunning) {
  let best = null;
  for (const r of rows ?? []) {
    const v = isRunning ? r.timeSeconds : r.distanceInches;
    if (v == null) continue;
    if (best == null || (isRunning ? v < best.value : v > best.value)) best = { ...r, value: v };
  }
  return best;
}
