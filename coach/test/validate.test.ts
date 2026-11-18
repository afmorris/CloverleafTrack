import { describe, expect, it } from "vitest";
import { cleanBlocks, cleanDate, cleanGroups, cleanPhases } from "../src/validate";

describe("cleanGroups", () => {
  it("dedupes and puts groups in the standard order", () =>
    expect(cleanGroups(["throws", "sprints", "throws"], true).value).toEqual(["sprints", "throws"]));
  it("rejects unknown groups", () => expect(cleanGroups(["relays"], false).error).toMatch(/unknown group/));
  it("requires one when asked", () => expect(cleanGroups([], true).error).toMatch(/at least one/));
});

describe("cleanBlocks", () => {
  const block = { kind: "measured", title: " Full throws ", minutes: "30", groups: ["throws"], details: "x", attempts: 6,
    results: [{ athleteId: 7, mark: "38' 2\"" }, { athleteId: 8, mark: "  " }] };
  it("trims text, converts numbers, and drops empty marks", () => {
    const b = cleanBlocks([block]).value![0];
    expect(b).toMatchObject({ title: "Full throws", minutes: 30, attempts: 6, workoutId: null });
    expect(b.results).toEqual([{ athleteId: 7, mark: "38' 2\"" }]);
  });
  it.each([
    [{ ...block, kind: "nap" }, /unknown kind/],
    [{ ...block, minutes: 500 }, /0-240/],
    [{ ...block, attempts: 0 }, /1-20/],
    [{ ...block, results: [{ mark: "20'" }] }, /pick an athlete/]
  ])("rejects bad block %#", (b, msg) => expect(cleanBlocks([b]).error).toMatch(msg));
  it("caps the block count", () => expect(cleanBlocks(Array(31).fill(block)).error).toMatch(/30 blocks/));
});

describe("cleanDate", () => {
  it("accepts a date", () => expect(cleanDate("2027-03-01", "Date").value).toBe("2027-03-01"));
  it("rejects anything else", () => expect(cleanDate("3/1/2027", "Date").error).toMatch(/YYYY-MM-DD/));
});

describe("cleanPhases", () => {
  const p = (startDate: string, endDate: string, kind = "general") => ({ kind, startDate, endDate, volume: 3, intensity: 3 });
  it("fills a default name and sorts by start", () => {
    const v = cleanPhases([p("2027-03-01", "2027-03-31", "competition"), p("2027-01-04", "2027-02-28")], "2027-01-04", "2027-06-30").value!;
    expect(v.map((x) => x.name)).toEqual(["General prep", "Competition"]);
  });
  it("rejects overlaps with a readable message", () =>
    expect(cleanPhases([p("2027-01-04", "2027-02-10"), p("2027-02-01", "2027-03-01", "specific")], "2027-01-01", "2027-06-30").error)
      .toBe("General prep overlaps Specific prep."));
  it("rejects out-of-range volume", () =>
    expect(cleanPhases([{ ...p("2027-01-04", "2027-02-10"), volume: 9 }], "2027-01-01", "2027-06-30").error).toMatch(/1-5/));
});
