---
title: CI/CD pipeline and application startup
type: architecture
summary: What the GitHub Actions workflow really does (jobs, when each runs, the Azure deployment) and what the application does at startup (pipeline, database initialization, test-mode seeding), including what is not there: migrations, a post-deploy health check, rollback, scanning.
tags: [ci, cd, pipeline, github-actions, workflow, deployment, azure, startup, program, database-initialization, migrations, test-mode, seeding, health-check, rollback, functional-tests, coverage]
sources:
  - .github/workflows/ci-cd.yml
  - Program.cs
  - Fiszki.Services/Services/TestDataSeeder.cs
  - test-mode.sh
  - Fiszki.csproj
verified_commit: c20f71dc4bf41ad1693b6ec4a52ab177fb5fe501
confidence: high
---

# CI/CD pipeline and application startup

## Responsibility

How the code is built, tested and deployed, and what the application does when it starts. The
in-app claims about both are compared with this page in [[information-pages]]. The layers and the
database provider are in [[application-layers]].

## CI/CD pipeline

One workflow, `.github/workflows/ci-cd.yml`, with five jobs. It uses the .NET 10 SDK and the
`Fiszki.sln` solution.

**When it runs:** on a push to any branch, on a pull request into the main branch, and on a manual
run.

| Job | Runs when | What it does |
|---|---|---|
| Build | always | Restores and builds in Release; on the main branch also publishes the application and uploads it as an artifact |
| Unit Tests | after Build | Runs the unit test project in Release, with a TRX result file and code coverage collection, and uploads the results |
| Functional Tests (E2E) | pull requests and pushes to main only | Builds, installs the Playwright browser, **starts the application in test mode** in the background, waits until it answers, runs the browser tests, shows the application log if they fail, stops it and uploads the results |
| Required Status Check | after the three above | Fails if Build or Unit Tests failed; for pull requests and pushes to main also fails if Functional Tests did not succeed; for pushes to other branches it accepts skipped functional tests |
| Deploy to Azure | only for a **push to main** whose status check succeeded | Downloads the published artifact and deploys it to the Azure Web App named `Fiszki`, slot `Production`, using a publish profile stored as a repository secret |

Notes:

- **Feature-branch pushes do not run the browser tests**; pull requests do.
- **The functional-test job gives the application a real AI key** taken from a repository secret, so
  generation can call the real model there, see [[openrouter-integration]]. Three repository secrets
  are used in total: the AI key, a "secrets check" value and the Azure publish profile. Their values
  are not in the repository.
- **A manual run builds and runs the unit tests, and skips the browser tests and the deploy**, because
  both of those jobs are limited to pull requests and pushes to main (the deploy to pushes only).

**Not in the workflow:** a database migration step, a health check or smoke test after deployment,
an automatic rollback, a coverage report or quality gate (coverage is only collected), a security
scan, and a deployment from a manual run. The only "health" wait is the loop that waits for the
test-mode application to start.

## Application startup

`Program.cs` does the following, in this order:

1. **Registers services:** interactive server rendering, MudBlazor, the unused cookie setup, the
   authorization services and the per-connection authentication state ([[authentication-session]]),
   the database context (PostgreSQL, or in-memory in test mode) and the application services.
2. **Builds the request pipeline.** Outside the Development environment it uses the `/Error` page
   and HSTS. Always: HTTPS redirection, static files, anti-forgery, authentication and authorization,
   then the interactive components.
3. **Initializes the database.**
   - **Test mode:** creates the in-memory database and **seeds sample users and flashcards** on every
     start (`Fiszki.Services/Services/TestDataSeeder.cs` `SeedAsync`).
   - **Otherwise:** only checks that the database can be reached and prints whether it could. It
     **does not apply migrations and does not create the schema**.
4. **Starts.** In test mode a failure in step 3 is ignored; otherwise an exception is rethrown and
   the application does not start. A connection that merely cannot be reached is reported by the
   check as a failure message and, as the code is written, does not stop the application (inferred
   from how the check is written; not tested).

**Test mode** is on when the environment is `Test`, the `TestMode` setting is true or the
`--test-mode` argument is given (`Program.cs` `isTestMode`). `test-mode.sh` starts the application
that way on port 5000, which is what the functional tests and the CI job rely on.

## Interactions

- **Database schema:** because neither the application nor the workflow applies migrations, the
  schema of a real database has to be created some other way (outside the repository's automation).
- **Test data:** the seeder creates the accounts and flashcards that the browser tests use; their
  details are intentionally not recorded in this wiki.

## Behavior worth knowing (code reading)

- A deployment to production is not preceded by a migration, and nothing checks that the deployed
  application is healthy; a failed deployment would not roll back by itself.
- The test-mode application and the production application run the same code but with different
  database providers, so behavior that depends on PostgreSQL features (such as the case-insensitive
  email column, see [[user-entity]]) is not exercised by the browser tests.

## Related

- [[information-pages]]
- [[application-layers]]
- [[openrouter-integration]]
- [[authentication-session]]
