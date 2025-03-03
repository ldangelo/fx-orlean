# fx-expert development conventions

## General

- Type Hints: Use type hints wherever possible to improve code clarity and maintainability
- Code should be easy to read and understand.
- Keep the code as simple as possible. Avoid unnecessary complexity.
- Use meaningful names for variables, functions, etc. Names should reveal intent.
- Function names should describe the action being performed.
- Prefer fewer arguments in functions and methods. Ideally, aim for no more than two or three.
- Only use comments when necessary, as they can become outdated. Instead, strive to make the code self-explanitory.
- When functions are used, they should add useful information that is not readily apparent from the code itself.
- Properly handle errors and exceptions to ensure the software's robustness.
- Use exceptions rather than error codes for handling errors.
- Consider security implications of the code. Implement security best practices to protect against vulnerabilities and attacks.
- Use immutable types whenever possible.
- When working with pulumi use kubernetes and AWS.
- When creating kubernetes cluster use AWS.

## Package Information

- [Marten Documentation](https://martendb.io/introduction.html "Marten Documentation")
- [Wolverine](https://wolverinefx.net/guide/basics.html)
- [Blazer](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-9.0&WT.mc_id=dotnet-35129-website)
- [MudBlazor](https://mudblazor.com/docs/overview)
- [Google .Net Client Libraries](https://developers.google.com/api-client-library/dotnet)
- [Stripe API's](https://docs.stripe.com/api?lang=dotnet)
- [Newtonsolft](https://www.newtonsoft.com/json/help/html/Introduction.htm)

## Architecture

This document presents the high-level architecture for the project, supporting a Partner-based system with additional features for **Users** and **Videoconferences**. The system manages **Partner profiles**, **User accounts**, and **Videoconferences scheduling**, while leveraging **MartenDB** for event-sourcing and persistence and enabling deep integration with **CQRS**. A robust testing suite implemented in xUnit ensures system reliability.

## **Architectural Components**

### **1. Application Layer**

- **Technology**: ASP.NET Core 9.0
- **Responsibility**:
  - Routes all incoming HTTP requests to their corresponding commands, queries, and business rules.
  - Events propagate to other components as necessary.
  - Offers RESTful Web APIs for interacting with Users, Partners, and Videoconferences.

### Key Responsibilities

- Request validation via Middleware.
- Publishing **Commands** (for state changes).
- Executing\*\*Queries\*\* (for reading/viewing data).
- Exposing endpoints for external applications:
- `/api/partners` (Partners management).
- /api/users (Users management).
- /api/videoconferences (Videoconference scheduling)

### Critical Modules

- **Controllers**: For Partners, Users, and Videoconferences.
- **Request Handlers**: Handle commands (`ICommand`) and queries (`IQuery`).
  -\*\*Dependency Injection (DI)\*\*: Binds service and repository dependencies

### **2. Domain Layer**

- **Technology**: C# (Domain-Driven Design and Event Sourcing principles applied)
- **Responsibility**:
  - Houses the **Commands**, **Events**, and domain **Aggregates** (e.g., `Partner`, `User`, `Videoconference`) along with their business logic.

#### Key Aggregates: - **Partner**: - Entity representing a partner in the system. - Attributes: `Bio`, `Skills`, `WorkHistory`, etc. - Supports commands such as `CreatePartner`, `UpdatePartner`, and `AddSkill`

    - **User**:
        - Represents a participant in the system.
        - Attributes: `Email`, `Role (e.g., Admin/Viewer)`, `Settings` (e.g., notification preferences).
        - Commands: `RegisterUser`, `AuthenticateUser`, and `UpdateUserSettings`.

    - **Videoconference**:
        - Handles **scheduling** between partners and/or users.
        - Attributes: `MeetingId`, `Participants`, `Time (Start/End)`, `Details`.
        - Commands: `CreateVideoconference`, `CancelVideoconference`, `UpdateVideoconference`.

**Key Concepts**: - **Commands**: Represent the intent to **change system state**. - **Events**: Triggered as a result of commands, representing something that has happened in the system.
