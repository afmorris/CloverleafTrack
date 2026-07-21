#!/usr/bin/env python3
"""Deploy the CloverleafTrack Portainer stack and regenerate static site.

Steps:
1. Redeploy the cloverleaftrack Portainer stack (pulls latest image from ghcr).
2. Wait for https://ctf.morriscloud.com to return HTTP 200.
3. Run the local static-generation script at /home/tony/ctf.

Required env vars:
    PORTAINER_ACCESS_TOKEN  Portainer API access token
    PORTAINER_URL           defaults to https://portainer.morriscloud.com
    PORTAINER_STACK_NAME    defaults to cloverleaftrack
"""

import json
import os
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

PORTAINER_URL = os.environ.get("PORTAINER_URL", "https://portainer.morriscloud.com").rstrip("/")
PORTAINER_TOKEN = os.environ.get("PORTAINER_ACCESS_TOKEN", "")
STACK_NAME = os.environ.get("PORTAINER_STACK_NAME", "cloverleaftrack")
SITE_URL = os.environ.get("SITE_URL", "https://ctf.morriscloud.com")
STATIC_SCRIPT = os.environ.get("STATIC_SCRIPT", "/home/tony/ctf")


def portainer_request(path, method="GET", data=None, headers=None):
    url = f"{PORTAINER_URL}/api{path}"
    req_headers = {
        "X-API-Key": PORTAINER_TOKEN,
        "Accept": "application/json",
    }
    if headers:
        req_headers.update(headers)

    body = None
    if data is not None:
        body = json.dumps(data).encode("utf-8")
        req_headers["Content-Type"] = "application/json"

    req = urllib.request.Request(url, data=body, method=method, headers=req_headers)
    try:
        with urllib.request.urlopen(req) as resp:
            return json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        err_body = e.read().decode("utf-8")
        raise RuntimeError(f"Portainer API {method} {url} failed: {e.code} {e.reason} - {err_body}")


def portainer_endpoint():
    endpoints = portainer_request("/endpoints")
    if not endpoints:
        raise RuntimeError("No Portainer endpoints found.")
    active = [e for e in endpoints if e.get("Status") == 1]
    return (active or endpoints)[0]["Id"]


def portainer_stack(name):
    stacks = portainer_request("/stacks")
    for s in stacks:
        if s.get("Name") == name:
            return s
    raise RuntimeError(f"Stack {name!r} not found in Portainer.")


def redeploy_stack(stack_id, endpoint_id):
    payload = {"pullLatest": True}
    print(f"Redeploying stack {STACK_NAME} (id={stack_id}) with latest image...")
    portainer_request(
        f"/stacks/{stack_id}/git/redeploy?endpointId={endpoint_id}",
        method="PUT",
        data=payload,
    )
    print(f"Stack '{STACK_NAME}' redeployed.")


def wait_for_site(timeout=300, interval=10):
    print(f"Waiting for {SITE_URL} to return 200...")
    elapsed = 0
    while elapsed < timeout:
        try:
            req = urllib.request.Request(SITE_URL, method="GET", headers={"User-Agent": "opencode-deploy/1.0"})
            with urllib.request.urlopen(req, timeout=10) as resp:
                if resp.status == 200:
                    print(f"{SITE_URL} is up.")
                    return
        except urllib.error.HTTPError as e:
            print(f"  HTTP {e.code}, retrying...")
        except Exception as e:
            print(f"  Not ready yet ({type(e).__name__}), retrying...")

        time.sleep(interval)
        elapsed += interval

    raise RuntimeError(f"Timed out waiting for {SITE_URL} after {timeout}s.")


def run_static_script():
    if not Path(STATIC_SCRIPT).exists():
        raise RuntimeError(f"Static generation script not found: {STATIC_SCRIPT}")

    print(f"Running static site generation script: {STATIC_SCRIPT}")
    result = subprocess.run(
        [STATIC_SCRIPT],
        check=False,
        text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(f"Static generation script failed with exit code {result.returncode}")
    print("Static site generation completed.")


def main():
    if not PORTAINER_TOKEN:
        print(
            "Error: PORTAINER_ACCESS_TOKEN is not set.\n"
            "Generate an API token in Portainer (My account → API access token) and export it.",
            file=sys.stderr,
        )
        sys.exit(1)

    endpoint_id = portainer_endpoint()
    stack = portainer_stack(STACK_NAME)
    stack_id = stack["Id"]

    redeploy_stack(stack_id, endpoint_id)
    wait_for_site()
    run_static_script()

    print(f"\nDeployed {STACK_NAME} and updated static site at {SITE_URL}.")


if __name__ == "__main__":
    main()
