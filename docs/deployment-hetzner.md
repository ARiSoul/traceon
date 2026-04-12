# Deploying Traceon to Hetzner — Step-by-Step Guide

This guide walks you through deploying the **Traceon API**, **Blazor WebAssembly frontend**, and **SQL Server database** on a Hetzner Cloud VPS from scratch.

> **Time estimate:** ~45–60 minutes for the first deployment.

---

## Table of Contents

1. [Overview of what we're building](#1-overview-of-what-were-building)
2. [Create a Hetzner Cloud server](#2-create-a-hetzner-cloud-server)
3. [Point your domains to the server](#3-point-your-domains-to-the-server)
4. [Connect to the server via SSH](#4-connect-to-the-server-via-ssh)
5. [Install Docker](#5-install-docker)
6. [Clone the repository](#6-clone-the-repository)
7. [Configure environment variables](#7-configure-environment-variables)
8. [Update the Blazor client's API URL](#8-update-the-blazor-clients-api-url)
9. [Build and start everything](#9-build-and-start-everything)
10. [Verify the deployment](#10-verify-the-deployment)
11. [Common operations](#11-common-operations)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Overview of what we're building

```
Internet
   │
   ▼
┌──────────────────────────────────────────────────┐
│  Hetzner VPS (Ubuntu 24.04)                      │
│                                                  │
│  ┌────────┐     ┌─────────┐     ┌────────────┐  │
│  │ Caddy  │────▶│  API    │────▶│ SQL Server │  │
│  │ :443   │     │  :8080  │     │  :1433     │  │
│  │        │     └─────────┘     └────────────┘  │
│  │        │     ┌─────────┐                      │
│  │        │────▶│ Blazor  │                      │
│  │        │     │  :80    │                      │
│  └────────┘     └─────────┘                      │
└──────────────────────────────────────────────────┘
```

| Component | Role |
|---|---|
| **Caddy** | Reverse proxy with **automatic** HTTPS certificates (Let's Encrypt) |
| **API** | ASP.NET Core backend serving REST + OData endpoints |
| **Blazor** | Static Blazor WebAssembly files served by Nginx inside a container |
| **SQL Server** | Microsoft SQL Server 2022 running in Docker |

Everything runs in **Docker containers** orchestrated by **Docker Compose**.

---

## 2. Create a Hetzner Cloud server

1. Go to [https://console.hetzner.cloud](https://console.hetzner.cloud) and sign up / log in.
2. Click **+ Create Server**.
3. Choose a **Location** close to your users (e.g. `Nuremberg`, `Helsinki`, `Ashburn`).
4. Under **Image**, select **Ubuntu 24.04**.
5. Under **Type**, pick at least a **CX22** (2 vCPU, 4 GB RAM).
   > SQL Server requires **at least 2 GB RAM**. 4 GB gives breathing room for the API + Blazor + Caddy.
6. Under **SSH Keys**, click **Add SSH Key**.
   - If you don't have one yet, open a terminal on your PC and run:
     ```bash
     ssh-keygen -t ed25519 -C "your-email@example.com"
     ```
   - Copy the public key (`cat ~/.ssh/id_ed25519.pub` on Linux/Mac or `type $env:USERPROFILE\.ssh\id_ed25519.pub` on PowerShell) and paste it into Hetzner.
7. Give your server a name (e.g. `traceon-prod`) and click **Create & Buy Now**.
8. Note the **IPv4 address** shown on the dashboard (e.g. `49.13.xx.xx`).

---

## 3. Point your domains to the server

You need **two domains** (or subdomains). For example:

| Subdomain | Purpose |
|---|---|
| `traceon.yourdomain.com` | Blazor frontend |
| `api.traceon.yourdomain.com` | API |

Go to your **DNS provider** (Cloudflare, Namecheap, Hetzner DNS, etc.) and create **A records**:

| Type | Name | Value | TTL |
|---|---|---|---|
| A | `traceon` | `49.13.xx.xx` | 300 |
| A | `api.traceon` | `49.13.xx.xx` | 300 |

> Replace `49.13.xx.xx` with your actual server IP.

Wait a few minutes for DNS propagation. You can verify with:
```bash
nslookup traceon.yourdomain.com
```

---

## 4. Connect to the server via SSH

From your local machine:

```bash
ssh root@49.13.xx.xx
```

> On Windows, you can use the built-in `ssh` command in PowerShell or Windows Terminal.

If this is your first connection, type `yes` when asked to accept the fingerprint.

### 4.1 — Create a non-root user (recommended)

```bash
adduser deploy
usermod -aG sudo deploy
```

Copy the SSH key so you can log in as `deploy`:

```bash
mkdir -p /home/deploy/.ssh
cp /root/.ssh/authorized_keys /home/deploy/.ssh/
chown -R deploy:deploy /home/deploy/.ssh
```

Log out and reconnect:

```bash
exit
ssh deploy@49.13.xx.xx
```

### 4.2 — Enable the firewall

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

Type `y` to confirm. This blocks everything except SSH, HTTP, and HTTPS.

---

## 5. Install Docker

Run these commands one by one:

```bash
# Update packages
sudo apt update && sudo apt upgrade -y

# Install prerequisites
sudo apt install -y ca-certificates curl gnupg

# Add Docker's official GPG key
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

# Add the Docker repository
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine + Compose
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Let your user run Docker without sudo
sudo usermod -aG docker $USER
```

**Log out and back in** for the group change to take effect:

```bash
exit
ssh deploy@49.13.xx.xx
```

Verify Docker is working:

```bash
docker --version
docker compose version
```

---

## 6. Clone the repository

```bash
sudo chown deploy:deploy /home/deploy
cd ~
git clone https://github.com/ARiSoul/traceon.git
cd traceon
```

> If the repo is private, you'll need a [personal access token](https://github.com/settings/tokens) or deploy key.

---

## 7. Configure environment variables

### 7.1 — Create the `.env` file

```bash
cd ~/traceon
cp .env.example .env
nano .env
```

Fill in **every value**:

```env
# SQL Server SA password — must be 16+ chars with upper, lower, digit, and symbol
DB_SA_PASSWORD=MyStr0ng!Passw0rd2025

# JWT signing key — at least 32 random characters
JWT_KEY=aVeryLongRandomStringAtLeast32Ch!!

# SendGrid API key (from https://app.sendgrid.com/settings/api_keys)
SMTP_PASSWORD=SG.xxxxxxxxxxxxxxxxxxxx

# OAuth secrets (from Google Cloud Console / Azure Portal)
GOOGLE_CLIENT_SECRET=GOCSPX-xxxxxxxxxx
MICROSOFT_CLIENT_SECRET=xxxxxxxxxxxxxxxx

# OpenAI (from https://platform.openai.com/api-keys)
OPENAI_API_KEY=sk-proj-xxxxxxxxxx

# Azure Document Intelligence key (from Azure Portal → your DI resource → Keys and Endpoint)
AZURE_DI_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# GitHub personal access token (for user feedback → GitHub issue creation)
# Create at https://github.com/settings/tokens with "repo" scope
GITHUB_TOKEN=ghp_xxxxxxxxxxxxxxxxxx

# Your actual domains (must match your DNS records from step 3)
API_DOMAIN=api.traceon.yourdomain.com
BLAZOR_DOMAIN=traceon.yourdomain.com
```

Save the file: press `Ctrl+O`, `Enter`, then `Ctrl+X`.

### 7.2 — Update the Caddyfile

```bash
nano Caddyfile
```

Replace the placeholder domains with your real ones:

```
api.traceon.yourdomain.com {
    reverse_proxy api:8080
}

traceon.yourdomain.com {
    reverse_proxy blazor:80
}
```

Save and exit (`Ctrl+O` → `Enter` → `Ctrl+X`).

---

## 8. Update the Blazor client's API URL

The Blazor app needs to know the production API URL. Edit the settings file that gets baked into the WASM build:

```bash
nano src/Traceon.Blazor/Traceon.Blazor/wwwroot/appsettings.json
```

Change it to:

```json
{
  "ApiBaseAddress": "https://api.traceon.yourdomain.com"
}
```

Save and exit.

> **Tip:** If you want to keep a separate production config, you can create an `appsettings.Production.json` file instead and configure the Blazor host environment accordingly. For simplicity, we edit the base file here since the Blazor WASM build doesn't have ASP.NET Core's environment-based config out of the box.

---

## 9. Build and start everything

From the `traceon` directory (repo root):

```bash
docker compose up -d --build
```

**What happens:**
1. Docker builds the **API image** (compiles .NET, publishes, packages into a container).
2. Docker builds the **Blazor image** (compiles WASM, publishes, copies to Nginx).
3. Docker pulls the **SQL Server 2022** image.
4. Docker pulls the **Caddy** image.
5. All four containers start. The API automatically runs EF Core migrations on startup.
6. Caddy detects your domains and obtains **free HTTPS certificates** from Let's Encrypt automatically.

The first build takes 3–5 minutes. Watch the logs:

```bash
docker compose logs -f
```

Press `Ctrl+C` to stop watching (containers keep running).

### Wait for the database to be ready

The `docker-compose.yml` has a health check. The API container waits for SQL Server to accept connections before starting.

---

## 10. Verify the deployment

### 10.1 — Check all containers are running

```bash
docker compose ps
```

You should see all four containers with status `Up`:

```
NAME              STATUS
traceon-db        Up (healthy)
traceon-api       Up
traceon-blazor    Up
traceon-caddy     Up
```

### 10.2 — Test the API

```bash
curl https://api.traceon.yourdomain.com/api/identity/ping
```

Or open it in your browser.

### 10.3 — Open the Blazor app

Navigate to:

```
https://traceon.yourdomain.com
```

You should see the Traceon login page with a valid HTTPS certificate (padlock icon).

---

## 11. Common operations

### View logs

```bash
# All containers
docker compose logs -f

# Just the API
docker compose logs -f api

# Just SQL Server
docker compose logs -f sqlserver
```

### Deploy an update

```bash
cd ~/traceon
git pull origin main
docker compose up -d --build
```

Docker Compose rebuilds only the images that changed. The database volume persists across rebuilds.

### Stop everything

```bash
docker compose down
```

> This stops containers but **keeps your data** (SQL Server data, Caddy certificates, logs are in named Docker volumes).

### Stop everything AND delete data (⚠️ destructive)

```bash
docker compose down -v
```

### Restart a single service

```bash
docker compose restart api
```

### Enter the SQL Server container

```bash
docker exec -it traceon-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourPasswordHere' -C
```

### Back up the database

```bash
docker exec traceon-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourPasswordHere' -C \
  -Q "BACKUP DATABASE TraceonDb TO DISK='/var/opt/mssql/backup/TraceonDb.bak' WITH FORMAT"

# Copy the backup to the host
docker cp traceon-db:/var/opt/mssql/backup/TraceonDb.bak ~/TraceonDb.bak
```

---

## 12. Troubleshooting

### Container won't start

```bash
docker compose logs <service-name>
```

| Problem | Likely cause | Fix |
|---|---|---|
| API crashes on start | Bad connection string or DB not ready | Check `DB_SA_PASSWORD` matches in `.env`; wait for health check |
| SQL Server exits immediately | Password doesn't meet complexity | Use 16+ chars with upper, lower, digit, symbol |
| Caddy shows certificate error | DNS not pointing to server yet | Wait for propagation; run `nslookup` to verify |
| Blazor shows "Failed to fetch" | API URL wrong in `appsettings.json` | Rebuild Blazor after editing: `docker compose up -d --build blazor` |
| Port 80/443 already in use | Another service on the VPS | `sudo lsof -i :80` to find it; stop it or remove it |

### Check if ports are open from your local machine

```bash
curl -v https://api.traceon.yourdomain.com 2>&1 | head -20
```

### Rebuild from scratch (fresh images, no cache)

```bash
docker compose build --no-cache
docker compose up -d
```

---

## Quick Reference — File Map

| File | Purpose |
|---|---|
| `docker-compose.yml` | Defines all services (DB, API, Blazor, Caddy) |
| `Caddyfile` | Caddy reverse proxy config (domains → containers) |
| `.env` | Secrets and domain names (**never commit**) |
| `.env.example` | Template for `.env` (safe to commit) |
| `src/Traceon.Api/Traceon.Api/Dockerfile` | Multi-stage build for the API |
| `src/Traceon.Blazor/Dockerfile` | Multi-stage build for the Blazor WASM client |
| `src/Traceon.Blazor/nginx.conf` | Nginx config for serving Blazor static files |
| `.dockerignore` | Files excluded from Docker build context |

---

## What's Next?

- [ ] Set up a **GitHub Actions** CI/CD pipeline to auto-deploy on push to `main`
- [ ] Add **Hetzner Cloud Firewall** rules via the dashboard for an extra layer
- [ ] Configure **automated backups** (Hetzner snapshots or cron + `sqlcmd`)
- [ ] Monitor with **Uptime Kuma** (self-hosted) or an external service
- [ ] Add a **staging environment** with a separate `.env` and subdomain
