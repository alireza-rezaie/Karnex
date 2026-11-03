# Development database seed

The API runs EF Core migrations in every environment. Sample/demo records are inserted **only when ASP.NET Core is running in the Development environment**.

## Seeded entities

The deterministic seed covers all application-owned entity sets in `ApplicationDbContext`:

- Identity: Admin/User roles, four users, role assignments and one address per seeded user.
- Billing: Free/Professional/Business plans and a subscription for each seeded user.
- Projects: WEB, APP and CRM projects, memberships, one sprint per project and representative work items.
- Work item relations: labels, comments and attachment metadata.
- Teams and team members, project role definitions.
- System workflow definition and one board setting per project.
- Meetings and participants, reminders.
- User notifications and notification preferences.

The seeder uses stable IDs and natural keys, checks existing rows before insertion, and does not delete or overwrite existing user-created data. It can be run again safely after a database has already been seeded.

## Development accounts

All seeded accounts use the same initial development password:

- Username / phone: `09120000001` — System Admin — `admin@karnex.local`
- Username / phone: `09120000002` — Developer — `developer@karnex.local`
- Username / phone: `09120000003` — Designer — `designer@karnex.local`
- Username / phone: `09120000004` — QA — `qa@karnex.local`

Default password: `Karnex.Dev!2026`

Set `KARNEX_SEED_PASSWORD` in the local process environment before starting the API to use a different password for newly created seed accounts. Existing accounts are never reset by the seeder.

> These are development-only credentials. Do not use this password outside a disposable local development database. The seeder is guarded by `app.Environment.IsDevelopment()`.

## Run

Start the API with ASP.NET Core environment set to `Development`. The existing startup flow applies EF Core migrations and then runs the seed. No `appsettings.json` changes are required.

If a seeded user ID, phone number or email already belongs to a real local account, the seeder reuses that account and does not overwrite its password or profile.
