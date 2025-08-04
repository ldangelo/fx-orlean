# FX-Orleans Development Guide
## Enhanced System Architecture

### Home Page / Problem Statement Interface

Clean, professional form for users to describe their challenges
AI-powered analysis of the problem statement
Optional fields for industry, urgency, and specific expertise needs


### AI Matching Engine

Natural language processing to extract key requirements
Matching algorithm to identify partners with relevant skills and experience
Availability filter to only show partners who can meet soon


### Partner Selection Interface

Display of matched partners with relevance scores
Brief profiles highlighting expertise relevant to the stated problem
Availability calendar for each partner

### Booking System

Seamless transition to scheduling once a partner is selected
Calendar integration
Confirmation and reminder system
The booking system should add a meeting request to the partners calander,
inviting the user and adding a google meeting link.

### Payment Authorization

Using stripe authorize a payment for 800.00 for a 60 minute session.
The payment should be collected prior to the user joining the meeting.

### Confirmation Email

The confirmation page will display the details of the scheduled meeting.
Additionally confirmation e-mails will be sent out from the google meeting request.


### Implementation Approach
 
For the AI matching component, you could:

Use a Large Language Model (LLM) like OpenAI's GPT or similar:

Process problem statements
Extract key skills, industries, and technologies mentioned
Match against structured partner profiles


Partner Profile Database:

Detailed skills inventory for each partner
Previous experience categorized by industry and role
Areas of specialty and expertise levels


Availability System:

Calendar API integration (Google Calendar)
Real-time availability checks

## Build & Run Commands
- Build: `dotnet build`
- Run EventServer: `dotnet watch --project src/EventServer/EventServer.csproj`
- Run FxExpert Blazor: `dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj`
- Run both services: `just run`

## Test Commands
- Run all tests: `dotnet test`
- Run specific test: `dotnet test --filter "FullyQualifiedName=EventServer.Tests.UserTests.CreateUserTest"`
- Tests use xUnit, Alba for HTTP testing, and Shouldly for assertions

## Code Style Guidelines
- Use type hints everywhere for clarity
- Code should be simple, readable, and self-explanatory
- Use meaningful names that reveal intent
- Function names should describe actions performed
- Prefer exceptions over error codes for error handling
- Use immutable types whenever possible
- In controllers, use Wolverine attributes and return command events
- In the UI use Blazor and MudComponents.
- Prefer MudCompenents over standard html.

## Architecture Overview
- System uses CQRS and Event Sourcing patterns
- Commands represent intent to change state
- Events represent state changes that have occurred
- Main aggregates: Partners, Users, VideoConferences, Payments, Calendar
- UI uses Blazor with MudBlazor component library

## Instructions
read ai_docs/task-master.md

## Agent OS Documentation

### Product Context
- **Mission & Vision:** @.agent-os/product/mission.md
- **Technical Architecture:** @.agent-os/product/tech-stack.md
- **Development Roadmap:** @.agent-os/product/roadmap.md
- **Decision History:** @.agent-os/product/decisions.md

### Development Standards
- **Code Style:** @~/.agent-os/standards/code-style.md
- **Best Practices:** @~/.agent-os/standards/best-practices.md

### Project Management
- **Active Specs:** @.agent-os/specs/
- **Spec Planning:** Use `@~/.agent-os/instructions/create-spec.md`
- **Tasks Execution:** Use `@~/.agent-os/instructions/execute-tasks.md`

## Workflow Instructions

When asked to work on this codebase:

1. **First**, check @.agent-os/product/roadmap.md for current priorities
2. **Then**, follow the appropriate instruction file:
   - For new features: @.agent-os/instructions/create-spec.md
   - For tasks execution: @.agent-os/instructions/execute-tasks.md
3. **Always**, adhere to the standards in the files listed above

## Important Notes

- Product-specific files in `.agent-os/product/` override any global standards
- User's specific instructions override (or amend) instructions found in `.agent-os/specs/...`
- Always adhere to established patterns, code style, and best practices documented above.
