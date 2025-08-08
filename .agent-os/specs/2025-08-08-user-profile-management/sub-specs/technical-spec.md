# Technical Specification - User Profile Management

> Spec: User Profile Management
> Created: 2025-08-08
> Status: Planning

## Architecture Overview

The user profile management system will extend the existing CQRS/Event Sourcing architecture with new commands, events, and UI components for comprehensive profile editing.

## Backend Components

### Commands

```csharp
// Update basic profile information
public class UpdateUserProfile
{
    public string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? Address { get; set; }
}

// Update user preferences
public class UpdateUserPreferences  
{
    public string EmailAddress { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public string? Theme { get; set; }
}

// Upload profile picture
public class UploadProfilePicture
{
    public string EmailAddress { get; set; }
    public IFormFile ImageFile { get; set; }
}
```

### Events

```csharp
public class UserProfileUpdated
{
    public string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? Address { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserPreferencesUpdated
{
    public string EmailAddress { get; set; }
    public UserPreferences Preferences { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProfilePictureUpdated
{
    public string EmailAddress { get; set; }
    public string ProfilePictureUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### API Endpoints

```csharp
// UserController.cs
[HttpPut("profile")]
[Authorize]
public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfile command)

[HttpPut("preferences")]
[Authorize] 
public async Task<IActionResult> UpdatePreferences([FromBody] UpdateUserPreferences command)

[HttpPost("profile-picture")]
[Authorize]
public async Task<IActionResult> UploadProfilePicture([FromForm] UploadProfilePicture command)

[HttpGet("profile-picture/{emailAddress}")]
public async Task<IActionResult> GetProfilePicture(string emailAddress)
```

### Services

```csharp
public interface IImageUploadService
{
    Task<string> UploadImageAsync(IFormFile file, string userId);
    Task DeleteImageAsync(string imageUrl);
    Task<bool> ValidateImageAsync(IFormFile file);
}

public class ImageUploadService : IImageUploadService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageUploadService> _logger;
    
    // Implementation with file validation, resizing, and storage
}
```

## Frontend Components

### Profile Edit Page

```razor
@page "/profile/edit"
@using MudBlazor
@inject IUserService UserService
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.Medium">
    <MudPaper Class="pa-6">
        <MudText Typo="Typo.h4" Class="mb-4">Edit Profile</MudText>
        
        <EditForm Model="@profileModel" OnValidSubmit="@SaveProfile">
            <DataAnnotationsValidator />
            
            <!-- Profile Picture Section -->
            <ProfilePictureUpload @bind-ImageUrl="@profileModel.ProfilePictureUrl" />
            
            <!-- Basic Information -->
            <MudTextField @bind-Value="profileModel.FirstName" 
                         Label="First Name" 
                         Variant="Variant.Outlined" />
            
            <MudTextField @bind-Value="profileModel.LastName" 
                         Label="Last Name" 
                         Variant="Variant.Outlined" />
            
            <!-- Address Component -->
            <AddressEdit @bind-Address="@profileModel.Address" />
            
            <!-- Preferences Component -->
            <UserPreferencesEdit @bind-Preferences="@profileModel.Preferences" />
            
            <MudButton ButtonType="ButtonType.Submit" 
                      Variant="Variant.Filled" 
                      Color="Color.Primary">
                Save Changes
            </MudButton>
        </EditForm>
    </MudPaper>
</MudContainer>
```

### Profile Picture Upload Component

```razor
<MudCard Class="mb-4">
    <MudCardContent>
        <MudText Typo="Typo.h6">Profile Picture</MudText>
        
        <div class="d-flex align-center gap-4">
            <MudAvatar Size="Size.Large" Class="ma-2">
                @if (!string.IsNullOrEmpty(ImageUrl))
                {
                    <MudImage Src="@ImageUrl" Alt="Profile Picture" />
                }
                else
                {
                    <MudIcon Icon="@Icons.Material.Filled.Person" />
                }
            </MudAvatar>
            
            <div>
                <MudFileUpload T="IBrowserFile" 
                              Accept=".jpg,.jpeg,.png"
                              MaxFileSize="5242880"
                              OnFilesChanged="@OnFileSelected">
                    <ButtonTemplate>
                        <MudButton HtmlTag="label"
                                  Variant="Variant.Outlined"
                                  Color="Color.Primary"
                                  for="@context">
                            Choose Image
                        </MudButton>
                    </ButtonTemplate>
                </MudFileUpload>
                
                <MudText Typo="Typo.caption" Class="mt-2">
                    Max size: 5MB. Formats: JPG, PNG
                </MudText>
            </div>
        </div>
    </MudCardContent>
</MudCard>
```

## Data Validation

### Server-Side Validation

```csharp
public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfile>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.FirstName));
            
        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LastName));
            
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
```

### Client-Side Validation

```csharp
public class ProfileEditModel
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = "";
    
    [MaxLength(50)]
    public string? FirstName { get; set; }
    
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public Address? Address { get; set; }
    public UserPreferences? Preferences { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
```

## Security Considerations

### Authorization

- Users can only edit their own profiles
- Profile picture endpoints require authentication
- File upload validation prevents malicious files
- Rate limiting on upload endpoints

### File Upload Security

```csharp
public class ImageUploadService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    
    public async Task<bool> ValidateImageAsync(IFormFile file)
    {
        // Check file size
        if (file.Length > _maxFileSize)
            return false;
            
        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;
            
        // Validate file content (magic bytes)
        using var stream = file.OpenReadStream();
        var buffer = new byte[8];
        await stream.ReadAsync(buffer, 0, 8);
        
        return IsValidImageHeader(buffer);
    }
}
```

## Database Schema Updates

No schema changes required - existing User and UserPreferences models support all needed fields.

## Performance Considerations

- Image uploads will be processed asynchronously
- Profile pictures will be cached with appropriate headers
- Large images will be resized to optimize storage and loading
- Optimistic updates for better user experience

## Error Handling

- Comprehensive validation error messages
- Graceful handling of file upload failures
- Rollback mechanisms for failed operations
- User-friendly error notifications in UI