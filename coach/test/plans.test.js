import { describe, expect, it } from "vitest";
import { addDays, daysBetween, formatDay, isIsoDate, weekStart } from "../public/js/dates.js";
import {
  blocksFromTemplate, formatMinutes, mergeTemplateBlocks, moveBlock, newBlock, practiceLength, resultCount, totalMinutes, planTime
} from "../public/js/practice.js";
import {
  defaultFirstPractice, isChampionshipMeet, labelWeeks, phaseForDate, phaseProblems, suggestPhases, summarizeWeeks, weeksOf
} from "../public/js/season.js";

describe("dates", () => {
  it("validates real calendar dates only", () => {
    expect(isIsoDate("2027-02-28")).toBe(true);
    expect(isIsoDate("2027-02-29")).toBe(false);
    expect(isIsoDate("2027-2-1")).toBe(false);
    expect(isIsoDate(null)).toBe(false);
  });
  it("adds days across months and years", () => {
    expect(addDays("2026-12-30", 3)).toBe("2027-01-02");
    expect(daysBetween("2027-01-01", "2027-03-01")).toBe(59);
  });
  it("finds the Monday of a week", () => {
    expect(weekStart("2027-02-03")).toBe("2027-02-01"); // Wednesday
    expect(weekStart("2027-02-07")).toBe("2027-02-01"); // Sunday
    expect(weekStart("2027-02-01")).toBe("2027-02-01");
  });
  it("formats without time zone drift", () => expect(formatDay("2027-03-01")).toBe("Mon, Mar 1"));
});

describe("practice blocks", () => {
  it("totals minutes, ignoring blanks", () =>
    expect(totalMinutes([{ minutes: 20 }, { minutes: "15" }, { minutes: "" }, {}])).toBe(35));
  it("measures practice by the longest group, since groups train side by side", () => {
    const blocks = [
      { minutes: 20, groups: ["sprints", "throws"] },
      { minutes: 45, groups: ["sprints"] },
      { minutes: 60, groups: ["throws"] },
      { minutes: 10, groups: [] }
    ];
    expect(practiceLength(blocks)).toBe(90); // throws: 20 + 60 + 10
    expect(practiceLength([{ minutes: 30 }, { minutes: 15 }])).toBe(45);
  });
  it.each([[0, "0m"], [45, "45m"], [60, "1h"], [95, "1h 35m"]])("formats %s as %s", (m, s) => expect(formatMinutes(m)).toBe(s));
  it("moves blocks and ignores moves off the ends", () => {
    const b = ["a", "b", "c"];
    expect(moveBlock(b, 0, 1)).toEqual(["b", "a", "c"]);
    expect(moveBlock(b, 0, -1)).toBe(b);
  });
  it("keeps only the plan's groups when copying a template", () => {
    const out = blocksFromTemplate([
      { kind: "drills", title: "A", minutes: 10, groups: ["sprints", "jumps"] },
      { kind: "workout", title: "B", minutes: 20, groups: ["throws"] }
    ], ["sprints"]);
    expect(out[0].groups).toEqual(["sprints"]);
    expect(out[1].groups).toEqual(["sprints"]); // nothing left, so it takes the plan's groups
    expect(out[0].results).toEqual([]);
  });
  it("shares the warm-up and cool-down when adding a second template", () => {
    const plan = [
      { kind: "warmup", title: "W", minutes: 20, groups: ["sprints"] },
      { kind: "workout", title: "Accel", minutes: 25, groups: ["sprints"] },
      { kind: "cooldown", title: "C", minutes: 10, groups: ["sprints"] }
    ];
    const tpl = [
      { kind: "warmup", title: "TW", minutes: 15, groups: ["throws"] },
      { kind: "measured", title: "Throws", minutes: 30, groups: ["throws"] },
      { kind: "cooldown", title: "TC", minutes: 15, groups: ["throws"] }
    ];
    const out = mergeTemplateBlocks(plan, tpl, ["sprints", "throws"]);
    expect(out.map((b) => b.title)).toEqual(["W", "Accel", "Throws", "C"]);
    expect(out[0]).toMatchObject({ groups: ["sprints", "throws"], minutes: 20 });
    expect(out[3]).toMatchObject({ groups: ["sprints", "throws"], minutes: 15 });
    expect(plan[0].groups).toEqual(["sprints"]); // input untouched
  });
  it("starts new blocks with the plan's groups", () => expect(newBlock("warmup", ["jumps"]).groups).toEqual(["jumps"]));
  it("counts recorded marks", () =>
    expect(resultCount([{ results: [{ mark: "18' 2\"" }, { mark: "" }] }, { results: [] }, {}])).toBe(1));
});

describe("season phases", () => {
  it("recognizes post-season meets", () => {
    expect(isChampionshipMeet("2026 OHSAA Division III State Track and Field Championships")).toBe(true);
    expect(isChampionshipMeet("2026 Division III Regional Track Meet")).toBe(true);
    expect(isChampionshipMeet("2026 Division III District Track Meet")).toBe(true);
    expect(isChampionshipMeet("2026 Metro Athletic Conference Championships")).toBe(true);
    expect(isChampionshipMeet("2026 Heidelberg HS Qualifier")).toBe(false);
    expect(isChampionshipMeet("2026 Kent State HS Meet #1")).toBe(false);
    expect(isChampionshipMeet("2026 Ohio State High School Qualifier #4")).toBe(false);
    expect(isChampionshipMeet("2026 Ravenna-Streetsboro Double Dual")).toBe(false);
  });

  const phases = [
    { name: "A", kind: "general", startDate: "2027-01-04", endDate: "2027-01-31" },
    { name: "B", kind: "competition", startDate: "2027-02-01", endDate: "2027-05-15" }
  ];
  it("finds the phase for a date, inclusive", () => {
    expect(phaseForDate(phases, "2027-01-31").name).toBe("A");
    expect(phaseForDate(phases, "2027-02-01").name).toBe("B");
    expect(phaseForDate(phases, "2027-06-01")).toBeNull();
  });
  it("reports overlaps, reversed ranges, and phases outside the plan", () => {
    expect(phaseProblems(phases, "2027-01-04", "2027-06-30")).toEqual([]);
    const bad = [...phases, { name: "C", kind: "championship", startDate: "2027-05-10", endDate: "2027-05-01" }];
    const p = phaseProblems(bad, "2027-01-10", "2027-05-12");
    expect(p).toContain("C ends before it starts.");
    expect(p).toContain("A starts before the season plan.");
    expect(p).toContain("B overlaps C.");
    expect(p).toContain("B ends after the season plan.");
  });
  it("lists Monday weeks covering the range", () => {
    const w = weeksOf("2027-01-06", "2027-01-20");
    expect(w.map((x) => x.start)).toEqual(["2027-01-04", "2027-01-11", "2027-01-18"]);
    expect(w[2]).toEqual({ start: "2027-01-18", end: "2027-01-24", index: 3 });
  });

  it("builds phases from the meet schedule with no gaps or overlaps", () => {
    const meets = [
      { name: "Indoor Opener", date: "2027-01-30", environment: 2 },
      { name: "Dual vs X", date: "2027-04-06", environment: 1 },
      { name: "Division III District Track Meet", date: "2027-05-20" },
      { name: "Division III Regional Track Meet", date: "2027-05-27" },
      { name: "State Championships", date: "2027-06-04" }
    ];
    const p = suggestPhases("2026-12-01", "2027-06-30", meets);
    expect(p.map((x) => x.kind)).toEqual(["general", "specific", "competition", "championship", "transition"]);
    expect(p[0].startDate).toBe("2026-12-01");
    expect(p[2].startDate).toBe("2027-04-05"); // week of the first outdoor meet
    expect(p[3].startDate).toBe("2027-05-17"); // week of districts
    expect(p[3].endDate).toBe("2027-06-04");   // last meet
    expect(p[4].endDate).toBe("2027-06-30");
    expect(phaseProblems(p, "2026-12-01", "2027-06-30")).toEqual([]);
    for (let i = 1; i < p.length; i++) expect(daysBetween(p[i - 1].endDate, p[i].startDate)).toBe(1);
  });
  it("treats indoor meets and early 'championships' as prep, like last season's schedule", () => {
    const meets = [
      { name: "2025 Akron Rust Buster HS Open", date: "2025-12-13", environment: 2 },
      { name: "2026 Kent State HS Meet #1", date: "2026-01-24", environment: 2 },
      { name: "Indoor Conference Championships", date: "2026-02-21", environment: 2 },
      { name: "2026 Cloverleaf Early Bird Relays", date: "2026-03-28", environment: 1 },
      { name: "2026 Metro Athletic Conference Championships", date: "2026-05-16", environment: 1 },
      { name: "2026 Division III District Track Meet", date: "2026-05-23", environment: 1 },
      { name: "2026 OHSAA Division III State Track and Field Championships", date: "2026-06-06", environment: 1 }
    ];
    const p = suggestPhases("2025-12-01", "2026-06-30", meets);
    expect(p.map((x) => [x.kind, x.startDate])).toEqual([
      ["general", "2025-12-01"], ["specific", "2026-02-01"], ["competition", "2026-03-23"],
      ["championship", "2026-05-11"], ["transition", "2026-06-07"]]);
    expect(phaseProblems(p, "2025-12-01", "2026-06-30")).toEqual([]);
  });
  it("splits the season when there are no meets yet", () => {
    const p = suggestPhases("2027-01-04", "2027-06-06", []);
    expect(p[0].startDate).toBe("2027-01-04");
    expect(p[p.length - 1].endDate).toBe("2027-06-06");
    expect(phaseProblems(p, "2027-01-04", "2027-06-06")).toEqual([]);
  });
  it("uses one phase for a very short plan", () => expect(suggestPhases("2027-03-01", "2027-03-05", [])).toHaveLength(1));

  it("puts winter lifting before the first practice and builds the cycles from it", () => {
    const meets = [
      { name: "2027 Kent State HS Meet #1", date: "2027-01-23", environment: 2 },
      { name: "2027 Cloverleaf Early Bird Relays", date: "2027-03-27", environment: 1 },
      { name: "2027 Division III District Track Meet", date: "2027-05-22", environment: 1 },
      { name: "2027 OHSAA Division III State Track and Field Championships", date: "2027-06-05", environment: 1 }
    ];
    const p = suggestPhases("2026-12-01", "2027-06-30", meets, "2027-02-22");
    expect(p.map((x) => [x.kind, x.startDate])).toEqual([
      ["offseason", "2026-12-01"], ["general", "2027-02-22"], ["specific", "2027-03-09"], ["competition", "2027-03-22"],
      ["championship", "2027-05-17"], ["transition", "2027-06-06"]]);
    expect(p[0]).toMatchObject({ name: "Winter lifting", endDate: "2027-02-21" });
    expect(phaseProblems(p, "2026-12-01", "2027-06-30")).toEqual([]);
  });
  it("ignores a first practice outside the plan", () => {
    expect(suggestPhases("2027-02-22", "2027-06-30", [], "2027-02-22")[0].kind).toBe("general");
    expect(suggestPhases("2027-02-22", "2027-06-30", [], "2027-08-01")[0].kind).toBe("general");
  });
  it("defaults the first practice to the Monday of the last week of February", () => {
    expect(defaultFirstPractice("2027-06-30")).toBe("2027-02-22");
    expect(defaultFirstPractice("2028-06-30")).toBe("2028-02-28");
  });
  it("numbers off-season weeks apart from season weeks", () => {
    const w = labelWeeks("2027-02-01", "2027-03-14", "2027-02-24");
    expect(w.map((x) => x.label)).toEqual(["Off 1", "Off 2", "Off 3", "1", "2", "3"]);
    expect(w[3]).toMatchObject({ start: "2027-02-22", offseason: false });
    expect(labelWeeks("2027-02-01", "2027-02-14", null).map((x) => x.label)).toEqual(["1", "2"]);
  });
});

describe("practice summary", () => {
  it("reads the practice time from the start of a plan's notes", () => {
    expect(planTime("3:00-5:15 PM, rings + HS lifting room. Season week 3.")).toBe("3:00-5:15 PM");
    expect(planTime("9:00-11:00 AM (no school), rings")).toBe("9:00-11:00 AM");
    expect(planTime("Morning, about 9:00-11:00 AM (school is out), rings")).toBe("Morning, about 9:00-11:00 AM");
    expect(planTime("Districts at TBD. 3:00 PM start")).toBe("");
    expect(planTime(null)).toBe("");
  });
  it("groups plans and meets by labelled week and keeps empty weeks", () => {
    const plan = { startDate: "2027-02-01", endDate: "2027-03-07", firstPractice: "2027-02-22",
      phases: [{ kind: "offseason", name: "Winter", startDate: "2027-02-01", endDate: "2027-02-21" },
               { kind: "general", name: "M0", startDate: "2027-02-22", endDate: "2027-03-07" }] };
    const practices = [{ id: 2, date: "2027-02-23" }, { id: 1, date: "2027-02-02" }, { id: 3, date: "2027-02-23" }, { id: 9, date: "2027-04-01" }];
    const meets = [{ name: "Indoor", date: "2027-02-06" }];
    const w = summarizeWeeks(plan, practices, meets);
    expect(w.map((x) => x.label)).toEqual(["Off 1", "Off 2", "Off 3", "1", "2"]);
    expect(w[0].days.map((d) => d.date)).toEqual(["2027-02-02", "2027-02-06"]);
    expect(w[0].phase.name).toBe("Winter");
    expect(w[1].days).toEqual([]);
    expect(w[3].days[0].plans.map((p) => p.id)).toEqual([2, 3]);
    expect(w[3].phase.name).toBe("M0");
    expect(w.reduce((s, x) => s + x.practiceCount, 0)).toBe(3);
  });
});

