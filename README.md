
# Internal Cleaning Operations Platform

A centralized operations platform for cleaning service businesses that combines CRM, booking management, workforce coordination, scheduling, SOP execution, invoicing, reporting, and customer operations into a single system.

---

# Overview

This platform is designed to replace fragmented tools and spreadsheets with one integrated operational system.

The architecture follows a strict backend-driven approach:

- Backend contains all business logic
- Frontends are lightweight clients
- Centralized API powers all applications
- Operational data is managed from one source of truth

The project currently includes:

- ASP.NET Core API
- Razor Pages admin dashboard
- SQL Server database
- .NET MAUI worker application
- Web frontend for customer booking

---

# Core Objectives

## Operational

- Centralize bookings, jobs, employees, invoices, and reporting
- Standardize workflows across teams
- Reduce manual coordination
- Improve scheduling visibility
- Track service execution in real time

## Financial

- Accelerate invoicing
- Improve receivables tracking
- Generate operational and financial reporting
- Reduce administrative overhead

## Technical

- Maintain strict separation of concerns
- Keep business logic centralized
- Support multiple frontend clients
- Enable scalable module-based development

---

# System Architecture

```text
                    +---------------------+
                    |  Customer Website   |
                    |  HTML / CSS / JS    |
                    +----------+----------+
                               |
                               v
+----------------+    +----------------------+    +----------------+
| Worker Mobile  |    |   ASP.NET Core API   |    | Razor Pages    |
| .NET MAUI App  +--->+ Business Logic Layer +<---+ Admin Dashboard|
+----------------+    +----------+-----------+    +----------------+
                                  |
                                  v
                         +------------------+
                         |   SQL Server     |
                         |   Database       |
                         +------------------+
```

---

# Technology Stack

## Backend

- ASP.NET Core Web API
- ASP.NET Core Razor Pages
- Entity Framework Core
- Microsoft SQL Server

## Frontend

### Customer Booking Website

- HTML
- CSS
- JavaScript

### Internal Admin Dashboard

- Razor Pages

### Worker Mobile Application

- .NET MAUI

---

# Main Modules

## CRM & Client Management

- Client profiles
- Contact information
- Site/location management
- Contract and service tracking
- Historical activity logs

---

## Booking & Scheduling

- Service booking
- Time-slot availability
- Capacity management
- Schedule planning
- Working hour configuration
- Slot overrides
- Recurring scheduling support

---

## Workforce Management

- Employee assignments
- Shift visibility
- Mobile task execution
- Job status tracking
- Cleaner coordination
- Attendance and accountability support

---

## SOP & Task Execution

- Cleaning checklists
- SOP attribution
- Task verification
- Proof-of-service workflows
- Standardized operational execution

---

## Financial Operations

- Invoice generation
- Payment tracking
- Revenue reporting
- Receivables management
- Client profitability visibility

---

# Business Rules

## Backend as Source of Truth

All validation, scheduling, and operational rules exist exclusively in the backend.

Frontends only:

- Display data
- Send requests
- Render UI

---

## Working Hours

Working hours are configurable per day.

Example:

| Day | Hours |
|---|---|
| Monday–Friday | 08:00–17:00 |
| Saturday | 09:00–13:00 |
| Sunday | Closed |

---

## Booking Rules

A booking is valid only when:

- The selected time is inside working hours
- Slot capacity is available
- Slot is not blocked or closed
- Validation passes all business constraints

---

## Slot Capacity

Default slot capacity can be overridden per hour or per date.

Example:

| Time Slot | Capacity |
|---|---|
| 09:00 | 2 |
| 10:00 | 2 |
| 11:00 | 0 (blocked) |

---

# Core Data Models

## Booking

```text
- Id
- CustomerName
- Phone
- ScheduledDate
- ScheduledHour
- Status
```

---

## WorkingHours

```text
- Id
- DayOfWeek
- StartHour
- EndHour
- IsClosed
```

---

## SlotOverride

```text
- Id
- Date
- Hour
- Capacity
```

---

# Frontend Applications

## 1. Customer Website

Public booking interface.

### Features

- View available time slots
- Create bookings
- Submit customer details
- Receive confirmations

---

## 2. Admin Dashboard

Internal management system built with Razor Pages.

### Features

- Manage bookings
- Configure working hours
- Adjust slot capacities
- Review schedules
- Monitor operations
- Update statuses

---

## 3. Worker Mobile App

Mobile application for operational staff.

### Features

- View assigned jobs
- Access customer details
- Review SOPs and checklists
- Track completion
- Update job status

---

# Scheduling Flow

## Customer Booking Flow

```text
1. Customer selects date
2. Frontend requests available slots
3. Backend validates availability
4. User submits booking
5. Backend stores booking
6. Confirmation returned to client
```

---

## Availability Calculation

```text
Available Capacity = Slot Capacity - Existing Bookings
```

Validation process:

1. Load working hours
2. Check if day is closed
3. Apply slot overrides
4. Count active bookings
5. Calculate remaining capacity
6. Return availability response

---

# Design Principles

## Centralized Business Logic

All rules live in one place to avoid inconsistencies between applications.

---

## Modular Architecture

Modules are separated by responsibility:

- Scheduling
- CRM
- Finance
- Workforce
- Reporting
- Operations

---

## API-First Approach

All external clients communicate through the backend API.

---

## Scalable Foundation

The system is structured to support:

- Additional mobile apps
- Multi-location operations
- Advanced analytics
- Role-based permissions
- Automation workflows
- Future integrations

---

# Current Project Summary

| Metric | Value |
|---|---|
| Files analyzed | 98 |
| Modules | 16 |
| Main languages | C#, JavaScript, HTML, CSS |
| Architecture | Multi-client backend-driven system |

---

# Future Roadmap

## Planned Enhancements

- Authentication & role management
- Real-time notifications
- GPS job tracking
- Advanced reporting dashboards
- Automated invoicing workflows
- Multi-tenant architecture
- Customer portal
- Analytics and KPI dashboards
- Employee performance tracking
- Calendar synchronization

---

# Development Philosophy

The platform is being developed as a long-term operational foundation for internal business management.

Primary focus areas:

- Reliability
- Maintainability
- Scalability
- Operational efficiency
- Clear separation of concerns

---

# Repository Structure

```text
/Backend
    /API
    /Services
    /Managers
    /Data
    /Entities

/Frontend
    /CustomerWebsite
    /AdminDashboard

/Mobile
    /WorkerApp

/Documentation
```

---

# License

Private internal business software.

All rights reserved.
