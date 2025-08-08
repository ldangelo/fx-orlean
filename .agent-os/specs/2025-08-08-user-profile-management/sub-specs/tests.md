# Test Specification - User Profile Management

> Spec: User Profile Management
> Created: 2025-08-08
> Status: Planning

## Test Strategy

Comprehensive testing approach covering unit tests, integration tests, and end-to-end scenarios for user profile management functionality.

## Unit Tests

### Command Handler Tests

```csharp
public class UpdateUserProfileHandlerTests
{
    [Fact]
    public async Task Handle_ValidProfile_UpdatesUserSuccessfully()
    {
        // Arrange
        var command = new UpdateUserProfile
        {
            EmailAddress = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890"
        };
        
        // Act & Assert
        var result = await handler.Handle(command);
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_InvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateUserProfile
        {
            EmailAddress = "invalid-email",
            FirstName = "John"
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command));
    }
    
    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedException()
    {
        // Test that users cannot update other users' profiles
    }
}

public class UpdateUserPreferencesHandlerTests
{
    [Fact]
    public async Task Handle_ValidPreferences_UpdatesSuccessfully()
    {
        // Test preferences update
    }
    
    [Theory]
    [InlineData("InvalidTheme")]
    [InlineData("")]
    public async Task Handle_InvalidTheme_ThrowsValidationException(string theme)
    {
        // Test theme validation
    }
    
    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("UTC")]
    public async Task Handle_ValidTimezone_UpdatesSuccessfully(string timezone)
    {
        // Test timezone validation
    }
}
```

### Image Upload Service Tests

```csharp
public class ImageUploadServiceTests
{
    [Fact]
    public async Task UploadImageAsync_ValidImage_ReturnsImageUrl()
    {
        // Arrange
        var mockFile = CreateMockImageFile("test.jpg", "image/jpeg");
        
        // Act
        var result = await service.UploadImageAsync(mockFile, "user123");
        
        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("https://");
    }
    
    [Fact]
    public async Task ValidateImageAsync_ValidJpeg_ReturnsTrue()
    {
        // Test JPEG validation
    }
    
    [Fact]
    public async Task ValidateImageAsync_InvalidFileType_ReturnsFalse()
    {
        // Test file type validation
    }
    
    [Fact]
    public async Task ValidateImageAsync_FileTooLarge_ReturnsFalse()
    {
        // Test file size validation
    }
    
    [Fact]
    public async Task ValidateImageAsync_MaliciousFile_ReturnsFalse()
    {
        // Test security validation
    }
}
```

### Validation Tests

```csharp
public class ProfileValidationTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    public void EmailValidation_VariousInputs_ReturnsExpectedResult(string email, bool expected)
    {
        // Test email validation
    }
    
    [Theory]
    [InlineData("+1234567890", true)]
    [InlineData("123-456-7890", true)]
    [InlineData("invalid-phone", false)]
    public void PhoneValidation_VariousInputs_ReturnsExpectedResult(string phone, bool expected)
    {
        // Test phone validation
    }
    
    [Theory]
    [InlineData("John", true)]
    [InlineData("", true)] // Optional field
    [InlineData(new string('a', 51), false)] // Too long
    public void NameValidation_VariousInputs_ReturnsExpectedResult(string name, bool expected)
    {
        // Test name validation
    }
}
```

## Integration Tests

### API Endpoint Tests

```csharp
public class UserProfileApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UpdateProfile_AuthenticatedUser_ReturnsSuccess()
    {
        // Arrange
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "test@example.com");
        
        var updateRequest = new UpdateUserProfile
        {
            EmailAddress = "test@example.com",
            FirstName = "Updated",
            LastName = "Name"
        };
        
        // Act
        var response = await client.PutAsJsonAsync("/api/users/profile", updateRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task UpdateProfile_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Test unauthorized access
    }
    
    [Fact]
    public async Task UpdateProfile_DifferentUser_ReturnsForbidden()
    {
        // Test that users cannot update other profiles
    }
    
    [Fact]
    public async Task UploadProfilePicture_ValidImage_ReturnsSuccess()
    {
        // Test image upload endpoint
    }
    
    [Fact]
    public async Task UploadProfilePicture_InvalidImage_ReturnsBadRequest()
    {
        // Test invalid image upload
    }
}
```

### Database Integration Tests

```csharp
public class UserProfilePersistenceTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task UpdateProfile_ValidData_PersistsToDatabase()
    {
        // Test that profile updates are saved correctly
    }
    
    [Fact]
    public async Task UpdatePreferences_ValidData_PersistsToDatabase()
    {
        // Test that preferences are saved correctly
    }
    
    [Fact]
    public async Task ProfilePictureUpdate_ValidUrl_UpdatesUserRecord()
    {
        // Test profile picture URL persistence
    }
}
```

## Component Tests (Blazor)

### Profile Edit Component Tests

```csharp
public class ProfileEditComponentTests : TestContext
{
    [Fact]
    public void ProfileEdit_RendersCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        Services.AddSingleton<IUserService>(Mock.Of<IUserService>());
        
        // Act
        var component = RenderComponent<ProfileEdit>();
        
        // Assert
        component.Find("form").Should().NotBeNull();
        component.Find("input[name='FirstName']").Should().NotBeNull();
    }
    
    [Fact]
    public void ProfileEdit_SubmitValidForm_CallsUpdateService()
    {
        // Test form submission
    }
    
    [Fact]
    public void ProfileEdit_InvalidData_ShowsValidationErrors()
    {
        // Test validation error display
    }
}

public class ProfilePictureUploadTests : TestContext
{
    [Fact]
    public void ProfilePictureUpload_ValidImage_ShowsPreview()
    {
        // Test image preview functionality
    }
    
    [Fact]
    public void ProfilePictureUpload_InvalidImage_ShowsError()
    {
        // Test error handling for invalid images
    }
}
```

## End-to-End Tests

### User Profile Management Flow

```csharp
[Test]
public async Task CompleteProfileUpdateFlow_Success()
{
    // Navigate to profile page
    await Page.GotoAsync("/profile/edit");
    
    // Update basic information
    await Page.FillAsync("[data-testid='first-name']", "John");
    await Page.FillAsync("[data-testid='last-name']", "Doe");
    await Page.FillAsync("[data-testid='phone']", "+1234567890");
    
    // Upload profile picture
    await Page.SetInputFilesAsync("[data-testid='profile-picture-upload']", "test-image.jpg");
    
    // Update preferences
    await Page.CheckAsync("[data-testid='email-notifications']");
    await Page.SelectOptionAsync("[data-testid='theme-select']", "Dark");
    
    // Submit form
    await Page.ClickAsync("[data-testid='save-profile']");
    
    // Verify success
    await Expect(Page.Locator("[data-testid='success-message']")).ToBeVisibleAsync();
    
    // Verify data persistence
    await Page.ReloadAsync();
    await Expect(Page.Locator("[data-testid='first-name']")).ToHaveValueAsync("John");
}

[Test]
public async Task ProfilePictureUpload_InvalidFile_ShowsError()
{
    await Page.GotoAsync("/profile/edit");
    
    // Try to upload invalid file
    await Page.SetInputFilesAsync("[data-testid='profile-picture-upload']", "invalid-file.txt");
    
    // Verify error message
    await Expect(Page.Locator("[data-testid='upload-error']")).ToBeVisibleAsync();
}

[Test]
public async Task ThemeChange_AppliesImmediately()
{
    await Page.GotoAsync("/profile/edit");
    
    // Change theme to dark
    await Page.SelectOptionAsync("[data-testid='theme-select']", "Dark");
    await Page.ClickAsync("[data-testid='save-profile']");
    
    // Verify theme is applied
    await Expect(Page.Locator("body")).ToHaveClassAsync(/.*dark-theme.*/);
}
```

## Performance Tests

### Image Upload Performance

```csharp
[Test]
public async Task ImageUpload_LargeFile_CompletesWithinTimeout()
{
    var stopwatch = Stopwatch.StartNew();
    
    // Upload 4MB image
    await UploadImageAsync("large-image-4mb.jpg");
    
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // 10 seconds
}

[Test]
public async Task ProfileUpdate_ConcurrentUsers_HandlesCorrectly()
{
    // Test concurrent profile updates
    var tasks = Enumerable.Range(0, 10)
        .Select(i => UpdateProfileAsync($"user{i}@example.com"))
        .ToArray();
        
    await Task.WhenAll(tasks);
    
    // Verify all updates succeeded
    tasks.All(t => t.Result.IsSuccess).Should().BeTrue();
}
```

## Security Tests

### Authorization Tests

```csharp
[Test]
public async Task UpdateProfile_DifferentUser_Forbidden()
{
    // Login as user1
    await AuthenticateAsync("user1@example.com");
    
    // Try to update user2's profile
    var response = await UpdateProfileAsync("user2@example.com", new UpdateUserProfile());
    
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}

[Test]
public async Task UploadProfilePicture_MaliciousFile_Rejected()
{
    // Try to upload executable file disguised as image
    var response = await UploadFileAsync("malicious.exe.jpg");
    
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

## Test Data Setup

### Test User Factory

```csharp
public static class TestUserFactory
{
    public static User CreateTestUser(string email = "test@example.com")
    {
        return new User
        {
            EmailAddress = email,
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Active = true,
            Preferences = new UserPreferences
            {
                Theme = "Light",
                TimeZone = "UTC",
                ReceiveEmailNotifications = true
            }
        };
    }
    
    public static IFormFile CreateMockImageFile(string fileName, string contentType)
    {
        var content = "Mock image content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
```

## Test Coverage Goals

- **Unit Tests:** 90%+ code coverage for all handlers and services
- **Integration Tests:** All API endpoints and database operations
- **Component Tests:** All UI components and user interactions
- **E2E Tests:** Complete user workflows and edge cases
- **Security Tests:** Authorization and file upload security
- **Performance Tests:** Upload timeouts and concurrent operations