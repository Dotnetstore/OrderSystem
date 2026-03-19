# OrderSystem API integration tests

These tests exercise the minimal API end to end with:

- a real ASP.NET Core test host
- a real SQLite database created in memory for each test
- a test message publisher that captures published domain events

## Covered scenarios

- create an order
- start processing and complete an order
- cancel an order
- fail when an order does not exist
- fail when an order transition is not allowed

## Run the tests

```powershell
dotnet test .\tests\OrderSystem.Tests\OrderSystem.Tests.csproj
```

## Notes

- The test host uses the `Testing` environment.
- RabbitMQ is replaced with an in-memory collector so the tests stay fast and deterministic.
- The database is reset before each test.

