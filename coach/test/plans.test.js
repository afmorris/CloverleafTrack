import { describe, expect, it } from "vitest";
import { addDays, daysBetween, formatDay, isIsoDate, weekStart } from "../public/js/dates.js";
import {
  blocksFromTemplate, formatMinutes, mergeTemplateBlocks, moveBlock, newBlock, practiceLength, resultCount, totalMinutes
} from "../public/js/practice.js";
import { isChampionshipMeet, phaseForDate, phaseProblems, suggestPhases, weeksOf } from "../public/js/season.js";

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
});
