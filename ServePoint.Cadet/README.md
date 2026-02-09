# Overview

ServePoint Cadet is a web application I built to practice modern .NET web development skills as a software engineer. It allows JROTC cadets to log volunteer and community service hours and lets instructors review and approve those entries.

To start a test server on your computer:
1. Open the project in Visual Studio 2026 Professional.
2. Press F5 (or click the green Start button).
3. The browser opens automatically to https://localhost:xxxx/ (accept the certificate warning if prompted).

The first page is the home/landing page with navigation links.

The purpose of writing this software is to create a useful tool for tracking service hours in a JROTC setting while gaining experience building secure, interactive web applications with C# and Blazor Server.

[Software Demo Video](http://youtube.link.goes.here)

# Web Pages

- Home / Landing Page (/)
  Shows welcome message and navigation. No dynamic content until logged in.

- Dashboard (/dashboard)
  Requires login. Dynamically shows total approved hours and recent entries for the current user.

- My Volunteer Logs (/volunteer)
  Cadet-only page. Dynamically loads a table of the user's own entries from the database. Buttons to add, edit, or delete entries.

- Add New Entry (/volunteer/create)
  Cadet-only form. Dynamically saves a new entry to the database with "Pending" status.

- Edit Entry (/volunteer/edit/{id})
  Cadet-only. Loads and dynamically updates an existing entry.

- Pending Approvals (/approvals)
  Instructor-only. Dynamically lists all pending entries from all cadets. Buttons to approve or reject (updates status in database).

- Login/Register/Logout
  Standard ASP.NET Identity pages for user management.

Navigation uses a responsive menu that shows different links based on the logged-in user.

# Development Environment

- IDE: Visual Studio 2026 Professional on Windows
- Language: C# with .NET 10
- Framework: Blazor Server
- Database access: ADO.NET (SqlConnection, SqlCommand, SqlDataReader) with SQLite
- Authentication: ASP.NET Core Identity (minimal setup)
- Libraries: System.Data.SQLite (for raw SQL), Microsoft.AspNetCore.Identity (for login)

# Useful Websites

* [Microsoft Learn – Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-9.0)
* [ASP.NET Core Identity Overview](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0)
* [ADO.NET SQLite Tutorial](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)

# Future Work

* Add role checks so cadets only see their own entries and instructors see pending approvals
* Implement full CRUD with raw SQL INSERT/UPDATE/DELETE commands
* Add basic validation on the add/edit forms
* Create a simple dashboard summary with total hours
* Deploy to a cloud service like Render.com
* Record and upload the 4-5 minute demo video

# License

**Academic Use Only – Commercial Rights Reserved**

This software (ServePoint Cadet) is provided for academic purposes as part of the BYU-Idaho / Pathway .NET Web Applications course.

Copyright © 2026 Matthew Barker (developer for course project)

All rights, including but not limited to source code, design, branding, and future commercial exploitation, are retained by **ShadowWorx Systems LLC**.  

No license is granted for any use beyond the scope of the current academic assignment.  
Any reproduction, distribution, modification, or commercial use outside this course requires explicit written permission from ShadowWorx Systems LLC.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.