## CI

The `CI` GitHub Actions workflow runs for pull requests targeting `main` and pushes
to `main` only. Feature-branch pushes do not create a separate push-event run.
One `ubuntu-latest` job checks out the repository, installs the .NET 10 SDK,
restores the solution, builds in Release, and executes the existing tests.

To reproduce the checks locally, install a .NET 10 SDK and run these commands
from the application Git root (the directory containing `WebApiApplication.slnx`
and `global.json`):

```sh
dotnet restore WebApiApplication.slnx
dotnet build WebApiApplication.slnx --configuration Release --no-restore
dotnet test --solution WebApiApplication.slnx --configuration Release --no-build
```

The sole existing test is `Assert.True(true)`. Green CI confirms a successful
build and execution of the existing tests only, not correctness of business
logic, endpoints, or database behavior. Local command success is not hosted CI
verification; workflow discovery, event routing, SDK setup, and Ubuntu execution
must be verified separately from actual GitHub Actions runs and logs.