# FHS - Fault Handling System

FHS records and tracks quality faults from a production plant. It handles defects found inside the plant, escapes reported by customers, stations, error-code classifications, and customers.

## Chain Composition

FHS uses Vertical Slice Architecture. Within a feature, it uses **Chain Composition**: an endpoint declares the ordered story of its work as a chain instead of hiding that work across a controller, handler, service, and repository.

A chain is an ordered list of **links**. A link is one focused unit of work, such as validating a request, resolving the current actor, loading an entity, applying domain logic, recording events, or saving changes.

Each feature carries a typed `State` object through its chain. State begins with the request and accumulates values produced by earlier links. Shared links target small capability interfaces implemented by a feature's State; local links target that feature's concrete State.

The endpoint declaration should be readable as the feature's story:

```csharp
.Link<ValidateRequest<CreateDefectRequest>>()
.Link<ResolveActor>()
.Link<LoadAndEnsureStationExistence>()
.Link<ClassifyDefect>()
.Link<RaiseDefect>()
.Link<RecordDomainEvents>()
.Link<SaveChanges>()
```

`ValidateRequest` is the first link and `SaveChanges` is terminal. Links that hand State values to later links declare their requirements and outputs with `Requires` and `Produces` attributes. The chain builder checks this declared ordering when the application starts.

For the complete architecture and its constraints, see [docs/chain-composition.md](docs/chain-composition.md).

## Project Structure

| Path | Purpose |
| --- | --- |
| `backend/FHS.Api` | .NET 10 Minimal API with vertical feature slices, links, persistence, and authentication. |
| `backend/FHS.Chain` | The framework-independent Chain Composition kernel. |
| `frontend` | React 19, TypeScript, Vite, Tailwind, and Keycloak OIDC frontend. Bun is the package manager. |
| `infra/FHS.AppHost` | .NET Aspire orchestrator for the API, frontend, PostgreSQL, pgAdmin, and Keycloak. |
| `infra/FHS.ServiceDefaults` | Shared Aspire telemetry, health-check, and resilience defaults. |
| `infra/Keycloak` | The Keycloak realm imported by the local environment. |
| `tests/Fhs.ArchitectureTests` | Tests that enforce structural and Chain Composition rules. |
| `tests/Fhs.IntegrationTests` | API integration tests, including PostgreSQL Testcontainers. |
| `docs` | Architecture, phase notes, and UI wireframes. |
| `temp` | Local runtime data bind-mounted into PostgreSQL, pgAdmin, and Keycloak containers. |

## Prerequisites

FHS runs locally through Aspire and requires a Docker-compatible Linux container runtime. On Windows, use WSL 2 and choose either Docker Desktop or Rancher Desktop configured with the Moby (`dockerd`) engine.

Install these tools before starting the application:

- Windows 11, or Windows 10 version 2004 (build 19041) or later.
- Hardware virtualization enabled in BIOS/UEFI.
- [.NET SDK 10.0.400](https://dotnet.microsoft.com/download/dotnet/10.0), or a later compatible feature band. The required SDK is pinned in `global.json`.
- [Bun](https://bun.sh/docs/installation) on `PATH`.
- One supported container runtime, described below.

### 1. Enable WSL 2 prerequisites from Command Prompt

Open **Command Prompt as Administrator**. Confirm that virtualization is available, then enable the Windows features required by WSL 2:

```cmd
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"Hyper-V Requirements"

dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```

Restart Windows after the commands finish. If `Hyper-V Requirements` reports that virtualization is disabled, enable Intel VT-x or AMD-V in BIOS/UEFI before continuing.

### 2. Install WSL

After restarting, open **Command Prompt as Administrator** and install Ubuntu on WSL 2:

```cmd
wsl --install -d Ubuntu
```

Restart if prompted. Launch **Ubuntu** from the Start menu once to create its Linux username and password, then run:

```cmd
wsl --update
wsl --set-default-version 2
wsl --status
wsl --list --verbose
```

`Ubuntu` should show version `2` in the final command. If WSL is already present and `wsl --install` only displays help, use `wsl --list --online` and then install a distribution with `wsl --install -d <DistroName>`.

Microsoft's current instructions are available at [Install WSL](https://learn.microsoft.com/windows/wsl/install).

### 3. Install a container runtime

Choose one option. Do not install both unless you deliberately manage which Docker daemon is active.

#### Option A: Docker Desktop

1. Download and install [Docker Desktop for Windows](https://docs.docker.com/desktop/setup/install/windows-install/).
2. During setup, select the **WSL 2** backend.
3. Start Docker Desktop and accept its terms if prompted.
4. Verify that the Docker daemon is available:

   ```cmd
   docker version
   docker run --rm hello-world
   ```

Docker Desktop's WSL 2 backend requires a current WSL installation and hardware virtualization.

#### Option B: Rancher Desktop with Moby

1. Download the Windows installer from the [Rancher Desktop releases page](https://rancherdesktop.io/) and install it.
2. Start Rancher Desktop. On first launch, select the **Moby (`dockerd`)** container engine, rather than `containerd`.
3. Wait for Rancher Desktop to report that the runtime is ready. Kubernetes is not required for FHS.
4. Verify that the Moby Docker API is available:

   ```cmd
   docker version
   docker run --rm hello-world
   ```

Rancher Desktop on Windows requires WSL and hardware virtualization. See its [Windows installation guidance](https://docs.rancherdesktop.io/getting-started/installation/) for platform requirements.

### Verify .NET and Bun

Open a new terminal after installing the tools and check that they are on `PATH`:

```cmd
dotnet --version
bun --version
docker version
```

`dotnet --version` should resolve to `10.0.400` or a compatible later feature band. The Docker command must show both Client and Server sections before running FHS.

## Run the Application

1. Start Docker Desktop or Rancher Desktop and wait until its Docker daemon is ready.
2. From the repository root, install the frontend dependencies:

   ```cmd
   cd frontend
   bun install
   cd ..
   ```

3. Start the Aspire AppHost:

   ```cmd
   dotnet run --project infra\FHS.AppHost\FHS.AppHost.csproj
   ```

Aspire starts PostgreSQL, pgAdmin, Keycloak, the API, and the Vite frontend. It prints the Aspire dashboard URL in the terminal. Open the dashboard, wait until the resources are healthy, then use the `web` resource URL to open FHS in a browser. The API's `http` and `https` resource URLs open its Scalar API reference.

The imported local Keycloak realm includes these development accounts:

| User | Password | Access |
| --- | --- | --- |
| `operator` | `operator` | Standard application user. |
| `admin` | `admin` | Standard user plus lookup lifecycle actions. |

Use these accounts only for local development. Stop the application with `Ctrl+C`. PostgreSQL, pgAdmin, and Keycloak data persists under `temp/` between runs.

## Troubleshooting

- `Docker daemon is not running`: start Docker Desktop or Rancher Desktop, confirm it is using the Moby engine, then rerun `docker version`.
- A WSL distribution shows version `1`: run `wsl --set-version Ubuntu 2` in an elevated Command Prompt, then retry.
- `bun` is not recognized: close and reopen the terminal after installing Bun, then run `bun --version`.
- First startup takes longer: Aspire must download container images and install frontend dependencies. Later launches reuse the local data and images.
