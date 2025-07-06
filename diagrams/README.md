# FX-Orleans System Architecture Diagrams

This directory contains Mermaid diagrams that visualize the FX-Orleans system architecture and flows.

## Available Diagrams

1. **System Flow** ([system-flow.md](system-flow.md))
   - High-level overview of the entire system
   - Shows main components and their interactions
   - Illustrates the end-to-end user journey

2. **Aggregates Flow** ([aggregates-flow.md](aggregates-flow.md))
   - Details the domain aggregates and their relationships
   - Shows commands and events for each aggregate
   - Visualizes the domain model structure

3. **CQRS Flow** ([cqrs-flow.md](cqrs-flow.md))
   - Illustrates the Command Query Responsibility Segregation pattern
   - Shows separation between command and query paths
   - Demonstrates event sourcing and projection process

## Viewing the Diagrams

These diagrams use Mermaid markdown syntax. To view them:

1. Open the markdown files in a Mermaid-compatible viewer
2. Use GitHub's built-in Mermaid rendering
3. Copy the diagram content to the [Mermaid Live Editor](https://mermaid.live/)

## System Overview

FX-Orleans is built on:
- EventServer: ASP.NET Core backend with CQRS/Event Sourcing
- FxExpert: Blazor WebAssembly UI
- MartenDB: Event store and document database
- External services: OpenAI GPT, Stripe, Calendar APIs