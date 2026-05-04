# PCShop Backend API Reference

## 1. Introduction
This document provides a detailed reference for the PCShop Backend REST API.
- **Base URL**: `http://localhost:5000/api` (default)
- **Version**: v1.0.0
- **Auth**: JWT Bearer Token required for most endpoints.

## 2. Authentication
Endpoints for user session management and password recovery.

### Login
- **Endpoint**: `POST /api/Auth/Login`
- **Auth**: None
- **Body**: `{ "email": "string", "password": "string" }`
- **Response**: `{ "token": "JWT_TOKEN_STRING" }`

### Forgot Password
- **Endpoint**: `POST /api/Auth/forgot-password?email={email}`
- **Auth**: None
- **Response**: "Password reset token sent to your email."

### Reset Password
- **Endpoint**: `POST /api/Auth/reset-password`
- **Auth**: None
- **Body**: `{ "email": "string", "token": "string", "newPassword": "string" }`

---

## 3. Product Catalog
Management of components, categories, and custom builds.

### Components
- `GET /api/Products/components` (Public) - Supports Gridify querying.
- `GET /api/Products/component/{id}` (Public)
- `POST /api/Products/component/create` (Admin)
- `PUT /api/Products/component/update/{id}` (Admin)
- `DELETE /api/Products/component/delete/{id}` (Admin)

### Categories
- `GET /api/Products/component-categories` (Public)
- `POST /api/Products/component-category/create` (Admin)
- `PUT /api/Products/component-category/update/{id}` (Admin)
- `DELETE /api/Products/component-category/delete/{id}` (Admin)

### PC Builds
- `GET /api/Products/pcbuilds` (Public)
- `GET /api/Products/pcbuild/{id}` (Public)
- `POST /api/Products/pcbuild/create` (Admin)
- `PUT /api/Products/pcbuild/update/{id}` (Admin)

---

## 4. Shopping Cart
User-scoped persistent cart management.

### Manage Cart Items
- `GET /api/Cart/cart-items` (Auth)
- `POST /api/Cart/cart-items` (Auth) - Body: `{ "componentId": int, "quantity": int, "buildId": int? }`
- `PUT /api/Cart/cart-items/{cartId}` (Auth)
- `DELETE /api/Cart/cart-items/{cartItemId}` (Auth)
- `DELETE /api/Cart/cart-items/clear` (Auth)

---

## 5. Orders & Receipts
Financial transaction management and sales reporting.

### Receipts
- `GET /api/Orders/receipts` (Auth) - User's own receipts.
- `GET /api/Orders/admin/receipts` (Admin) - All system receipts.
- `POST /api/Orders/receipts` (Auth) - Checkout process.
- `GET /api/Orders/sales-statistics` (Admin) - Query: `startDate`, `endDate`.

### Receipt Items
- `GET /api/Orders/receipt-items?receiptId={id}` (Auth)
- `POST /api/Orders/receipt-items?receiptId={id}` (Auth)

---

## 6. Customer Support
Ticketing system for user inquiries.

### Tickets
- `GET /api/Support/tickets` (Admin)
- `GET /api/Support/user-tickets` (Auth)
- `POST /api/Support/supportTicket-create` (Auth)
- `GET /api/Support/ticket/{id}` (Auth)

### Ticket Comments
- `GET /api/Support/{ticketId}/ticketComments` (Auth)
- `POST /api/Support/{ticketId}/ticketComment-create` (Auth)

---

## 7. User Management
System users and RBAC management.

### Roles
- `GET /api/Users/roles` (Admin)
- `POST /api/Users/roles/create` (Admin)

### Users
- `POST /api/Users/register` (Public)
- `GET /api/Users/users` (Admin)
- `PUT /api/Users/{userId}` (Auth) - Update self or Admin update any.
- `DELETE /api/Users/{userId}` (Admin)

---

## 8. Querying & Pagination
This API uses **Gridify** for all collection endpoints.
- **Filtering**: `filter=Name=*RTX*`
- **Sorting**: `orderBy=Price desc`
- **Pagination**: `page=1&pageSize=20`

## 9. Error Handling
The API returns standard HTTP status codes:
- `200 OK`: Success.
- `400 Bad Request`: Validation or logic error.
- `401 Unauthorized`: Token missing or invalid.
- `403 Forbidden`: Insufficient permissions (Role-based).
- `404 Not Found`: Resource not found.
- `429 Too Many Requests`: Rate limit exceeded (100 req/min).
- `500 Internal Server Error`: Centralized exception handler will catch these and log to Serilog.
