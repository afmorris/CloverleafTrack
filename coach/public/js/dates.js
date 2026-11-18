// Calendar dates as "YYYY-MM-DD" strings, done in UTC so a user's time zone can never shift a day.
const DAY = 86400000;

export function isIsoDate(s) {
  if (typeof s !== "string" || !/^\d{4}-\d{2}-\d{2}$/.test(s)) return false;
  const d = new Date(`${s}T00:00:00Z`);
  return !Number.isNaN(d.getTime()) && d.toISOString().slice(0, 10) === s;
}

const toMs = (s) => Date.parse(`${s}T00:00:00Z`);
const fromMs = (ms) => new Date(ms).toISOString().slice(0, 10);

export function addDays(s, n) { return fromMs(toMs(s) + n * DAY); }

/** Whole days from a to b (b - a). */
export function daysBetween(a, b) { return Math.round((toMs(b) - toMs(a)) / DAY); }

/** Monday of the week containing s. */
export function weekStart(s) {
  const dow = new Date(toMs(s)).getUTCDay(); // 0 Sunday
  return addDays(s, dow === 0 ? -6 : 1 - dow);
}

/** Today in the browser's (or Worker's) local calendar. */
export function today(now = new Date()) {
  const y = now.getFullYear(), m = String(now.getMonth() + 1).padStart(2, "0"), d = String(now.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
}

const DOW = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const MON = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

/** "Mon, Feb 2" */
export function formatDay(s) {
  const d = new Date(toMs(s));
  return `${DOW[d.getUTCDay()]}, ${MON[d.getUTCMonth()]} ${d.getUTCDate()}`;
}

/** "Feb 2" */
export function formatShort(s) {
  const d = new Date(toMs(s));
  return `${MON[d.getUTCMonth()]} ${d.getUTCDate()}`;
}

/** "Feb 2 – Feb 8" */
export function formatRange(a, b) { return `${formatShort(a)} – ${formatShort(b)}`; }
