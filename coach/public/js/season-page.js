// Season plan: phases on a calendar, volume and intensity by week, and the meet schedule from the results
// snapshot. /season lists plans; /season?id=N opens one. BRAIN.md [C134].
import { api, el, markDirty, params, fill } from "./app.js";
import { addDays, daysBetween, formatRange, formatShort, isIsoDate, today } from "./dates.js";
import {
  PHASE_KINDS, defaultFirstPractice, isChampionshipMeet, labelWeeks, phaseForDate, phaseKind, phaseProblems, suggestPhases
} from "./season.js";

const root = document.getElementById("content");
let plan = null, meets = [], practices = [];
let status = "";

// ---- List and create -------------------------------------------------------------------------------------------

/** Default dates for a new plan: the snapshot season covering today or later, else Dec 1 - Jun 30 of this school year. */
function defaultSeason(seasons) {
  const now = today();
  const s = seasons.find((x) => x.endDate >= now);
  if (s) return { name: s.name, startDate: s.startDate, endDate: s.endDate };
  const y = Number(now.slice(0, 4)) - (Number(now.slice(5, 7)) < 7 ? 1 : 0);
  return { name: `${y}-${y + 1}`, startDate: `${y}-12-01`, endDate: `${y + 1}-06-30` };
}

async function showList() {
  const [plans, seasons] = await Promise.all([api("/season-plans"), api("/seasons")]);
  const d = defaultSeason(seasons);
  const name = el("input", { id: "new-name", value: d.name, maxlength: 120 });
  const start = el("input", { id: "new-start", type: "date", value: d.startDate });
  const first = el("input", { id: "new-first", type: "date", value: defaultFirstPractice(d.endDate) });
  let firstTouched = false;
  first.addEventListener("input", () => { firstTouched = true; });
  const end = el("input", { id: "new-end", type: "date", value: d.endDate, oninput: () => {
    if (!firstTouched && isIsoDate(end.value)) first.value = defaultFirstPractice(end.value);
  } });
  const suggest = el("input", { id: "new-suggest", type: "checkbox", checked: true, style: "width:auto" });
  const msg = el("span", { class: "status", role: "status" });

  async function create() {
    if (!isIsoDate(start.value) || !isIsoDate(end.value)) return (msg.textContent = "Pick start and end dates.");
    let phases = [];
    if (suggest.checked) {
      const m = await api(`/meets?from=${start.value}&to=${end.value}`);
      phases = suggestPhases(start.value, end.value, m, isIsoDate(first.value) ? first.value : null);
    }
    try {
      const r = await api("/season-plans", { method: "POST", body: JSON.stringify({
        name: name.value, startDate: start.value, endDate: end.value,
        firstPractice: isIsoDate(first.value) ? first.value : null, phases }) });
      location.href = `/season?id=${r.id}`;
    } catch (e) { msg.textContent = e.message; msg.className = "status error"; }
  }

  fill(root,
    el("h1", {}, "Season plans"),
    el("p", { class: "lede" }, "Lay out training phases across the season. Meets come from cloverleaftrack.com, so enter the schedule there first."),
    plans.length ? el("div", { class: "template-list" }, plans.map((p) => el("a", { class: "card tool", href: `/season?id=${p.id}` },
      el("h2", {}, p.name), el("p", {}, formatRange(p.startDate, p.endDate), ` · ${p.phases.length} phases`),
      p.phases.length ? timeline(p, []) : null))) : null,
    el("section", { class: "card", style: "margin-top:1.25rem" },
      el("h2", {}, "New season plan"),
      el("div", { class: "grid" },
        el("div", {}, el("label", { for: "new-name" }, "Name"), name),
        el("div", {}, el("label", { for: "new-start" }, "Start"), start),
        el("div", {}, el("label", { for: "new-end" }, "End"), end),
        el("div", {}, el("label", { for: "new-first" }, "First practice"), first)),
      el("p", { class: "hint" }, "Before the first practice is off-season (winter lifting): its own phase, outside the season's week count. Leave it blank for none."),
      el("div", { class: "row sub" }, suggest, el("label", { for: "new-suggest", style: "margin:0;font-weight:500" },
        "Start with suggested phases from the meet schedule")),
      el("div", { class: "row sub" }, el("button", { type: "button", onclick: create }, "Create season plan"), msg)));
}

// ---- Timeline --------------------------------------------------------------------------------------------------

function timeline(p, meetList) {
  const total = daysBetween(p.startDate, p.endDate) + 1;
  const pct = (d) => `${(Math.max(0, Math.min(total, d)) / total) * 100}%`;
  const sorted = [...p.phases].sort((a, b) => a.startDate.localeCompare(b.startDate));
  const bar = el("div", { class: "timeline", role: "img",
    "aria-label": sorted.map((x) => `${x.name} ${formatRange(x.startDate, x.endDate)}`).join("; ") || "No phases" });
  let cursor = p.startDate;
  for (const ph of sorted) {
    if (ph.startDate > cursor) bar.append(el("div", { class: "gap", style: `width:${pct(daysBetween(cursor, ph.startDate))}` }));
    const s = ph.startDate < p.startDate ? p.startDate : ph.startDate;
    const e = ph.endDate > p.endDate ? p.endDate : ph.endDate;
    if (e >= s) {
      bar.append(el("div", { class: "seg", title: `${ph.name}: ${formatRange(ph.startDate, ph.endDate)}`,
        style: `width:${pct(daysBetween(s, e) + 1)};background:${phaseKind(ph.kind).color}` }, ph.name));
    }
    if (addDays(ph.endDate, 1) > cursor) cursor = addDays(ph.endDate, 1);
  }
  const marks = el("div", { class: "timeline-marks", "aria-hidden": "true" });
  for (const m of meetList) {
    marks.append(el("span", { class: `tick${isChampionshipMeet(m.name) ? " champ" : ""}`,
      style: `left:${pct(daysBetween(p.startDate, m.date) + 0.5)}`, title: `${formatShort(m.date)}: ${m.name}` }));
  }
  const now = today();
  if (now >= p.startDate && now <= p.endDate) marks.append(el("span", { class: "now", style: `left:${pct(daysBetween(p.startDate, now) + 0.5)}`, title: "Today" }));
  return el("div", {}, bar, meetList.length || marks.children.length ? marks : null);
}

const meter = (n, cls) => el("span", { class: `meter ${cls}`, "aria-label": `${cls} ${n} of 5` },
  [1, 2, 3, 4, 5].map((i) => el("i", { class: i <= n ? "on" : "" })));

// ---- Plan view -------------------------------------------------------------------------------------------------

async function showPlan(id) {
  plan = await api(`/season-plans/${encodeURIComponent(id)}`);
  await loadRangeData();
  renderPlan();
}

async function loadRangeData() {
  [meets, practices] = await Promise.all([
    api(`/meets?from=${plan.startDate}&to=${plan.endDate}`),
    isIsoDate(plan.startDate) && isIsoDate(plan.endDate) ? api(`/practices?from=${plan.startDate}&to=${plan.endDate}`) : []]);
}

const changed = () => { markDirty(true); refreshDerived(); };

function phaseRow(ph, i) {
  const set = (k, num = false) => (e) => { ph[k] = num ? Number(e.target.value) : e.target.value; changed(); };
  const scale = (k, label) => el("div", {}, el("label", { for: `ph-${i}-${k}` }, label),
    el("select", { id: `ph-${i}-${k}`, onchange: set(k, true) }, [1, 2, 3, 4, 5].map((n) => el("option", { value: n, selected: n === ph[k] }, n))));
  return el("div", { class: "phase-row" },
    el("div", {}, el("label", { for: `ph-${i}-kind` }, "Kind"),
      el("select", { id: `ph-${i}-kind`, onchange: (e) => {
        const wasDefault = ph.name === phaseKind(ph.kind).label;
        ph.kind = e.target.value;
        if (wasDefault) ph.name = phaseKind(ph.kind).label;
        markDirty(true); renderPlan();
      } }, PHASE_KINDS.map((k) => el("option", { value: k.id, selected: k.id === ph.kind }, k.label)))),
    el("div", {}, el("label", { for: `ph-${i}-name` }, "Name"), el("input", { id: `ph-${i}-name`, value: ph.name, maxlength: 80, oninput: set("name") })),
    el("div", {}, el("label", { for: `ph-${i}-start` }, "Start"), el("input", { id: `ph-${i}-start`, type: "date", value: ph.startDate, oninput: set("startDate") })),
    el("div", {}, el("label", { for: `ph-${i}-end` }, "End"), el("input", { id: `ph-${i}-end`, type: "date", value: ph.endDate, oninput: set("endDate") })),
    scale("volume", "Volume"), scale("intensity", "Intensity"),
    el("button", { type: "button", class: "link", "aria-label": `Remove phase ${ph.name}`,
      onclick: () => { plan.phases.splice(i, 1); markDirty(true); renderPlan(); } }, "Remove"),
    el("div", { class: "focus" }, el("label", { for: `ph-${i}-focus` }, "Focus"),
      el("input", { id: `ph-${i}-focus`, value: ph.focus ?? "", maxlength: 500, placeholder: "What this phase is for", oninput: set("focus") })));
}

function weeksTable() {
  if (!isIsoDate(plan.startDate) || !isIsoDate(plan.endDate) || plan.endDate < plan.startDate) return el("p", { class: "empty" }, "Set valid dates.");
  const now = today();
  return el("div", { style: "overflow-x:auto" }, el("table", {},
    el("thead", {}, el("tr", {}, ["Week", "Dates", "Phase", "Volume", "Intensity", "Meets", "Practices"].map((h) => el("th", {}, h)))),
    el("tbody", {}, labelWeeks(plan.startDate, plan.endDate, plan.firstPractice).map((w) => {
      const ph = phaseForDate(plan.phases, w.start < plan.startDate ? plan.startDate : w.start)
        ?? phaseForDate(plan.phases, w.end);
      const wm = meets.filter((m) => m.date >= w.start && m.date <= w.end);
      const count = practices.filter((p) => p.date >= w.start && p.date <= w.end).length;
      const cls = [now >= w.start && now <= w.end ? "current-week" : "", w.offseason ? "offseason" : ""].filter(Boolean).join(" ");
      return el("tr", { class: cls },
        el("td", { style: "white-space:nowrap" }, w.label),
        el("td", {}, el("a", { href: `/practice?week=${w.start}` }, formatRange(w.start, w.end))),
        el("td", {}, ph ? el("span", { class: "phase-pill", style: `--phase:${phaseKind(ph.kind).color}` }, ph.name) : el("span", { class: "empty" }, "—")),
        el("td", {}, ph ? meter(ph.volume, "volume") : null),
        el("td", {}, ph ? meter(ph.intensity, "intensity") : null),
        el("td", {}, wm.map((m) => el("span", { class: `chip small meet${isChampionshipMeet(m.name) ? " champ" : ""}`, title: formatShort(m.date) }, m.name))),
        el("td", { class: "num" }, count || ""));
    }))));
}

/** Redraw the parts that depend on dates and phases without rebuilding the inputs being typed in. */
function refreshDerived() {
  const t = document.getElementById("timeline"); if (t) fill(t, timeline(plan, meets));
  const w = document.getElementById("weeks"); if (w) fill(w, weeksTable());
  const p = document.getElementById("problems");
  if (p) {
    const problems = isIsoDate(plan.startDate) && isIsoDate(plan.endDate) ? phaseProblems(plan.phases, plan.startDate, plan.endDate) : ["Set valid start and end dates."];
    fill(p, ...problems.map((x) => el("li", {}, x)));
  }
}

function renderPlan() {
  const setPlan = (k) => (e) => { plan[k] = e.target.value; changed(); };
  const reload = async () => { if (isIsoDate(plan.startDate) && isIsoDate(plan.endDate)) { await loadRangeData(); refreshDerived(); } };

  fill(root,
    el("div", { class: "page-head" }, el("h1", {}, plan.name), el("a", { class: "no-print", href: "/season" }, "All season plans")),
    el("section", { class: "card" },
      el("div", { class: "grid" },
        el("div", {}, el("label", { for: "p-name" }, "Name"), el("input", { id: "p-name", value: plan.name, maxlength: 120, oninput: setPlan("name") })),
        el("div", {}, el("label", { for: "p-start" }, "Start"), el("input", { id: "p-start", type: "date", value: plan.startDate, oninput: setPlan("startDate"), onchange: reload })),
        el("div", {}, el("label", { for: "p-end" }, "End"), el("input", { id: "p-end", type: "date", value: plan.endDate, oninput: setPlan("endDate"), onchange: reload })),
        el("div", {}, el("label", { for: "p-first" }, "First practice"), el("input", { id: "p-first", type: "date", value: plan.firstPractice ?? "",
          oninput: (e) => { plan.firstPractice = e.target.value || null; changed(); } }))),
      el("div", { class: "sub" }, el("label", { for: "p-notes" }, "Notes"),
        el("textarea", { id: "p-notes", maxlength: 4000, oninput: setPlan("notes") }, plan.notes ?? ""))),
    el("section", { class: "card" },
      el("h2", {}, "Timeline"),
      el("div", { id: "timeline" }),
      el("div", { class: "timeline-legend" },
        PHASE_KINDS.map((k) => el("span", { class: "phase-pill", style: `--phase:${k.color}` }, k.label)),
        el("span", {}, "▮ meet, ▮▮ post-season meet, red line: today"))),
    el("section", { class: "card no-print" },
      el("div", { class: "page-head" }, el("h2", {}, "Phases"),
        el("button", { type: "button", class: "secondary", onclick: async () => {
          if (plan.phases.length && !confirm("Replace all phases with a suggestion from the meet schedule?")) return;
          plan.phases = suggestPhases(plan.startDate, plan.endDate, meets, plan.firstPractice); markDirty(true); renderPlan();
        } }, "Suggest from meets")),
      plan.phases.length ? plan.phases.map(phaseRow) : el("p", { class: "empty" }, "No phases yet."),
      el("button", { type: "button", class: "secondary", onclick: () => {
        const last = [...plan.phases].sort((a, b) => b.endDate.localeCompare(a.endDate))[0];
        const start = last ? addDays(last.endDate, 1) : plan.startDate;
        const endGuess = addDays(start, 27);
        plan.phases.push({ kind: "competition", name: "Competition", startDate: start,
          endDate: endGuess > plan.endDate ? plan.endDate : endGuess, volume: 3, intensity: 3, focus: "" });
        markDirty(true); renderPlan();
      } }, "Add phase"),
      el("ul", { id: "problems", class: "problems", "aria-live": "polite" })),
    el("section", { class: "card" }, el("h2", {}, "Week by week"), el("div", { id: "weeks" })),
    el("section", { class: "card no-print" }, el("div", { class: "row" },
      el("button", { type: "button", onclick: save }, "Save"),
      el("button", { type: "button", class: "secondary", onclick: () => window.print() }, "Print"),
      el("button", { type: "button", class: "link", onclick: remove }, "Delete plan"),
      el("span", { id: "status", class: "status", role: "status" }, status))));
  refreshDerived();
}

async function save() {
  const node = document.getElementById("status");
  try {
    await api(`/season-plans/${plan.id}`, { method: "PUT", body: JSON.stringify(plan) });
    markDirty(false);
    plan = await api(`/season-plans/${plan.id}`);
    status = "Saved.";
    renderPlan();
  } catch (e) {
    node.textContent = e.message; node.className = "status error";
  }
}

async function remove() {
  if (!confirm(`Delete the season plan “${plan.name}”? Practice plans are kept. This can't be undone.`)) return;
  await api(`/season-plans/${plan.id}`, { method: "DELETE" });
  markDirty(false);
  location.href = "/season";
}

(params.get("id") ? showPlan(params.get("id")) : showList())
  .catch((e) => fill(root, el("p", { class: "error" }, e.message)));
