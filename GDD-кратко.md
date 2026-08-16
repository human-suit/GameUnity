# GDD (кратко) — Unstable Experiment

## Pitch

Объект эксперимента должен пройти 3 нестабильных сектора (деревня → болото → двор) и выбраться. Монстры на карте, бой картами. Правила меняются каждый сектор.

## ГГ

**Subject 07** — не Wanderer. Пациент/подопытный в рубашке с номером, сорванные датчики. Без меча — карты = «протоколы» эксперимента.

## Core loop

1. **Комнаты:** top-down WASD, каждая локация — отдельная комната с **дверями на краях**
2. **Карта маршрута:** находишь в деревне → Route Map как STS (узлы/пути), но переход **только через дверь**
3. Aggro → **lottery combat** (HP 40, energy 3, hand 4 **билета**, Roll 12)
4. Дойти до exit-door → новый сектор + Unstable Rule
5. Сектор 3 → босс → 2 концовки

→ Полная модель мира: **`КОНЦЕПЦИЯ-ИГРЫ.md`** · граф: **`игра/data/rooms_graph.json`**

## Бой = лотерея (не STS)

Билет ≠ фиксированная карта. **Scratch** → roll из пула исходов (A/B/C).  
Roll расходуется за бой → давление на длину fight.

→ Полная таблица: `ui-карты/lottery-combat.md`

### Ticket Roll (12)

| Тип | × | Cost | Суть |
|-----|---|------|------|
| Protocol Red | 3 | 1 | dmg pool |
| Protocol Blue | 3 | 1 | block pool |
| Purge Slip | 2 | 1 | anti-poison pool |
| Steady Slip | 2 | 1 | next-turn pool |
| Wild Ticket | 1 | 2 | random pool |
| Blank | 1 | 0 | chaos |

## Unstable (3 сектора)

| Сектор | Rule |
|--------|------|
| A Деревня | (нет — tutorial) |
| B Болото | Яд +1 стак в начале боя |
| C Двор | 1 карта в руке «лжёт» каждый бой |

## Враги (демка)

| Враг | Сектор | HP | Фишка |
|------|--------|-----|-------|
| Subject 03 | A, B, C | 20 | Strike / Guard — tutorial |
| Ward Hulk | A, C | 28 | Cleave / Chop (FH battle art) |
| Mask Wretch | A, B | 18 | Claw / Scream (FH battle art) |
| Plague | B | 32 | Poison (main path) |
| Subject 12 | B | 44 | Scythe / Bleed (side path) |
| Pit Dweller | B | 14 | Bite / Hide (FH battle art) |
| Patchwork Butcher | C | 85 | Hook / Snip / Stuffing |

## Cut

- Mind / panic
- Замок, маленькие комнаты → **теперь core:** комнаты + двери (см. `КОНЦЕПЦИЯ-ИГРЫ.md`)
- Slay the Spire map → **Route Map** (планирование, не телепорт)
- Deep deck building
