# FishingBuddy Project Guidelines

## UX/UI Delegation — Required

When generating, modifying, reviewing, or debugging **any UX/UI code** for this project — including Razor Views (`.cshtml`), CSS rules, Bootstrap layout, typography, color, icons, animations, or any visual element — you **must** spawn the **FishingStylist** subagent to handle the work.

Do not write UX/UI code yourself. Delegate to FishingStylist and return its output to the user.

Trigger phrases that require FishingStylist delegation: design, style, UI, UX, visual, color, layout, CSS, Razor view appearance, theme, branding, illustration, navbar, buttons, cards, forms styling, animations.

## Project Stack

- ASP.NET MVC (.NET 9, Razor Views)
- Bootstrap 5
- Vanilla CSS in `wwwroot/css/site.css`
- C# models in `Models/`, repositories in `Repositories/`

## Architecture

- Data layer: mock repository pattern (`Repositories/MockFishingRepository.cs` implementing `IFishingRepository`)
- Controllers in `Controllers/`
- Views follow standard MVC layout with shared `_Layout.cshtml`

## Conventions

- Repository interface first: add methods to `IFishingRepository` before implementing in `MockFishingRepository`
- Seed data lives exclusively in `MockFishingRepository` — not in `FBuddy.cs` or controllers
