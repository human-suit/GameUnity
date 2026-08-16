# Окружение — Unstable Experiment (v1)

## Стиль (как персонажи v5)

- Fear & Hunger hand-drawn, thick black outlines
- Desaturated grey/brown, block shadows, vignette
- Sector B: green tint + slime | Sector C: red tint + industrial
- Soft blur atmospheric, NOT crisp pixel

## Два слоя Unity

| Слой | Когда | Assets |
|------|-------|--------|
| **Top-down** | бродилка WASD | `topdown_*_props_sheet.png` |
| **Combat BG** | lottery fight overlay | `combat_bg_sector_*.png` |

## Sector A — Деревня (baseline)

| Asset | Содержимое |
|-------|------------|
| `combat_bg_sector_a.png` | Серый коридор/двор эксперимента, арки, маски, табличка SECTOR A |
| `topdown_sector_a_sheet.png` | трава, дорога, дом, дверь, колодец, ворота, sign, barrel |

## Sector B — Болото

| Asset | Содержимое |
|-------|------------|
| `combat_bg_sector_b.png` | Болото, туман, зелёный tint, пузыри, доски |
| `topdown_sector_b_sheet.png` | вода, грязь, настил, мост, костёр, fog, bubbles |

## Sector C — Двор (disposal)

| Asset | Содержимое |
|-------|------------|
| `combat_bg_sector_c.png` | Забор, тележки, красное небо, прожектор, SECTOR C DISPOSAL |
| `topdown_sector_c_sheet.png` | забор, тележка, мусор, ворота, прожектор, предупреждение |

## Tile size

Top-down props: **32×32** grid на листе (Unity PPU 32).

## Unstable events (визуал)

| Event | Prop |
|-------|------|
| Туман B | `fog_overlay.png` на листе B |
| Сдвиг прохода | swap barrier/gate tiles |
| Rule табличка | sign tiles на каждом секторе |
