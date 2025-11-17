# Error Handling Improvements for UsersController

## Overview
Comprehensive error handling has been implemented for the `UsersController` to properly handle authorization failures and other error scenarios. The solution includes a global exception handling middleware, custom exception filters, and enhanced controller methods with proper validation and error handling.

## Changes Made

### 1. **New Exception Class** (`Exceptions\UnauthorizedException.cs`)
- Created a custom `UnauthorizedException` to distinguish authorization failures from authentication failures
- Used when a user lacks the required role or permission to access a resource

### 2. **Global Exception Handling Middleware** (`Middleware\GlobalExceptionHandlingMiddleware.cs`)
- Centralized exception handling for the entire application
- Maps different exception types to appropriate HTTP status codes:
  - `UnauthorizedException` ? 403 Forbidden
  - `InvalidCredentialsException` ? 401 Unauthorized
  - `InvalidTokenException` ? 401 Unauthorized
  - `NotFoundException` ? 404 Not Found
  - `ConflictException` ? 409 Conflict
  - `ArgumentException` ? 400 Bad Request
  - Other exceptions ? 500 Internal Server Error
- Returns consistent JSON response format with status code and message
- Logs all exceptions appropriately using Serilog

### 3. **Authorization Exception Filter** (`Filters\AuthorizationExceptionFilter.cs`)
- Converts authorization/authentication failures from ASP.NET Core into custom exceptions
- Distinguishes between unauthenticated requests (no token) and unauthorized requests (insufficient role)
- Provides clear error messages to clients

### 4. **Enhanced UsersController** (`Controllers\UsersController.cs`)
Improvements include:

#### **Input Validation**
- Validates all IDs are positive numbers
- Validates required fields in DTOs
- Throws `ArgumentException` for invalid inputs ? 400 Bad Request

#### **Authorization Handling**
- Added custom authorization logic for the `UpdateUser` endpoint
- Users can only update their own profile unless they are admins
- Throws `UnauthorizedException` when users lack proper permissions ? 403 Forbidden

#### **Try-Catch Blocks**
- All endpoints now have try-catch blocks
- Specific exception handling for different scenarios
- Generic exception logging for unexpected errors

#### **Consistent Logging**
- Logs all operations with appropriate levels (Information, Warning, Error)
- Includes contextual data in log messages

#### **Null Checks**
- Validates that resources exist before returning them
- Throws `NotFoundException` when resources don't exist ? 404 Not Found

### 5. **Updated Program.cs**
- Registered `AuthorizationExceptionFilter` globally for all controllers
- Added `GlobalExceptionHandlingMiddleware` to the request pipeline
- Middleware positioned after authentication/authorization but before routing

## Error Response Format

All error responses follow this consistent format:

```json
{
  "message": "Error description",
  "statusCode": 400
}
```

## HTTP Status Codes Used

| Status Code | Exception Type | Scenario |
|-------------|---|---|
| 400 | ArgumentException | Invalid input parameters or validation errors |
| 401 | InvalidCredentialsException, InvalidTokenException | No token or invalid token |
| 403 | UnauthorizedException | User lacks required role/permissions |
| 404 | NotFoundException | Resource not found |
| 409 | ConflictException | Resource already exists or conflict condition |
| 500 | Other exceptions | Unexpected server errors |

## Example Scenarios

### Scenario 1: Missing or Invalid Token
- **Request**: GET `/api/users/roles` without token
- **Response**: 401 Unauthorized
```json
{
  "message": "Authentication required. Please provide a valid JWT token.",
  "statusCode": 401
}
```

### Scenario 2: Invalid Role
- **Request**: GET `/api/users/roles` as non-Admin user
- **Response**: 403 Forbidden
```json
{
  "message": "You do not have the required role to access this resource.",
  "statusCode": 403
}
```

### Scenario 3: Invalid Input
- **Request**: GET `/api/users/users/-1`
- **Response**: 400 Bad Request
```json
{
  "message": "User ID must be greater than 0.",
  "statusCode": 400
}
```

### Scenario 4: Resource Not Found
- **Request**: GET `/api/users/users/9999`
- **Response**: 404 Not Found
```json
{
  "message": "User with ID 9999 not found.",
  "statusCode": 404
}
```

## Benefits

1. **Consistent Error Handling**: All errors follow the same response format
2. **Clear Authorization Messages**: Users know exactly why they're denied access
3. **Input Validation**: Invalid data is caught early with clear error messages
4. **Comprehensive Logging**: All errors are logged for debugging and monitoring
5. **Better User Experience**: Clients receive meaningful error messages and appropriate HTTP status codes
6. **Maintainability**: Centralized exception handling makes the codebase easier to maintain
7. **Security**: Proper authorization checks prevent unauthorized access

## Extensibility

The middleware can be easily extended to handle additional exception types by adding more `case` statements in the `HandleExceptionAsync` method. You can also customize error messages and logging behavior as needed.
