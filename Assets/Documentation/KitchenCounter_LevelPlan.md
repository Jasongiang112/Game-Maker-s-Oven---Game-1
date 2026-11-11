# Burger Bash — The Long Counter

## Level pitch

The playable burger starts on a plate at one end of a long kitchen counter. The level continually pushes forward through a cutting-board work area, around a large sink, and into a crowded serving area filled with plates and oversized food dishes.

## Intended flow

1. **Starting Plate** — The burger begins on a clean plate at the near end of the counter.
2. **Counter Run-Up** — A short open section introduces movement before obstacles begin.
3. **Cutting Board** — Navigate around a giant knife, chopped tomatoes, lettuce, and an ant placeholder.
4. **Sink** — Continue forward along either narrow side rim. Touching the water returns the burger to the starting plate.
5. **Serving Area** — Weave between multiple plates, an apple, pancakes, dessert, and grounded enemy placeholders.
6. **Order Bell** — Reach the bell at the far end of the counter.

## Controls

- **WASD or arrow keys:** move
- **Space:** jump

## Placeholder legend

- **Magenta cubes:** static enemy placeholders
- **Green discs:** checkpoints
- **Orange bell:** level goal

The generated overview image is `Assets/Documentation/KitchenCounter_Overview.png`. The playable greybox scene is `Assets/Scenes/KitchenCounterLevel.unity`.

## Scope notes

This draft includes playable movement and a following camera. All enemies are grounded, and there are no floating collectible placeholders. Enemy movement, damage, checkpoint activation, and goal scripting are reserved for a later gameplay pass.
