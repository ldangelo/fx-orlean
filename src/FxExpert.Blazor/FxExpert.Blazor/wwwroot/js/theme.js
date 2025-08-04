// Theme management JavaScript helpers for FX-Orleans
window.ThemeHelpers = {
    // Storage key for theme preference
    STORAGE_KEY: 'theme',

    // Get stored theme preference
    getStoredTheme: function() {
        try {
            return localStorage.getItem(this.STORAGE_KEY);
        } catch (e) {
            console.warn('Failed to access localStorage for theme:', e);
            return null;
        }
    },

    // Set theme preference in storage
    setStoredTheme: function(theme) {
        try {
            localStorage.setItem(this.STORAGE_KEY, theme);
            return true;
        } catch (e) {
            console.warn('Failed to store theme preference:', e);
            return false;
        }
    },

    // Detect system theme preference
    getSystemTheme: function() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'Dark';
        }
        return 'Light';
    },

    // Get effective theme (resolves System to actual theme)
    getEffectiveTheme: function(themeMode) {
        if (themeMode === 'System') {
            return this.getSystemTheme();
        }
        return themeMode;
    },

    // Apply theme to document
    applyTheme: function(theme, mudThemeProvider) {
        const effectiveTheme = this.getEffectiveTheme(theme);
        const isDark = effectiveTheme === 'Dark';
        
        // Update document class for CSS-based theme switching
        document.documentElement.setAttribute('data-theme', effectiveTheme.toLowerCase());
        
        // Update MudBlazor theme if provider is available
        if (mudThemeProvider && mudThemeProvider.SetDarkMode) {
            mudThemeProvider.SetDarkMode(isDark);
        }
        
        // Dispatch custom event for theme change
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: effectiveTheme, isDark: isDark }
        }));
    },

    // Listen for system theme changes
    watchSystemTheme: function(dotNetObjectRef) {
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            // Callback function that notifies .NET
            const callback = function(e) {
                try {
                    dotNetObjectRef.invokeMethodAsync('OnSystemThemeChanged', e.matches);
                } catch (error) {
                    console.warn('Failed to notify .NET of system theme change:', error);
                }
            };
            
            // Modern browsers
            if (mediaQuery.addEventListener) {
                mediaQuery.addEventListener('change', callback);
            } else {
                // Fallback for older browsers
                mediaQuery.addListener(callback);
            }
            
            // Return object with cleanup function
            return {
                stop: function() {
                    if (mediaQuery.removeEventListener) {
                        mediaQuery.removeEventListener('change', callback);
                    } else {
                        mediaQuery.removeListener(callback);
                    }
                }
            };
        }
        
        // Return object with no-op function if matchMedia not supported
        return {
            stop: function() {}
        };
    },

    // Initialize theme on page load
    initializeTheme: function(mudThemeProvider) {
        const storedTheme = this.getStoredTheme() || 'Light';
        this.applyTheme(storedTheme, mudThemeProvider);
        return storedTheme;
    },

    // CSS class utilities for manual theme switching
    addThemeClass: function(isDark) {
        const body = document.body;
        if (isDark) {
            body.classList.add('mud-theme-dark');
            body.classList.remove('mud-theme-light');
        } else {
            body.classList.add('mud-theme-light');
            body.classList.remove('mud-theme-dark');
        }
    },

    // Utility to check if dark mode is supported
    isDarkModeSupported: function() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').media !== 'not all';
    }
};

// Authentication helpers for logout functionality
window.AuthHelpers = {
    // Submit logout form via POST request
    submitLogoutForm: function(returnUrl) {
        try {
            // Create a hidden form for logout
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/auth/logout';
            form.style.display = 'none';

            // Add return URL if provided
            if (returnUrl) {
                const returnUrlInput = document.createElement('input');
                returnUrlInput.type = 'hidden';
                returnUrlInput.name = 'returnUrl';
                returnUrlInput.value = returnUrl;
                form.appendChild(returnUrlInput);
            }

            // Add CSRF token if available
            const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
            if (csrfToken) {
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = '__RequestVerificationToken';
                tokenInput.value = csrfToken.value;
                form.appendChild(tokenInput);
            }

            // Add form to document, submit, and remove
            document.body.appendChild(form);
            form.submit();
            document.body.removeChild(form);
            
            return true;
        } catch (error) {
            console.error('Failed to submit logout form:', error);
            return false;
        }
    },

    // Alternative logout method using fetch API
    logoutWithFetch: async function(returnUrl) {
        try {
            const formData = new FormData();
            if (returnUrl) {
                formData.append('returnUrl', returnUrl);
            }

            // Add CSRF token if available
            const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
            if (csrfToken) {
                formData.append('__RequestVerificationToken', csrfToken.value);
            }

            const response = await fetch('/auth/logout', {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            });

            if (response.ok) {
                // Redirect to return URL or home page
                const redirectUrl = returnUrl || '/';
                window.location.href = redirectUrl;
                return true;
            } else {
                console.error('Logout request failed:', response.status, response.statusText);
                return false;
            }
        } catch (error) {
            console.error('Failed to logout with fetch:', error);
            return false;
        }
    }
};

// Auto-initialize theme helpers when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.ThemeHelpers.initializeTheme();
    });
} else {
    window.ThemeHelpers.initializeTheme();
}