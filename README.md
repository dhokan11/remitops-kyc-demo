# RemitOps KYC Demo

<p align="center">
  <strong>Multi-tenant remittance operations and KYC review platform</strong><br/>
  Built to demonstrate secure, observable, and maintainable fintech system design for employer review.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-Web%20API-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/React-Frontend-61DAFB?style=for-the-badge&logo=react&logoColor=111827" alt="React" />
  <img src="https://img.shields.io/badge/Vite-Build%20Tool-646CFF?style=for-the-badge&logo=vite&logoColor=white" alt="Vite" />
  <img src="https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Auth-RBAC%20%2B%20Protected%20APIs-0F766E?style=for-the-badge" alt="RBAC" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Status-Demo%20Prototype-orange?style=flat-square" alt="Status" />
  <img src="https://img.shields.io/badge/Domain-Fintech%20%7C%20KYC%20%7C%20Operations-blue?style=flat-square" alt="Domain" />
  <img src="https://img.shields.io/badge/Review-Employer%20Ready-success?style=flat-square" alt="Employer Ready" />
</p>

## Overview

This repository contains a business-oriented demo platform prepared for technical and stakeholder review. It was designed as more than a simple CRUD application and instead models a multi-tenant remittance operations environment with KYC review, admin workflows, protected APIs, seeded business data, and dashboard-oriented thinking.

The project was prepared for public GitHub review, and it is intended to demonstrate both implementation capability and business/domain awareness.

## Why this project stands out

- Multi-tenant structure instead of a single-user demo.
- Role-based access design across platform admins, org unit admins, and end users.
- Business-facing dashboards and operations visibility instead of only forms and tables.
- Practical backend stack using ASP.NET Core, EF Core, Dapper, and SQL Server.
- Responsive frontend using React, Vite, and modular API integration.
- Branding-aware presentation aligned with Dahabshiil-inspired visual direction.

## Core capabilities

### Administration
- Tenant management with persistent create/update flows.
- Org unit and user administration through the admin route structure.
- Dynamic business-oriented navigation across admin modules.

### KYC and compliance workflow
- KYC document workflow and review queue concepts.
- Protected API endpoints and RBAC-focused access control.
- Audit-friendly development approach with logging and separation of concerns.

### Operations and reporting
- Transactions monitoring and operations-focused views.
- Recent activity, queue summaries, and dashboard card patterns.
- Reporting and audit-trail areas within the admin structure.

### UI and usability
- Responsive design for desktop and smaller screens.
- KPI-oriented charts and map-based visualization direction.
- Data table usability improvements, including transaction table visibility enhancements.

## Tech stack

| Layer | Technologies |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API, Entity Framework Core, Dapper |
| Frontend | React, Vite, TypeScript, Axios-based API integration |
| Database | SQL Server with a consolidated single `RemitOpsDB` database |
| Access control | Protected APIs and role-based access design  |
| Visual layer | Dashboard cards, charts, maps, and operational tables |
| Data initialization | Idempotent app-based seeding for demo readiness |

## Main application areas

| Area | Purpose |
|---|---|
| Overview | KPI-style operational dashboard with summaries, activity patterns and geo maps. |
| Tenants | Manage tenants with key profile data including code, location, and active status. |
| Org Units | Model enterprise structure below the tenant level. |
| Users | Support administrative and operational user management. |
| KYC Review | Demonstrate document-review and compliance-oriented workflow thinking. |
| Transactions | Provide operational monitoring and business-table usage. |
| Audit Trail / Reports / Settings | Support oversight, reporting, and platform administration. |

## Architecture approach

The project follows a modular structure that separates frontend pages, API modules, services, controllers, DTOs, and data-access responsibilities so the codebase is easier to maintain and explain.

The broader design direction also includes resilient, idempotent seeding so the environment can be initialized repeatedly without becoming fragile on reruns.

This approach is meant to demonstrate maintainability, traceability, and team-readability rather than just short-term feature assembly.

## Local setup

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server
- Git

### Backend
```bash
cd backend/RemitOps.API
dotnet restore
dotnet build
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Configuration notes

Before running locally, set your environment-specific values for database access, auth configuration, and frontend API endpoints. Real secrets, private credentials, and non-public environment files should not be committed to a public repository.

## Demo scope note

This project is a demo prototype intended to communicate technical capability, domain familiarity, and solution design thinking. In a real enterprise implementation, business requirements would be owned and validated with stakeholders and delivered through formal change management, compliance review, and iterative planning.


## Contact

**Mohamed Abdiwahid**  
Full-Stack Architect

> dhokan11@gmail.com.
