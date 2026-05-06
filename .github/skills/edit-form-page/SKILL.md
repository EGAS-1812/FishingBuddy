---
name: edit-form-page
description: "Use when: creating or updating MVC create pages, edit pages, form pages, data entry forms, validation forms, or Razor forms that post changes. Trigger phrases: create page, edit form, edit page, create form, upsert page, data entry form, save form, validation form."
---

# Edit Form Page Skill

Use this skill when you need a Razor MVC form for creating or editing data.

## Scope

- Add GET and POST controller actions for create/edit flows.
- Bind form fields to models or view models.
- Use model validation and anti-forgery protection.
- Save through the repository abstraction when the project architecture expects it.
- Return the same view when validation fails.

## Workflow

1. Start from the owning controller and model.
2. Decide whether the form should use the entity directly or a dedicated view model.
3. Add GET action for initial render.
4. Add POST action with `[ValidateAntiForgeryToken]` and `ModelState` checks.
5. Persist through the repository or service layer.
6. Redirect after successful save.
7. Include validation summary, field validation spans, and `_ValidationScriptsPartial` in the Razor view.

## Repo-Specific Example

- Example create actions: `TechniqueController.Create`
- Example edit actions: `TechniqueController.Edit`
- Example views: `Views/Technique/Create.cshtml` and `Views/Technique/Edit.cshtml`

## Notes

- If the task changes UI/Razor markup, use the FishingStylist agent for the view markup.
- Use the existing Bootstrap form language already present in the app.