# UserProfile Forms Troubleshooting Guide

## Enhanced Error Visibility

The UserProfile forms now have comprehensive error logging and display. Here's how to troubleshoot form save issues:

## 1. Check Browser Console

Open your browser's Developer Tools (F12) and look at the **Console** tab. You should now see detailed logs like:

```
🚀 Starting profile save for user: user@example.com
   Profile data: FirstName='John', LastName='Doe'
🔍 Checking if user exists: user@example.com
📥 Loading user data for: user@example.com
   Base URL: http://localhost:8080/
   Full URL: http://localhost:8080/users/user@example.com
   Response status: NotFound
👤 User doesn't exist, creating...
🆕 Creating new user: user@example.com
   Request data: {"EmailAddress":"user@example.com","FirstName":"John","LastName":"Doe"}
   Response status: Created
✅ User creation successful
🔄 Updating user profile for: user@example.com
   Request data: {"EmailAddress":"user@example.com","FirstName":"John","LastName":"Doe","PhoneNumber":"555-1234","ProfilePictureUrl":""}
   Response status: OK
✅ Profile update successful
```

## 2. Common Error Scenarios

### Error: "ERR_CONNECTION_REFUSED" or "Failed to fetch"
**Problem**: EventServer is not running
**Solution**: Start the EventServer:
```bash
cd src/EventServer
dotnet run
# OR
dotnet watch
```
The EventServer should be running on http://localhost:8080

### Error: "HTTP 404: Not Found"
**Problem**: User doesn't exist in the system
**Solution**: The forms now automatically create users, but if this fails:
1. Check EventServer logs for validation errors
2. Ensure email address is valid
3. Check that EventServer database is accessible

### Error: "HTTP 400: Bad Request" with validation errors
**Problem**: Data validation failed on the server
**Solution**: Check the detailed error message in the console for specific validation issues

### Error: "HTTP 500: Internal Server Error"
**Problem**: Server-side error
**Solution**: 
1. Check EventServer logs for detailed error information
2. Verify database connection is working
3. Check that all required services are running

## 3. System Requirements

### For UserProfile forms to work, you need:

1. **EventServer running** on http://localhost:8080
2. **PostgreSQL database** accessible to EventServer
3. **User authentication** working (for userEmail to be populated)

### Start the full system:
```bash
# Terminal 1: Start EventServer
cd src/EventServer
dotnet watch

# Terminal 2: Start Blazor app  
cd src/FxExpert.Blazor/FxExpert.Blazor
dotnet watch
```

## 4. Testing the Forms

1. Navigate to `/profile` in your Blazor app
2. Open browser Developer Tools (F12) → Console tab
3. Fill out a form and click "Save Changes"
4. Watch the console for detailed logs
5. Check the notification snackbar for user-friendly messages

## 5. Manual User Creation

If automatic user creation fails, you can manually create a user:

```bash
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{
    "EmailAddress": "your-email@example.com",
    "FirstName": "Your",
    "LastName": "Name"
  }'
```

## 6. API Endpoints Being Used

The UserProfile forms make these API calls:
- `GET /users/{email}` - Get user profile
- `POST /users` - Create user (auto-created if missing)
- `POST /users/profile/{email}` - Update profile
- `POST /users/address/{email}` - Update address
- `POST /users/preferences/{email}` - Update preferences

All endpoints expect the EventServer to be running on http://localhost:8080

## 7. Next Steps

If you're still experiencing issues after checking the above:
1. Share the console logs from the browser
2. Share any EventServer logs/errors
3. Confirm which services are running and on which ports