# Internal Cleaning Operations Platform

## Developer Product Brief and Step-by-Step Build Plan

### Objective
Build one internal platform that replaces separate CRM, reservation, facility management, workforce coordination, SOP, communication, and financial reporting tools.

---

# 1. Executive Summary

This document defines the product vision, functional scope, workflows, data model, modules, implementation roadmap, and business value proposition for a custom internal platform for a cleaning services business.

The platform should centralize the complete operating model: lead capture, booking, job planning, employee assignment, SOP attribution, field execution, client communication, invoicing, payment tracking, and management reporting.

- **Strategic goal:** create a single source of truth for clients, sites, bookings, jobs, cleaners, invoices, payments, and operational history.
- **Commercial goal:** reduce administrative work, improve scheduling utilization, accelerate invoicing, and increase management visibility.
- **Operational goal:** standardize service delivery through checklists, SOPs, proof-of-service records, and mobile employee execution flows.
- **Financial goal:** produce real-time revenue, receivables, client profitability, and monthly performance dashboards from transactional data.

---

# 2. Value Proposition: Why Build Internally

| Pain Point | Internal Platform Value | Business Impact |
|---|---|---|
| Fragmented systems | Unifies CRM, booking, task management, SOPs, communication, and finance in one product. | Lower tool cost, fewer manual handoffs, less operational leakage. |
| Manual invoicing and spreadsheet tracking | Connects completed tasks to invoice generation and payment status. | Faster billing cycle and improved cash visibility. |
| Difficult employee coordination | Mobile employee view shows assigned jobs, location, time window, SOP, checklist, and completion confirmation. | Fewer errors, better accountability, faster onboarding. |
| No centralized service history | Every client, site, task, issue, photo, invoice, and communication is stored in one timeline. | Better client service, stronger retention, easier dispute resolution. |
| Limited management reporting | Operational and financial dashboards provide revenue, utilization, unpaid invoices, recurring clients, and client concentration. | Better business decisions and scalable growth management. |

---

# 3. Target Users and Roles

| Role | Core Needs | Permissions |
|---|---|---|
| Owner / Admin | Full visibility across bookings, clients, employees, invoices, payments, reports, configuration, and user permissions. | Full access. |
| Operations Manager / Dispatcher | Create tasks, assign cleaners, monitor schedule, handle changes, view job progress, manage SOPs. | Operational access, limited financial controls. |
| Cleaner / Employee | View assigned tasks, SOPs, checklists, location, schedule, notes, and submit completion evidence. | Mobile-only execution access. |
| Finance / Accountant | View invoices, export reports, track payments, reconcile revenue, manage client billing details. | Financial access. |
| Client / Customer | Request service, approve quote, view booking, receive reminders, confirm completion, access invoices. | Client portal access. |

---

# 4. Core Product Modules

| Module | Purpose | Key Features |
|---|---|---|
| CRM & Client Registry | Maintain all client, site, and contact information. | Client profiles, OIB/VAT ID, addresses, billing terms, service history, notes, files. |
| Reservation / Booking System | Capture and schedule one-time or recurring service requests. | Online booking, internal booking, recurring frequency, site selection, service type, quote estimate. |
| Task & Work Order Management | Convert bookings into operational tasks. | Task creation, status tracking, assignment, priority, due dates, dependencies, linked SOPs. |
| Employee Mobile View | Enable field teams to execute work cleanly. | Daily agenda, map/location, check-in/out, SOP checklist, photos, comments, proof-of-service. |
| SOP & Quality Control | Standardize service delivery. | SOP library, checklist templates, service-specific instructions, inspection results, issue logging. |
| Communication Funnel | Manage all client and employee communication. | Lead funnel, automated reminders, job updates, email/SMS/WhatsApp templates, internal comments. |
| Financial Reporting | Provide financial visibility and billing control. | Invoice generation, payment status, revenue dashboards, receivables, client revenue, exports. |
| Management Dashboard | Provide decision-ready operational insights. | Today schedule, jobs by status, cleaner utilization, revenue by month, overdue invoices, top clients. |

---

# 5. End-to-End Operating Workflow

## 5.1 Booking Workflow

1. Lead or existing client submits a booking request through the website, client portal, phone entry by admin, or recurring contract setup.
2. System captures client, site address, service type, preferred date/time, frequency, estimated duration, special requirements, and attachments/photos if available.
3. System checks availability using employee capacity, existing jobs, travel buffers, and service duration rules.
4. Admin confirms booking, creates quote if needed, or converts directly into a scheduled job.
5. Client receives confirmation message with date, time window, service summary, and terms.
6. Booking automatically creates a work order and initial task record.

## 5.2 Task Creation Workflow

7. Work order is created from a booking, recurring contract, manual admin entry, or client request.
8. System assigns required service category: regular cleaning, deep cleaning, garage cleaning, carpet maintenance, window cleaning, drainage/emergency cleaning, office cleaning, etc.
9. Task stores client, site, service line, estimated time, required staff, required materials, price, due date, and operational notes.
10. System recommends SOP templates based on service type and site profile.
11. Task enters status: Draft, Scheduled, Assigned, In Progress, Completed, Reviewed, Invoiced, Paid.

## 5.3 Task Assignment Workflow

12. Dispatcher reviews task pipeline and daily calendar.
13. System shows available employees based on working hours, existing assignments, location, skills, and workload.
14. Dispatcher assigns one or multiple cleaners to the task.
15. Employee receives notification and sees the task in mobile agenda.
16. Employee can accept assignment or flag conflict if enabled.
17. Dispatcher monitors assignment status and can reassign if needed.

## 5.4 SOP Attribution Workflow

18. Each service category has default SOP templates and checklists.
19. When a task is created, the system automatically attaches the SOP relevant to service type, client, and site.
20. Admin can override or add custom instructions per client or site.
21. Employee must review SOP before starting and complete checklist before closing the job.
22. For high-risk or special jobs, mandatory photo evidence or supervisor approval can be required.
23. Completed SOP data is stored against the task and visible in client/service history.

## 5.5 Field Execution Workflow

24. Employee opens mobile app and views Today agenda.
25. Employee opens job card: client, address, time window, notes, SOP, checklist, materials, contact details.
26. Employee taps Check in when arriving on site; GPS/time stamp is stored.
27. Employee completes checklist items, uploads photos if required, logs issues, and adds comments.
28. Employee taps Complete; system captures end time, duration, checklist completion, issues, and evidence.
29. Task moves to Review or Completed depending on business rules.

## 5.6 Funnel Communication Workflow

30. Lead enters funnel through website form, phone entry, referral, existing client request, or imported contact.
31. Lead status moves through New Lead, Contacted, Quoted, Won, Scheduled, Active Client, Dormant, Lost.
32. System triggers communication templates at each stage: booking confirmation, quote follow-up, reminder, job completed, invoice sent, payment reminder.
33. All emails, SMS, WhatsApp messages, and internal notes are stored in the client timeline.
34. Admin can filter the funnel by source, status, expected value, follow-up date, and owner.
35. Management dashboard shows conversion rate, open quotes, won revenue, and lost reasons.

## 5.7 Financial Reporting Workflow

36. Completed job automatically becomes invoice-ready.
37. Admin reviews price, discount, VAT status, client billing details, and payment terms.
38. System generates invoice or exports invoice data to accounting software.
39. Payment status is tracked as Draft, Sent, Partially Paid, Paid, Overdue, Written Off.
40. Dashboard aggregates revenue by month, client, service type, employee/team, and payment status.
41. Reports can be exported to Excel/PDF for accountant, tax, or management review.

---

# 6. Functional Requirements

| Area | Requirement | Priority |
|---|---|---|
| Client Management | Create, edit, search, and archive clients; store OIB, billing address, sites, contacts, terms, notes, files. | MVP |
| Booking | Create bookings manually and from web form; support one-time and recurring schedules. | MVP |
| Scheduling | Calendar and dispatch board with day/week/month view. | MVP |
| Task Assignment | Assign cleaners, set status, track capacity and job ownership. | MVP |
| Mobile Execution | Cleaner view with task list, check-in/out, checklist, notes, and photo upload. | MVP |
| SOP Library | Create SOP templates and attach to service types and tasks. | MVP |
| Communication | Email templates and notification logs; SMS/WhatsApp as later integrations. | Phase 2 |
| Finance | Invoice-ready records, revenue dashboard, payment tracking, Excel export. | MVP |
| Reporting | Monthly revenue, top clients, unpaid invoices, job completion rate, employee utilization. | MVP |
| Client Portal | Client booking, job history, invoices, and service feedback. | Phase 2 |
| Automation | Recurring jobs, auto reminders, payment reminders, late invoice alerts. | Phase 2 |
| Advanced Analytics | Forecasting, profitability by client/service, route optimization. | Phase 3 |

---

# 7. Core Data Model

| Entity | Key Fields | Relationships |
|---|---|---|
| Client | client_id, name, OIB/VAT ID, billing address, email, phone, payment terms, status | Has many Sites, Contacts, Bookings, Invoices. |
| Site | site_id, client_id, address, access notes, site type, preferred SOPs | Belongs to Client; has many Jobs. |
| Contact | contact_id, client_id, name, role, phone, email, communication preference | Belongs to Client. |
| Booking | booking_id, client_id, site_id, service type, date/time, recurrence, estimated price, status | Creates Jobs / Work Orders. |
| Job / Task | task_id, booking_id, client_id, site_id, assigned users, status, date, price, SOP, evidence | Belongs to Booking; has Assignments, Checklist Responses, Invoice Line. |
| Employee | employee_id, name, role, skills, availability, phone, status | Has Assignments and Timesheets. |
| SOP Template | sop_id, title, service type, checklist items, required photos, quality criteria | Attached to Service Types, Sites, and Tasks. |
| Invoice | invoice_id, invoice number, client, issue date, due date, amount, VAT, discount, status | Generated from completed Tasks. |
| Payment | payment_id, invoice_id, date, amount, method, reference | Belongs to Invoice. |
| Communication Log | message_id, channel, recipient, template, timestamp, status, related object | Linked to Client, Booking, Task, or Invoice. |

---

# 8. Required Screens / Pages

| Screen | Audience | Description |
|---|---|---|
| Dashboard | Owner / Admin | KPIs: revenue month-to-date, open jobs, completed jobs, unpaid invoices, overdue amount, top clients. |
| Client List | Admin / Finance | Search, filter, create, edit, view client history. |
| Client Profile | Admin / Finance | Client data, sites, contacts, jobs, invoices, messages, notes, documents. |
| Booking Form | Admin / Client | Create booking request with service, date, address, recurrence, price estimate. |
| Dispatch Calendar | Operations | Visual scheduling with drag-and-drop assignments and capacity overview. |
| Task Detail | Operations / Employee | Job summary, SOP, assigned staff, checklist, evidence, timeline, invoice linkage. |
| Employee Mobile Agenda | Employee | Today’s tasks, check-in/out, SOP checklist, comments, upload photos. |
| SOP Library | Admin / Ops | Create and maintain SOP templates by service type. |
| Finance Dashboard | Owner / Finance | Revenue, open invoices, overdue invoices, client revenue, exports. |
| Funnel Board | Admin / Sales | Lead pipeline from inquiry to active client. |

---

# 9. Recommended Technical Architecture

- Frontend web app: React / Next.js for admin, dispatcher, finance, and client portal.
- Mobile-first employee interface: responsive PWA initially; native app can follow later if needed.
- Backend API: Node.js/NestJS, Laravel, Django, or Ruby on Rails; choose based on developer capability and speed.
- Database: PostgreSQL as primary relational database.
- File storage: S3-compatible storage for job photos, attachments, invoices, and documents.
- Authentication: role-based access control with admin, dispatcher, employee, finance, and client roles.
- Notifications: email first; SMS and WhatsApp API integration as Phase 2.
- Reporting: SQL-based aggregation tables first; BI layer later if needed.
- Accounting export: Excel/CSV first; accounting system integration later.

---

# 10. MVP Roadmap

| Phase | Scope | Outcome |
|---|---|---|
| Phase 0: Discovery | Map current workflows, invoice fields, client list, employee roles, service categories, and SOP templates. | Confirmed requirements and data structure. |
| Phase 1: Core Database + Admin App | Build clients, sites, bookings, tasks, employees, invoice-ready records, login/roles. | Operational backbone established. |
| Phase 2: Scheduling + Assignment | Calendar, dispatch board, employee assignment, status tracking, recurring bookings. | Business can manage daily operations in the platform. |
| Phase 3: Employee Mobile Execution | Mobile task view, SOP checklist, check-in/out, notes, photo upload, completion flow. | Field execution becomes standardized. |
| Phase 4: Financial Reporting | Invoice records, payment status, monthly revenue, client revenue, overdue invoices, Excel export. | Management can track cash and performance. |
| Phase 5: Communication Funnel | Lead pipeline, templates, automated reminders, client timeline. | Sales and client communication become systematic. |
| Phase 6: Client Portal + Automation | Online booking, client history, self-service invoice view, automated payment reminders. | Platform becomes client-facing and scalable. |

---

# 11. Acceptance Criteria for MVP

- Admin can create a client, site, booking, task, assignment, SOP, and invoice-ready record without using Excel.
- Employee can log in from mobile, view assigned tasks, open SOP, check in, complete checklist, upload evidence, and mark task complete.
- Owner can view monthly revenue, unpaid invoices, top clients, completed jobs, and upcoming scheduled jobs.
- Dispatcher can manage schedule and assignments from one operational calendar.
- Finance user can export invoice data for accounting and track payment status.
- Every task has a clear audit trail: who created it, who was assigned, SOP used, completion time, evidence, and billing status.

---

# 12. Success Metrics

| Metric | Why It Matters | Target Direction |
|---|---|---|
| Admin hours per week | Measures operational efficiency. | Decrease. |
| Booking-to-job conversion time | Measures scheduling speed. | Decrease. |
| Job completion accuracy | Measures SOP adherence and quality. | Increase. |
| Invoice cycle time | Measures speed from completed work to invoice. | Decrease. |
| Overdue invoice value | Measures cash discipline. | Decrease. |
| Revenue per client | Measures account value. | Increase. |
| Cleaner utilization | Measures workforce efficiency. | Increase without overloading staff. |
| Client repeat rate | Measures retention and service quality. | Increase. |

---

# 13. Build vs Buy Conclusion

Buying separate CRM, reservation, field-service, facility-management, and financial tools may solve individual problems, but it creates data fragmentation and recurring subscription cost. Building internally can create a tailored operating system that matches the exact business model, invoice structure, service categories, employee workflows, and reporting needs.

The internal platform should start as a focused MVP, not a large enterprise system. The first version should prioritize operational control, employee execution, invoice-ready data, and management visibility. More advanced modules such as client portal, route optimization, WhatsApp automation, and profitability forecasting should be added only after the core workflow is stable.

---

# 14. Developer Handoff Checklist

- Confirm business roles and permissions.
- Confirm service categories and default SOPs.
- Import existing client and invoice dataset from Excel.
- Define invoice numbering rules and uniqueness logic.
- Design database schema and API endpoints.
- Build MVP screens: Dashboard, Clients, Bookings, Tasks, Calendar, Employees, SOPs, Finance.
- Build mobile employee task execution flow.
- Implement reporting exports and payment status tracking.
- Test with 10 real clients and 20 historical invoices before full rollout.
- Create admin training guide and SOP governance process.

---

# 15. Prioritized Product Backlog

| Priority | Feature | User Story |
|---|---|---|
| P0 | Client registry | As an admin, I need to store clients, OIB, addresses, contacts, and billing terms. |
| P0 | Booking creation | As an admin, I need to create one-time and recurring bookings. |
| P0 | Task assignment | As dispatcher, I need to assign cleaners and track status. |
| P0 | Employee mobile view | As a cleaner, I need to see jobs, instructions, and checklists. |
| P0 | SOP checklist | As manager, I need every job to follow a standard operating procedure. |
| P0 | Invoice-ready jobs | As finance, I need completed jobs ready for invoicing. |
| P1 | Financial dashboard | As owner, I need monthly revenue, unpaid invoices, and client revenue. |
| P1 | Communication templates | As admin, I need booking confirmations and reminders. |
| P1 | Client portal | As client, I want to request service and view invoices. |
| P2 | Route optimization | As dispatcher, I want optimized daily routes. |
| P2 | Advanced profitability | As owner, I want margin by client, service type, and employee team. |

---

Prepared as a developer-ready product brief for building an internal cleaning operations platform.
