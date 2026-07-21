---
description: Deploy CloverleafTrack stack and regenerate static site
agent: build
model: ollama-cloud/kimi-k2.7-code
---

Deploy CloverleafTrack from the local repo context.

Run the deploy script:

```bash
PORTAINER_ACCESS_TOKEN="$PORTAINER_ACCESS_TOKEN" \
PORTAINER_URL="https://portainer.morriscloud.com" \
PORTAINER_STACK_NAME="cloverleaftrack" \
/home/tony/src/CloverleafTrack/.opencode/scripts/deploy.py
```

The script will:
1. Redeploy the `cloverleaftrack` Portainer stack, pulling the latest `ghcr.io/afmorris/cloverleaftrack:latest` image.
2. Wait for `https://ctf.morriscloud.com` to return HTTP 200.
3. Run `/home/tony/ctf` to crawl the live site and sync static HTML to S3.

If `PORTAINER_ACCESS_TOKEN` is not available in the environment, stop and tell the user to generate a Portainer API access token (My account → API access token) and set it before running `/deploy`.

If any step fails, show the relevant output and ask the user what to do next. Do not proceed past a failure without confirmation.
