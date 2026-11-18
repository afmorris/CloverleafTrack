// Season plan model: phases on a calendar, tied to the meet schedule. Pure helpers shared by the page, the Worker
// (validation), and the tests.
import { addDays, daysBetween, weekStart } from "./dates.js";

export const PHASE_KINDS = [
  { id: "offseason", label: "Off-season", color: "#2f8f9d" },
  { id: "general", label: "General prep", color: "#5b8def" },
  { id: "specific", label: "Specific prep", color: "#8b6fd8" },
  { id: "competition", label: "Competition", color: "#3b7f2e" },
  { id: "championship", label: "Championship", color: "#d18a1f" },
  { id: "transition", label: "Transition", color: "#7a8591" }
];
export const PHASE_KIND_IDS = PHASE_KINDS.map((k) => k.id);
export const phaseKind = (id) => PHASE_KINDS.find((k) => k.id === id) ?? PHASE_KINDS[PHASE_KINDS.length - 1];

/**
 * Post-season meets: district, regional, state, and league/conference championships. "State" only counts as
 * "State Track/Championship/Meet" or OHSAA, because school names like Kent State and Ohio State host regular meets.
 */
export function isChampionshipMeet(name) {
  const n = name ?? "";
  return /\b(district|regional|championships?|conference)\b/i.test(n)
    || /\bstate (track|championships?|meet)\b/i.test(n) || /\bOHSAA\b/.test(n);
}

/** Post-season = championship meets in the last six weeks of the schedule. */
const POST_SEASON_DAYS = 42;

/** The phase whose start..end (inclusive) covers date, or null. */
export function phaseForDate(phases, date) {
  return (phases ?? []).find((p) => p.startDate <= date && date <= p.endDate) ?? null;
}

/** Problems with a phase list: bad ranges, overlaps, phases outside the plan. Empty means fine. */
export function phaseProblems(phases, planStart, planEnd) {
  const out = [];
  const sorted = [...(phases ?? [])].sort((a, b) => a.startDate.localeCompare(b.startDate));
  sorted.forEach((p, i) => {
    const name = p.name || phaseKind(p.kind).label;
    if (p.endDate < p.startDate) out.push(`${name} ends before it starts.`);
    if (planStart && p.startDate < planStart) out.push(`${name} starts before the season plan.`);
    if (planEnd && p.endDate > planEnd) out.push(`${name} ends after the season plan.`);
    const next = sorted[i + 1];
    if (next && next.startDate <= p.endDate) out.push(`${name} overlaps ${next.name || phaseKind(next.kind).label}.`);
  });
  return out;
}

/**
 * Default first practice: the Monday of the last week of February in the season's spring (Ohio spring sports start
 * then). Before it is off-season (winter lifting), which runs its own cycle, outside the season's week count.
 */
export function defaultFirstPractice(endDate) {
  return weekStart(`${endDate.slice(0, 4)}-02-28`);
}

/** Monday-start weeks covering start..end: [{ start, end, index }]. */
export function weeksOf(start, end) {
  const out = [];
  for (let s = weekStart(start), i = 1; s <= end; s = addDays(s, 7), i++) out.push({ start: s, end: addDays(s, 6), index: i });
  return out;
}

/**
 * Weeks labelled for the week-by-week table. With a first practice, weeks before its week are off-season ("Off 1",
 * "Off 2", ...) and season weeks count from 1 at the first practice, so winter lifting doesn't shift the numbering.
 */
export function labelWeeks(start, end, firstPractice) {
  const weeks = weeksOf(start, end);
  const first = firstPractice && firstPractice > start && firstPractice <= end ? weekStart(firstPractice) : null;
  const before = first ? weeks.filter((w) => w.start < first).length : 0;
  return weeks.map((w, i) => ({ ...w, offseason: i < before, label: i < before ? `Off ${i + 1}` : String(i + 1 - before) }));
}

/**
 * A starting set of phases for a season, from the meet schedule:
 *   general prep, then specific prep, then competition from the week of the first outdoor meet (indoor meets count
 *   as prep; with no outdoor meets, the first meet), then championship from the week of the first championship meet
 *   in the last six weeks, then transition after the last meet.
 * Without meets it splits the season 30/25/35/10. Coaches edit the result; nothing depends on this exact split.
 * With a first practice inside the plan, everything before it is one off-season phase (winter lifting) and the
 * cycles above are built from the first practice on; meets before it (indoor) don't shape them.
 */
export function suggestPhases(start, end, meets, firstPractice = null) {
  if (firstPractice && firstPractice > start && firstPractice <= end) {
    return [
      { kind: "offseason", name: "Winter lifting", startDate: start, endDate: addDays(firstPractice, -1), volume: 3, intensity: 3,
        focus: "Strength base and lifting technique, on its own cycle before the season starts" },
      ...suggestPhases(firstPractice, end, meets)
    ];
  }
  const inSeason = (meets ?? []).filter((m) => m.date >= start && m.date <= end).sort((a, b) => a.date.localeCompare(b.date));
  const total = daysBetween(start, end) + 1;
  if (total < 14) return [{ kind: "competition", name: "Competition", startDate: start, endDate: end, volume: 3, intensity: 3, focus: "" }];

  let compStart, champStart, lastMeet;
  if (inSeason.length) {
    const firstOutdoor = inSeason.find((m) => m.environment === 1) ?? inSeason[0];
    compStart = weekStart(firstOutdoor.date);
    const lastDate = inSeason[inSeason.length - 1].date;
    const champ = inSeason.find((m) => isChampionshipMeet(m.name) && daysBetween(m.date, lastDate) <= POST_SEASON_DAYS);
    champStart = champ ? weekStart(champ.date) : null;
    lastMeet = inSeason[inSeason.length - 1].date;
  } else {
    compStart = addDays(start, Math.round(total * 0.55));
    champStart = addDays(start, Math.round(total * 0.9));
    lastMeet = null;
  }
  if (compStart <= start) compStart = addDays(start, Math.min(14, total - 1));
  const prepDays = daysBetween(start, compStart);
  const specificStart = addDays(start, Math.max(1, Math.round(prepDays * 0.55)));

  const phases = [];
  const push = (kind, s, e, volume, intensity, focus) => {
    if (s <= e) phases.push({ kind, name: phaseKind(kind).label, startDate: s, endDate: e, volume, intensity, focus });
  };
  push("general", start, addDays(specificStart, -1), 4, 2, "Aerobic base, technique, general strength");
  push("specific", specificStart, addDays(compStart, -1), 4, 3, "Event-specific work, speed development");
  const compEnd = champStart && champStart > compStart ? addDays(champStart, -1) : (lastMeet ?? end);
  push("competition", compStart, compEnd > end ? end : compEnd, 3, 4, "Race sharpening, maintain strength");
  if (champStart && champStart > compStart) {
    push("championship", champStart, lastMeet && lastMeet < end ? lastMeet : end, 2, 5, "Taper, peak for post-season");
  }
  const last = phases[phases.length - 1];
  if (last && last.endDate < end) push("transition", addDays(last.endDate, 1), end, 1, 1, "Rest and recovery");
  return phases;
}
