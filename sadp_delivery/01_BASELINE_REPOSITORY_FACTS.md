# 01 · Baseline Repository Facts

## Identity

| Item | Value |
|---|---|
| Local path | `E:\ASP NET CORE\News-Archive-.Net-Core-MVC-3-tier-Architecture-` |
| Remote | `github.com/sadatnuman/News-Archive-.Net-Core-MVC-3-tier-Architecture-` (not changed, nothing pushed) |
| Product | The Meridian Newsfolio, a bilingual (English and Bangla) news archive |
| Baseline branch and HEAD | `develop` at `c9f08a3a54b9b300a52298f1f8a6cc5478e7c3a3` |
| Baseline working tree | clean (`git status --short` printed nothing) |
| Work branch | `sadp/midterm-readiness`, created from `develop` without switching away from or resetting anything |
| SDK | .NET SDK 10.0.403 (`global.json` allows the latest 10.0 feature band) |
| Machine | Windows 11 Home, AMD Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM, SQL Server 2025 Express (17.0.1000.7, full-text search not installed) |

## Solution graph (`NewsArchiveSolution/NewsArchiveSolution.slnx`)

```text
NewsArchive (ASP.NET Core MVC + Web API, Razor, Program.cs)
    └── BLL (services, rules, DTOs, AutoMapper, builders)
            └── DAL (EF Core DbContext, entities, repositories, migrations)   [PrivateAssets="compile"]
NewsArchive.Tests (xUnit) → BLL, DAL, NewsArchive
```

`BLL/BLL.csproj:14` references the DAL with `PrivateAssets="compile"`. Because of that, the UI project cannot compile against DAL types, even though it is the process that loads them.

## Packages

| Package | Version | Project |
|---|---|---|
| Microsoft.EntityFrameworkCore (.SqlServer, .Design) | 10.0.14 | DAL, NewsArchive |
| AutoMapper | 14.0.0 (advisory GHSA-rvv3-g6hj-g44x suppressed by an earlier user decision; every map uses `MaxDepth(3)`) | BLL |
| Markdig | 1.5.0 | NewsArchive |
| Swashbuckle.AspNetCore | 10.3.1 | NewsArchive |
| xunit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1, Microsoft.AspNetCore.Mvc.Testing 10.0.14 | — | Tests |

## Size

- 16,451 lines of C# and Razor, not counting migrations and generated code.
- **Web layer:** 17 MVC controllers, 13 API controllers, 3 filters.
- **Business layer:** 17 services, 7 rule classes, 27 DTOs.
- **Data layer:** 11 repositories, 18 EF files.

## Configuration found

| File | Key facts |
|---|---|
| `appsettings.json` | `ConnectionStrings:DbConn` (local SQL Express, integrated security, no password); `Site:TimeZone=Asia/Dhaka`; `ShowDemoNotice=false`; `DemoAccounts=[]`; `Hosting:TrustAllProxies=false`; SMTP host empty, so mail goes to the log or outbox |
| `appsettings.Development.json` | Swagger on, migrate on startup, demo banner and the three demo accounts (sample password, documented in the README) |
| `.github/workflows/build.yml` | ubuntu runner with a SQL Server 2022 service container; restore, build, test, vulnerable-package report |

## Baseline commands (before any edit)

| Command | Exit | Result |
|---|---|---|
| `dotnet restore NewsArchiveSolution.slnx` | 0 | OK |
| `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | Passed 92, Failed 0, Skipped 0 (about 6 s). The 92 include 8 integration tests that ran against the isolated `NewsArchiveTestDB`. |

Total wall time for the three commands: 32 s.

## Databases on the machine

| Database | Owner | Touched in this task? |
|---|---|---|
| `NewsArchiveDB` | The user's development data | **No.** Article count was 28 before and after; no script, migration or app run used it. |
| `NewsArchiveTestDB` | Created by the integration tests | Yes, by the tests only (disposable) |
| `NewsArchiveSmokeDB` | Created in this task for the runtime smoke run | Yes: migrated by the app, then filled with `database/sample-data.sql` (disposable) |
