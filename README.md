# Vehicle Cleaning System – Full Development Plan

## Overview

This is a vehicle cleaning reservation system where customers can book time slots for services such as car washing and interior cleaning.

The system is designed with a strict separation:

- Backend handles all business logic
- Frontends only display data and send requests
- All clients communicate through one API (except the Razor Pages admin which uses managers directly)

---

# System Architecture

## Backend (Core System)

Technology:
- ASP.NET Core API
- ASP.NET Core Razor Pages (Admin Dashboard)
- Microsoft SQL Server

Role:
This is the brain of the system.

Responsibilities:
- Booking creation and validation
- Capacity management per time slot
- Working hours configuration
- Slot overrides (custom capacity per hour/day)
- Preventing overbooking
- Data storage and retrieval

Important rule:
All business logic exists ONLY in the backend.

---

## Frontend Applications

The system has three frontend clients:

---

## 1. Customer Website

Technology:
- HTML
- CSS
- JavaScript

Purpose:
Public booking interface for customers.

Features:
- Select date
- View available time slots
- Choose hour
- Enter name and phone
- Submit booking
- Receive confirmation

Important:
- No business logic
- Only calls backend API

---

## 2. Admin Dashboard (Razor Pages)

Technology:
- ASP.NET Core Razor Pages (inside the API project)

Purpose:
Internal management tool.

Features:
- View bookings per day
- View occupancy per time slot
- Modify slot capacity (overrides)
- Configure working hours
- Update booking status

Important:
- Uses managers/services directly (no HTTP calls to own API)
- Lives in the same ASP.NET Core project as the API

---

## 3. Worker Mobile App

Technology:
- .NET MAUI

Purpose:
Used by workers for daily operations.

Features:
- View assigned jobs
- See schedule grouped by hour
- View customer details
- Mark job status (optional)

Important:
- No scheduling logic inside app
- Backend controls all rules

---

# System Architecture Diagram

Backend:
- ASP.NET Core API + Razor Pages + SQL Server

Frontends:
- Customer Website (HTML/JS)
- Admin Dashboard (Razor Pages inside backend)
- Worker App (.NET MAUI)

---

# Core Business Rules

## Working Hours

Configurable per day:

Example:
- Monday–Friday: 08:00–17:00
- Saturday: 09:00–13:00
- Sunday: Closed

Stored in database and editable by admin.

---

## Time Slots

- Slot duration: 1 hour
- Default capacity: 2 vehicles
- Capacity can be overridden per slot

Example:
- 09:00 → 2 cars max
- 10:00 → 2 cars max

---

## Booking Rules

A booking is allowed only if:

- Within working hours
- Slot is not closed
- Capacity is not exceeded

Otherwise:
- Booking is rejected

---

# Data Models

## Booking

- Id
- CustomerName
- Phone
- ScheduledDate
- ScheduledHour
- Status (Pending, Confirmed, InProgress, Completed, Cancelled)

---

## SlotOverride

- Id
- Date
- Hour
- Capacity

Purpose:
Overrides default capacity or blocks slot (Capacity = 0)

---

## WorkingHours

- Id
- DayOfWeek
- StartHour
- EndHour
- IsClosed

Purpose:
Defines weekly schedule

---

# System Flow

## Customer Booking Flow

1. Open website
2. Select date
3. Request available slots from backend
4. Backend calculates availability
5. User selects slot
6. Enters details
7. Sends booking request
8. Backend validates and stores booking

---

## Availability Calculation

For each slot:

- Get working hours for day
- Check if closed
- Apply slot override if exists
- Otherwise use default capacity
- Count bookings
- Calculate remaining slots

Formula:
Available = Capacity - Booked

---

## Admin Flow (Razor Pages)

- View bookings per day
- Modify capacity (slot overrides)
- Change working hours
- Update booking status

---

## Worker Flow

- Login to mobile app
- View daily jobs
- See grouped schedule
- Update job status (optional)

---

# Key Design Principles

## Backend is source of truth
All logic exists only in backend.

## Frontends are dumb clients
They only:
- display data
- send requests
- render responses

## Single API
All external apps use the same backend API.

---

# Final Architecture

                 BACKEND
        ASP.NET Core API + Razor Pages + SQL Server
                        |
        --------------------------------
        |              |               |
Customer Web     Admin (Razor Pages)   Worker Mobile (.NET MAUI)

---

# Summary

- 1 backend system (ASP.NET Core + Razor Pages + SQL Server)
- 3 frontend clients (web + Razor Pages + mobile)
- Fully API-driven for external clients
- Flexible scheduling system
- Scalable for real business use
