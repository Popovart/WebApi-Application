# Products API

.NET 10 REST API for retrieving products, paginating results, and updating product descriptions. Supports SQL Server and in-memory mock data.

## Prerequisites

- .NET 10 SDK.
- An IDE with .NET 10 support for running from an IDE.
- SQL Server (DataSource=Sql mode), with credentials permitted to apply migrations.

Run all commands below from the directory containing `WebApiApplication.slnx`.

## Run from an IDE

1. Open `WebApiApplication.slnx` in an IDE with .NET 10 support.
2. Set `Api` as the startup project and select the `http` launch profile.
3. Run the application.

Open Swagger UI: http://localhost:5220/swagger

Swagger provides the v1 and v2 API definitions.

Mock mode requires no database or connection string. Description changes remain available between requests and reset when the application restarts.

## Select the data source

Set `DataSource` in `Api/appsettings.Development.json`:

- `Mock`: use in-memory sample products.
- `Sql`: use SQL Server through `ConnectionStrings:ProductsDb`.

Restart the application after changing the setting.

## Use SQL Server

1. Set `DataSource` to `Sql` in `Api/appsettings.Development.json`.
2. Configure a connection to a development SQL Server database:

```sh
dotnet user-secrets set "ConnectionStrings:ProductsDb" "<connection-string>" --project Api
```

3. Apply migrations, explicitly selecting SQL for the EF tooling:

```sh
dotnet tool restore
dotnet ef database update --project Data --startup-project Api -- --environment Development --DataSource=Sql
```

Migrations create the schema and seed 25 sample products. The application does not apply migrations automatically.

4. Run the `Api` project using the `http` launch profile.

To return to Mock mode, stop the application, set `DataSource` to `Mock`, and run it again.

## Tests

```sh
dotnet test --solution WebApiApplication.slnx
```

The unit suite covers product reads, pagination, description updates, missing products, state isolation, and selected controller responses. Tests require neither SQL Server nor a running API.