---
description: How to run the HungryCalendar web application
---

To start the HungryCalendar web server, run the following command from the project root:

```powershell
dotnet run --project HungryCalendar.Web/HungryCalendar.Web.csproj
```

The application is configured to run on:
- HTTP: [http://localhost:5000](http://localhost:5000)
- HTTPS: [https://localhost:5001](https://localhost:5001)

You can also use the default .NET hot reload feature for development:

```powershell
dotnet watch --project HungryCalendar.Web/HungryCalendar.Web.csproj
```
