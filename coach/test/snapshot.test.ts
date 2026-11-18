import { describe, expect, it } from "vitest";
import { validateSnapshot } from "../src/snapshot";

const good = {
  version: 1, generatedAt: "2026-11-12T00:00:00Z",
  athletes: [{ id: 1 }], events: [{ id: 1 }], seasons: [], seasonBests: [], meets: []
};

describe("validateSnapshot", () => {
  it("accepts a well-formed snapshot", () => expect(validateSnapshot(good)).toEqual([]));
  it("rejects a non-object", () => expect(validateSnapshot("x")).toEqual(["body is not a JSON object"]));
  it("rejects the wrong version", () => expect(validateSnapshot({ ...good, version: 2 })).toContain("version must be 1"));
  it("refuses an empty roster rather than wiping the site", () =>
    expect(validateSnapshot({ ...good, athletes: [] })).toContain("athletes is empty"));
  it("requires every list", () => expect(validateSnapshot({ ...good, meets: undefined })).toContain("meets must be an array"));
});
