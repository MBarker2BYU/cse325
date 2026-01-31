# Overview

As a software engineer building my skills in modern .NET web development, I created ServePoint Cadet—a secure, role-based web application that helps JROTC cadets track and document their volunteer and community service hours while allowing instructors to review and approve those entries. The goal was to practice building a full-stack, authenticated application with real-world utility, focusing on user experience, data integrity, and role-based security.

ServePoint Cadet is a Blazor Server application. To run it locally for testing:

1. Open the solution in Visual Studio 2026 Professional
2. Make sure the project is set as startup project
3. Press **F5** (or click the green Start button)
4. The browser will open automatically to `https://localhost:xxxx/` (accept the self-signed certificate if prompted)

The first page you see is the home / landing page with a welcome message and navigation links. After logging in, you are taken to the dashboard.

The primary purpose of this software is to create a clean, functional prototype for tracking service hours in a structured educational/military program environment, while giving me hands-on experience with Blazor component architecture, ASP.NET Core Identity, Entity Framework Core, role-based authorization, and responsive web design.

[Software Demo Video](http://youtube.link.goes.here)  
*(4–5 minute demonstration – shows starting the server, logging in as cadet and instructor, creating/editing entries, approving entries, and viewing dashboard)*

# Web Pages

- **Home / Landing Page** (`/`)  
  Public page with welcome message “ServePoint Cadet – JROTC Volunteer Hour Tracker Prototype”, brief description, and buttons to Register or Login. No dynamic content until authenticated.

- **Dashboard** (`/dashboard`)  
  Requires login. Shows total approved volunteer hours for the current user, a list of recent entries (last 5), and progress indicators toward common JROTC service goals. Dynamically loads data from the database filtered by the logged-in user’s ID.

- **My Volunteer Logs** (`/volunteer`)  
  Cadet-only page. Displays a table of the current cadet’s own volunteer entries (filtered by UserId). Columns: Date, Hours, Description, Location, Status. Includes buttons to Add New Entry, Edit, and Delete (only own records). Table is dynamically populated from EF Core query.

- **Add New Entry** (`/volunteer/create`)  
  Cadet-only form page. Contains inputs for Service Date (date picker), Hours (numeric), Description (textarea), Location (text). Client + server validation. On submit, saves new record with Status = "Pending" and redirects back to /volunteer.

- **Edit Entry** (`/volunteer/edit/{id}`)  
  Cadet-only. Loads existing entry (only if owned by current user). Same form fields pre-filled. Updates record and keeps or resets status as needed.

- **Pending Approvals** (`/approvals`)  
  Instructor-only page. Shows table of all Pending entries from all cadets. Columns include cadet name/email, date, hours, description, location. Buttons per row to Approve (sets Status = "Approved") or Reject (sets Status = "Rejected"). Updates are saved immediately and reflected in cadet dashboards.

- **Login / Register / Logout** (Identity scaffolded pages)  
  Standard ASP.NET Identity pages under `/Identity/Account/*`. Handle user creation, authentication, and session management.

Navigation is handled via a responsive sidebar/top bar that shows different links depending on role (AuthorizeView + Roles).

# Development Environment

- **IDE / Editor**: Visual Studio 2026 Professional (Windows)
- **Framework**: .NET 9 (Blazor Server)
- **Programming Language**: C# 13
- **Authentication**: ASP.NET Core Identity
- **Data Access**: Entity Framework Core
- **Database (dev)**: SQLite (file-based)
- **Database (planned prod)**: PostgreSQL via Npgsql.EntityFrameworkCore.PostgreSQL
- **UI Styling**: Bootstrap (default Blazor template) + potential MudBlazor components
- **Version Control**: Git + GitHub
- **Project Management**: Trello
- **Testing Tools**: Browser developer tools, Lighthouse (accessibility/performance)

# Useful Websites

* [Microsoft Learn – Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0)
* [ASP.NET Core Identity scaffolding](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-9.0)
* [Entity Framework Core – Getting Started](https://learn.microsoft.com/en-us/ef/core/get-started/)
* [MudBlazor Components](https://mudblazor.com/)
* [Render.com – Deploy .NET Apps](https://render.com/docs/deploy-dotnet)
* [WCAG 2.1 Quick Reference](https://www.w3.org/WAI/WCAG21/quickref/)

# Future Work

* Implement full role assignment UI (admin page to assign Cadet/Instructor roles)
* Add progress bars or simple charts to dashboard for visual goal tracking
* Enable file upload for service verification photos/documents
* Build basic reporting/export (PDF/CSV of hours per cadet)
* Complete production deployment to Render.com with PostgreSQL
* Improve accessibility to reach Lighthouse AA score ≥ 95
* Add unit/integration tests for services and controllers
* Polish UI with MudBlazor tables, cards, and dialogs
* Create user documentation / help section inside the app

This README is now ready for your GitHub repo. Once pushed, you can submit the repo link for W02.

Let me know when you've added & pushed it, and we'll move to the next step (scaffolding Identity or creating the first Blazor page). What's your current status?

## License

MIT License (for academic/course use).  
Commercial rights reserved for ShadowWorx Systems LLC(future ServePoint product).
