# PCShop Backend — Technical Documentation

> **Project:** PCShop_Backend  
> **Framework:** ASP.NET Core 8 (.NET 8)  
> **Database:** SQL Server (EF Core 9)  
> **Cache:** Redis  
> **Auth:** JWT Bearer  
> **Generated:** 2026-04-24

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
3. [Getting Started](#3-getting-started)
4. [Configuration Reference](#4-configuration-reference)
5. [Authentication & Authorization](#5-authentication--authorization)
6. [API Reference](#6-api-reference)
   - [Auth](#61-auth-apiauth)
   - [Users](#62-users-apiusers)
   - [Products](#63-products-apiproducts)
   - [Cart](#64-cart-apicart)
   - [Orders](#65-orders-apiorders)
   - [Support](#66-support-apisupport)
7. [Data Models](#7-data-models)
8. [DTOs](#8-dtos)
9. [Service Layer](#9-service-layer)
10. [Caching Strategy](#10-caching-strategy)
11. [Middleware & Cross-Cutting Concerns](#11-middleware--cross-cutting-concerns)
12. [Background Jobs](#12-background-jobs)
13. [Error Handling](#13-error-handling)
14. [Logging](#14-logging)
15. [Database Schema](#15-database-schema)
16. [Dependencies](#16-dependencies)

---

## 1. Project Overview

PCShop Backend is a RESTful API for a PC hardware e-commerce platform. It supports:

- **Product catalog** — Components, categories, specifications, and pre-built PC configurations
- **User management** — Registration, role-based access control, profile management
- **Shopping cart** — Per-user cart with component and PC build support
- **Orders** — Receipt creation, status tracking, and admin-level management
- **Customer support** — Ticketing system with threaded comments
- **Analytics** — Sales statistics by date range

The system uses a layered architecture: Controllers → Services → EF Core DbContext → SQL Server, with Redis caching applied to read-heavy paths.

---

## 2. Architecture

```
┌──────────────────────────────────────────────┐
│                  HTTP Clients                │
└─────────────────────┬────────────────────────┘
                      │
        ┌─────────────▼────────────┐
        │     ASP.NET Core API     │
        │  ┌────────────────────┐  │
        │  │   Rate Limiter     │  │  100 req/min per IP
        │  │   Security Headers │  │
        │  │   Global Exception │  │
        │  │   JWT Auth Filter  │  │
        │  └────────┬───────────┘  │
        │           │              │
        │  ┌────────▼───────────┐  │
        │  │    Controllers     │  │
        │  └────────┬───────────┘  │
        │           │              │
        │  ┌────────▼───────────┐  │
        │  │     Services       │  │
        │  └──┬─────────────┬───┘  │
        │     │             │      │
        │  ┌──▼──┐      ┌───▼──┐  │
        │  │Cache│      │ EF   │  │
        │  │Redis│      │ Core │  │
        │  └─────┘      └───┬──┘  │
        └──────────────────┬┴──────┘
                           │
              ┌────────────▼──────────┐
              │  SQL Server Database  │
              └───────────────────────┘
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Controllers** | HTTP request/response mapping, authorization attributes, calling service methods |
| **Services** | Business logic, cache management, exception throwing |
| **ICacheService** | Redis abstraction with SHA256 key normalization |
| **ApplicationDbContext** | EF Core ORM, entity configuration, migrations |
| **Middleware** | Security headers, global exception handling, rate limiting |

---

## 3. Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (local or remote)
- Redis (default: `localhost:6379`)
- SMTP credentials (for password reset emails)

### Setup

```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

The API will start at `https://localhost:7xxx` / `http://localhost:5xxx`.  
Swagger UI is available at `/swagger`.  
Hangfire dashboard is at `/hangfire`.

### Connection String

Update `appsettings.Development.json` with your SQL Server and Redis details (see [Configuration Reference](#4-configuration-reference)).

---

## 4. Configuration Reference

### appsettings.json (structure)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=PCShop;Trusted_Connection=True;...",
    "RemoteConnection": "Server=db36860.public.databaseasp.net;...",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "<min 32-char secret>",
    "Issuer": "yourapp",
    "Audience": "yourapp_users",
    "ExpireMinutes": 60
  },
  "SmtpSettings": {
    "Server": "smtp.example.com",
    "Port": 587,
    "Username": "user@example.com",
    "Password": "...",
    "SenderEmail": "noreply@example.com",
    "SenderName": "PCShop"
  },
  "AllowedHosts": "*"
}
```

### Key Settings

| Key | Description | Default |
|-----|-------------|---------|
| `Jwt:Key` | HMAC-SHA256 signing secret | (required) |
| `Jwt:ExpireMinutes` | JWT validity window | `60` |
| `ConnectionStrings:Redis` | Redis connection string | `localhost:6379` |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | (required) |
| `SmtpSettings:Port` | SMTP port | `587` |

---

## 5. Authentication & Authorization

### JWT Bearer Authentication

All protected endpoints require an `Authorization: Bearer <token>` header.

Tokens are obtained via `POST /api/auth/login` and expire after **60 minutes** (configurable).

#### JWT Claims

| Claim | Type | Value |
|-------|------|-------|
| `ClaimTypes.Name` | string | Username |
| `ClaimTypes.Role` | string | Role name (Admin / Staff / User) |
| `ClaimTypes.NameIdentifier` | string | UserId |

#### Authorization Policies

| Policy | Required Role | Used On |
|--------|--------------|---------|
| `Admin` | Admin | User management, product writes, order deletion, role management |
| `User` | User | Cart operations, personal orders, support tickets |
| `Staff` | Staff | (available for future use) |

> Endpoints without an authorization attribute are **publicly accessible** (anonymous).

#### Registration Default Role

New users registered via `POST /api/users/register` receive **RoleId = 3** (User) by default.

---

## 6. API Reference

All endpoints are prefixed with `/api`. Pagination uses [Gridify](https://alirezanet.github.io/Gridify/) query parameters.

### Gridify Query Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `page` | Page number (1-based) | `1` |
| `pageSize` | Items per page | `20` |
| `filter` | Filter expression | `name=*Intel` |
| `orderBy` | Sort expression | `price asc` |

---

### 6.1 Auth (`/api/auth`)

#### `POST /api/auth/login`

Authenticates a user and returns a JWT.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response `200 OK`:**
```json
{
  "token": "eyJhbGci..."
}
```

**Errors:**
- `401 Unauthorized` — Invalid credentials

---

#### `POST /api/auth/forgot-password`

Triggers a password reset email. The reset token expires in **30 minutes** and is delivered asynchronously via Hangfire.

**Query Parameter:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `email` | string | Yes | Registered email address |

**Response `200 OK`:**
```json
{ "message": "Password reset token sent to your email." }
```

> For security, the response is identical whether the email exists or not (prevents user enumeration).

---

#### `POST /api/auth/reset-password`

Resets the user's password using a valid reset token.

**Request Body:**
```json
{
  "token": "string (GUID)",
  "newPassword": "string"
}
```

**Response `200 OK`:**
```json
{ "message": "Password reset successful." }
```

**Errors:**
- `400 Bad Request` — Token expired or not found

---

### 6.2 Users (`/api/users`)

#### Roles

##### `GET /api/users/roles` 🔒 Admin

Returns a paginated list of roles.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<RoleDto>`

---

##### `GET /api/users/roles/{roleId}` 🔒 Admin

**Path:** `roleId` (int)  
**Response `200 OK`:** `RoleDto`

---

##### `POST /api/users/roles/create` 🔒 Admin

**Request Body:**
```json
{
  "roleName": "string",
  "description": "string"
}
```
**Response `201 Created`**

---

##### `PUT /api/users/roles/{roleId}` 🔒 Admin

**Path:** `roleId` (int)  
**Request Body:** `UpdateRoleDto` (same shape as create)  
**Response `204 No Content`**

---

##### `DELETE /api/users/roles/{roleId}` 🔒 Admin

**Path:** `roleId` (int)  
**Response `204 No Content`**

---

#### Users

##### `GET /api/users/users` 🔒 Admin

Returns a paginated list of all users.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<UserDto>`

---

##### `GET /api/users/{userId}` 🔒 Admin

**Path:** `userId` (int)  
**Response `200 OK`:** `UserDto`

---

##### `POST /api/users/register` 🌐 Public

Registers a new user account. The new user is assigned the default **User** role.

**Request Body:**
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "fullName": "string",
  "phoneNumber": "string",
  "address": "string",
  "city": "string",
  "country": "string"
}
```

**Response `201 Created`**

**Errors:**
- `409 Conflict` — Username or email already exists

---

##### `PUT /api/users/{userId}` 🔒 User | Admin

Updates a user's profile. Regular users can only update their own profile.

**Path:** `userId` (int)  
**Request Body:**
```json
{
  "fullName": "string",
  "phoneNumber": "string",
  "address": "string",
  "city": "string",
  "country": "string"
}
```

**Response `204 No Content`**

---

##### `DELETE /api/users/{userId}` 🔒 Admin

**Path:** `userId` (int)  
**Response `204 No Content`**

---

### 6.3 Products (`/api/products`)

All read endpoints are publicly accessible. All write endpoints require Admin.

#### Components

##### `GET /api/products/components` 🌐 Public

Returns a paginated list of components.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<ComponentDto>`

Example `ComponentDto`:
```json
{
  "componentId": 1,
  "name": "Intel Core i9-14900K",
  "categoryName": "CPU",
  "brand": "Intel",
  "price": 589.99,
  "stockQuantity": 42,
  "description": "...",
  "imageUrl": "...",
  "specs": [
    { "specKey": "Cores", "specValue": "24", "displayOrder": 1 }
  ]
}
```

---

##### `GET /api/products/component/{id}` 🌐 Public

**Path:** `id` (int)  
**Response `200 OK`:** `ComponentDto`

---

##### `POST /api/products/component/create` 🔒 Admin

**Request Body:**
```json
{
  "name": "string",
  "categoryId": 0,
  "brand": "string",
  "price": 0.00,
  "stockQuantity": 0,
  "description": "string",
  "imageUrl": "string"
}
```
**Response `201 Created`**

---

##### `PUT /api/products/component/update/{id}` 🔒 Admin

**Path:** `id` (int)  
**Request Body:** Same shape as create  
**Response `204 No Content`**

---

##### `DELETE /api/products/component/delete/{id}` 🔒 Admin

**Path:** `id` (int)  
**Response `204 No Content`**

---

#### Component Categories

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/component-categories` | Public | List categories (Gridify) |
| GET | `/component-category/{categoryId}` | Public | Get category by ID |
| POST | `/component-category/create` | Admin | Create category |
| PUT | `/component-category/update/{categoryId}` | Admin | Update category |
| DELETE | `/component-category/delete/{categoryId}` | Admin | Delete category |

**Category DTO:**
```json
{
  "categoryId": 1,
  "categoryName": "CPU",
  "description": "Central Processing Units"
}
```

---

#### Component Specs

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/component-specs` | Public | List specs (Gridify) |
| GET | `/component-spec/{specId}` | Public | Get spec by ID |
| POST | `/component-spec/create` | Admin | Create spec |
| PUT | `/component-spec/update/{specId}` | Admin | Update spec |
| DELETE | `/component-spec/delete/{specId}` | Admin | Delete spec |

**Create/Update Spec Body:**
```json
{
  "componentId": 1,
  "specKey": "TDP",
  "specValue": "125W",
  "displayOrder": 3
}
```

---

#### PC Builds

Pre-assembled PC configurations composed of components.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/pcbuilds` | Public | List PC builds (Gridify) |
| GET | `/pcbuild/{id}` | Public | Get build by ID |
| POST | `/pcbuild/create` | Admin | Create new build |
| PUT | `/pcbuild/update/{id}` | Admin | Update build |
| DELETE | `/pcbuild/delete/{id}` | Admin | Delete build |

**PcBuildDto:**
```json
{
  "buildId": 1,
  "buildName": "Gaming Beast 2025",
  "description": "High-end gaming PC",
  "isPublic": true,
  "createdByUserId": 1,
  "createdByUserName": "admin",
  "createdAt": "2025-01-01T00:00:00",
  "updatedAt": "2025-01-10T00:00:00",
  "totalPrice": 2499.99,
  "components": [
    {
      "componentId": 1,
      "name": "Intel Core i9-14900K",
      "quantity": 1,
      "price": 589.99
    }
  ]
}
```

---

### 6.4 Cart (`/api/cart`)

All cart endpoints require **User** authorization. The cart is scoped to the authenticated user's ID extracted from the JWT.

#### `GET /api/cart/cart-items` 🔒 User

Returns paginated cart items for the current user.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<CartItemsDtos>`

```json
{
  "cartItemId": 1,
  "userId": 5,
  "componentId": 12,
  "buildId": null,
  "quantity": 2,
  "addedAt": "2025-06-01T10:00:00"
}
```

---

#### `POST /api/cart/cart-items` 🔒 User

Adds a component to the cart. Validates stock availability before adding.

**Request Body:**
```json
{
  "componentId": 12,
  "quantity": 2
}
```

**Response `201 Created`**

**Errors:**
- `400 Bad Request` — Insufficient stock (`OutOfStockException`)

---

#### `PUT /api/cart/cart-items/{cartId}` 🔒 User

Updates item quantity.

**Path:** `cartId` (int)  
**Request Body:**
```json
{ "quantity": 3 }
```
**Response `204 No Content`**

---

#### `DELETE /api/cart/cart-items/{cartItemId}` 🔒 User

Removes a single item from the cart.

**Path:** `cartItemId` (int)  
**Response `204 No Content`**

---

#### `DELETE /api/cart/cart-items/clear` 🔒 User

Removes all items from the current user's cart.

**Response `204 No Content`**

---

### 6.5 Orders (`/api/orders`)

All order endpoints require at minimum `[Authorize]`.

#### Receipts (Orders)

##### `GET /api/orders/receipts` 🔒 User

Returns the current user's orders (filtered by UserId from JWT).

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<ReceiptDtos>`

---

##### `GET /api/orders/admin/receipts` 🔒 Admin

Returns all orders across all users.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<ReceiptDtos>`

---

##### `GET /api/orders/receipts/{receiptId}` 🔒 User

**Path:** `receiptId` (int)  
**Response `200 OK`:** `ReceiptDtos`

```json
{
  "receiptId": 101,
  "userId": 5,
  "totalAmount": 1299.98,
  "status": "Shipped",
  "paymentMethod": "CreditCard",
  "shippingAddress": "123 Main St",
  "city": "Hanoi",
  "country": "Vietnam",
  "trackingNumber": "VN123456789",
  "notes": "",
  "createdAt": "2025-06-01T00:00:00",
  "updatedAt": "2025-06-03T00:00:00"
}
```

---

##### `POST /api/orders/receipts` 🔒 User

Creates a new order.

**Request Body:**
```json
{
  "userId": 5,
  "totalAmount": 1299.98,
  "paymentMethod": "CreditCard",
  "shippingAddress": "123 Main St",
  "city": "Hanoi",
  "country": "Vietnam",
  "notes": ""
}
```

**Response `201 Created`**

---

##### `PUT /api/orders/receipts/{receiptId}` 🔒 User

Updates order status or tracking information.

**Path:** `receiptId` (int)  
**Request Body:**
```json
{
  "status": "Shipped",
  "trackingNumber": "VN123456789",
  "notes": "Shipped via ViettelPost"
}
```

**Response `204 No Content`**

**Valid Status Values:** `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`

---

##### `DELETE /api/orders/receipts/{receiptId}` 🔒 Admin

**Path:** `receiptId` (int)  
**Response `204 No Content`**

---

#### Receipt Items (Order Line Items)

All receipt item endpoints require the parent `receiptId` as a query parameter.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/receipt-items?receiptId=X` | List items in a receipt (Gridify) |
| GET | `/receipt-items/{receiptItemId}?receiptId=X` | Get single item |
| POST | `/receipt-items?receiptId=X` | Add item to receipt |
| PUT | `/receipt-items/{receiptItemId}?receiptId=X` | Update item |
| DELETE | `/receipt-items/{receiptItemId}?receiptId=X` | Remove item |

**ReceiptItemsDto:**
```json
{
  "receiptItemId": 1,
  "receiptId": 101,
  "componentId": 12,
  "buildId": null,
  "itemName": "Intel Core i9-14900K",
  "quantity": 1,
  "unitPrice": 589.99
}
```

**Create Receipt Item Body:**
```json
{
  "componentId": 12,
  "buildId": null,
  "itemName": "Intel Core i9-14900K",
  "quantity": 1,
  "unitPrice": 589.99
}
```

---

#### Sales Statistics

##### `GET /api/orders/sales-statistics` 🔒 Admin

Returns aggregated sales data for a date range.

**Query Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `startDate` | DateOnly | Yes | Start of range (YYYY-MM-DD) |
| `endDate` | DateOnly | Yes | End of range (YYYY-MM-DD) |

**Response `200 OK`:** `List<SalesStatisticDto>`

---

### 6.6 Support (`/api/support`)

#### Tickets

##### `GET /api/support/tickets` 🔒 Admin

Returns all support tickets (admin view).

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<SupportTicketDto>`

---

##### `GET /api/support/user-tickets` 🔒 User

Returns tickets created by the current user.

**Query:** Gridify parameters  
**Response `200 OK`:** `Paging<SupportTicketDto>`

---

##### `GET /api/support/ticket/{id}` 🔒 User

**Path:** `id` (int)  
**Response `200 OK`:** `SupportTicketDto`

```json
{
  "ticketId": 1,
  "userId": 5,
  "title": "Order not received",
  "description": "My order #101 has not arrived after 2 weeks.",
  "status": "Open",
  "priority": "High",
  "assignedToUserId": null,
  "createdAt": "2025-06-05T00:00:00",
  "updatedAt": "2025-06-05T00:00:00"
}
```

---

##### `POST /api/support/supportTicket-create` 🔒 User

**Request Body:**
```json
{
  "title": "Order not received",
  "description": "My order #101 has not arrived after 2 weeks.",
  "priority": "High"
}
```

**Valid Priority Values:** `Low`, `Medium`, `High`, `Critical`

**Response `201 Created`**

---

##### `PUT /api/support/supportTicket-update/{id}` 🔒 Admin

**Path:** `id` (int)  
**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "status": "InProgress",
  "priority": "High",
  "assignedToUserId": 2
}
```

**Valid Status Values:** `New`, `Open`, `InProgress`, `Resolved`, `Closed`

**Response `204 No Content`**

---

##### `DELETE /api/support/supportTicket-delete/{id}` 🔒 User

**Path:** `id` (int)  
**Response `204 No Content`**

---

#### Ticket Comments

All comment endpoints are scoped under a parent ticket by `ticketId` in the path.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/{ticketId}/ticketComments` | User | List comments (Gridify) |
| POST | `/{ticketId}/ticketComment-create` | User | Add comment |
| PUT | `/{ticketId}/ticketComment-update/{commentId}` | User | Update comment |
| DELETE | `/{ticketId}/ticketComment-delete/{commentId}` | User | Delete comment |

**Comment DTO:**
```json
{
  "commentId": 1,
  "ticketId": 1,
  "userId": 5,
  "commentText": "Any updates on this?",
  "createdAt": "2025-06-06T00:00:00"
}
```

---

## 7. Data Models

### Entity Relationship Overview

```
Role ──< User ──< CartItem >── Component >── ComponentCategory
                   │                │
                   │              ComponentSpec
                   │
                   ├──< Receipt ──< ReceiptItem >── Component
                   │                         └───── Pcbuild
                   │
                   ├──< Ticket ──< TicketComment
                   │
                   └──< Pcbuild >── PcbuildComponent >── Component
```

### User

| Column | Type | Constraints |
|--------|------|-------------|
| `UserId` | int | PK, Identity |
| `Username` | nvarchar(50) | Unique, Not Null |
| `PasswordHash` | nvarchar(255) | Not Null |
| `Email` | nvarchar(100) | Unique, Indexed, Not Null |
| `FullName` | nvarchar(100) | Not Null |
| `PhoneNumber` | nvarchar(20) | |
| `RoleId` | int | FK → Role |
| `Address` | nvarchar(255) | |
| `City` | nvarchar(100) | |
| `Country` | nvarchar(100) | Default: "Vietnam" |
| `LoyaltyPoints` | int | Default: 0 |
| `CreatedAt` | datetime | Default: GETDATE() |
| `IsActive` | bit | Default: true, Indexed |

### Role

| Column | Type | Constraints |
|--------|------|-------------|
| `RoleId` | int | PK, Identity |
| `RoleName` | nvarchar(50) | Unique, Not Null |
| `Description` | nvarchar(255) | |

### Component

| Column | Type | Constraints |
|--------|------|-------------|
| `ComponentId` | int | PK, Identity |
| `Name` | nvarchar(100) | Not Null |
| `CategoryId` | int | FK → ComponentCategory |
| `Brand` | nvarchar(50) | |
| `Price` | decimal(18,2) | Indexed |
| `StockQuantity` | int | Not Null |
| `Description` | nvarchar(max) | |
| `ImageUrl` | nvarchar(255) | |
| `IsActive` | bit | Default: true, Indexed |
| `CreatedAt` | datetime | Indexed, Default: GETDATE() |
| `UpdatedAt` | datetime | |
| `RowVersion` | timestamp | Concurrency token |

### ComponentCategory

| Column | Type | Constraints |
|--------|------|-------------|
| `CategoryId` | int | PK, Identity |
| `CategoryName` | nvarchar(50) | Unique, Not Null |
| `Description` | nvarchar(255) | |

### ComponentSpec

| Column | Type | Constraints |
|--------|------|-------------|
| `SpecId` | int | PK, Identity |
| `ComponentId` | int | FK → Component |
| `SpecKey` | nvarchar(50) | Indexed |
| `SpecValue` | nvarchar(255) | Not Null |
| `DisplayOrder` | int | Default: 0 |

**Unique Constraint:** `(ComponentId, SpecKey)`

### Pcbuild

| Column | Type | Constraints |
|--------|------|-------------|
| `BuildId` | int | PK, Identity |
| `BuildName` | nvarchar(100) | Not Null |
| `Description` | nvarchar(max) | |
| `CreatedByUserId` | int | FK → User, Nullable |
| `CreatedAt` | datetime | Default: GETDATE() |
| `UpdatedAt` | datetime | |
| `IsPublic` | bit | Default: false |

### PcbuildComponent

| Column | Type | Constraints |
|--------|------|-------------|
| `BuildComponentId` | int | PK, Identity |
| `BuildId` | int | FK → Pcbuild |
| `ComponentId` | int | FK → Component |
| `Quantity` | int | Default: 1 |

**Unique Constraint:** `(BuildId, ComponentId)`

### CartItem

| Column | Type | Constraints |
|--------|------|-------------|
| `CartItemId` | int | PK, Identity |
| `UserId` | int | FK → User |
| `ComponentId` | int | FK → Component, Nullable |
| `BuildId` | int | FK → Pcbuild, Nullable |
| `Quantity` | int | Default: 1 |
| `AddedAt` | datetime | Default: GETDATE() |

> Either `ComponentId` or `BuildId` must be set (not both, not neither).

### Receipt

| Column | Type | Constraints |
|--------|------|-------------|
| `ReceiptId` | int | PK, Identity |
| `UserId` | int | FK → User |
| `TotalAmount` | decimal(18,2) | Not Null |
| `Status` | nvarchar(50) | Default: "Pending", Indexed |
| `PaymentMethod` | nvarchar(50) | |
| `ShippingAddress` | nvarchar(255) | |
| `City` | nvarchar(100) | |
| `Country` | nvarchar(100) | Default: "Vietnam" |
| `TrackingNumber` | nvarchar(100) | |
| `Notes` | nvarchar(max) | |
| `CreatedAt` | datetime | Indexed, Default: GETDATE() |
| `UpdatedAt` | datetime | |

### ReceiptItem

| Column | Type | Constraints |
|--------|------|-------------|
| `ReceiptItemId` | int | PK, Identity |
| `ReceiptId` | int | FK → Receipt |
| `ComponentId` | int | FK → Component, Nullable |
| `BuildId` | int | FK → Pcbuild, Nullable |
| `ItemName` | nvarchar(255) | Not Null |
| `Quantity` | int | Default: 1 |
| `UnitPrice` | decimal(18,2) | Not Null |

### Ticket

| Column | Type | Constraints |
|--------|------|-------------|
| `TicketId` | int | PK, Identity |
| `UserId` | int | FK → User |
| `Title` | nvarchar(100) | Not Null |
| `Description` | nvarchar(max) | |
| `Status` | nvarchar(50) | Default: "New", Indexed |
| `Priority` | nvarchar(20) | Default: "Medium" |
| `AssignedToUserId` | int | FK → User, Nullable |
| `CreatedAt` | datetime | Default: GETDATE() |
| `UpdatedAt` | datetime | |

### TicketComment

| Column | Type | Constraints |
|--------|------|-------------|
| `CommentId` | int | PK, Identity |
| `TicketId` | int | FK → Ticket |
| `UserId` | int | FK → User |
| `CommentText` | nvarchar(max) | Not Null |
| `CreatedAt` | datetime | Default: GETDATE() |

### PasswordReset

| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | int | PK, Identity |
| `Email` | nvarchar | Email-validated |
| `Token` | nvarchar | GUID |
| `ExpireDate` | datetime | 30 minutes from creation |

---

## 8. DTOs

### Auth DTOs

**LoginDto**
```csharp
{ string Username, string Password }
```

**ResetPasswordRequestDto**
```csharp
{ string Token, string NewPassword }
```

### User DTOs

**RegisterUserDto**
```csharp
{
    string Username, string Email, string Password,
    string FullName, string PhoneNumber,
    string Address, string City, string Country
}
```

**UpdateUserDto**
```csharp
{ string FullName, string PhoneNumber, string Address, string City, string Country }
```

**UserDto** (Response)
```csharp
{
    int UserId, string Username, string Email, string FullName,
    string PhoneNumber, int RoleId, string Address, string City,
    string Country, int LoyaltyPoints, DateTime CreatedAt, bool IsActive
}
```

**RoleDto / CreateRoleDto / UpdateRoleDto**
```csharp
{ int RoleId, string RoleName, string Description }
```

### Product DTOs

**ComponentDto** (Response)
```csharp
{
    int ComponentId, string Name, string CategoryName, string Brand,
    decimal Price, int StockQuantity, string Description, string ImageUrl,
    List<ComponentSpecDto> Specs
}
```

**ComponentSpecDto**
```csharp
{ string SpecKey, string SpecValue, int DisplayOrder }
```

**PcBuildDto** (Response)
```csharp
{
    int BuildId, string BuildName, string Description, bool IsPublic,
    int? CreatedByUserId, string CreatedByUserName,
    DateTime CreatedAt, DateTime UpdatedAt,
    decimal TotalPrice, List<PcBuildComponentDto> Components
}
```

### Cart DTOs

**CartItemsDtos** (Response)
```csharp
{ int CartItemId, int UserId, int? ComponentId, int? BuildId, int Quantity, DateTime AddedAt }
```

**AddItemToCartDtos** (Request)
```csharp
{ int ComponentId, int Quantity }
```

**UpdateCartItemsDto**
```csharp
{ int Quantity }
```

### Order DTOs

**ReceiptDtos** (Response)
```csharp
{
    int ReceiptId, int UserId, decimal TotalAmount, string Status,
    string PaymentMethod, string ShippingAddress, string City, string Country,
    string TrackingNumber, string Notes, DateTime CreatedAt, DateTime UpdatedAt
}
```

**CreateReceiptDto**
```csharp
{
    int UserId, decimal TotalAmount, string PaymentMethod,
    string ShippingAddress, string City, string Country, string Notes
}
```

**UpdateReceiptDto**
```csharp
{ string Status, string TrackingNumber, string Notes }
```

**ReceiptItemsDto** (Response)
```csharp
{
    int ReceiptItemId, int ReceiptId, int? ComponentId, int? BuildId,
    string ItemName, int Quantity, decimal UnitPrice
}
```

**SalesStatisticDto**
```csharp
{ /* revenue/product aggregates per date */ }
```

### Support DTOs

**SupportTicketDto** (Response)
```csharp
{
    int TicketId, int UserId, string Title, string Description,
    string Status, string Priority, int? AssignedToUserId,
    DateTime CreatedAt, DateTime UpdatedAt
}
```

**CreateSupportTicketDto**
```csharp
{ string Title, string Description, string Priority }
```

**UpdateSupportTicketDto**
```csharp
{ string Title, string Description, string Status, string Priority, int? AssignedToUserId }
```

**SupportTicketCommentDto** (Response)
```csharp
{ int CommentId, int TicketId, int UserId, string CommentText, DateTime CreatedAt }
```

---

## 9. Service Layer

### ICacheService

Abstraction over `IDistributedCache` (Redis). All keys are SHA256-hashed before storage to normalize length and handle special characters.

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value);
    Task RemoveAsync(string key);
}
```

**Expiry:** 10-minute absolute, 5-minute sliding.  
**Serialization:** JSON via `System.Text.Json`.

---

### IProductService

```csharp
// Components
Task<Paging<ComponentDto>> getComponents(GridifyQuery query);
Task<ComponentDto> getComponentById(int id);
Task createComponent(createComponentDto dto);
Task updateComponent(int id, updateComponentDto dto);
Task deleteComponent(int id);

// Specs
Task<Paging<ComponentSpecsDto>> getComponentSpecs(GridifyQuery query);
Task<ComponentSpecsDto> getComponentSpecById(int specId);
Task addComponentSpecs(CreateComponentSpecDto dto);
Task updateComponentSpecs(int specId, UpdateComponentSpecDto dto);
Task deleteComponentSpecs(int specId);

// Categories
Task<Paging<ComponentCategoriesDto>> getComponentCategories(GridifyQuery query);
Task<ComponentCategoriesDto?> getComponentCategoryById(int categoryId);
Task addComponentCategory(CreateComponentCategoryDto dto);
Task updateComponentCategory(int categoryId, UpdateComponentCategoryDto dto);
Task deleteComponentCategory(int categoryId);

// PC Builds
Task<Paging<PcBuildDto>> getPcBuilds(GridifyQuery query);
Task<PcBuildDto> getPcbuildById(int buildId);
Task createPcbuild(CreatePcBuildDto dto);
Task UpdatePcBuild(int buildId, UpdatePcBuildDto dto);
Task deletePcbuild(int buildId);
```

---

### IAuthService

```csharp
Task<string> Login(LoginDto dto);
PasswordVerificationResult VerifyHashPassword(User user, string userPassword, string inputPassword);
Task GenerateResetPasswordToken(string email);
Task ResetPassword(ResetPasswordRequestDto dto);
```

---

### IJwtTokenService

```csharp
string GenerateToken(User existUser);
```

Produces a signed JWT with claims for Name, Role, and NameIdentifier. Validity: **1 hour**.

---

### IUserService

```csharp
// Roles
Task<Paging<RoleDto>> getRoles(GridifyQuery query);
Task<RoleDto> getRoleById(int roleId);
Task CreateRole(CreateRoleDto dto);
Task UpdateRole(int roleId, UpdateRoleDto dto);
Task DeleteRole(int roleId);

// Users
Task<Paging<UserDto>> getUsers(GridifyQuery query);
Task<UserDto> GetUserById(int id);
Task RegisterUser(RegisterUserDto dto);
Task UpdateUser(int userId, UpdateUserDto dto);
Task DeleteUser(int userId);
```

---

### ICartService

```csharp
Task<Paging<CartItemsDtos>> getCartItems(GridifyQuery query);
Task AddToCart(AddItemToCartDtos dto);
Task UpdateCartItems(int cartId, UpdateCartItemsDto dto);
Task RemoveFromCart(int cartItemId);
Task ClearCart();
```

User identity is extracted from `IHttpContextAccessor` JWT claims internally — no userId parameter needed.

---

### IOrderService

```csharp
// Receipts
Task<Paging<ReceiptDtos>> getReceipts(GridifyQuery query);
Task<Paging<ReceiptDtos>> getAllReceiptsByAdmin(GridifyQuery query);
Task<ReceiptDtos> getReceiptById(int receiptId);
Task CreateReceipt(CreateReceiptDto dto);
Task UpdateReceipt(int receiptId, UpdateReceiptDto dto);
Task DeleteReceipt(int receiptId);

// Receipt Items
Task<Paging<ReceiptItemsDto>> getReceiptItems(int receiptId, GridifyQuery query);
Task<ReceiptItemsDto> GetReceiptItemById(int receiptId, int receiptItemId);
Task CreateReceiptItem(int receiptId, CreateReceiptItemDto dto);
Task UpdateReceiptItem(int receiptId, int receiptItemId, UpdateReceiptItemDto dto);
Task DeleteReceiptItem(int receiptId, int receiptItemId);

// Analytics
Task<List<SalesStatisticDto>> GetSalesStatistics(DateOnly startDate, DateOnly endDate);
```

---

### ISupportService

```csharp
Task<Paging<SupportTicketDto>> getTickets(GridifyQuery query);
Task<Paging<SupportTicketDto>> getTicketsForUser(GridifyQuery query);
Task<SupportTicketDto> getTicketById(int ticketId);
Task CreateSupportTicket(CreateSupportTicketDto dto);
Task UpdateSupportTicket(int ticketId, UpdateSupportTicketDto dto);
Task DeleteSupportTicket(int ticketId);

Task<Paging<SupportTicketCommentDto>> getTicketComments(int ticketId, GridifyQuery query);
Task AddTicketComment(int ticketId, AddSupportTicketCommentDto dto);
Task UpdateTicketComment(int ticketId, int commentId, UpdateSupportTicketCommentDto dto);
Task DeleteTicketComment(int ticketId, int commentId);
```

---

## 10. Caching Strategy

### Overview

Redis is used as a distributed cache via `Microsoft.Extensions.Caching.StackExchangeRedis`. The `ICacheService` abstraction wraps `IDistributedCache` to provide typed Get/Set/Remove with consistent expiry settings.

### Cache Key Naming

Raw keys are passed as strings and then **SHA256-hashed** to produce normalized, fixed-length cache keys. This handles pagination/filter combinations cleanly.

**Raw key patterns (before hashing):**

| Resource | Raw Key Pattern |
|----------|----------------|
| Components list | `Components_{page}_{pageSize}_{filter}_{orderBy}` |
| Single component | `Component_{id}` |
| Categories list | `ComponentCategories_{page}_{pageSize}_{filter}_{orderBy}` |
| Single role | `Role_{roleId}` |
| Roles list | `Roles_{page}_{pageSize}_{filter}_{orderBy}` |
| Single user | `User_{userId}` |
| Users list | `Users_{page}_{pageSize}_{filter}_{orderBy}` |

### Expiry Configuration

| Setting | Value |
|---------|-------|
| Absolute expiration | 10 minutes |
| Sliding expiration | 5 minutes |

### Cache Invalidation

On any **Create**, **Update**, or **Delete**, the service explicitly calls `ICacheService.RemoveAsync(key)` with the affected key (SHA256 of the raw key) to evict stale data.

### Cache Flow (Read-Through)

```
Request → Service.GetXxx()
    ↓
Check Redis (ICacheService.GetAsync)
    ├── HIT  → return cached value
    └── MISS → query DB → ICacheService.SetAsync → return fresh value
```

---

## 11. Middleware & Cross-Cutting Concerns

### SecurityHeadersMiddleware

Applied early in the pipeline. Injects the following response headers on every request:

| Header | Value |
|--------|-------|
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `X-XSS-Protection` | `1; mode=block` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Content-Security-Policy` | `default-src 'self'; script-src ...` |

### GlobalExceptionHandlingMiddleware

Catches unhandled exceptions and maps them to structured JSON error responses.

| Exception Type | HTTP Status |
|----------------|-------------|
| `ArgumentException` | `400 Bad Request` |
| `UnauthorizedAccessException` | `401 Unauthorized` |
| `UnauthorizedException` | `403 Forbidden` |
| `NotFoundException` | `404 Not Found` |
| `ConflictException` | `409 Conflict` |
| Any other | `500 Internal Server Error` |

**Error response shape:**
```json
{
  "message": "Resource not found.",
  "statusCode": 404
}
```

5xx errors are logged at `Error` level; 4xx at `Warning` level.

### Rate Limiting

Fixed-window rate limiter applied globally.

| Setting | Value |
|---------|-------|
| Window | 1 minute |
| Request limit | 100 per window |
| Partition key | Client IP (`RemoteIpAddress`) |
| Response on exceeded | `429 Too Many Requests` |

### CORS

| Setting | Value |
|---------|-------|
| Policy name | `AllowFrontend` |
| Allowed origin | `http://localhost:5173` |
| Allowed methods | Any |
| Allowed headers | Any |
| Allow credentials | Yes |

> Update `AllowedOrigins` in production to the deployed frontend URL.

---

## 12. Background Jobs

**Hangfire** is used for asynchronous job execution with SQL Server as the backing store.

### Configured Jobs

| Job | Trigger | Description |
|-----|---------|-------------|
| Send password reset email | On-demand (from `AuthService`) | Sends reset token email via SMTP |

### Hangfire Dashboard

- **URL:** `/hangfire`
- **Auth:** Basic authentication required (configured in `Program.cs`)

### Email Service

`IEmailService.SendEmailAsync` is called via Hangfire:

```csharp
await _emailService.SendEmailAsync(
    email: dto.Email,
    subject: "Password Reset",
    body: $"Your reset token: {token}. Expires in 30 minutes."
);
```

SMTP configuration is sourced from `SmtpSettings` in `appsettings`.

---

## 13. Error Handling

### Custom Exceptions

| Class | HTTP Code | Usage |
|-------|-----------|-------|
| `NotFoundException` | 404 | Entity not found by ID |
| `UnauthorizedException` | 403 | User lacks permission |
| `ConflictException` | 409 | Duplicate resource (username, email) |
| `OutOfStockException` | 400 | Cart: stock insufficient |

### Usage Example (Service)

```csharp
var component = await _context.Components.FindAsync(id)
    ?? throw new NotFoundException($"Component {id} not found.");
```

The `GlobalExceptionHandlingMiddleware` catches these and returns the appropriate HTTP response automatically — no try/catch needed in controllers.

---

## 14. Logging

**Serilog** is configured with two sinks:

| Sink | Level | Format |
|------|-------|--------|
| Console | Debug | `[Timestamp] [Level] Message` |
| Rolling file | All | `[Timestamp] [Level] Message{Exception}` |

**Log file path:** `logs/log-{date}.txt` (daily rolling)

**Log levels by context:**
- Exception middleware: `Error` for 5xx, `Warning` for 4xx
- Other: inherits minimum level from configuration

---

## 15. Database Schema

### Migrations

EF Core migrations are stored in the `/Migrations/` directory. Apply with:

```bash
dotnet ef database update
```

### Cascade Behaviors

| Relationship | Behavior |
|-------------|---------|
| Component deleted → CartItem.ComponentId | Set Null |
| Pcbuild deleted → CartItem.BuildId | Set Null |
| Component deleted → ReceiptItem.ComponentId | Set Null |
| Pcbuild deleted → ReceiptItem.BuildId | Set Null |
| Category deleted → Component.CategoryId | Client Set Null (EF enforced) |
| Role deleted → User.RoleId | Client Set Null (EF enforced) |

### Performance Indexes

| Table | Indexed Columns |
|-------|----------------|
| `Users` | `Email`, `IsActive` |
| `Components` | `Price`, `CreatedAt`, `IsActive` |
| `ComponentSpecs` | `SpecKey` |
| `Receipts` | `Status`, `CreatedAt` |
| `Tickets` | `Status` |

---

## 16. Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore` | 9.0.10 | ORM |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.10 | SQL Server provider |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | JWT authentication |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 9.0.10 | Redis distributed cache |
| `Gridify.EntityFramework` | 2.17.0 | Dynamic filtering/sorting/pagination |
| `Hangfire.AspNetCore` | 1.8.22 | Background job framework |
| `Hangfire.SqlServer` | 1.8.22 | Hangfire SQL Server storage |
| `Serilog` | 4.3.0 | Structured logging |
| `Serilog.AspNetCore` | 9.0.0 | ASP.NET Core Serilog integration |
| `Serilog.Sinks.File` | 7.0.0 | File sink for Serilog |
| `Swashbuckle.AspNetCore` | 9.0.6 | Swagger/OpenAPI UI |
| `Humanizer` | — | Human-readable strings |

### Runtime Requirements

| Requirement | Version |
|-------------|---------|
| .NET Runtime | 8.0+ |
| SQL Server | 2019+ (Express supported) |
| Redis | 6.0+ |

---

*End of documentation.*
