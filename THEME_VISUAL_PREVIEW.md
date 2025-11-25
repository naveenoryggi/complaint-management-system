# Visual Theme Preview
## Complaint Management System - Before & After

> **A visual guide to the comprehensive theme transformation**

---

## 🎨 Color Palette

### Primary Colors

| Color | Hex | Usage | Preview |
|-------|-----|-------|---------|
| **Primary** | `#2563eb` | Buttons, links, focus states | 🔵 Blue 600 |
| **Primary Dark** | `#1d4ed8` | Hover states, gradients | 🔵 Blue 700 |
| **Primary Light** | `#dbeafe` | Backgrounds, highlights | 🔵 Blue 100 |

### Semantic Colors

| Color | Hex | Usage | Preview |
|-------|-----|-------|---------|
| **Success** | `#16a34a` | Success states, completed | 🟢 Green 600 |
| **Warning** | `#d97706` | Warning states, pending | 🟠 Orange 600 |
| **Error** | `#dc2626` | Error states, destructive | 🔴 Red 600 |
| **Info** | `#2563eb` | Information, help | 🔵 Blue 600 |

### Neutral Palette

| Shade | Hex | Usage |
|-------|-----|-------|
| **White** | `#ffffff` | Cards, surfaces |
| **Neutral 100** | `#f5f5f5` | Backgrounds |
| **Neutral 300** | `#d4d4d4` | Borders |
| **Neutral 600** | `#525252` | Secondary text |
| **Neutral 900** | `#171717` | Primary text |

---

## 🖼️ Background Gradients

### Login Page
```
Gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%)
Effect: Vibrant purple-blue with animated orbs
Mood: Welcoming, professional, energetic
```

**Description**: A bold, eye-catching gradient that makes a strong first impression.

### Application Pages
```
Gradient: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)
Overlays: Subtle radial gradients with blue tints
Effect: Calm, professional, modern
```

**Description**: A subtle, non-distracting background that lets content shine.

---

## 💳 Card Components

### Standard Card

```scss
// Visual Effect
Background: rgba(255, 255, 255, 0.95) with backdrop-filter: blur(20px)
Border: 1px solid rgba(255, 255, 255, 0.5)
Shadow: Soft, multi-layered
Border Radius: 16px (1rem)
```

**Key Features**:
- ✨ Frosted glass appearance
- 🎯 Subtle shadow for depth
- 🔄 Smooth hover animation (lift + shadow increase)
- 📐 Rounded corners for modern feel

### Card on Hover

```scss
// Hover State
Transform: translateY(-2px) // Lifts up
Shadow: Increases intensity
Backdrop Blur: Increases to 24px
Opacity: Increases to 100%
Transition: Smooth 200ms ease-in-out
```

**Animation**: Cards gently float up when hovered, creating an interactive feel.

---

## 🔘 Button Styles

### Primary Button

**Visual Appearance**:
```
Background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)
Shadow: 0 4px 15px rgba(59, 130, 246, 0.4)
Text: White, semibold (600)
Padding: 12px 24px
Border Radius: 8px
```

**Hover Effect**:
- Shadow intensifies to `0 6px 20px`
- Lifts up 2px
- Ripple effect on click (expanding white circle)

**States**:
- Normal: Gradient with shadow
- Hover: Lifted with stronger shadow
- Active: Slightly compressed (scale 0.98)
- Disabled: 60% opacity, no pointer

### Secondary Button

**Visual Appearance**:
```
Background: Glassmorphic (blurred white)
Border: 2px solid #2563eb
Text: Primary color
Padding: 12px 24px
```

**Hover Effect**:
- Background fills with primary gradient
- Text changes to white
- Lifts up 2px

### Button Variants

| Variant | Background | Use Case |
|---------|------------|----------|
| **Primary** | Blue gradient | Main actions |
| **Success** | Green gradient | Confirm, save |
| **Warning** | Orange gradient | Caution |
| **Danger** | Red gradient | Delete, destructive |
| **Secondary** | Glassmorphic | Cancel, secondary |
| **Ghost** | Transparent | Tertiary actions |

---

## 📝 Form Elements

### Input Fields

**Visual Appearance**:
```scss
Background: rgba(255, 255, 255, 0.95) with blur(10px)
Border: 2px solid rgba(59, 130, 246, 0.1)
Padding: 12px 16px
Border Radius: 8px
Font: 14px (0.875rem)
```

**States**:

1. **Default**: Light glassmorphism, subtle blue border
2. **Hover**: Border opacity increases to 0.3
3. **Focus**:
   - Border changes to solid primary color
   - Blue glow (ring) appears around input
   - Background becomes fully opaque
4. **Error**:
   - Border changes to red
   - Red glow appears
   - Error message shows below
5. **Success**:
   - Border changes to green
   - Green checkmark appears

### Labels

```
Font: 14px, semibold (600)
Color: Neutral 900
Icon: Primary color, 16px
Required asterisk: Red
Optional badge: Gray pill
```

### Helper Text

```
Font: 12px
Color: Neutral 600
Icon: Primary color (for hints)
Margin: 8px top
```

---

## 📊 Table Styling

### Table Header

```scss
Background: linear-gradient(135deg,
  rgba(59, 130, 246, 0.08) 0%,
  rgba(147, 197, 253, 0.08) 100%)
Text: Primary color, uppercase, 12px
Padding: 16px
Border: None
Font Weight: Semibold (600)
Letter Spacing: 0.05em
```

**Effect**: Subtle blue gradient that indicates header without being overwhelming.

### Table Rows

**Default State**:
```
Background: Transparent
Border Bottom: 1px solid rgba(0, 0, 0, 0.05)
Padding: 16px
```

**Hover State**:
```
Background: linear-gradient(135deg,
  rgba(59, 130, 246, 0.03) 0%,
  rgba(147, 197, 253, 0.03) 100%)
Transform: scale(1.005)
Transition: All 150ms ease-in-out
```

**Effect**: Row subtly highlights and scales up slightly, providing clear feedback.

---

## 🏷️ Badge Components

### Visual Styles

All badges feature:
- Glassmorphic background (gradient + blur)
- Border matching semantic color
- Uppercase text
- Letter spacing: 0.5px
- Pill shape (fully rounded)
- Shadow for depth

### Badge Variants

**Success Badge**:
```
Background: linear-gradient(135deg,
  rgba(34, 197, 94, 0.15) 0%,
  rgba(34, 197, 94, 0.25) 100%)
Text: Green 700
Border: Green 600
Example: "ACTIVE", "COMPLETED"
```

**Warning Badge**:
```
Background: Orange gradient (similar opacity)
Text: Orange 700
Border: Orange 600
Example: "PENDING", "IN REVIEW"
```

**Danger Badge**:
```
Background: Red gradient
Text: Red 700
Border: Red 600
Example: "REJECTED", "ERROR"
```

**Info Badge**:
```
Background: Blue gradient
Text: Blue 700
Border: Blue 600
Example: "NEW", "DRAFT"
```

---

## ⚠️ Alert Messages

### Visual Structure

```
Layout: Flexbox (icon + content)
Border Left: 4px solid (semantic color)
Padding: 20px
Border Radius: 8px
Glassmorphism: Yes (subtle)
Shadow: Medium elevation
```

### Alert Types

**Info Alert**:
```
Border Left: Blue 600
Background: Blue gradient (very subtle)
Icon: Info circle, blue
Text: Neutral 900
```

**Success Alert**:
```
Border Left: Green 600
Background: Green gradient
Icon: Check circle, green
Text: Neutral 900
```

**Warning Alert**:
```
Border Left: Orange 600
Background: Orange gradient
Icon: Warning triangle, orange
Text: Neutral 900
```

**Error Alert**:
```
Border Left: Red 600
Background: Red gradient
Icon: X circle, red
Text: Neutral 900
```

---

## 🪟 Modal Dialogs

### Visual Composition

**Backdrop**:
```
Color: rgba(0, 0, 0, 0.6)
Backdrop Filter: blur(8px)
Effect: Darkened, blurred background
```

**Modal Container**:
```
Background: Glassmorphic (high opacity)
Border Radius: 16px
Shadow: Extra large (dramatic)
Max Width: 600px (default)
Animation: Slide up + fade in
```

**Modal Header**:
```
Background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)
Color: White
Padding: 24px 32px
Border Radius: 16px 16px 0 0
Shadow: Inset highlight at top
```

**Modal Body**:
```
Padding: 32px
Background: White (glassmorphic)
Max Height: 90vh
Overflow: Auto
```

**Modal Footer**:
```
Padding: 24px 32px
Background: rgba(248, 250, 252, 0.5) with blur
Border Top: 1px solid rgba(0, 0, 0, 0.05)
Buttons: Right-aligned with spacing
```

---

## 📱 Responsive Behavior

### Mobile (< 768px)

- **Cards**: Full width, stacked
- **Buttons**: Full width on forms
- **Tables**: Horizontal scroll
- **Modals**: 95% width, minimal padding
- **Typography**: Scales down 10-20%
- **Spacing**: Reduced by 25%

### Tablet (768px - 1024px)

- **Cards**: 2-column grid
- **Buttons**: Natural width
- **Tables**: Full width
- **Modals**: 80% width
- **Typography**: Standard sizes
- **Spacing**: Standard

### Desktop (> 1024px)

- **Cards**: 3-4 column grid
- **Buttons**: Natural width with icons
- **Tables**: Full features visible
- **Modals**: Optimal width (600px)
- **Typography**: Full scale
- **Spacing**: Generous

---

## ✨ Animation & Transitions

### Page Load Animation

```scss
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

Duration: 500ms
Timing: ease-out
Effect: Content fades in while sliding up
```

### Card Hover Animation

```
Property: transform, box-shadow
Duration: 200ms
Timing: ease-in-out
Effect: Lifts 2px, shadow intensifies
```

### Button Click Animation

```
Property: transform
Duration: 100ms
Timing: ease-in-out
Effect: Scales to 0.98, then returns
Ripple: Expanding white circle
```

### Input Focus Animation

```
Property: border-color, box-shadow
Duration: 200ms
Timing: ease-in-out
Effect: Border color transitions, ring appears
```

### Modal Animation

```
Entry: fadeIn + slideUp (300ms)
Exit: fadeOut + slideDown (200ms)
Backdrop: fadeIn/fadeOut (200ms)
```

---

## 🎭 Glassmorphism Examples

### Light Glassmorphism (Forms)

```scss
background: rgba(255, 255, 255, 0.95);
backdrop-filter: blur(10px) saturate(180%);
border: 1px solid rgba(255, 255, 255, 0.5);
```

**Effect**: Subtle blur, mostly opaque, professional

### Medium Glassmorphism (Cards)

```scss
background: rgba(255, 255, 255, 0.95);
backdrop-filter: blur(20px) saturate(180%);
border: 1px solid rgba(255, 255, 255, 0.5);
```

**Effect**: Frosted glass, clear content, modern

### Strong Glassmorphism (Modals)

```scss
background: rgba(255, 255, 255, 0.98);
backdrop-filter: blur(30px) saturate(180%);
border: 1px solid rgba(255, 255, 255, 0.5);
```

**Effect**: Strong blur, elevated surface, premium

---

## 🌈 Gradient Patterns

### Button Gradients

```scss
// Primary
background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);

// Success
background: linear-gradient(135deg, #16a34a 0%, #15803d 100%);

// Warning
background: linear-gradient(135deg, #d97706 0%, #b45309 100%);

// Danger
background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
```

**Angle**: 135° (diagonal from bottom-left to top-right)
**Effect**: Dynamic, modern, adds depth

### Background Gradients

```scss
// Application
background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);

// Login
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

**Angle**: 135° (consistent with buttons)
**Effect**: Immersive, non-distracting, professional

### Badge/Alert Gradients

```scss
// Semantic badges (low opacity)
background: linear-gradient(135deg,
  rgba(COLOR, 0.15) 0%,
  rgba(COLOR, 0.25) 100%);
```

**Effect**: Subtle color wash, maintains readability

---

## 📏 Spacing Examples

### Component Spacing

```
Card Padding: 24px (var(--spacing-6))
Button Padding: 12px 24px
Input Padding: 12px 16px
Modal Padding: 32px
Table Cell Padding: 16px
```

### Layout Spacing

```
Section Margin: 48px (var(--spacing-12))
Card Gap: 24px (var(--spacing-6))
Form Group Margin: 20px (var(--spacing-5))
Button Gap: 12px (var(--spacing-3))
```

### Micro Spacing

```
Icon-Text Gap: 8px (var(--spacing-2))
Badge Padding: 8px 12px
Label-Input Gap: 8px (var(--spacing-2))
Helper Text Margin: 8px top
```

---

## 🎯 Key Visual Principles

### 1. Hierarchy Through Size

```
Hero Heading: 48px (3rem) - font-size-5xl
Page Heading: 30px (1.875rem) - font-size-3xl
Section Heading: 24px (1.5rem) - font-size-2xl
Subsection: 20px (1.25rem) - font-size-xl
Body: 16px (1rem) - font-size-base
Caption: 12px (0.75rem) - font-size-xs
```

### 2. Hierarchy Through Color

```
Primary Text: Neutral 900 (darkest)
Secondary Text: Neutral 600 (medium)
Muted Text: Neutral 500 (light)
Disabled Text: Neutral 400 (lighter)
```

### 3. Hierarchy Through Weight

```
Bold: 700 - Page titles, emphasis
Semibold: 600 - Headings, labels
Medium: 500 - Emphasized body
Normal: 400 - Body text (default)
Light: 300 - Subtle text
```

### 4. Depth Through Shadows

```
Level 1: sm - Subtle cards
Level 2: base - Default cards
Level 3: md - Elevated elements
Level 4: lg - Modals, dropdowns
Level 5: xl - Overlays
Level 6: 2xl - Maximum elevation
```

---

## ✅ Accessibility Features

### Focus Indicators

```
Outline: 2px solid #2563eb
Offset: 2px
Visibility: Only when :focus-visible
```

**Effect**: Clear blue outline appears on keyboard focus, not mouse click.

### Color Contrast

All combinations meet WCAG 2.1 AA:
- Text on backgrounds: ≥ 4.5:1
- UI components: ≥ 3:1
- Large text: ≥ 3:1

### Reduced Motion

```scss
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

**Effect**: Respects user's motion preferences, disables animations.

---

## 🖥️ Browser Compatibility

### Full Support (All Features)

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

### Partial Support (Fallbacks)

- Older Chrome/Firefox: Solid backgrounds instead of blur
- Older Safari: Alternative filter effects
- IE11: Not supported (CSS variables required)

---

## 📸 Visual Examples (Conceptual)

### Login Page

```
┌─────────────────────────────────────────────┐
│                                             │
│  [Purple-Blue Gradient Background]         │
│                                             │
│         ┌─────────────────────┐            │
│         │  [Glassmorphic Card]│            │
│         │                     │            │
│         │   🏢 Logo (Pulsing) │            │
│         │   Login to System   │            │
│         │                     │            │
│         │   📧 [Email Input]  │            │
│         │   🔒 [Password]     │            │
│         │   ☐ Remember Me     │            │
│         │                     │            │
│         │   [🔐 LOGIN BUTTON] │            │
│         │   (Gradient)        │            │
│         └─────────────────────┘            │
│                                             │
└─────────────────────────────────────────────┘
```

### Dashboard

```
┌─────────────────────────────────────────────────────┐
│ [Subtle Blue-Gray Gradient Background]             │
│                                                     │
│  ┌─────────────────────────────────────┐          │
│  │ 👋 Welcome, User                     │          │
│  │ Overview of your complaints          │          │
│  └─────────────────────────────────────┘          │
│                                                     │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐         │
│  │ 📊   │  │ ⏳   │  │ ⚙️   │  │ ✅   │         │
│  │ 150  │  │ 12   │  │ 45   │  │ 93   │         │
│  │Total │  │Pending│  │Active│  │Done│          │
│  └──────┘  └──────┘  └──────┘  └──────┘         │
│  (Glassmorphic cards with hover lift)             │
│                                                     │
│  ┌─────────────────────────────────────┐          │
│  │ 📝 Recent Complaints                 │          │
│  ├─────────────────────────────────────┤          │
│  │ [Table with gradient header]         │          │
│  │ [Rows with hover effect]             │          │
│  └─────────────────────────────────────┘          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Form Page

```
┌─────────────────────────────────────────────────────┐
│ [Gradient Background]                               │
│                                                     │
│  ┌─────────────────────────────────────┐          │
│  │ 📝 Create New Complaint              │          │
│  │ Fill in the details below            │          │
│  └─────────────────────────────────────┘          │
│                                                     │
│  ┌─────────────────────────────────────┐          │
│  │ 📋 Basic Information                 │          │
│  ├─────────────────────────────────────┤          │
│  │ 📝 Title *                           │          │
│  │ [Glassmorphic Input]                 │          │
│  │                                       │          │
│  │ 📄 Description *                     │          │
│  │ [Glassmorphic Textarea]              │          │
│  │                                       │          │
│  │ 🏢 Category *                        │          │
│  │ [Glassmorphic Select]                │          │
│  │                                       │          │
│  │ ⚠️ Priority *                        │          │
│  │ [Glassmorphic Select]                │          │
│  └─────────────────────────────────────┘          │
│                                                     │
│  [✅ SUBMIT (Gradient)] [❌ Cancel]               │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 🎊 Summary

This visual preview demonstrates:

1. **Consistent Color Usage**: Primary blue throughout with semantic colors
2. **Glassmorphism Effects**: Subtle to strong based on context
3. **Gradient Accents**: Diagonal gradients for modern feel
4. **Smooth Animations**: GPU-accelerated, 60fps
5. **Clear Hierarchy**: Size, color, weight working together
6. **Accessibility**: High contrast, visible focus, motion preferences
7. **Responsive Design**: Adapts to all screen sizes
8. **Professional Quality**: Premium, production-ready appearance

**Result**: A cohesive, modern, professional UI that delights users and enhances brand perception.

---

**Preview Created**: November 2, 2025
**Design System Version**: 1.0.0
**Visual Style**: Modern Glassmorphism with Gradients
