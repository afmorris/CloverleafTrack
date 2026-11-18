# Cloverleaf Coach

Coaching tools for Cloverleaf track and field at **coach.cloverleaftrack.com**: a Cloudflare Worker (Hono) with
static pages and a D1 database. Separate from the ASP.NET app and its static export. Design notes: BRAIN.md [C133].

```
coach/
├── public/            static pages, CSS, and browser JS (served by the Worker's ASSETS binding)
│   ├── practice.html, practice/edit.html, templates.html, templates/edit.html, season.html, pace.html
│   └── js/            pace.js, dates.js, practice.js, season.js: pure logic shared with the Worker and tests;
│                      app.js (header, api, el/fill); one script per page
├── src/index.ts       JSON API under /api/* (snapshot, athletes, workouts)
├── src/plans.ts       practice plans, templates, season plans, meets
├── src/validate.ts    input checks for plans, templates, and phases
├── src/snapshot.ts    validates and loads the nightly results snapshot
├── migrations/        D1 schema (wrangler d1 migrations)
├── scripts/push-snapshot.sh   nightly push from the home server
└── test/              vitest
```

## How data moves

The home SQL Server stays private. Each night `scripts/push-snapshot.sh` fetches
`/Admin/CoachSnapshot` from the internal app (active athletes, individual events, seasons, each athlete's best mark
per event per season, recent meets) and POSTs it to `/api/snapshot`, which empties and refills the snapshot tables
in one D1 transaction. Nothing on this site writes back. Tables the coaches fill (`workouts`, `practice_plans`,
`practice_templates`, `season_plans`, `season_phases`) live only in D1.

## Local development

There's no Node on the dev Mac, so everything runs in Docker (the full `node:22-bookworm` image; the `-slim` one has
no CA certificates and workerd refuses to start).

```bash
cd coach
docker run --rm -v "$PWD":/app -w /app node:22-bookworm npm ci
docker run --rm -v "$PWD":/app -w /app node:22-bookworm npm test
docker run --rm -v "$PWD":/app -w /app node:22-bookworm npx wrangler d1 migrations apply cloverleaf-coach --local
echo 'SNAPSHOT_TOKEN=local-dev-token' > .dev.vars
```

Start the dev server with the `coach-dev` entry in `.claude/launch.json` (port 8787), then load real data from the
local ASP.NET app (`ctf-web-local`, port 5110):

```bash
COACH_SNAPSHOT_TOKEN=local-dev-token SNAPSHOT_SOURCE=http://localhost:5110/Admin/CoachSnapshot \
  COACH_URL=http://localhost:8787 coach/scripts/push-snapshot.sh
```

`node_modules` installed in the container holds Linux binaries; run `npm ci` again if you later run Node on macOS.

## First deploy (one time)

Do these in order: Access (step 4) must exist before the first deploy (step 5), or the site is briefly public.

1. **API token.** Cloudflare → My Profile → API Tokens → Create Custom Token, named `cloverleaf-coach-deploy`:
   - Account (Morris Cloud): Workers Scripts Edit, D1 Edit, Account Settings Read, Workers Tail Read,
     Workers Observability Edit.
   - Zone (cloverleaftrack.com only): Workers Routes Edit, DNS Edit, Zone Read.

   No User permissions are needed when `CLOUDFLARE_ACCOUNT_ID` is set. Save it in 1Password (Home vault, API
   Credential) as `Cloudflare token (coach.cloverleaftrack.com deploy)` with a text field `account id`. The title has
   parentheses, which `op read` references reject, so read it with
   `op item get '<title>' --vault Home --fields credential --reveal`.
2. **GitHub secrets.** `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID` on `afmorris/CloverleafTrack`.
3. **Database.** `npx wrangler d1 create cloverleaf-coach --location enam` and put the printed id in `database_id`
   in `wrangler.jsonc`. Until then the deploy job skips itself. Migrations are applied by the deploy workflow.
4. **Sign-in (Cloudflare Access).** Two self-hosted applications:
   - `Cloverleaf Coach` on `coach.cloverleaftrack.com`, policy `Cloverleaf coaches` (Allow, the coaches' emails,
     one-time PIN login).
   - `Cloverleaf Coach (snapshot push)` on `coach.cloverleaftrack.com/api/snapshot`, policy `Snapshot push (bypass)`
     (Bypass, Everyone). That path is protected by the bearer token instead, so the nightly script needs no Access
     credential.

   The other Access apps (the wiki) are managed by OpenTofu in `homelab-infrastructure/terraform/cloudflare`. Don't
   add a DNS record for `coach` there: the Worker's custom domain creates and owns it.
5. **Deploy.** Merge to `main`; `.github/workflows/coach-deploy.yml` tests, applies D1 migrations, and deploys.
6. **Snapshot secret.** 1Password item `Cloverleaf Coach snapshot token` (Home vault, generated password), then
   `op read "op://Home/Cloverleaf Coach snapshot token/password" | npx wrangler secret put SNAPSHOT_TOKEN`.
   Until it is set, `/api/snapshot` answers 503.
7. **Nightly push.** On svp01docker01: the script is `/home/tony/push-coach-snapshot`; its settings file
   `/home/tony/.config/cloverleaf-coach.env` (mode 600) exports `COACH_SNAPSHOT_TOKEN`; cron runs
   `15 3 * * * . /home/tony/.config/cloverleaf-coach.env && /home/tony/push-coach-snapshot >> /home/tony/logs/coach-snapshot.log 2>&1`.
   It reads the container directly (`http://localhost:8081/Admin/CoachSnapshot`), so it doesn't depend on `ctf`.

After that, any change under `coach/` merged to `main` deploys automatically.
