// Practice week: seven days with their plans, meets, and season phase. ?week=YYYY-MM-DD (any day in the week).
import { api, el, params, fill } from "./app.js";
import { addDays, formatDay, formatRange, isIsoDate, today, weekStart } from "./dates.js";
import { formatMinutes, groupLabel, practiceLength } from "./practice.js";
import { isChampionshipMeet, phaseForDate, phaseKind } from "./season.js";

const $ = (id) => document.getElementById(id);
let monday = weekStart(isIsoDate(params.get("week")) ? params.get("week") : today());

function go(date) {
  monday = weekStart(date);
  history.replaceState(null, "", `/practice?week=${monday}`);
  load();
}

/** The phase for a date from whichever season plan covers it. */
function phaseOn(seasonPlans, date) {
  for (const sp of seasonPlans) {
    if (sp.startDate <= date && date <= sp.endDate) return phaseForDate(sp.phases, date);
  }
  return null;
}

const phasePill = (phase) => phase
  ? el("span", { class: "phase-pill", style: `--phase:${phaseKind(phase.kind).color}`, title: phase.focus || "" }, phase.name)
  : null;

async function load() {
  const sunday = addDays(monday, 6);
  $("week-title").textContent = `Week of ${formatRange(monday, sunday)}`;
  document.title = `Practice: ${formatRange(monday, sunday)}`;
  $("jump").value = monday;
  $("athlete-link").href = `/practice/athletes?week=${monday}`;
  const box = $("week");
  let practices, meets, seasonPlans;
  try {
    [practices, meets, seasonPlans] = await Promise.all([
      api(`/practices?from=${monday}&to=${sunday}`), api(`/meets?from=${monday}&to=${sunday}`), api("/season-plans")]);
  } catch (e) {
    return fill(box, el("p", { class: "error" }, e.message));
  }
  fill($("week-phase"), phasePill(phaseOn(seasonPlans, monday)) ?? "");

  const now = today();
  fill(box, ...[0, 1, 2, 3, 4, 5, 6].map((i) => {
    const date = addDays(monday, i);
    const dayPlans = practices.filter((p) => p.date === date);
    const dayMeets = meets.filter((m) => m.date === date);
    const phase = phaseOn(seasonPlans, date);
    const phaseChanged = i > 0 && phase && phase !== phaseOn(seasonPlans, addDays(date, -1));
    return el("section", { class: `day${date === now ? " today" : ""}`, "aria-label": formatDay(date) },
      el("h2", {}, formatDay(date)),
      phaseChanged ? phasePill(phase) : null,
      dayMeets.length ? el("div", {}, dayMeets.map((m) =>
        el("span", { class: `chip small meet${isChampionshipMeet(m.name) ? " champ" : ""}` }, m.name))) : null,
      dayPlans.map((p) => el("a", { class: "plan-card", href: `/practice/edit?id=${p.id}` },
        el("strong", {}, p.title),
        el("span", { class: "meta" }, `${p.groups.map(groupLabel).join(", ")} · ${formatMinutes(practiceLength(p.blocks))}`),
        p.blocks.length ? el("ul", {}, p.blocks.map((b) => el("li", {}, b.title || b.kind, b.minutes ? ` (${b.minutes}m)` : ""))) : null)),
      el("a", { class: "add no-print", href: `/practice/edit?date=${date}` }, "+ Add plan"));
  }));
}

$("prev").addEventListener("click", () => go(addDays(monday, -7)));
$("next").addEventListener("click", () => go(addDays(monday, 7)));
$("this-week").addEventListener("click", () => go(today()));
$("jump").addEventListener("change", (e) => { if (isIsoDate(e.target.value)) go(e.target.value); });
load();
