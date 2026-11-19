import { describe, expect, it } from "vitest";
import { buildHandout, handoutText, planPlace, weekFocus } from "../public/js/handout.js";

const notes = "3:00-5:15 PM, rings + HS lifting room. Season week 6.\n\nFocus this week: Rhythm: slow-to-fast.\n\n"
  + "Next meet: X.\n\nASSUMPTION: coach-only note.";

describe("athlete handout", () => {
  it("reads the place after the time", () => {
    expect(planPlace(notes)).toBe("rings + HS lifting room");
    expect(planPlace("9:00-11:00 AM (no school), rings. Season week 6.")).toBe("rings");
    expect(planPlace("Morning, about 9:00-11:00 AM (school is out), rings + track. Season week 14.")).toBe("rings + track");
    expect(planPlace("Bring spikes.")).toBe("");
  });
  it("reads the weekly focus", () => {
    expect(weekFocus(notes)).toBe("Rhythm: slow-to-fast.");
    expect(weekFocus("nothing here")).toBe("");
  });
  it("builds days without coach notes or marks, and shows meets", () => {
    const practices = [
      { id: 2, date: "2027-04-01", title: "Meet: Madison Invite", groups: ["throws"], notes: "Madison Invite at Ashland. Leave 8:00 AM.\n\nRoutine...", blocks: [] },
      { id: 1, date: "2027-03-31", title: "Throws: pre-meet day", groups: ["throws"], notes,
        blocks: [{ kind: "measured", title: "Throws", minutes: 20, details: "Best of 3.", results: [{ athleteId: 5, mark: "40-2" }] }] },
      { id: 3, date: "2027-03-31", title: "Sprints day", groups: ["sprints"], notes: "", blocks: [] }
    ];
    const meets = [{ name: "Madison Invite", date: "2027-04-01" }, { name: "Other Meet", date: "2027-04-03" }];
    const h = buildHandout({ practices, meets, group: "throws" });
    expect(h.focus).toBe("Rhythm: slow-to-fast.");
    expect(h.days.map((d) => d.date)).toEqual(["2027-03-31", "2027-04-01", "2027-04-03"]);
    expect(h.days[0].entries).toHaveLength(1);
    expect(h.days[1].entries).toEqual([{ kind: "meet", title: "Madison Invite", info: "Madison Invite at Ashland. Leave 8:00 AM." }]);
    expect(h.days[2].entries[0]).toEqual({ kind: "meet", title: "Other Meet", info: "" });
    const text = handoutText(h, { heading: "Throws: week of Mar 29", message: "Bring spikes." });
    expect(text).toContain("Bring spikes.");
    expect(text).toContain("Throws: pre-meet day · 3:00-5:15 PM · rings + HS lifting room");
    expect(text).toContain("- Throws (20 min): Best of 3.");
    expect(text).toContain("MEET: Madison Invite");
    expect(text).not.toContain("ASSUMPTION");
    expect(text).not.toContain("40-2");
    expect(handoutText(h, { heading: "x", details: false })).toContain("- Throws (20 min)\n");
  });
});
