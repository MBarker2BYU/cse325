→ Open https://localhost:5001 (or the port displayed)

5. Register test accounts
- Navigate to `/Identity/Account/Register`
- Create accounts for testing (e.g., cadet@test.com, instructor@test.com)
- Roles are seeded automatically on first run (see Program.cs)

6. Login and test
- Cadet: /volunteer
- Instructor: /approvals

## Deployment (Production)

Planned target: Render.com (cloud service meeting course requirement)

1. Create free account at render.com
2. New → Web Service → Connect GitHub repo
3. Build command: `dotnet publish -c Release -o out`
4. Start command: `dotnet out/ServePoint.dll`
5. Add PostgreSQL database service in Render
6. Set environment variable: `ConnectionStrings__DefaultConnection` to Postgres connection string
7. Deploy → obtain live URL

## Project Management & Collaboration

- **Trello Board**: [Paste your board link here after creation]
- Lists: Backlog, To Do, In Progress, Done
- **GitHub Repository**: https://github.com/YOUR-USERNAME/ServePoint
- **Weekly Status Updates**: 60-second reports submitted in Canvas
- **Course Communication**: Microsoft Teams

## Course Outcomes Alignment

1. Describe the components of the .NET developer ecosystem  
→ Blazor Server, Identity, EF Core, Git, cloud deployment
2. Design, develop, and deploy a functional .NET application  
→ Complete Blazor web app with auth, CRUD, roles, cloud deploy
3. Demonstrate the skills of a productive team member  
→ Solo self-management: Trello tasks, Git commits, deadlines, status updates

## License

MIT License (for academic/course use).  
Commercial rights reserved for ShadowWorx Systems (future ServePoint product).

Built in Cape Coral, Florida – January/February 2026  
For BYU-Idaho .NET course – Course Project submission