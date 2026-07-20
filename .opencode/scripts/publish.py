#!/usr/bin/env python3
"""Publish a new CloverleafTrack release.

Steps:
1. Determine the next semver tag from conventional commits since the last tag.
2. Ask for confirmation if the bump is unclear.
3. Create and push the annotated tag.
4. Poll GitHub Actions for the "Build and Push Docker Image" workflow run.
5. Redeploy the ctf container via Portainer with the latest image.

Required env vars:
    PORTAINER_ACCESS_TOKEN  Portainer API access token
    PORTAINER_URL           defaults to https://portainer.morriscloud.com
    PORTAINER_CONTAINER     defaults to ctf
"""

import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]

PORTAINER_URL = os.environ.get("PORTAINER_URL", "https://portainer.morriscloud.com").rstrip("/")
PORTAINER_TOKEN = os.environ.get("PORTAINER_ACCESS_TOKEN", "")
CONTAINER_NAME = os.environ.get("PORTAINER_CONTAINER", "ctf")
WORKFLOW_NAME = "Build and Push Docker Image"
TAG_PREFIX = "v"


def run(cmd, *, check=True, capture=True, text=True):
    return subprocess.run(
        cmd,
        cwd=REPO_ROOT,
        check=check,
        capture_output=capture,
        text=text,
    )


def latest_tag():
    result = run(["git", "describe", "--tags", "--abbrev=0"], check=False)
    if result.returncode != 0:
        return f"{TAG_PREFIX}0.0.0"
    return result.stdout.strip()


def parse_version(tag):
    m = re.match(rf"{re.escape(TAG_PREFIX)}(\d+)\.(\d+)\.(\d+)", tag)
    if not m:
        raise ValueError(f"Tag {tag!r} does not match semver {TAG_PREFIX}X.Y.Z")
    return int(m.group(1)), int(m.group(2)), int(m.group(3))


def commits_since(tag):
    result = run(["git", "log", f"{tag}..HEAD", "--pretty=format:%s"], check=False)
    if result.returncode != 0:
        return []
    return [line.strip() for line in result.stdout.splitlines() if line.strip()]


CONVENTIONAL_RE = re.compile(
    r"^(?P<type>[a-z]+)(?:\([^)]+\))?(?P<bang>!)?:\s*(?P<description>.+)",
    re.IGNORECASE,
)


def determine_bump(commits):
    """Return (bump, uncertain).

    bump is one of 'major', 'minor', 'patch'.
    uncertain is True if non-conventional commits were found and no major/minor
    conventional commit provided a clear signal.
    """
    bump = "patch"
    uncertain = False
    has_conventional = False

    for subject in commits:
        # Ignore merge commits
        if subject.startswith("Merge "):
            continue

        if re.search(r"BREAKING[ -]?CHANGE", subject, re.IGNORECASE) or "!:" in subject:
            return "major", False

        m = CONVENTIONAL_RE.match(subject)
        if m:
            has_conventional = True
            ctype = m.group("type").lower()
            bang = m.group("bang") == "!"
            if bang:
                return "major", False
            if ctype == "feat":
                if bump not in ("major",):
                    bump = "minor"
            elif ctype in ("fix", "docs", "style", "refactor", "perf", "test", "chore", "build", "ci"):
                # patch-level by default; already patch
                pass
            else:
                # unknown type counts as uncertain only if we have no stronger signal
                if bump == "patch":
                    uncertain = True
        else:
            # non-conventional commit
            if bump == "patch":
                uncertain = True

    if not has_conventional and uncertain:
        # No conventional commits at all and only patch-level signal
        uncertain = True

    return bump, uncertain


def next_version(current, bump):
    major, minor, patch = current
    if bump == "major":
        return major + 1, 0, 0
    if bump == "minor":
        return major, minor + 1, 0
    return major, minor, patch + 1


def confirm(message):
    try:
        answer = input(f"{message} [y/N] ").strip().lower()
    except (EOFError, KeyboardInterrupt):
        answer = "n"
    return answer in ("y", "yes")


def create_and_push_tag(tag, version):
    print(f"Creating annotated tag {tag}...")
    run(["git", "tag", "-a", tag, "-m", f"Release {version}"])
    print(f"Pushing {tag} to origin...")
    run(["git", "push", "origin", tag])


def tag_commit_sha(tag):
    result = run(["git", "rev-list", "-n", "1", tag])
    return result.stdout.strip()


def wait_for_build(tag, timeout=1800, interval=15):
    head_sha = tag_commit_sha(tag)
    print(f"Waiting for '{WORKFLOW_NAME}' workflow for commit {head_sha}...")

    elapsed = 0
    while elapsed < timeout:
        result = run(
            [
                "gh", "run", "list",
                "-w", WORKFLOW_NAME,
                "--json", "databaseId,status,conclusion,headSha,event,createdAt",
            ],
            check=False,
        )
        if result.returncode != 0:
            print("Warning: gh run list failed; retrying...", file=sys.stderr)
            time.sleep(interval)
            elapsed += interval
            continue

        try:
            runs = json.loads(result.stdout)
        except json.JSONDecodeError:
            print("Warning: could not parse gh run list output; retrying...", file=sys.stderr)
            time.sleep(interval)
            elapsed += interval
            continue

        # Find the newest push run for our commit
        matching = [
            r for r in runs
            if r.get("event") == "push" and r.get("headSha") == head_sha
        ]
        if not matching:
            print(f"  No matching run yet ({elapsed}s elapsed)...")
            time.sleep(interval)
            elapsed += interval
            continue

        run_info = max(matching, key=lambda r: r.get("createdAt", ""))
        run_id = run_info.get("databaseId")
        status = run_info.get("status")
        conclusion = run_info.get("conclusion")

        print(f"  Run {run_id}: {status}")
        if status == "completed":
            if conclusion == "success":
                print("Workflow completed successfully.")
                return run_id
            raise RuntimeError(f"Workflow failed with conclusion: {conclusion}")

        time.sleep(interval)
        elapsed += interval

    raise RuntimeError("Timed out waiting for workflow to complete.")


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
    # Prefer the first environment; in most single-node setups this is correct.
    return endpoints[0]["Id"]


def portainer_container(endpoint_id, name):
    containers = portainer_request(f"/endpoints/{endpoint_id}/docker/containers/json?all=true")
    for c in containers:
        for n in c.get("Names", []):
            if n.lstrip("/") == name:
                return c
    raise RuntimeError(f"Container {name!r} not found on Portainer endpoint {endpoint_id}.")


def strip_runtime_network_config(networks):
    """Return only the fields valid for container create."""
    valid_keys = {"Aliases", "IPAMConfig", "Links"}
    cleaned = {}
    for net_name, cfg in networks.items():
        cleaned[net_name] = {k: v for k, v in cfg.items() if k in valid_keys}
    return cleaned


def exposed_ports_from_bindings(port_bindings):
    """Derive ExposedPorts from PortBindings keys (e.g. '80/tcp')."""
    exposed = {}
    for port in (port_bindings or {}).keys():
        exposed[port] = {}
    return exposed


def redeploy_container(endpoint_id, container_name):
    print(f"Looking up container {container_name!r} on Portainer...")
    container_summary = portainer_container(endpoint_id, container_name)
    container_id = container_summary["Id"]
    image = container_summary["Image"]
    print(f"Found container {container_name} ({container_id[:12]}) using image {image}")

    print("Pulling latest image...")
    portainer_request(
        f"/endpoints/{endpoint_id}/docker/images/create?fromImage={urllib.parse.quote(image)}",
        method="POST",
    )

    print("Inspecting container configuration...")
    inspect = portainer_request(f"/endpoints/{endpoint_id}/docker/containers/{container_id}/json")

    config = inspect.get("Config", {})
    host_config = inspect.get("HostConfig", {})
    network_settings = inspect.get("NetworkSettings", {})
    networks = network_settings.get("Networks", {})

    port_bindings = host_config.get("PortBindings")
    exposed_ports = exposed_ports_from_bindings(port_bindings)

    create_payload = {
        "Hostname": config.get("Hostname"),
        "Domainname": config.get("Domainname"),
        "User": config.get("User"),
        "AttachStdin": config.get("AttachStdin", False),
        "AttachStdout": config.get("AttachStdout", False),
        "AttachStderr": config.get("AttachStderr", False),
        "Tty": config.get("Tty", False),
        "OpenStdin": config.get("OpenStdin", False),
        "StdinOnce": config.get("StdinOnce", False),
        "Env": config.get("Env"),
        "Cmd": config.get("Cmd"),
        "Entrypoint": config.get("Entrypoint"),
        "Image": image,
        "Volumes": config.get("Volumes"),
        "WorkingDir": config.get("WorkingDir"),
        "Labels": config.get("Labels"),
        "ExposedPorts": exposed_ports,
        "NetworkingConfig": {
            "EndpointsConfig": strip_runtime_network_config(networks),
        },
        "HostConfig": {
            "Binds": host_config.get("Binds"),
            "PortBindings": port_bindings,
            "RestartPolicy": host_config.get("RestartPolicy"),
            "AutoRemove": host_config.get("AutoRemove", False),
            "LogConfig": host_config.get("LogConfig"),
            "NetworkMode": host_config.get("NetworkMode"),
            "PublishAllPorts": host_config.get("PublishAllPorts", False),
            "Privileged": host_config.get("Privileged", False),
            "CpuShares": host_config.get("CpuShares"),
            "Memory": host_config.get("Memory"),
            "MemoryReservation": host_config.get("MemoryReservation"),
            "NanoCpus": host_config.get("NanoCpus"),
            "PidsLimit": host_config.get("PidsLimit"),
        },
    }

    # Remove null-ish keys that Docker may reject
    for key in list(create_payload.keys()):
        if create_payload[key] is None:
            del create_payload[key]

    host_config_payload = create_payload["HostConfig"]
    for key in list(host_config_payload.keys()):
        if host_config_payload[key] is None:
            del host_config_payload[key]

    if create_payload.get("ExposedPorts"):
        # Docker doesn't like HostConfig.NetworkMode when NetworkingConfig is provided
        if "NetworkingConfig" in create_payload:
            host_config_payload.pop("NetworkMode", None)

    print("Stopping container...")
    portainer_request(
        f"/endpoints/{endpoint_id}/docker/containers/{container_id}/stop",
        method="POST",
    )

    print("Removing container...")
    portainer_request(
        f"/endpoints/{endpoint_id}/docker/containers/{container_id}?force=true",
        method="DELETE",
    )

    print(f"Creating new container {container_name} with latest image...")
    encoded_name = urllib.parse.quote(container_name)
    new_container = portainer_request(
        f"/endpoints/{endpoint_id}/docker/containers/create?name={encoded_name}",
        method="POST",
        data=create_payload,
    )

    new_id = new_container.get("Id") or new_container.get("id")
    if not new_id:
        raise RuntimeError(f"Failed to create container: {new_container}")

    print(f"Starting new container {new_id[:12]}...")
    portainer_request(
        f"/endpoints/{endpoint_id}/docker/containers/{new_id}/start",
        method="POST",
    )

    print(f"Container {container_name} redeployed successfully.")


def main():
    if not PORTAINER_TOKEN:
        print(
            "Error: PORTAINER_ACCESS_TOKEN is not set.\n"
            "Generate an API token in Portainer (My account → API access token) and export it.\n"
            "Example: export PORTAINER_ACCESS_TOKEN=ptr_...",
            file=sys.stderr,
        )
        sys.exit(1)

    # Safety checks
    branch = run(["git", "branch", "--show-current"]).stdout.strip()
    if branch != "main":
        print(f"Warning: current branch is {branch!r}, not main.", file=sys.stderr)
        if not confirm("Continue anyway?"):
            print("Aborted.")
            sys.exit(0)

    status = run(["git", "status", "--porcelain"]).stdout.strip()
    if status:
        print("Warning: working tree is not clean:", file=sys.stderr)
        print(status, file=sys.stderr)
        if not confirm("Continue anyway?"):
            print("Aborted.")
            sys.exit(0)

    latest = latest_tag()
    current = parse_version(latest)
    commits = commits_since(latest)

    print(f"Latest tag: {latest}")
    if not commits:
        print(f"No commits since {latest}. Nothing to publish.", file=sys.stderr)
        sys.exit(1)

    print("Commits since last tag:")
    for c in commits:
        print(f"  - {c}")

    bump, uncertain = determine_bump(commits)
    next_ver = next_version(current, bump)
    next_tag = f"{TAG_PREFIX}{next_ver[0]}.{next_ver[1]}.{next_ver[2]}"

    print(f"\nProposed bump: {bump} -> {next_tag}")
    if uncertain:
        print(
            "Warning: recent commits do not clearly follow Conventional Commits, "
            "so the bump level is uncertain.",
            file=sys.stderr,
        )

    if not confirm(f"Push tag {next_tag} and redeploy {CONTAINER_NAME}?"):
        print("Aborted.")
        sys.exit(0)

    create_and_push_tag(next_tag, f"{next_ver[0]}.{next_ver[1]}.{next_ver[2]}")
    wait_for_build(next_tag)
    endpoint_id = portainer_endpoint()
    redeploy_container(endpoint_id, CONTAINER_NAME)

    print(f"\nPublished {next_tag} and redeployed {CONTAINER_NAME}.")


if __name__ == "__main__":
    main()
