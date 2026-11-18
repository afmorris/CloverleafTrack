import { describe, expect, it } from "vitest";
import {
  bestMark, buildWorkout, evenSplits, formatChange, formatRest, formatTime, parseEventDistance, parseTime, repTarget,
  splitLength
} from "../public/js/pace.js";

describe("parseEventDistance", () => {
  it.each([
    ["100M", 100], ["1600M", 1600], ["100M Hurdles", 100], ["180M Low Hurdles", 180], ["2K Steeple Chase", 2000],
    ["Seated 400M", 400], ["Long Jump", null], ["", null]
  ])("%s -> %s", (name, meters) => expect(parseEventDistance(name)).toBe(meters));
});

describe("parseTime", () => {
  it.each([
    ["24", 24], ["24.31", 24.31], ["1:05.2", 65.2], ["10:12", 612], ["1:02:03", 3723], [" 58.5 ", 58.5]
  ])("%s -> %s", (text, seconds) => expect(parseTime(text)).toBeCloseTo(seconds, 6));

  it.each(["", "abc", "1:75", "-3", "0", "1::2", "1.2.3"])("rejects %j", (text) => expect(parseTime(text)).toBeNull());
});

describe("formatTime", () => {
  it.each([[24, "24.00"], [65.234, "1:05.23"], [612, "10:12.00"], [59.999, "1:00.00"], [9.5, "9.50"]])(
    "%s -> %s", (seconds, text) => expect(formatTime(seconds)).toBe(text));
  it("is empty for missing values", () => expect(formatTime(null)).toBe(""));
});

describe("formatChange", () => {
  it("is negative when the new time is faster (BRAIN.md [C129])", () => expect(formatChange(25.1, 24.6)).toBe("-0.50s"));
  it("is positive when slower", () => expect(formatChange(60, 61.25)).toBe("+1.25s"));
  it("drops the s suffix past a minute", () => expect(formatChange(300, 240)).toBe("-1:00.00"));
  it("shows zero plainly", () => expect(formatChange(24, 24)).toBe("0.00s"));
});

describe("formatRest", () => {
  it.each([[0, ""], [45, "45s"], [90, "1:30"], [180, "3:00"]])("%s -> %j", (s, text) => expect(formatRest(s)).toBe(text));
});

describe("repTarget", () => {
  it("matches the classic example: 24.00 200m goal, 200m reps at 90%", () =>
    expect(repTarget(24, 200, 200, 90)).toBeCloseTo(26.667, 3));
  it("scales to a shorter rep at full effort", () => expect(repTarget(60, 400, 200, 100)).toBe(30));
  it("returns null for missing input", () => expect(repTarget(0, 400, 200, 90)).toBeNull());
});

describe("splits", () => {
  it("picks readable split lengths", () => {
    expect(splitLength(200)).toBe(100);
    expect(splitLength(800)).toBe(200);
    expect(splitLength(1600)).toBe(400);
  });
  it("splits evenly and ends at the full distance", () => {
    expect(evenSplits(60, 400)).toEqual([
      { at: 100, time: 15 }, { at: 200, time: 30 }, { at: 300, time: 45 }, { at: 400, time: 60 }]);
  });
  it("adds a short last split when the distance doesn't divide", () => {
    const s = evenSplits(36, 300, 200);
    expect(s.map((x) => x.at)).toEqual([200, 300]);
    expect(s[0].time).toBeCloseTo(24, 6);
  });
});

describe("buildWorkout", () => {
  it("computes each set and total volume", () => {
    const w = buildWorkout({ goalSeconds: 60, eventMeters: 400, sets: [
      { reps: 4, distance: 200, effort: 100, rest: 120 }, { reps: 2, distance: 300, effort: 90, rest: 300 }] });
    expect(w.volume).toBe(1400);
    expect(w.sets[0].target).toBe(30);
    expect(w.sets[1].target).toBeCloseTo(50, 6);
    expect(w.sets[0].splits).toHaveLength(2);
  });
});

describe("bestMark", () => {
  const rows = [{ timeSeconds: 25.4, distanceInches: null }, { timeSeconds: 24.9 }, { timeSeconds: null }];
  it("takes the lowest time for running", () => expect(bestMark(rows, true).value).toBe(24.9));
  it("takes the longest distance for field", () =>
    expect(bestMark([{ distanceInches: 200 }, { distanceInches: 240 }], false).value).toBe(240));
  it("is null with no marks", () => expect(bestMark([], true)).toBeNull());
});
