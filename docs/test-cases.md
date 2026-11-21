# EPMS test cases

Run `dotnet test .\backend\Epms.sln`. The service tests use SQLite in memory. Integration tests exercise the ASP.NET Core HTTP pipeline through `WebApplicationFactory` with an isolated, seeded SQLite in-memory database for each test class. See [the recorded output](test-results.txt).

| Coverage | Representative cases |
|---|---|
| Authentication | Both roles log in, wrong passwords return 401, registration grants Employee only, duplicate email returns 409, `/me` requires a token. |
| Existing employee registration | Demo registration links the existing profile by email without duplicating it. Linking by email is blocked in Production. |
| Employee directory | Admin CRUD; duplicate and invalid input; paging, search, department filter, and sorting; unauthorized access is rejected. |
| Projects | Admin CRUD and reassignment; missing or unknown assignees are rejected; deadline validation; status filtering; Completed sets progress to 100%. |
| Project access | An employee lists only assigned projects. Unassigned detail and progress requests are rejected. An assigned progress update succeeds. |
| Dashboard | Global totals, status counts, upcoming deadlines, and employee-scoped counts match the seeded data. |
| API documentation | Swagger JSON is served and lists project routes. |

## Manual UI checklist

1. Sign in as Admin and confirm global counts, the chart, and employee assignment totals.
2. Create an employee, try the same email again, edit the original, filter the table, and confirm deletion.
3. Create a project with at least two assignees, filter by status and priority, and replace the assignment list.
4. Sign in as John, confirm that only assigned projects appear, update a project, and reload to verify the change persisted.
5. In the local demo, create an employee record as Admin and assign it to a project. Register with the same email and confirm the new login sees the assigned project without creating another employee record.
