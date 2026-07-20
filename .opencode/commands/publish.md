---
description: Publish a new CloverleafTrack release
agent: build
model: ollama-cloud/kimi-k2.7-code
---

Publish a new release of CloverleafTrack from the local repo at /home/tony/src/CloverleafTrack.

Run the publish script:

```bash
PORTAINER_ACCESS_TOKEN="$PORTAINER_ACCESS_TOKEN" \
PORTAINER_URL="https://portainer.morriscloud.com" \
PORTAINER_STACK_NAME="cloverleaftrack" \
PORTAINER_ENV_DefaultConnection="$CLOVERLEAFTRACK_CONNECTION_STRING" \
/home/tony/src/CloverleafTrack/.opencode/scripts/publish.py
```

The script will:
1. Determine the next semver tag using Conventional Commits since the last tag.
2. Ask for confirmation if the bump is unclear.
3. Create and push the annotated tag to origin.
4. Poll GitHub Actions until the "Build and Push Docker Image" workflow succeeds.
5. Ensure a Git-backed Portainer stack named `cloverleaftrack` exists on endpoint 3 using the compose file at `https://github.com/afmorris/homelab-infrastructure/stacks/cloverleaftrack/docker-compose.yml`.
6. Redeploy the stack, pulling the latest compose file and image.

If `PORTAINER_ACCESS_TOKEN` is not available in the environment, stop and tell the user to generate a Portainer API access token (My account → API access token) and set it before running `/publish`.

The database connection string must be provided via `PORTAINER_ENV_DefaultConnection` (or `CLOVERLEAFTRACK_CONNECTION_STRING` mapped to it). Do NOT commit secrets to either repo.

If any step fails, show the relevant output and ask the user what to do next. Do not proceed past a failure without confirmation.
