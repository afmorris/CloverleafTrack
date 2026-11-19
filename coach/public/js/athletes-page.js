// Athlete week: one practice week rewritten for athletes, to copy into an Apptegy message or save as a PDF.
// /practice/athletes?week=YYYY-MM-DD[&group=throws]. Leaves out coach notes and recorded marks. BRAIN.md [C139].
import { api, el, params, fill } from "./app.js";
import { addDays, formatDay, formatRange, isIsoDate, today, weekStart } from "./dates.js";
import { GROUPS, groupLabel } from "./practice.js";
import { labelWeeks, phaseForDate } from "./season.js";
import { buildHandout, handoutText } from "./handout.js";

const root = document.getElementById("content");
let monday = weekStart(isIsoDate(params.get("week")) ? params.get("week") : today());
let group = params.get("group");
let details = params.get("details") !== "0";
let practices = [], meets = [], seasonPlans = [];

const store = {
  get: (k) => { try { return localStorage.getItem(k) ?? ""; } catch { return ""; } },
  set: (k, v) => { try { v ? localStorage.setItem(k, v) : localStorage.removeItem(k); } catch { /* private mode */ } }
};
const messageKey = () => `athlete-message:${monday}:${group || "all"}`;

function setUrl() {
  const q = new URLSearchParams({ week: monday });
  if (group) q.set("group", group);
  if (!details) q.set("details", "0");
  history.replaceState(null, "", `/practice/athletes?${q}`);
}

/** "Week 6 · M2: Specific prep" from the season plan covering this week, or "". */
function weekContext() {
  const sp = seasonPlans.find((p) => p.startDate <= addDays(monday, 6) && monday <= p.endDate);
  if (!sp) return "";
  const w = labelWeeks(sp.startDate, sp.endDate, sp.firstPractice).find((x) => x.start === monday);
  const ph = phaseForDate(sp.phases, monday < sp.startDate ? sp.startDate : monday) ?? phaseForDate(sp.phases, addDays(monday, 6));
  return [w ? (w.offseason ? w.label : `Week ${w.label}`) : "", ph?.name ?? ""].filter(Boolean).join(" · ");
}

async function load() {
  const sunday = addDays(monday, 6);
  [practices, meets, seasonPlans] = await Promise.all([
    api(`/practices?from=${monday}&to=${sunday}`), api(`/meets?from=${monday}&to=${sunday}`), api("/season-plans")]);
  if (group === null) {
    const used = new Set(practices.flatMap((p) => p.groups));
    group = used.size === 1 ? [...used][0] : "";
  }
  setUrl();
  render();
}

function go(date) { monday = weekStart(date); group = params.get("group") ?? group; load(); }

function render() {
  const sunday = addDays(monday, 6);
  const heading = `${group ? groupLabel(group) : "Practice"}: week of ${formatRange(monday, sunday)}`;
  const context = weekContext();
  const handout = buildHandout({ practices, meets, group });
  const message = el("textarea", { id: "a-message", rows: 3, maxlength: 1000,
    placeholder: "Optional message at the top: what to bring, ride info, reminders. Saved on this device for this week.",
    oninput: (e) => { store.set(messageKey(), e.target.value); renderPreview(); } }, store.get(messageKey()));
  const status = el("span", { class: "status", role: "status" });
  const text = () => handoutText(handout, { heading: context ? `${heading} (${context})` : heading, message: message.value, details });

  const fallback = el("textarea", { id: "a-plain", rows: 12, readonly: true, class: "no-print", hidden: true,
    "aria-label": "Plain text for Apptegy" });
  async function copy() {
    const value = text();
    let ok = false;
    try { await navigator.clipboard.writeText(value); ok = true; } catch { /* clipboard blocked: try the older way */ }
    if (!ok) {
      fallback.value = value; fallback.hidden = false; fallback.select();
      try { ok = document.execCommand("copy"); } catch { ok = false; }
    }
    if (ok) {
      fallback.hidden = true;
      status.textContent = "Copied. Paste it into your Apptegy message."; status.className = "status";
    } else {
      fallback.focus(); fallback.select();
      status.textContent = "This browser blocked copying. The text is selected below: press Cmd+C (Ctrl+C)."; status.className = "status error";
    }
  }

  const preview = el("article", { id: "handout", class: "handout" });
  function renderPreview() {
    fill(preview,
      el("h1", {}, heading),
      context ? el("p", { class: "handout-context" }, context) : null,
      message.value.trim() ? el("p", { class: "handout-message" }, message.value.trim()) : null,
      handout.focus ? el("p", { class: "handout-focus" }, el("strong", {}, "Focus this week: "), handout.focus) : null,
      handout.days.length ? handout.days.map((d) => el("section", { class: "handout-day" },
        el("h2", {}, formatDay(d.date)),
        d.entries.map((e) => e.kind === "meet"
          ? el("div", { class: "handout-meet" }, el("strong", {}, `Meet: ${e.title}`), e.info ? el("p", {}, e.info) : null)
          : el("div", { class: "handout-practice" },
              el("p", { class: "handout-title" }, el("strong", {}, e.title), [e.time, e.place].filter(Boolean).map((x) => el("span", {}, ` · ${x}`))),
              e.blocks.length ? el("ol", {}, e.blocks.map((b) => el("li", {},
                el("strong", {}, b.title), b.minutes ? ` (${b.minutes} min)` : "",
                details && b.details ? el("div", { class: "handout-details" }, b.details) : null))) : null))))
        : el("p", { class: "empty" }, "No practices or meets this week."));
  }
  renderPreview();

  fill(root,
    el("div", { class: "page-head no-print" }, el("h1", {}, "Athlete week"),
      el("a", { href: `/practice?week=${monday}` }, "Coach view")),
    el("p", { class: "lede no-print" }, "This week as athletes should see it: times, places, the weekly focus, and each block. "
      + "Coach notes and recorded marks are left out. Copy it into an Apptegy message, or print / save as PDF to attach."),
    el("div", { class: "toolbar no-print" },
      el("button", { type: "button", class: "secondary", onclick: () => go(addDays(monday, -7)), "aria-label": "Previous week" }, "← Prev"),
      el("button", { type: "button", class: "secondary", onclick: () => go(today()) }, "This week"),
      el("button", { type: "button", class: "secondary", onclick: () => go(addDays(monday, 7)), "aria-label": "Next week" }, "Next →"),
      el("label", { class: "visually-hidden", for: "a-group" }, "Event group"),
      el("select", { id: "a-group", style: "width:auto", onchange: (e) => { group = e.target.value; setUrl(); render(); } },
        el("option", { value: "" }, "All groups"), GROUPS.map((g) => el("option", { value: g.id, selected: g.id === group }, g.label))),
      el("label", { for: "a-details", style: "margin:0;font-weight:500;display:flex;gap:.4rem;align-items:center" },
        el("input", { id: "a-details", type: "checkbox", checked: details, style: "width:auto",
          onchange: (e) => { details = e.target.checked; setUrl(); render(); } }), "Block details")),
    el("div", { class: "no-print" }, el("label", { for: "a-message" }, "Message to athletes"), message),
    el("div", { class: "toolbar no-print", style: "margin-top:.75rem" },
      el("button", { type: "button", onclick: copy }, "Copy for Apptegy"),
      el("button", { type: "button", class: "secondary", onclick: () => window.print() }, "Print / save as PDF"),
      status),
    fallback,
    preview);
}

load().catch((e) => fill(root, el("p", { class: "error" }, e.message)));
