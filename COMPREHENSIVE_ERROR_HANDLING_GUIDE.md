# Comprehensive Error Handling Implementation

## Overview
Complete error handling has been implemented across the application with a focus on the `UsersController` and `ProductsController`. The implementation includes:

1. **Global Exception Handling Middleware** - Centralized error handling
2. **Authorization Exception Filter** - Proper authorization failure handling
3. **Custom Exception Classes** - Specific exceptions for different error scenarios
4. **Enhanced Controllers** - All endpoints with validation and error handling

---

## Architecture

```
???????????????????????????????????????????????
?         HTTP Request/Response                ?
???????????????????????????????????????????????
?      Global Exception Handling Middleware     ?
???????????????????????????????????????????????
?  Authorization Exception Filter              ?
???????????????????????????????????????????????
?   Controller Endpoints with Try-Catch        ?
???????????????????????????????????????????????
?         Service Layer / Database             ?
???????????????????????????????????????????????
```

---

## Components Implemented

### 1. Custom Exception Classes

**UnauthorizedException** (`Exceptions\UnauthorizedException.cs`)
- Thrown when user lacks required role/permissions
- HTTP Status: 403 Forbidden

**Existing Exception Classes**
- `InvalidCredentialsException` - HTTP Status: 401 Unauthorized
- `InvalidTokenException` - HTTP Status: 401 Unauthorized
- `NotFoundException` - HTTP Status: 404 Not Found
- `ConflictException` - HTTP Status: 409 Conflict

### 2. Global Exception Handling Middleware

**File**: `Middleware\GlobalExceptionHandlingMiddleware.cs`

Maps exceptions to appropriate HTTP status codes:
```
UnauthorizedException        ? 403 Forbidden
InvalidCredentialsException  ? 401 Unauthorized
InvalidTokenException        ? 401 Unauthorized
NotFoundException           ? 404 Not Found
ConflictException          ? 409 Conflict
ArgumentException          ? 400 Bad Request
Other Exceptions           ? 500 Internal Server Error
```

Features:
- Centralized error response format
- Structured logging with Serilog
- JSON response format for all errors
- Error messages sent to clients

### 3. Authorization Exception Filter

**File**: `Filters\AuthorizationExceptionFilter.cs`

Converts ASP.NET Core authorization failures into custom exceptions:
- Distinguishes between unauthenticated (401) and unauthorized (403)
- Provides clear error messages
- Integrated globally via Program.cs

### 4. Enhanced Controllers

#### UsersController
**File**: `Controllers\UsersController.cs`

Features:
- Input validation for all endpoints
- Authorization checks (users can only update their own profiles)
- Try-catch blocks with proper logging
- Specific exception handling for different scenarios

Endpoints Enhanced:
- `GET /api/users/roles` - List roles (Admin only)
- `GET /api/users/roles/{roleId}` - Get role (Admin only)
- `POST /api/users/roles/create` - Create role (Admin only)
- `PUT /api/users/roles/{roleId}` - Update role (Admin only)
- `DELETE /api/users/roles/{roleId}` - Delete role (Admin only)
- `GET /api/users/users` - List users (Admin only)
- `GET /api/users/users/{userId}` - Get user (Admin only)
- `POST /api/users/users/register` - Register user (Public)
- `PUT /api/users/users/{userId}` - Update user (Authenticated, with ownership check)
- `DELETE /api/users/users/{userId}` - Delete user (Admin only)

#### ProductsController
**File**: `Controllers\ProductsController.cs`

Features:
- Complete error handling for all product operations
- Validation of all input IDs and required fields
- Proper exception throwing with descriptive messages
- Comprehensive logging at all levels

Endpoints Enhanced:
- Component Management (Create, Read, Update, Delete)
- Component Category Management (Create, Read, Update, Delete)
- Component Specs Management (Create, Read, Update, Delete)
- PC Build Management (Create, Read, Update, Delete)

Public Endpoints (GET only):
- List components
- Get component details
- List categories
- Get category details
- List specs
- Get spec details
- List PC builds
- Get PC build details

Protected Endpoints (Create/Update/Delete):
- All create operations (Admin only)
- All update operations (Admin only)
- All delete operations (Admin only)

---

## Error Response Format

All error responses follow this format:

```json
{
  "message": "Error description",
  "statusCode": <HTTP_STATUS_CODE>
}
```

### Examples

**401 Unauthorized (No Token)**
```json
{
  "message": "Authentication required. Please provide a valid JWT token.",
  "statusCode": 401
}
```

**403 Forbidden (Insufficient Role)**
```json
{
  "message": "You do not have the required role to access this resource.",
  "statusCode": 403
}
```

**400 Bad Request (Invalid Input)**
```json
{
  "message": "Component name is required.",
  "statusCode": 400
}
```

**404 Not Found**
```json
{
  "message": "Component with ID 999 not found.",
  "statusCode": 404
}
```

**409 Conflict (Duplicate)**
```json
{
  "message": "A category with this name already exists.",
  "statusCode": 409
}
```

**500 Internal Server Error**
```json
{
  "message": "An unexpected error occurred. Please try again later.",
  "statusCode": 500
}
```

---

## Validation Rules

### ID Parameters
- Must be greater than 0
- Throws `ArgumentException` ? 400 Bad Request

### String Fields
- Must not be null
- Must not be empty
- Must not be whitespace only
- Throws `ArgumentException` ? 400 Bad Request

### DTO Objects
- Must not be null
- Throws `ArgumentException` ? 400 Bad Request

### Business Rules
- Resource existence checks throw `NotFoundException` ? 404 Not Found
- Duplicate entries throw `ConflictException` ? 409 Conflict

---

## Logging Implementation

Using Serilog for structured logging:

### Log Levels
- **Information** - Successful operations
- **Warning** - Business logic failures (not found, conflicts)
- **Error** - Exceptions with full stack traces

### Log Examples
```csharp
Log.Information("Component created successfully: {ComponentName}", name);
Log.Warning("Component not found while updating: {Message}", ex.Message);
Log.Error(ex, "Error updating component with ID {ComponentId}", id);
```

---

## Integration with Program.cs

Key configurations in `Program.cs`:

1. **Add Authorization Exception Filter**
   ```csharp
   builder.Services.AddControllers(options =>
   {
       options.Filters.Add<AuthorizationExceptionFilter>();
   });
   ```

2. **Add Global Exception Middleware**
   ```csharp
   app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
   ```

3. **Middleware Order**
   - Authentication
   - Authorization
   - Exception Handling
   - Routing

---

## Testing Scenarios

### Scenario 1: Missing Authorization Header
- **Request**: GET `/api/users/roles`
- **Response**: 401 Unauthorized
- **Message**: "Authentication required..."

### Scenario 2: Invalid Role
- **Request**: GET `/api/users/roles` as User role
- **Response**: 403 Forbidden
- **Message**: "You do not have the required role..."

### Scenario 3: Invalid ID
- **Request**: GET `/api/products/component/-1`
- **Response**: 400 Bad Request
- **Message**: "Component ID must be greater than 0."

### Scenario 4: Resource Not Found
- **Request**: GET `/api/products/component/9999`
- **Response**: 404 Not Found
- **Message**: "Component with ID 9999 not found."

### Scenario 5: Missing Required Field
- **Request**: POST `/api/products/component/create` with empty name
- **Response**: 400 Bad Request
- **Message**: "Component name is required."

### Scenario 6: Duplicate Entry
- **Request**: POST `/api/products/component/create` with existing name
- **Response**: 409 Conflict
- **Message**: "A component with this name already exists."

### Scenario 7: Update Own Profile (Authorized)
- **Request**: PUT `/api/users/users/123` as user with ID 123
- **Response**: 200 OK
- **Message**: "User updated successfully!"

### Scenario 8: Update Other's Profile (Unauthorized)
- **Request**: PUT `/api/users/users/456` as user with ID 123 (non-admin)
- **Response**: 403 Forbidden
- **Message**: "You can only update your own profile unless you are an admin."

---

## Benefits

? **Consistency** - All endpoints follow the same error handling pattern
? **Security** - Proper authorization and validation prevents unauthorized access
? **Debugging** - Comprehensive logging helps identify issues quickly
? **User Experience** - Clear, actionable error messages
? **Maintainability** - Centralized exception handling reduces code duplication
? **Scalability** - Easy to add new validation rules or exception types
? **Professional** - Follows REST API best practices
? **Production Ready** - Handles edge cases and unexpected errors gracefully

---

## Files Modified/Created

### Created
- `Exceptions\UnauthorizedException.cs`
- `Middleware\GlobalExceptionHandlingMiddleware.cs`
- `Filters\AuthorizationExceptionFilter.cs`
- `ERROR_HANDLING_IMPROVEMENTS.md` (UsersController documentation)
- `PRODUCTS_CONTROLLER_ERROR_HANDLING.md` (ProductsController documentation)

### Modified
- `Controllers\UsersController.cs` - Added error handling
- `Controllers\ProductsController.cs` - Added error handling
- `Program.cs` - Registered filters and middleware

---

## Next Steps

To apply the same error handling to other controllers:

1. Add try-catch blocks to all endpoint methods
2. Validate input parameters (IDs > 0, required strings not empty)
3. Use appropriate custom exceptions for different scenarios
4. Add comprehensive logging with Serilog
5. Document the error scenarios in the endpoint comments

The global middleware and filters will automatically handle all exceptions, so no changes needed to `Program.cs` for additional controllers.

---

## Support

For questions or issues:
1. Check the error message and HTTP status code
2. Review the relevant controller method
3. Check the application logs via Serilog
4. Ensure proper JWT token is included in Authorization header
5. Verify user role matches endpoint requirements
