# Serilog Configuration & Logging Guide

## Overview
The application now has comprehensive logging with Serilog configured to output to both console and file.

## Configuration Changes Made

### Program.cs Serilog Setup

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()                    // Capture DEBUG and above
    .WriteTo.Console(...)                    // Console output
    .WriteTo.File(...)                       // File output
    .Enrich.FromLogContext()                 // Add contextual data
    .Enrich.WithProperty(...)                // Add application metadata
    .CreateLogger();
```

## Key Features

### 1. **Minimum Level: Debug**
- Captures DEBUG, Information, Warning, Error, and Fatal logs
- Provides detailed execution flow visibility

### 2. **Console Output**
```
[2024-01-15 10:30:45.123 +00:00] [INF] User registered successfully: user@example.com
[2024-01-15 10:30:46.456 +00:00] [WRN] Component not found: Component with ID 999 not found
[2024-01-15 10:30:47.789 +00:00] [ERR] Error updating component with ID 5
```

**Output Template:**
```
[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

### 3. **File Logging**
- **Location:** `logs/log-<date>.txt`
- **Rolling Interval:** Daily
- **Minimum Level:** Information (reduces file size)
- **Format:** Same as console with full exception details

## Logging in Your Controllers and Services

### UsersController Example
```csharp
try
{
    if (roleId <= 0)
        throw new ArgumentException("Role ID must be greater than 0.");

    var role = await _userService.getRoleById(roleId);
    if (role == null)
        throw new NotFoundException($"Role with ID {roleId} not found.");

    Log.Information("Fetched role: {@role}", role);
    return Ok(role);
}
catch (Exception ex)
{
    Log.Error(ex, "Error fetching role with ID {RoleId}", roleId);
    throw;
}
```

### ProductService Example
```csharp
public async Task createComponent(createComponentDto createComponentDto)
{
    Log.Information("Creating new component: {ComponentName} in category {CategoryId}", 
        createComponentDto.Name, createComponentDto.CategoryId);

    var component = new Models.Component { /* ... */ };
    await _context.Components.AddAsync(component);
    await _context.SaveChangesAsync();
    
    Log.Information("Component created successfully: {ComponentId} - {ComponentName}", 
        component.ComponentId, component.Name);
}
```

## Log Levels & When to Use Them

| Level | Use Case | Example |
|-------|----------|---------|
| **Debug** | Detailed execution flow | Cache hits, query parameters |
| **Information** | Successful operations | "User logged in", "Component created" |
| **Warning** | Unexpected conditions | "Not found", "Duplicate entry" |
| **Error** | Operations that failed | Exceptions, validation failures |
| **Fatal** | Application-level failures | Startup errors, critical issues |

## What You'll See in Terminal

### Startup Logs
```
===============================================
Starting up PCShop Backend Service
===============================================
[2024-01-15 10:30:40.123 +00:00] [INF] Application built successfully
[2024-01-15 10:30:40.456 +00:00] [INF] Running in Development environment
===============================================
PCShop Backend Service Started Successfully
Listening for requests on configured endpoints...
===============================================
```

### Request Logs (from Controllers)
```
[2024-01-15 10:30:45.789 +00:00] [INF] Fetched component with ID 1
[2024-01-15 10:30:46.123 +00:00] [INF] Component created successfully: CPU-001
[2024-01-15 10:30:46.456 +00:00] [WRN] Component not found while updating: Component with ID 999 not found
```

### Error Logs (from Middleware)
```
[2024-01-15 10:30:47.789 +00:00] [ERR] Error fetching user with ID 123
System.InvalidOperationException: Invalid operation
   at PCShop_Backend.Service.UserService.GetUserById(Int32 id)
   at PCShop_Backend.Controllers.UsersController.<>c__DisplayClass1_0.<GetUserById>d__0.MoveNext()
```

### Authorization Logs (from Filters)
```
[2024-01-15 10:30:48.123 +00:00] [WRN] Authorization failed: You do not have the required role to access this resource.
[2024-01-15 10:30:48.456 +00:00] [WRN] Unauthorized update attempt for user 123: You can only update your own profile unless you are an admin.
```

## File Logging

### Log File Location
```
project_root/logs/log-2024-01-15.txt
project_root/logs/log-2024-01-16.txt
project_root/logs/log-2024-01-17.txt
```

### Sample Log File Content
```
[2024-01-15 10:30:40.123 +00:00] [INF] Application built successfully
[2024-01-15 10:30:40.456 +00:00] [INF] Running in Development environment
[2024-01-15 10:30:40.789 +00:00] [INF] PCShop Backend Service Started Successfully
[2024-01-15 10:30:45.123 +00:00] [INF] Fetched component with ID 1
[2024-01-15 10:30:46.456 +00:00] [ERR] Error updating component with ID 5
System.InvalidOperationException: Component not found
   at PCShop_Backend.Service.ProductService.updateComponent(Int32 id, updateComponentDto dto)
```

## Troubleshooting

### 1. No Logs Appearing in Console?
- **Check:** Is the application running in Development mode?
- **Solution:** Ensure `app.Environment.IsDevelopment()` is true
- **Alternative:** Check if Serilog is properly configured with `builder.Host.UseSerilog()`

### 2. Logs Appearing in File But Not Console?
- **Check:** File logging minimum level vs console level
- **Solution:** Console has `MinimumLevel.Debug()`, file has `MinimumLevel.Information()`
- **Increase Verbosity:** Change file level to `.Information()` to see more

### 3. Too Many Logs in Console?
- **Solution:** Increase MinimumLevel to `Information()` to hide Debug logs
- **Code:**
  ```csharp
  .MinimumLevel.Information()  // Instead of Debug
  ```

### 4. Logs Not Persisting to File?
- **Check:** That `logs/` folder has write permissions
- **Check:** That the file path is correct: `"logs/log-.txt"`
- **Solution:** Ensure the application has permission to create files in the root directory

## Best Practices

### ? DO

1. **Log at appropriate levels:**
   ```csharp
   Log.Information("User {UserId} logged in successfully", userId);
   Log.Warning("Duplicate entry attempted: {EntityName}", entityName);
   Log.Error(ex, "Database operation failed for {Operation}", operationName);
   ```

2. **Include contextual data:**
   ```csharp
   Log.Information("Component created: {ComponentId} - {ComponentName} in category {CategoryId}", 
       componentId, componentName, categoryId);
   ```

3. **Log exceptions with context:**
   ```csharp
   catch (Exception ex)
   {
       Log.Error(ex, "Error processing component {ComponentId}", id);
       throw;
   }
   ```

### ? DON'T

1. **Don't log sensitive information:**
   ```csharp
   // BAD
   Log.Information("User password: {Password}", password);
   
   // GOOD
   Log.Information("User {Username} authenticated successfully", username);
   ```

2. **Don't create noise with excessive logging:**
   ```csharp
   // BAD - Too verbose
   Log.Debug("Starting loop iteration {Index}", i);
   
   // GOOD - Log only important operations
   Log.Information("Processing batch of {Count} items", itemCount);
   ```

3. **Don't swallow exceptions:**
   ```csharp
   // BAD
   try { /* code */ } catch { }
   
   // GOOD
   try { /* code */ } 
   catch (Exception ex) 
   { 
       Log.Error(ex, "Operation failed");
       throw;
   }
   ```

## Integration Points

### Program.cs
- Startup and shutdown logging
- Configuration logging
- Middleware registration logging

### Controllers
- Request handling logging
- Authorization/authentication logging
- Validation failure logging

### Services
- Database operation logging
- Business logic logging
- Cache operation logging

### Middleware
- Global exception logging
- Request/response logging
- Performance metrics

### Filters
- Authorization result logging
- Filter execution logging

## Performance Considerations

1. **Structured Logging** - Uses placeholders instead of string interpolation
   ```csharp
   // GOOD - Structured
   Log.Information("User {UserId} performed {Action}", userId, action);
   
   // LESS GOOD - String interpolation
   Log.Information($"User {userId} performed {action}");
   ```

2. **Log Level Performance**
   - Debug: Most verbose, slight performance impact
   - Information: Balanced approach
   - Warning+: Minimal performance impact

3. **File Rollover** - Automatic daily rollover prevents huge log files

## Summary

? **Logging is now fully configured and working:**
- Console output with detailed timestamps
- File logging with daily rotation
- Integration across all layers (Controllers, Services, Middleware, Filters)
- Proper exception handling with stack traces
- Structured logging for better analysis

**To see logs in action:**
1. Run the application
2. Make API requests
3. Watch the console for detailed logs
4. Check `logs/` folder for persistent file logs

Your error handling and logging implementation is now production-ready! ??
