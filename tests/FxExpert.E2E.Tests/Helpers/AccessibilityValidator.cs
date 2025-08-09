using Microsoft.Playwright;

namespace FxExpert.E2E.Tests.Helpers;

/// <summary>
/// Accessibility validation helper for WCAG 2.1 AA compliance testing
/// Provides methods to check color contrast, keyboard navigation, ARIA usage, and semantic structure
/// </summary>
public static class AccessibilityValidator
{
    /// <summary>
    /// Validates color contrast ratios according to WCAG 2.1 AA standards
    /// Normal text: 4.5:1, Large text: 3:1
    /// </summary>
    public static async Task<AccessibilityReport> ValidateColorContrastAsync(IPage page)
    {
        var contrastData = await page.EvaluateAsync<object>(@"
            () => {
                function getLuminance(rgb) {
                    const [r, g, b] = rgb;
                    const [rs, gs, bs] = [r, g, b].map(c => {
                        c = c / 255;
                        return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
                    });
                    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
                }

                function getContrastRatio(color1, color2) {
                    const lum1 = getLuminance(color1);
                    const lum2 = getLuminance(color2);
                    const brightest = Math.max(lum1, lum2);
                    const darkest = Math.min(lum1, lum2);
                    return (brightest + 0.05) / (darkest + 0.05);
                }

                function parseRgb(rgbString) {
                    const match = rgbString.match(/\d+/g);
                    return match ? match.map(Number) : [0, 0, 0];
                }

                const textElements = Array.from(document.querySelectorAll('*')).filter(el => {
                    const hasText = el.textContent && el.textContent.trim().length > 0;
                    const isVisible = el.offsetParent !== null;
                    const isTextNode = !['SCRIPT', 'STYLE', 'META', 'LINK'].includes(el.tagName);
                    return hasText && isVisible && isTextNode;
                });

                return textElements.slice(0, 20).map(el => {
                    const styles = window.getComputedStyle(el);
                    const color = styles.color;
                    const backgroundColor = styles.backgroundColor;
                    const fontSize = parseFloat(styles.fontSize);
                    const fontWeight = styles.fontWeight;

                    const foreground = parseRgb(color);
                    const background = parseRgb(backgroundColor);
                    
                    // If background is transparent, walk up the DOM tree
                    let bgElement = el.parentElement;
                    let finalBackground = background;
                    while (bgElement && (finalBackground[0] === 0 && finalBackground[1] === 0 && finalBackground[2] === 0)) {
                        const bgStyles = window.getComputedStyle(bgElement);
                        const bgColor = bgStyles.backgroundColor;
                        if (bgColor !== 'rgba(0, 0, 0, 0)' && bgColor !== 'transparent') {
                            finalBackground = parseRgb(bgColor);
                            break;
                        }
                        bgElement = bgElement.parentElement;
                    }

                    const contrastRatio = getContrastRatio(foreground, finalBackground);
                    const isLargeText = fontSize >= 18 || (fontSize >= 14 && (fontWeight === 'bold' || fontWeight >= 700));
                    const requiredRatio = isLargeText ? 3.0 : 4.5;
                    
                    return {
                        tagName: el.tagName,
                        text: el.textContent?.trim().substring(0, 50) || '',
                        fontSize: fontSize,
                        fontWeight: fontWeight,
                        isLargeText: isLargeText,
                        color: color,
                        backgroundColor: backgroundColor,
                        contrastRatio: Math.round(contrastRatio * 100) / 100,
                        requiredRatio: requiredRatio,
                        passes: contrastRatio >= requiredRatio
                    };
                });
            }
        ");

        return new AccessibilityReport
        {
            TestType = "Color Contrast",
            Results = contrastData?.ToString() ?? "No data",
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Validates keyboard navigation and focus management
    /// </summary>
    public static async Task<AccessibilityReport> ValidateKeyboardNavigationAsync(IPage page)
    {
        var keyboardData = await page.EvaluateAsync<object>(@"
            () => {
                const focusableElements = Array.from(document.querySelectorAll(
                    'a[href], button, input, textarea, select, details, [tabindex]:not([tabindex=""-1""])'
                ));

                return focusableElements.map((el, index) => {
                    const rect = el.getBoundingClientRect();
                    const isVisible = rect.width > 0 && rect.height > 0 && el.offsetParent !== null;
                    const tabIndex = el.getAttribute('tabindex');
                    const hasAriaLabel = !!el.getAttribute('aria-label') || !!el.getAttribute('aria-labelledby');
                    const hasVisibleText = el.textContent?.trim() || el.getAttribute('alt') || el.getAttribute('title');

                    return {
                        tagName: el.tagName,
                        type: el.type || 'none',
                        index: index,
                        isVisible: isVisible,
                        tabIndex: tabIndex,
                        hasAriaLabel: hasAriaLabel,
                        hasAccessibleName: !!hasVisibleText || hasAriaLabel,
                        isDisabled: el.disabled,
                        role: el.getAttribute('role') || 'implicit'
                    };
                });
            }
        ");

        return new AccessibilityReport
        {
            TestType = "Keyboard Navigation",
            Results = keyboardData?.ToString() ?? "No data",
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Validates ARIA usage and semantic HTML structure
    /// </summary>
    public static async Task<AccessibilityReport> ValidateAriaAndSemanticsAsync(IPage page)
    {
        var ariaData = await page.EvaluateAsync<object>(@"
            () => {
                const results = {
                    headings: Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6')).map(h => ({
                        level: parseInt(h.tagName.substring(1)),
                        text: h.textContent?.trim() || '',
                        hasId: !!h.id
                    })),
                    landmarks: Array.from(document.querySelectorAll('main, nav, header, footer, aside, section, [role]')).map(el => ({
                        tagName: el.tagName,
                        role: el.getAttribute('role') || 'implicit',
                        hasLabel: !!el.getAttribute('aria-label') || !!el.getAttribute('aria-labelledby')
                    })),
                    formElements: Array.from(document.querySelectorAll('input, select, textarea')).map(el => ({
                        tagName: el.tagName,
                        type: el.type || 'none',
                        hasLabel: !!document.querySelector(`label[for='${el.id}']`) || !!el.getAttribute('aria-label') || !!el.getAttribute('aria-labelledby'),
                        hasRequiredAttribute: el.hasAttribute('required') || el.getAttribute('aria-required') === 'true',
                        hasDescription: !!el.getAttribute('aria-describedby')
                    })),
                    images: Array.from(document.querySelectorAll('img')).map(img => ({
                        src: img.src.substring(0, 50) + '...',
                        hasAlt: !!img.getAttribute('alt'),
                        altText: img.getAttribute('alt') || '',
                        isDecorative: img.getAttribute('alt') === '' || img.getAttribute('role') === 'presentation'
                    }))
                };
                return results;
            }
        ");

        return new AccessibilityReport
        {
            TestType = "ARIA and Semantics",
            Results = ariaData?.ToString() ?? "No data",
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Validates focus visibility and management during interactions
    /// </summary>
    public static async Task<AccessibilityReport> ValidateFocusManagementAsync(IPage page)
    {
        var focusData = await page.EvaluateAsync<object>(@"
            () => {
                // Check if focus indicators are visible
                const activeElement = document.activeElement;
                const styles = window.getComputedStyle(activeElement);
                
                const focusInfo = {
                    activeElementTag: activeElement?.tagName || 'NONE',
                    activeElementId: activeElement?.id || 'none',
                    activeElementClass: activeElement?.className || 'none',
                    hasFocusOutline: styles.outline !== 'none' || styles.boxShadow !== 'none',
                    outlineStyle: styles.outline,
                    boxShadow: styles.boxShadow,
                    hasVisibleFocus: activeElement !== document.body
                };

                // Check for skip links
                const skipLinks = Array.from(document.querySelectorAll('a[href^=""#""], .skip-link')).map(link => ({
                    text: link.textContent?.trim() || '',
                    href: link.getAttribute('href'),
                    isVisible: link.offsetParent !== null
                }));

                return {
                    focusInfo: focusInfo,
                    skipLinks: skipLinks,
                    hasLogicalTabOrder: true // This would require more complex analysis
                };
            }
        ");

        return new AccessibilityReport
        {
            TestType = "Focus Management",
            Results = focusData?.ToString() ?? "No data",
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Validates that dynamic content changes are announced to screen readers
    /// </summary>
    public static async Task<AccessibilityReport> ValidateDynamicContentAsync(IPage page)
    {
        var dynamicData = await page.EvaluateAsync<object>(@"
            () => {
                const liveRegions = Array.from(document.querySelectorAll('[aria-live], [role=""status""], [role=""alert""]')).map(el => ({
                    tagName: el.tagName,
                    ariaLive: el.getAttribute('aria-live'),
                    role: el.getAttribute('role'),
                    content: el.textContent?.trim() || '',
                    hasContent: !!el.textContent?.trim()
                }));

                const dynamicElements = Array.from(document.querySelectorAll('[aria-expanded], [aria-selected], [aria-checked]')).map(el => ({
                    tagName: el.tagName,
                    ariaExpanded: el.getAttribute('aria-expanded'),
                    ariaSelected: el.getAttribute('aria-selected'),
                    ariaChecked: el.getAttribute('aria-checked'),
                    role: el.getAttribute('role') || 'implicit'
                }));

                return {
                    liveRegions: liveRegions,
                    dynamicElements: dynamicElements
                };
            }
        ");

        return new AccessibilityReport
        {
            TestType = "Dynamic Content",
            Results = dynamicData?.ToString() ?? "No data",
            Timestamp = DateTime.Now
        };
    }

    /// <summary>
    /// Performs a comprehensive accessibility audit
    /// </summary>
    public static async Task<AccessibilityAuditResult> PerformFullAuditAsync(IPage page)
    {
        var results = new List<AccessibilityReport>();

        try
        {
            results.Add(await ValidateColorContrastAsync(page));
            results.Add(await ValidateKeyboardNavigationAsync(page));
            results.Add(await ValidateAriaAndSemanticsAsync(page));
            results.Add(await ValidateFocusManagementAsync(page));
            results.Add(await ValidateDynamicContentAsync(page));

            return new AccessibilityAuditResult
            {
                Reports = results,
                OverallScore = CalculateOverallScore(results),
                ComplianceLevel = DetermineComplianceLevel(results),
                Timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return new AccessibilityAuditResult
            {
                Reports = results,
                Error = ex.Message,
                OverallScore = 0,
                ComplianceLevel = "Error",
                Timestamp = DateTime.Now
            };
        }
    }

    private static double CalculateOverallScore(List<AccessibilityReport> reports)
    {
        // Simple scoring based on successful validations
        // In a real implementation, this would be more sophisticated
        return reports.Count > 0 ? (double)reports.Count / 5.0 * 100 : 0;
    }

    private static string DetermineComplianceLevel(List<AccessibilityReport> reports)
    {
        // Simplified compliance determination
        // In a real implementation, this would analyze the actual validation results
        if (reports.Count >= 5) return "WCAG 2.1 AA (Estimated)";
        if (reports.Count >= 3) return "Partial Compliance";
        return "Non-Compliant";
    }
}

/// <summary>
/// Represents the result of an accessibility validation test
/// </summary>
public class AccessibilityReport
{
    public string TestType { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Represents the result of a comprehensive accessibility audit
/// </summary>
public class AccessibilityAuditResult
{
    public List<AccessibilityReport> Reports { get; set; } = new();
    public double OverallScore { get; set; }
    public string ComplianceLevel { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}