# Architectural Decisions Log

> Last Updated: 2025-08-12
> Version: 1.1.0

## ADR-004: Standardize Blazor Render Mode to InteractiveServer

**ID:** ADR-004
**Date:** 2025-08-12
**Status:** Accepted
**Category:** Technical Architecture
**Stakeholders:** Tech Lead, Development Team

### Decision

FX-Orleans will use **InteractiveServer** render mode exclusively for all interactive Blazor components instead of InteractiveAuto. All pages requiring event handlers and interactive behavior will explicitly declare `@rendermode InteractiveServer`.

### Context

The application was originally configured to use InteractiveAuto render mode, which attempts to:
1. Start with Server-side rendering for fast initial load
2. Download WebAssembly runtime in the background  
3. Seamlessly transition to WebAssembly for better performance

However, this approach has been causing recurring issues:

- **Event handlers stop working**: Components get stuck between server and WebAssembly modes
- **Authentication state problems**: Complex OIDC flows fail during mode transitions
- **JavaScript interop failures**: Theme switching, Stripe integration, and other JS services break
- **Service registration mismatches**: Different service configurations between server and client
- **Development friction**: Unpredictable behavior requiring frequent troubleshooting

### Problem Statement

InteractiveAuto render mode creates an unreliable user experience where:
- Buttons and interactive elements randomly stop working
- Users lose functionality mid-session without clear error messages
- Development time is wasted on render mode debugging instead of feature development
- The theoretical performance benefits don't outweigh the reliability costs

### Alternatives Considered

1. **Continue with InteractiveAuto + Fix Issues**
   - **Pros**: Potential performance benefits, follows Microsoft recommended patterns
   - **Cons**: Complex debugging, ongoing maintenance burden, service registration complexity
   - **Effort**: High (weeks of debugging and testing)

2. **Switch to InteractiveWebAssembly Only** 
   - **Pros**: Consistent client-side execution, no mode transitions
   - **Cons**: Slower initial load, larger bundle size, authentication complexity
   - **Effort**: High (authentication reconfiguration)

3. **Use InteractiveServer Exclusively** (CHOSEN)
   - **Pros**: Reliable, consistent behavior, works well with OIDC authentication
   - **Cons**: Requires persistent connection, slightly higher server load
   - **Effort**: Low (render mode standardization only)

### Decision Rationale

**InteractiveServer is the optimal choice because:**

1. **Application Requirements Favor Server Mode**:
   - Complex authentication using Keycloak OIDC
   - Multiple JavaScript integrations (Stripe, theme switching)
   - Real-time features work naturally with persistent connections
   - Business logic benefits from server-side execution

2. **Reliability Over Theoretical Performance**:
   - Consistent user experience is more valuable than potential performance gains
   - Server-side execution is more predictable and debuggable
   - Eliminates entire class of render mode transition bugs

3. **Development Efficiency**:
   - Reduces complexity and debugging time
   - Enables faster feature development
   - Provides stable foundation for MVP completion

4. **Technical Fit**:
   - SignalR already required for real-time features
   - Server has sufficient capacity for interactive connections
   - Authentication flows work seamlessly with server-side rendering

### Implementation Details

**Components Updated**:
- **App.razor**: Changed `PageRenderMode` from `InteractiveAuto` to `InteractiveServer`
- **All Interactive Pages**: Added explicit `@rendermode InteractiveServer` directives:
  - Home.razor (main booking flow)
  - PartnerInfo.razor (partner details and scheduling)
  - PartnerHome.razor (partner dashboard)
  - UserProfile.razor (user management)
  - ClientSessionsPage.razor (session history)
  - ConfirmationPage.razor (booking confirmation)
  - AuthenticationFailed.razor (error handling)
  - LoginComponent.razor (authentication flow)
- **MainLayout.razor**: Explicit InteractiveServer mode for consistent navigation

**Service Configuration**:
- All services remain registered on both server and client for future flexibility
- SignalR configuration optimized for reliable connections
- Authentication flows simplified without mode transitions

### Success Criteria

✅ **Event handlers work consistently** - No random button/menu failures
✅ **Authentication flows are stable** - OIDC login/logout works reliably  
✅ **JavaScript interop functions** - Theme switching, Stripe integration work
✅ **Mobile responsiveness maintained** - Touch interactions work across devices
✅ **Build succeeds cleanly** - No render mode related compilation errors

### Consequences

**Positive**:
- **Reliability**: Eliminates entire class of render mode bugs
- **Development Speed**: Faster feature development without debugging render mode issues
- **User Experience**: Consistent interactive behavior across all sessions
- **Authentication Stability**: OIDC flows work reliably without transitions
- **Maintainability**: Simpler architecture with fewer moving parts

**Negative**:
- **Server Load**: Slightly higher server resource usage for interactive connections
- **Connection Dependency**: Requires stable internet connection for full functionality
- **Initial Load**: Marginally slower initial page load compared to WebAssembly

**Neutral**:
- **Performance**: Real-world performance difference is negligible for this application type
- **Scalability**: Server can handle expected load; can be addressed if needed
- **Future Flexibility**: Can switch to other render modes if requirements change

### Monitoring and Success Metrics

**Reliability Metrics**:
- Zero event handler failures reported by users
- No authentication timeout issues
- JavaScript interop success rate: 100%

**Performance Monitoring**:
- Page load time: <3 seconds on 3G networks
- Interactive response time: <200ms for user actions
- Connection stability: 99%+ uptime for SignalR connections

### Future Considerations

- **If load becomes an issue**: Consider hybrid approach with static pages using SSR
- **If offline support needed**: Evaluate Progressive Web App features
- **If mobile performance critical**: Consider InteractiveWebAssembly for specific mobile scenarios

### References

- [Blazor render modes documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
- [SignalR connection management](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration)
- Previous discussions in ADR-002 (Technology Architecture Decision)

---

*This decision can be revisited if requirements change significantly or if Microsoft resolves the InteractiveAuto reliability issues in future .NET releases.*