---
name: list-page
description: "Use when: creating or updating an MVC list page, index table, admin list, catalog table, searchable list, or page that displays many records with actions. Trigger phrases: list page, index page, table page, admin list, catalog list, management page, show all records."
---

# List Page Skill

Use this skill when you need a Razor MVC page that lists many records.

## Scope

- Add or update controller actions that return collections.
- Add or update Razor views that render tables or card lists.
- Add count badges, action links, empty states, and lightweight filtering when requested.
- Keep the page aligned with existing Bootstrap/Razor patterns in FishingBuddy.

## Workflow

1. Start from the existing controller and model that already own the data.
2. Reuse repository methods when possible instead of bypassing the architecture.
3. Return a concrete collection type the view can iterate safely.
4. Build a list page with these sections when relevant:
   - title and short context text
   - record count
   - primary action button
   - table or list of records
   - action buttons per row
   - empty state
5. Validate the route, the controller action, and the view model type.

## Repo-Specific Example

- Example controller action: `TechniqueController.Manage`
- Example Razor page: `Views/Technique/Manage.cshtml`
- Example navigation link: `Views/Technique/Index.cshtml`

## Notes

- If the task changes UI/Razor markup, use the FishingStylist agent for the view markup.
- Prefer simple Bootstrap tables first; add CSS only when the existing design cannot express the layout.