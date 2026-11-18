// Goal pace builder page. All math lives in pace.js; this file only wires inputs to it.
import { api, el, params, fill } from "./app.js";
import {
  bestMark, buildWorkout, evenSplits, formatChange, formatRest, formatTime, parseTime
} from "./pace.js";

const $ = (id) => document.getElementById(id);
const state = {
  mode: "athlete",
  athletes: [],
  marks: [],            // season bests for the chosen athlete, running events only
  event: null,          // { eventId, eventName, environment, distanceM, pr, latest }
  sets: [{ reps: 4, distance: 200, effort: 90, rest: 180 }]
};

const eventLabel = (name, environment) => (environment === 2 ? `${name} (indoor)` : name);
const classOf = (year) => `’${String(year).slice(-2)}`;

// ---- Goal ---------------------------------------------------------------------------------------------------

function setMode(mode) {
  state.mode = mode;
  $("mode-athlete").setAttribute("aria-pressed", String(mode === "athlete"));
  $("mode-goal").setAttribute("aria-pressed", String(mode === "goal"));
  $("athlete-fields").hidden = mode !== "athlete";
  $("marks").hidden = mode !== "athlete";
  $("distance-field").hidden = mode !== "goal";
  render();
}

async function loadAthletes() {
  try {
    state.athletes = await api("/athletes");
  } catch (e) {
    fill($("athlete"), el("option", { value: "" }, "Couldn't load athletes"));
    return;
  }
  const select = $("athlete");
  fill(select, el("option", { value: "" }, state.athletes.length ? "Choose…" : "No athletes loaded yet"));
  for (const [gender, label] of [[2, "Girls"], [1, "Boys"]]) {
    const group = el("optgroup", { label });
    for (const a of state.athletes.filter((x) => x.gender === gender)) {
      group.append(el("option", { value: a.id }, `${a.lastName}, ${a.firstName} ${classOf(a.graduationYear)}`));
    }
    if (group.children.length) select.append(group);
  }
}

async function chooseAthlete(id) {
  state.event = null;
  state.marks = [];
  const select = $("event");
  select.disabled = true;
  if (!id) {
    fill(select, el("option", { value: "" }, "Pick an athlete first"));
    renderMarks();
    return render();
  }
  fill(select, el("option", { value: "" }, "Loading…"));
  const rows = await api(`/athletes/${id}/marks`);
  state.marks = rows.filter((r) => r.eventType === 1 && r.distanceM);

  const events = new Map();
  for (const r of state.marks) {
    if (!events.has(r.eventId)) events.set(r.eventId, { eventId: r.eventId, eventName: r.eventName,
      environment: r.environment, distanceM: r.distanceM, rows: [] });
    events.get(r.eventId).rows.push(r);
  }
  for (const ev of events.values()) {
    ev.pr = bestMark(ev.rows, true);
    ev.latest = ev.rows[0]; // newest season first from the API
  }
  const list = [...events.values()];
  fill(select, el("option", { value: "" }, list.length ? "Choose…" : "No running marks yet"));
  for (const ev of list) {
    select.append(el("option", { value: ev.eventId },
      `${eventLabel(ev.eventName, ev.environment)} · PR ${formatTime(ev.pr.timeSeconds)}`));
  }
  select.disabled = list.length === 0;
  select._events = events;
  if (list.length === 1) { select.value = String(list[0].eventId); chooseEvent(select.value); }
  else { renderMarks(); render(); }
}

function chooseEvent(id) {
  state.event = $("event")._events?.get(Number(id)) ?? null;
  if (state.event) $("goal").value = formatTime(state.event.pr.timeSeconds);
  renderMarks();
  render();
}

function renderMarks() {
  const box = $("marks");
  const ev = state.event;
  if (!ev) return fill(box);
  const chip = (label, seconds) => el("button", {
    type: "button", class: "chip", title: "Use as goal",
    onclick: () => { $("goal").value = formatTime(seconds); render(); }
  }, label);
  const chips = [chip(`PR ${formatTime(ev.pr.timeSeconds)} · ${ev.pr.meetName}, ${ev.pr.meetDate}`, ev.pr.timeSeconds)];
  if (ev.latest && ev.latest.seasonId !== ev.pr.seasonId) {
    chips.push(chip(`${ev.latest.seasonName} best ${formatTime(ev.latest.timeSeconds)}`, ev.latest.timeSeconds));
  }
  fill(box, ...chips);
}

/** { goalSeconds, eventMeters, eventName, athleteId } from the inputs, or null fields when incomplete. */
function currentGoal() {
  const goalSeconds = parseTime($("goal").value);
  if (state.mode === "athlete") {
    return { goalSeconds, eventMeters: state.event?.distanceM ?? null,
      eventName: state.event ? eventLabel(state.event.eventName, state.event.environment) : null,
      athleteId: $("athlete").value ? Number($("athlete").value) : null };
  }
  const meters = Number($("distance").value);
  return { goalSeconds, eventMeters: meters > 0 ? meters : null, eventName: meters > 0 ? `${meters}M` : null, athleteId: null };
}

// ---- Sets ---------------------------------------------------------------------------------------------------

function renderSets() {
  const box = $("sets");
  fill(box, ...state.sets.map((set, i) => {
    const field = (key, label, attrs, value) => {
      const id = `set-${i}-${key}`;
      return el("div", {},
        el("label", { for: id }, label),
        el("input", { id, value, ...attrs, oninput: (e) => {
          state.sets[i][key] = key === "rest" ? (parseTime(e.target.value) ?? 0) : Number(e.target.value);
          render(false);
        } }));
    };
    return el("div", { class: "set-row" },
      field("reps", "Reps", { type: "number", min: 1, max: 50, inputmode: "numeric" }, set.reps),
      field("distance", "Distance (m)", { type: "number", min: 1, inputmode: "numeric", list: "distances" }, set.distance),
      field("effort", "Effort %", { type: "number", min: 50, max: 110, inputmode: "numeric" }, set.effort),
      field("rest", "Rest", { inputmode: "decimal", placeholder: "3:00" }, formatRest(set.rest)),
      el("button", { type: "button", class: "link remove no-print", "aria-label": `Remove set ${i + 1}`,
        disabled: state.sets.length === 1,
        onclick: () => { state.sets.splice(i, 1); render(); } }, "Remove"));
  }));
}

// ---- Output -------------------------------------------------------------------------------------------------

function render(redrawSets = true) {
  if (redrawSets) renderSets();
  const goal = currentGoal();
  const hint = $("goal-hint");
  hint.textContent = "";
  if ($("goal").value && goal.goalSeconds == null) hint.textContent = "Use seconds (58.5) or minutes:seconds (2:15.0).";
  else if (state.mode === "athlete" && state.event && goal.goalSeconds) {
    hint.textContent = `Change from PR: ${formatChange(state.event.pr.timeSeconds, goal.goalSeconds)}`;
  }

  if (!(goal.goalSeconds && goal.eventMeters)) {
    const msg = el("p", { class: "empty" }, state.mode === "athlete" && !state.event ? "Pick an athlete and event." : "Enter a goal time.");
    fill($("race-splits"), msg);
    fill($("targets"), msg.cloneNode(true));
    return;
  }

  const race = evenSplits(goal.goalSeconds, goal.eventMeters);
  fill($("race-splits"), el("table", {},
    el("thead", {}, el("tr", {}, el("th", {}, "Mark"), el("th", { class: "num" }, "Time"), el("th", { class: "num" }, "Segment"))),
    el("tbody", {}, race.map((s, i) => el("tr", {},
      el("td", {}, `${s.at}m`),
      el("td", { class: "num" }, formatTime(s.time)),
      el("td", { class: "num" }, formatTime(s.time - (i ? race[i - 1].time : 0))))))));

  const w = buildWorkout({ goalSeconds: goal.goalSeconds, eventMeters: goal.eventMeters, sets: state.sets });
  fill($("targets"),
    el("p", { class: "status" }, `${goal.eventName} goal ${formatTime(goal.goalSeconds)}`,
      goal.athleteId ? ` for ${$("athlete").selectedOptions[0].textContent}` : "",
      ` · total volume ${w.volume.toLocaleString()}m`),
    el("table", {},
      el("thead", {}, el("tr", {}, el("th", {}, "Set"), el("th", { class: "num" }, "Each rep"),
        el("th", {}, "Splits"), el("th", {}, "Rest"))),
      el("tbody", {}, w.sets.map((s) => el("tr", {},
        el("td", {}, `${s.reps} × ${s.distance}m @ ${s.effort}%`),
        el("td", { class: "num big" }, s.target == null ? "–" : formatTime(s.target)),
        el("td", { class: "splits" }, s.splits.length > 1 ? s.splits.map((p) => `${p.at}: ${formatTime(p.time)}`).join(" · ") : ""),
        el("td", {}, formatRest(s.rest)))))));
}

// ---- Saved workouts -----------------------------------------------------------------------------------------

async function loadSaved() {
  const box = $("saved");
  let rows;
  try { rows = await api("/workouts"); } catch (e) { return fill(box, el("p", { class: "error" }, e.message)); }
  if (!rows.length) return fill(box, el("p", { class: "empty" }, "Nothing saved yet."));
  fill(box, el("table", { class: "saved-table" },
    el("thead", {}, el("tr", {}, el("th", {}, "Workout"), el("th", {}, "Goal"), el("th", { class: "col-saved" }, "Saved"),
      el("th", {}, el("span", { class: "visually-hidden" }, "Actions")))),
    el("tbody", {}, rows.map((w) => el("tr", {},
      el("td", {}, el("strong", {}, w.title), w.athleteName ? el("div", { class: "splits" }, w.athleteName) : null,
        el("div", { class: "splits" }, w.sets.map((s) => `${s.reps}×${s.distance} @ ${s.effort}%`).join(", "))),
      el("td", {}, `${w.eventName ?? `${w.eventDistanceM}m`} ${formatTime(w.goalSeconds)}`),
      el("td", { class: "splits col-saved" }, `${new Date(w.createdAt).toLocaleDateString()} · ${w.createdBy}`),
      el("td", { class: "actions" },
        el("button", { type: "button", class: "secondary", onclick: () => loadWorkout(w) }, "Open"),
        el("button", { type: "button", class: "link", onclick: () => deleteWorkout(w) }, "Delete")))))));
}

function loadWorkout(w) {
  // Opened as a typed goal so it shows exactly what was saved, even if the athlete has since graduated.
  setMode("goal");
  $("distance").value = w.eventDistanceM;
  $("goal").value = formatTime(w.goalSeconds);
  $("title").value = w.title;
  $("notes").value = w.notes ?? "";
  state.sets = w.sets.map((s) => ({ ...s }));
  render();
  window.scrollTo({ top: 0, behavior: "smooth" });
}

async function deleteWorkout(w) {
  if (!confirm(`Delete “${w.title}”? This can't be undone.`)) return;
  try { await api(`/workouts/${w.id}`, { method: "DELETE" }); } catch (e) { alert(e.message); }
  loadSaved();
}

async function save() {
  const status = $("save-status");
  const goal = currentGoal();
  const title = $("title").value.trim();
  if (!goal.goalSeconds || !goal.eventMeters) return (status.textContent = "Set a goal first.");
  if (!title) { status.textContent = "Give it a name."; return $("title").focus(); }
  $("save").disabled = true;
  try {
    await api("/workouts", { method: "POST", body: JSON.stringify({
      title, notes: $("notes").value.trim() || null, athleteId: goal.athleteId, eventName: goal.eventName,
      eventDistanceM: goal.eventMeters, goalSeconds: goal.goalSeconds, sets: state.sets }) });
    status.textContent = "Saved.";
    loadSaved();
  } catch (e) {
    status.textContent = e.message;
  } finally {
    $("save").disabled = false;
  }
}

// ---- Wiring -------------------------------------------------------------------------------------------------

$("mode-athlete").addEventListener("click", () => setMode("athlete"));
$("mode-goal").addEventListener("click", () => setMode("goal"));
$("athlete").addEventListener("change", (e) => chooseAthlete(e.target.value).catch((err) => alert(err.message)));
$("event").addEventListener("change", (e) => chooseEvent(e.target.value));
$("goal").addEventListener("input", () => render(false));
$("distance").addEventListener("input", () => render(false));
$("add-set").addEventListener("click", () => {
  const last = state.sets[state.sets.length - 1];
  state.sets.push({ ...last });
  render();
});
$("save").addEventListener("click", save);

render();
loadAthletes();
loadSaved();
// /pace?workout=12 opens a saved workout (linked from practice plans).
if (params.get("workout")) {
  api(`/workouts/${encodeURIComponent(params.get("workout"))}`).then(loadWorkout).catch(() => {
    $("save-status").textContent = "That saved workout no longer exists.";
  });
}
