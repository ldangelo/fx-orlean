using FxExpert.Blazor.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Shouldly;

namespace FxExpert.Blazor.Client.Tests.Services;

public class ThemeServiceTests
{
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<IUserThemeService> _mockUserThemeService;
    private readonly Mock<ILogger<ThemeService>> _mockLogger;
    private readonly ThemeService _themeService;

    public ThemeServiceTests()
    {
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockUserThemeService = new Mock<IUserThemeService>();
        _mockLogger = new Mock<ILogger<ThemeService>>();
        _themeService = new ThemeService(_mockJSRuntime.Object, _mockUserThemeService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WhenNoStoredTheme_ReturnsLight()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getStoredTheme", It.IsAny<object[]>()))
                     .ReturnsAsync((string?)null);

        // Act
        var result = await _themeService.GetCurrentThemeAsync();

        // Assert
        result.ShouldBe(ThemeMode.Light);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WhenDarkThemeStored_ReturnsDark()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getStoredTheme", It.IsAny<object[]>()))
                     .ReturnsAsync("Dark");

        // Act
        var result = await _themeService.GetCurrentThemeAsync();

        // Assert
        result.ShouldBe(ThemeMode.Dark);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WhenInvalidThemeStored_ReturnsLight()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getStoredTheme", It.IsAny<object[]>()))
                     .ReturnsAsync("InvalidTheme");

        // Act
        var result = await _themeService.GetCurrentThemeAsync();

        // Assert
        result.ShouldBe(ThemeMode.Light);
    }

    [Fact]
    public async Task SetThemeAsync_WhenCalled_UpdatesLocalStorage()
    {
        // Arrange
        var theme = ThemeMode.Dark;
        _mockJSRuntime.Setup(x => x.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", It.IsAny<object[]>()))
                     .ReturnsAsync(true);

        // Act
        await _themeService.SetThemeAsync(theme);

        // Assert
        _mockJSRuntime.Verify(x => x.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", 
            It.Is<object[]>(args => args[0].ToString() == "Dark")), 
            Times.Once);
    }

    [Fact]
    public async Task SetThemeAsync_WhenCalled_FiresThemeChangedEvent()
    {
        // Arrange
        var theme = ThemeMode.Dark;
        var eventFired = false;
        ThemeMode? eventTheme = null;

        _themeService.ThemeChanged += (sender, newTheme) =>
        {
            eventFired = true;
            eventTheme = newTheme;
        };

        // Act
        await _themeService.SetThemeAsync(theme);

        // Assert
        eventFired.ShouldBeTrue();
        eventTheme.ShouldBe(ThemeMode.Dark);
    }

    [Theory]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    [InlineData(ThemeMode.System)]
    public async Task SetThemeAsync_ForAllThemeModes_StoresCorrectValue(ThemeMode theme)
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", It.IsAny<object[]>()))
                     .ReturnsAsync(true);

        // Act
        await _themeService.SetThemeAsync(theme);

        // Assert
        _mockJSRuntime.Verify(x => x.InvokeAsync<bool>("ThemeHelpers.setStoredTheme", 
            It.Is<object[]>(args => args[0].ToString() == theme.ToString())), 
            Times.Once);
    }

    [Fact]
    public void ThemeMode_Enum_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ThemeMode>().ShouldContain(ThemeMode.Light);
        Enum.GetValues<ThemeMode>().ShouldContain(ThemeMode.Dark);
        Enum.GetValues<ThemeMode>().ShouldContain(ThemeMode.System);
        Enum.GetValues<ThemeMode>().Length.ShouldBe(3);
    }

    [Fact]
    public async Task GetSystemThemeAsync_WhenCalled_ReturnsSystemTheme()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getSystemTheme", It.IsAny<object[]>()))
                     .ReturnsAsync("Dark");

        // Act
        var result = await _themeService.GetSystemThemeAsync();

        // Assert
        result.ShouldBe("Dark");
    }

    [Fact]
    public async Task GetEffectiveThemeAsync_WhenSystemMode_ReturnsSystemTheme()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getEffectiveTheme", It.IsAny<object[]>()))
                     .ReturnsAsync("Dark");

        // Act
        var result = await _themeService.GetEffectiveThemeAsync(ThemeMode.System);

        // Assert
        result.ShouldBe("Dark");
    }

    [Fact]
    public async Task GetEffectiveThemeAsync_WhenDarkMode_ReturnsDark()
    {
        // Arrange
        _mockJSRuntime.Setup(x => x.InvokeAsync<string>("ThemeHelpers.getEffectiveTheme", It.IsAny<object[]>()))
                     .ReturnsAsync("Dark");

        // Act
        var result = await _themeService.GetEffectiveThemeAsync(ThemeMode.Dark);

        // Assert
        result.ShouldBe("Dark");
    }

    [Fact]
    public async Task ApplyThemeAsync_WhenCalled_DoesNotThrow()
    {
        // This test verifies that ApplyThemeAsync doesn't throw exceptions
        // The actual JavaScript invocation is tested through integration tests
        
        // Act & Assert
        await Should.NotThrowAsync(async () => await _themeService.ApplyThemeAsync(ThemeMode.Dark));
    }

    [Fact]
    public async Task LoadUserThemeAsync_WhenUserEmailIsNull_DoesNotCallUserThemeService()
    {
        // Act
        await _themeService.LoadUserThemeAsync(null);

        // Assert
        _mockUserThemeService.Verify(x => x.GetUserThemeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoadUserThemeAsync_WhenUserEmailProvided_CallsUserThemeService()
    {
        // Arrange
        const string userEmail = "test@example.com";
        _mockUserThemeService.Setup(x => x.GetUserThemeAsync(userEmail))
                            .ReturnsAsync("Dark");

        // Act
        await _themeService.LoadUserThemeAsync(userEmail);

        // Assert
        _mockUserThemeService.Verify(x => x.GetUserThemeAsync(userEmail), Times.Once);
    }

    [Fact]
    public async Task SaveUserThemeAsync_WhenUserEmailIsNull_DoesNotCallUserThemeService()
    {
        // Act
        await _themeService.SaveUserThemeAsync(null, ThemeMode.Dark);

        // Assert
        _mockUserThemeService.Verify(x => x.SetUserThemeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveUserThemeAsync_WhenUserEmailProvided_CallsUserThemeService()
    {
        // Arrange
        const string userEmail = "test@example.com";
        const ThemeMode theme = ThemeMode.Dark;

        // Act
        await _themeService.SaveUserThemeAsync(userEmail, theme);

        // Assert
        _mockUserThemeService.Verify(x => x.SetUserThemeAsync(userEmail, "Dark"), Times.Once);
    }
}