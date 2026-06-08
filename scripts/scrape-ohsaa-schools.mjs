/**
 * Scrapes OHSAA high school data from ohsaa.finalforms.com.
 * Outputs JSON to stdout (redirect to a file) or to --output <path>.
 *
 * Usage:
 *   node scripts/scrape-ohsaa-schools.mjs > data/ohsaa-schools.json
 *   node scripts/scrape-ohsaa-schools.mjs --output data/ohsaa-schools.json
 */

import { writeFileSync, mkdirSync } from 'fs';
import { dirname } from 'path';

const BASE_URL = 'https://ohsaa.finalforms.com/state_schools';
const QUERY = new URLSearchParams({
  direction: 'asc',
  limit: '15',
  searching: 'true',
  sort: 'student_count',
  'state_schools.athletic_association_abbreviation_eq': 'OHSAA',
  'state_schools.is_archived_eq': 'false',
  'state_schools.levels_in': 'high_school',
});
const TOTAL_PAGES = 55;
const DELAY_MS = 600;

// ─── Helpers ────────────────────────────────────────────────────────────────

function clean(s) {
  return s ? s.replace(/\s+/g, ' ').trim() : null;
}

function decodeHtmlEntities(s) {
  return s
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&#39;/g, "'")
    .replace(/&middot;/g, '·')
    .replace(/&nbsp;/g, ' ');
}

function stripTags(s) {
  return s.replace(/<[^>]+>/g, '');
}

function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

// ─── Per-school parser ───────────────────────────────────────────────────────

function parseSchool(rowHtml) {
  const school = {};

  // FinalForms internal ID
  const idM = rowHtml.match(/id="state_school_(\d+)"/);
  school.finalFormsId = idM ? parseInt(idM[1], 10) : null;

  // Student count (used as proxy for school size / sort key)
  const studentM = rowHtml.match(/<td class="students-count">\s*(\d+)\s*<\/td>/);
  school.studentCount = studentM ? parseInt(studentM[1], 10) : null;

  // NCES canonical name (attribute on the link)
  const ncesNameM = rowHtml.match(/title="NCES name:\s*([^"]+)"/);
  school.ncesName = ncesNameM ? clean(ncesNameM[1]) : null;

  // Display name (bold text in the link)
  const displayNameM = rowHtml.match(/href="\/state_schools\/\d+">\s*<b>\s*([\s\S]*?)\s*<\/b>/);
  school.name = displayNameM ? clean(stripTags(displayNameM[1])) : null;

  // Grade levels e.g. "9th - 12th"
  const gradesM = rowHtml.match(/<small>\[([^\]]+)\]<\/small>/);
  school.grades = gradesM ? clean(gradesM[1]) : null;

  // School district (administrative)
  const sdM = rowHtml.match(/title="School District"><b>District: <\/b><a[^>]+>([\s\S]*?)<\/a>/);
  school.schoolDistrict = sdM ? clean(stripTags(sdM[1])) : null;

  // NCES ID
  const ncesIdM = rowHtml.match(/nces\.ed\.gov\/ccd\/schoolsearch[^>]+>\s*(\d+)\s*<\/a>/);
  school.ncesId = ncesIdM ? ncesIdM[1].trim() : null;

  // State ID (e.g. "OH-046797-018663")
  const stateIdM = rowHtml.match(/title="State ID"><b>State ID: <\/b>([\s\S]*?)<\/small>/);
  school.stateId = stateIdM ? clean(stripTags(stateIdM[1])) : null;

  // ── Classifications cell ────────────────────────────────────────────────

  const classifCellM = rowHtml.match(/<td class="classifications">([\s\S]*?)<\/td>/);
  if (classifCellM) {
    const cell = classifCellM[1];

    // Conference
    const confM = cell.match(/title="Conference"[\s\S]*?<a[^>]+>([\s\S]*?)<\/a>/);
    school.conference = confM ? clean(stripTags(confM[1])) : null;

    // Athletic district (different from school district)
    const adM = cell.match(/title="District"[\s\S]*?<a[^>]+>([\s\S]*?)<\/a>/);
    school.athleticDistrict = adM ? clean(stripTags(adM[1])) : null;

    // Classes section (between title="Class" and title="Division")
    const classSection = cell.match(/title="Class"([\s\S]*?)(?:title="Division"|$)/);
    if (classSection) {
      const classLabels = [...classSection[1].matchAll(/dropdown-toggle[^>]*>\s*([A-Z]+)<span class="caret"/g)];
      school.classes = classLabels.map((m) => clean(m[1])).filter(Boolean);
    } else {
      school.classes = [];
    }

    // Division section — collect the dropdown labels (IV, V, etc.) and the sport names
    const divSection = cell.match(/title="Division"([\s\S]*?)(?:<\/td>|$)/);
    if (divSection) {
      const divLabels = [...divSection[1].matchAll(/dropdown-toggle[^>]*>\s*([^<]+)<span class="caret"/g)];
      school.divisionLabels = divLabels.map((m) => clean(m[1])).filter(Boolean);

      const divSports = [...divSection[1].matchAll(/<li><a[^>]+>([\s\S]*?)<\/a><\/li>/g)];
      school.divisions = divSports.map((m) => clean(stripTags(m[1]))).filter(Boolean);
    } else {
      school.divisionLabels = [];
      school.divisions = [];
    }
  }

  // ── Address cell ─────────────────────────────────────────────────────────

  const addrCellM = rowHtml.match(/<td class="address">([\s\S]*?)<\/td>/);
  if (addrCellM) {
    const cell = addrCellM[1];

    // The address text lives in the Google Maps link after the glyphicon span
    const addrTextM = cell.match(/glyphicon-map-marker"><\/span>([\s\S]*?)<\/a>/);
    if (addrTextM) {
      const parts = addrTextM[1].split(/<br\s*\/?>/i);
      school.street = clean(stripTags(parts[0])) || null;
      if (parts[1]) {
        const cityLine = clean(stripTags(parts[1]));
        const cszM = cityLine?.match(/^(.*?),\s*([A-Z]{2})\s+(\d{5}(?:-\d{4})?)$/);
        if (cszM) {
          school.city = cszM[1].trim();
          school.state = cszM[2];
          school.zip = cszM[3];
        } else {
          school.cityStateZip = cityLine || null;
        }
      }
    }

    // County (attribute title has "NCES County Name: Erie County")
    const countyM = cell.match(/title="NCES County Name:\s*([^"]+)"/);
    school.county = countyM ? clean(countyM[1]) : null;
  }

  // ── Contact cell ─────────────────────────────────────────────────────────

  const contactCellM = rowHtml.match(/<td class="contact_info">([\s\S]*?)<\/td>/);
  if (contactCellM) {
    const cell = contactCellM[1];

    // Website (glyphicon-new-window icon precedes it)
    const websiteM = cell.match(/href="(https?:\/\/[^"]+)"[^>]*><span[^>]*glyphicon-new-window/);
    school.website = websiteM ? websiteM[1] : null;

    // Phone
    const phoneM = cell.match(/href="tel:([^"]+)"/);
    school.phone = phoneM ? phoneM[1] : null;
  }

  return school;
}

// ─── Main ────────────────────────────────────────────────────────────────────

async function scrape() {
  const outputArg = process.argv.indexOf('--output');
  const outputPath = outputArg !== -1 ? process.argv[outputArg + 1] : null;

  const allSchools = [];

  for (let page = 1; page <= TOTAL_PAGES; page++) {
    const url = `${BASE_URL}?${QUERY}&page=${page}`;
    process.stderr.write(`Fetching page ${page}/${TOTAL_PAGES}...\n`);

    let html;
    try {
      const res = await fetch(url, {
        headers: {
          'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
          Accept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        },
      });
      if (!res.ok) {
        process.stderr.write(`  HTTP ${res.status} — skipping page ${page}\n`);
        continue;
      }
      html = await res.text();
    } catch (err) {
      process.stderr.write(`  Error fetching page ${page}: ${err.message}\n`);
      continue;
    }

    // Each school is wrapped in <tr class="state_school" id="state_school_...">
    const rowSplit = html.split(/<tr class="state_school"/);
    const rows = rowSplit.slice(1); // first chunk is everything before the first <tr>

    for (const row of rows) {
      const school = parseSchool('<tr class="state_school"' + row);
      if (school.finalFormsId) {
        allSchools.push(school);
      }
    }

    process.stderr.write(`  Found ${rows.length} schools — running total: ${allSchools.length}\n`);

    if (page < TOTAL_PAGES) {
      await sleep(DELAY_MS);
    }
  }

  process.stderr.write(`\nDone — ${allSchools.length} schools total.\n`);

  const json = JSON.stringify(allSchools, null, 2);

  if (outputPath) {
    const dir = dirname(outputPath);
    if (dir && dir !== '.') mkdirSync(dir, { recursive: true });
    writeFileSync(outputPath, json, 'utf8');
    process.stderr.write(`Wrote to ${outputPath}\n`);
  } else {
    process.stdout.write(json);
  }
}

scrape().catch((err) => {
  process.stderr.write(`Fatal: ${err.stack || err.message}\n`);
  process.exit(1);
});
