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
PORTAINER_CONTAINER="ctf" \
/home/tony/src/CloverleafTrack/.opencode/scripts/publish.py
```

The script will:
1. Determine the next semver tag using Conventional Commits since the last tag.
2. Ask for confirmation if the bump is unclear.
3. Create and push the annotated tag to origin.
4. Poll GitHub Actions until the "Build and Push Docker Image" workflow succeeds.
5. Redeploy the `ctf` container via Portainer at https://portainer.morriscloud.com using the latest image.

If `PORTAINER_ACCESS_TOKEN` is not available in the environment, stop and tell the user to generate a Portainer API access token (My account → API access token) and set it before running `/publish`.

If any step fails, show the relevant output and ask the user what to do next. Do not proceed past a failure without confirmation.
