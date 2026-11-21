# EPMS entity relationship diagram

```mermaid
erDiagram
    EMPLOYEES o|--o| USERS : "links to login"
    EMPLOYEES ||--o{ PROJECT_ASSIGNMENTS : "receives"
    PROJECTS ||--o{ PROJECT_ASSIGNMENTS : "has"

    USERS {
        int Id PK
        string Email UK
        string PasswordHash
        string Role
        int EmployeeId FK, UK "nullable"
        datetime CreatedAt
    }
    EMPLOYEES {
        int Id PK
        string Name
        string Email UK
        string Phone
        string Department
        string Designation
        datetime CreatedAt
        datetime UpdatedAt
    }
    PROJECTS {
        int Id PK
        string Title
        string Description
        date StartDate
        date Deadline
        string Status
        string Priority
        int ProgressPercent
        datetime CreatedAt
        datetime UpdatedAt
    }
    PROJECT_ASSIGNMENTS {
        int ProjectId PK, FK
        int EmployeeId PK, FK
        datetime AssignedAt
    }
```

Project writes require at least one assignee. Removing a project cascades to its assignments; removing an employee cascades to their assignments and linked login. The database enforces unique normalized emails, unique linked employee IDs, deadline on or after start date, and progress between 0 and 100. Roles, statuses, and priorities are stored as strings.
