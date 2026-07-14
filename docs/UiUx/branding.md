# Branding — Logo, Color Palette & Theming

## Logo

![EnsyLabs logo](./assets/logo.png)

Source file: [`assets/logo.png`](./assets/logo.png).

## Color palette

Extracted directly by sampling the logo's pixels (background corners, wing highlights, and the outline/wordmark stroke), not guessed — a designer should feel free to fine-tune for contrast/accessibility, but these are the actual brand colors as they exist in the logo today.

| Role | Hex | Sampled from |
|---|---|---|
| **Ink / Base background (dark mode)** | `#14140C` | Background corners of the logo (near-black with a warm, slightly olive undertone — not a pure neutral black) |
| **Paper / Base background (light mode)** | `#FAF7EC` | Recommended, not sampled — a warm ivory/cream that mirrors Ink's warm undertone at the opposite end of the lightness scale, so light mode reads as the same brand rather than a generic white theme with the gold bolted on |
| **Gold — Bright** (primary accent) | `#E8C40F` | Wing highlights — the brightest, most saturated gold in the mark |
| **Gold — Deep/Bronze** (secondary accent) | `#C38E11` | Badge outline stroke and the "ENSYLABS" wordmark |

**Intent**: "the golden look of the logo" should carry through the product — dark, near-black surfaces with the two golds used as accent/interactive color (Bright Gold for primary actions/highlights, Deep Gold/Bronze for secondary elements, borders, and text-on-dark), rather than a generic corporate blue/gray palette.

## Light/dark mode

Every page — admin backoffice, customer backoffice, and the public status page — needs a **user-toggleable light/dark mode**, not just a dark theme:

- **Dark mode** is the natural home for this palette: Ink (`#14140C`) base with the two golds as accents, closely mirroring the logo itself.
- **Light mode** swaps Ink for Paper (`#FAF7EC`), keeping both golds as accents and dark, near-black (or deep bronze) text for readability. The gold identity must remain visible and recognizable in both modes — this isn't a generic white theme with the accent removed.
- **Contrast caution**: gold text directly on the Paper background has weak contrast, since both are light — fine for borders, fills, and buttons where the text color is controlled independently (e.g. dark text on a gold button), but body text and links in light mode should stay dark/Ink-colored, using gold only for accents rather than as a text color on light backgrounds.
- This applies uniformly across all three surfaces (admin backoffice, customer backoffice, public status page) — it's a platform-wide requirement, not per-page.

## Buttons & interactive elements

- **Primary button**: solid Bright Gold (`#E8C40F`) fill with near-black text — the highest-emphasis action on a screen.
- **Secondary button**: outline/ghost style using Deep Bronze (`#C38E11`) as the border and text color, transparent or dark fill. Two solid-gold buttons side by side blur the primary/secondary hierarchy (hard to visually rank at a glance, especially on a near-black background) — differentiating by fill-vs-outline solves that while staying entirely within the palette.
- **Destructive actions** (e.g. Revoke license, Reject renewal): should **not** use gold at all. Use a distinct, conventional danger color (red) so destructive actions are never confused with the brand's primary/secondary actions.
