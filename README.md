# TR_SERVER_20260922

TalesRunner private server emulator for **Windows**, targeting client revision **R186632**.

This repository contains the **source code** and **database scripts**. Pre-built server binaries are available on the [Releases](https://github.com/arstacth/TR_SERVER_20260922/releases) page.

---

## Features

- Multi-process server layout (load balance, agent, room, relay, community)
- MariaDB-backed game data (`tr_game_db`, `tr_game_db_log`)
- Configurable ports, caps, and optional hash check via `settings.ini`

---

## Requirements

| Component | Notes |
|-----------|--------|
| **OS** | Windows 10/11 (64-bit recommended) |
| **Database** | [MariaDB 10.4.28](https://mariadb.org/) with `utf8mb4` |
| **Runtime** | [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) |
| **Build (optional)** | Visual Studio 2019/2022 or Build Tools, MSBuild, .NET Framework developer pack |

---

## Quick start (release bundle)

1. Download **`TR_SERVER_20260922.rar`** from [Releases → 20260922](https://github.com/arstacth/TR_SERVER_20260922/releases/tag/20260922).
2. Extract to a folder (for example `C:\TR_SERVER_20260922`).
3. Set up the database (see [Database setup](#database-setup)).
4. Edit **`settings.ini`** (MariaDB connection string and IPs if not running locally).
5. Run **`START_SERVER.bat`** to launch all services.

To stop servers, use **`STOP_SERVER.bat`** or **`RESTART_SERVERS.cmd`**.

### Default ports (`settings.ini`)

| Setting | Default | Service |
|---------|---------|---------|
| `LoadBalanceServerPort` | 9500 | Load balance (client-facing) |
| `LoadBalanceServerLocalPort` | 9600 | Load balance (local) |
| `AgentServerTCPPort` | 9153 | Agent server |
| `AgentServerTCPPort2` | 9499 | Agent server (secondary) |
| `RoomServerLocalPort` | 9152 | Room server |
| `RelayServerPort` | 9155 | Relay server |
| `CommunityServerPort` | 9000 | Community agent |

Point your compatible **R186632** client at your server IP and load-balance port as required by your client setup.

---

## Build from source

1. Clone this repository:
   ```bash
   git clone https://github.com/arstacth/TR_SERVER_20260922.git
   cd TR_SERVER_20260922
   ```
   The main game database dump is stored with **Git LFS**. Install [Git LFS](https://git-lfs.github.com/) and run `git lfs pull` if the SQL file did not download.

2. Open **`TR_SERVER.sln`** in Visual Studio, or run from the repo root:
   ```bat
   BUILD.bat
   ```
   Release builds output server executables under **`bin\`**.

3. Copy or merge runtime files (`settings.ini`, `hash.ini`, `TRServer.dll`, `iteminfo`, etc.) into **`bin\`** as needed for your deployment layout.

4. Start servers using your **`bin\`** launch scripts or the same process order as the release bundle.

### Solution projects

| Project | Role |
|---------|------|
| **LoadBalanceServer** | Client entry / server list |
| **AgentServer** | Main game logic and sessions |
| **RoomServer** | Rooms and match flow |
| **RelayServer** | UDP relay for in-game traffic |
| **CommunityAgentServer** | Community / profile-related features |
| **LocalCommons** | Shared libraries |

---

## Database setup

Use **MariaDB 10.4.28**. Scripts are in the **`database/`** folder:

1. Create empty databases:
   ```sql
   -- database/make_db.sql
   ```
2. Import schemas and data:
   - **`tr_game_db_utf8mb4.sql`** — main game database (large; LFS in git)
   - **`tr_game_db_log_utf8mb4.sql`** — log database

Example with MariaDB 10.4.28 client tools (adjust user, host, and paths):

```bat
mysql -u root -p < database\make_db.sql
mysql -u root -p tr_game_db < database\tr_game_db_utf8mb4.sql
mysql -u root -p tr_game_db_log < database\tr_game_db_log_utf8mb4.sql
```

Match the connection string in **`settings.ini`**:

```ini
MySQLConnection=server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=utf8mb4;
```

---

## Configuration

Primary file: **`settings.ini`** (release root or **`bin\settings.ini`** when built from source).

Common options:

- **`AgentServerIP` / `RelayAdvertiseIP`** — addresses advertised to clients (use your LAN or public IP when not on localhost).
- **`HashCheck`** — enable/disable client hash validation.
- **`MaxUserCount` / `MaxTotalAgentUserCount`** — concurrency limits.
- **`BlockDDOS`** — connection flood protection settings.

**AgentServer** also expects **`hash.ini`** and **`TRServer.dll`** next to the executable (along with configs), depending on your deployment.

---

## Repository layout

```
TR_SERVER.sln          Solution file
BUILD.bat              Clean Release rebuild
AgentServer/           Agent server source
RoomServer/            Room server source
LoadBalanceServer/     Load balance source
RelayServer/           Relay source
CommunityAgentServer/  Community agent source
LocalCommons/          Shared code
packages/              Referenced third-party DLLs
database/              MariaDB scripts
```

---

## Troubleshooting

- **Build fails (missing targeting pack)** — Install the .NET Framework 4.8 Developer Pack; `BUILD.bat` tries to detect an installed targeting pack.
- **EXE locked on rebuild** — Stop all server processes (`STOP_SERVER.bat`) before running `BUILD.bat`.
- **Cannot connect to MariaDB** — Verify MariaDB 10.4.28 is running, credentials, and that `tr_game_db` was imported.
- **Clone missing large SQL** — Run `git lfs install` and `git lfs pull`.

---

## Links

- **Source:** https://github.com/arstacth/TR_SERVER_20260922  
- **Release (binaries):** https://github.com/arstacth/TR_SERVER_20260922/releases/tag/20260922
