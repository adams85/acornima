The test suite is generated using [the `Test262Harness.Console` tool](https://www.nuget.org/packages/Test262Harness.Console),
which emits a TUnit-based suite (see `TestFramework` in `Test262Harness.settings.json`).

Generation runs automatically as part of the build (see the `GenerateTestSuite` target) and regenerates whenever
`Test262Harness.settings.json` changes.

To generate the test suite manually, run:

```
dotnet tool restore
dotnet test262 generate
```

The test project also includes a custom runner that executes the suite directly (outside of MTP and TUnit) with progress
reporting and allow-list management:

```
dotnet run -- test262 --update-allow-list
```
