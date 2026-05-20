---
name: js-animation-mastery
agent: FishingStylist
description: |
  Use when: advanced, smooth, or visually impressive JavaScript/CSS animations are requested for FishingBuddy. Trigger phrases: animation, animate, smooth transition, fade, slide, highlight, row reveal, dropdown animation, visual feedback, micro-interaction, advanced JS animation, CSS transition, keyframes, motion, UI polish.
---

# JS Animation Mastery Skill

## When to Use
- User requests advanced, smooth, or visually impressive animations in FishingBuddy.
- Animating table rows, dropdowns, modals, form fields, or any UI element.
- Improving feedback for AJAX, search, or validation events.
- Adding micro-interactions or motion design polish.

## Guidelines
- Use CSS transitions for simple fades, slides, and transforms.
- Use CSS keyframes for more complex, staged, or looping animations.
- Use JavaScript to add/remove classes, control animation timing, or trigger animations in response to AJAX or user events.
- Always keep animations performant and accessible (respect reduced motion preferences).
- Animations should support the user experience, not distract from it.
- Prefer compositing properties (opacity, transform) for smoothness.
- Use cubic-bezier easing for natural feel.
- For AJAX row/table animations, stagger entry for a lively effect.
- For dropdowns, animate open/close with scale and fade.
- For validation, highlight fields with a brief color pulse or shake if needed.

## Example Triggers
- "Make the row reveal animation smoother."
- "Add a highlight when a search result changes."
- "Animate the dropdown open/close."
- "Add a micro-interaction to the submit button."

## Implementation
- Add or update CSS in wwwroot/css/site.css.
- Add or update JS in wwwroot/js/site.js.
- Add/remove classes in Razor views as needed.
- Always test with keyboard and screen reader navigation.

## Notes
- If animation is not visually clear, add a comment in the code to explain the intent.
- If user requests a specific animation style, follow their description closely.
- If animation is not possible with CSS alone, use JS to orchestrate timing and state.
