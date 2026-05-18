## Summary

- Closes #
- 

## Assumptions

- None.

## How to test

```bash
dotnet restore
dotnet build --configuration Release
dotnet test tests/TasksApi.UnitTests --configuration Release
dotnet test tests/TasksApi.IntegrationTests --configuration Release
```

## Checklist

- [ ] Linked to the GitHub issue
- [ ] Branch name follows the repository convention
- [ ] Added or updated unit tests
- [ ] Added or updated integration tests
- [ ] Updated documentation or instructions when needed
- [ ] No secrets or connection strings were committed