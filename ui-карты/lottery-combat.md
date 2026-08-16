# Lottery Combat — боевая система (не STS)

## Идея в одной фразе

**Билет = право на розыгрыш**, не карта с фиксированным эффектом.  
UNSTABLE **ломает таблицы розыгрыша**, а не только карту мира.

---

## Чем НЕ является

| Slay the Spire | Unstable Experiment |
|----------------|---------------------|
| Колода 15–30 карт, эффект на лице | **Roll** из 10–12 типов билетов |
| Строишь колоду | **Не строишь** — находишь/получаешь билеты в зоне |
| Рука = известные эффекты | Рука = **тип + пул исходов** (частично скрыт) |
| Энергия → сыграл карту | Энергия → **погасил билет** → RNG → эффект |

---

## Core loop боя

```
1. Start: HP 40, Energy 3, Ticket Roll = 12 слотов
2. Draw 4 билета в руку (случайные типы из Roll)
3. Ход игрока:
   - Выбрать билет(ы) → Pay cost → SCRATCH (анимация 0.4s)
   - Roll по таблице исходов → применить эффект
4. End turn → discard все → draw до 4
5. Победа / поражение
```

**Discard pile не reshuffle внутри боя** — билеты **расходуются**.  
Когда Roll пуст → **«Лотерея исчерпана»**: каждый ход auto-roll **Blank** (см. ниже).

Это создаёт **лимит ходов** и давление — не бесконечный цикл Strike/Defend.

---

## UI билета (Protocol Chit v2)

```
┌─────────────────────┐
│ FACILITY [X] 07-0041│
│ α [███]  β [███]  γ [███]
│ [LOT-R AGGRESSIVE]  │
│ cost ●●○  [ REDEEM ]│
└─────────────────────┘
```

- **Sector A:** после 1-го боя снимаем цензуру — видны иконки α/β/γ
- **Sector B+:** γ снова скрыт или веса врут
- **Sector C:** штамп типа подделан → **SPECIMEN MISMATCH**

---

## Стартовый Ticket Roll (12 шт, демка)

| × | ID | Имя | Cost | Пул исходов |
|---|-----|-----|------|-------------|
| 3 | `red` | Protocol Red | 1 | **A** 6 dmg 50% · **B** 4 dmg + Bleed 1 35% · **C** Whiff (0) 15% |
| 3 | `blue` | Protocol Blue | 1 | **A** 5 Block 55% · **B** 8 Block 30% · **C** 3 Block + draw 1 ticket 15% |
| 2 | `green` | Purge Slip | 1 | **A** −all Poison 60% · **B** −Poison + heal 4 25% · **C** −Poison, self 3 dmg 15% |
| 2 | `yellow` | Steady Slip | 1 | **A** +6 Block next turn 50% · **B** +3 Block next + draw 1 35% · **C** next turn Energy −1 15% |
| 1 | `wild` | Wild Ticket | 2 | Roll on **любой** пул Red/Blue/Green (равные веса типа) |
| 1 | `blank` | Blank | 0 | **A** Energy +1 40% · **B** nothing 40% · **C** 5 dmg себе 20% |

**Итого 12** — ~4–6 боёв на сектор при hand 4.

---

## Находки в зоне (вместо «+карта в колоду»)

| Лoot | Эффект |
|------|--------|
| Ticket Pack | +2 случайных билета в Roll **сейчас** |
| Loaded Red | следующий Red **без** исхода C (1 use) |
| Forged Preview | 1 бой видишь точный roll до scratch |
| Antidote | consumable: −all Poison вне боя |

---

## UNSTABLE × билеты (главная фишка)

| Сектор | Rule мира | Rule билетов |
|--------|-----------|--------------|
| **A** | baseline | честные пулы, tutorial раскрывает иконки |
| **B** | +1 Poison start | каждый scratch: **15%** → исход «+1 Poison себе» добавляется поверх |
| **C** | карты лгут | **1 билет в руке** каждый бой — **ложный тип** (Red выглядит Blue, roll по Red) |

### Event-билеты (1 на сектор, optional)

- **Misprint:** один тип в Roll навсегда +10% Whiff
- **Jackpot:** один Wild в Roll → Guaranteed A-tier 1 раз

---

## Враги — остаются читаемыми

Игрок = хаос. Враг = **Intent** (как сейчас на sprite sheet).

| Враг | Intent | Зачем |
|------|--------|-------|
| Subject 03 | Strike 6 / Guard 5 | учит scratch под telegraph |
| Plague | Poison | Purge Slip ценен |
| Subject 12 | Bleed | Red Whiff опасен |
| Butcher | Hook 12 | Roll исчерпан → Blank panic |

---

## Пример боя (3 хода)

**vs Plague, Sector B, уже +1 Poison на старте**

| Ход | Действие | Roll | Result |
|-----|----------|------|--------|
| 1 | Scratch Blue | B | 8 Block |
| 2 | Scratch Green | A | clear Poison |
| 3 | Scratch Red | C | Whiff — Plague ставит +2 Poison |

Игрок не знал ход 3. Purge Slip спас ход 2 — **history matters**, не math колоды.

---

## Unity — минимальная реализация

### Data (ScriptableObject)

```csharp
// TicketDefinition: id, cost, Outcome[] { weight, effectId, icon }
// TicketRoll: List<TicketDefinition> slots remaining
// UnstableTicketModifier: sector rules
```

### Flow

```
CombatManager
  ├── TicketRoll (queue)
  ├── Hand (4 TicketInstance — знает definition + forgedVisual?)
  ├── ScratchTicket(instance) → RollOutcome() → ApplyEffect()
  └── UnstableRules.ModifyPool(definition, sector)
```

### Effect IDs (enum, 8 штук хватит)

`Damage`, `Block`, `Poison`, `Bleed`, `Heal`, `DrawTicket`, `Energy`, `Whiff`

**Без** отдельных Card prefab на 30 карт — **6 TicketDefinition + 1 таблица исходов**.

---

## Scope демки (9 дней)

**Must:** 6 типов билетов, scratch UI, 3 unstable rule на билеты, Roll 12, hand 4  
**Cut:** колода, exhaust, upgrade билетов, shop  
**Art:** `sprites/protocol_chits_sheet.png` — талоны протокола (цензура αβγ, REDEEM).

---

## Одна строка для pitch

> *Your protocol is a lottery ticket — every fight is a scratch-off, and the experiment rewrites the odds.*
