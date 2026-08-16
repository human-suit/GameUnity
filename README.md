# Unstable Experiment

**2D horror run (~10–15 мин)** — подопытный **Subject 07** проходит три сектора экспериментального комплекса, сражается **protocol chits** (лотерея, не колода Slay the Spire) и пытается выбраться. Стиль: **Fear & Hunger** hand-drawn.

Репозиторий содержит **полный дизайн, JSON-данные, арт и документацию** для сборки игры в Unity. Код Unity — следующий этап; здесь source of truth для двух разработчиков (код + арт/level).

---

## О чём игра

Subject 07 — не герой-RPG, а **объект протокола**: рубашка с номером, сорванные датчики, без меча. Мир — заброшенный комплекс из трёх зон:

| Сектор | Атмосфера | UNSTABLE-правило |
|--------|-----------|------------------|
| **A — Деревня** | Серый baseline, tutorial | Стабильно |
| **B — Болото** | Зелёный toxin, яд | +1 Poison в начале каждого боя |
| **C — Двор** | Красный disposal, босс | 1 билет в руке «лжёт» каждый бой |

Финал: **Patchwork Butcher** у ворот → две концовки (побег / остаться).

---

## Как играется (core loop)

```
Комната (WASD) → aggro → lottery-бой → лут / ключ
       ↓
Route Map (Tab) — схема пути как в STS, но БЕЗ телепорта
       ↓
Дверь на краю комнаты [E] → следующая локация
       ↓
Exit-door сектора → новый сектор + новые правила
```

### Три системы — не путать

| Система | Что это | Не путать с |
|---------|---------|-------------|
| **Overworld** | Top-down комнаты, двери, патрули | Одной большой open-world картой |
| **Route Map** | Бумажная карта маршрута, узлы и подсказки | Телепортом по клику (как STS) |
| **Lottery combat** | 12 билетов на run, REDEEM → RNG A/B/C | Колодой карт STS |

**Переход между локациями — только через дверь.** Карта показывает, *куда* идти, а не переносит игрока.

---

## Что уже есть в репозитории

| Категория | Статус | Где |
|-----------|--------|-----|
| Master-концепт игры | ✅ | [`КОНЦЕПЦИЯ-ИГРЫ.md`](КОНЦЕПЦИЯ-ИГРЫ.md) |
| Граф комнат A/B/C | ✅ | [`игра/data/rooms_graph.json`](игра/data/rooms_graph.json) |
| Боевая система + JSON | ✅ | [`ui-карты/lottery-combat.md`](ui-карты/lottery-combat.md), `ui-карты/data/` |
| 8 персонажей (sprite sheets) | ✅ | `герои/`, `враги/` |
| Окружение 3 секторов | ✅ | `окружение/sector-*/` |
| Комната + Route Map (Sector A) | ✅ | `игра/rooms/sector-a/`, `игра/ui/` |
| Unity demo | ✅ scaffold | `Unity/UnstableExperiment/` |

**Галерея ассетов:** открыть [`demo/index.html`](demo/index.html) в браузере.

---

## Быстрый старт для разработчиков

### 1. Клонировать

```bash
git clone https://github.com/human-suit/GameUnity.git
cd GameUnity
```

### 2. Прочитать (по порядку)

1. **[`НАЧАЛО-РАБОТЫ.md`](НАЧАЛО-РАБОТЫ.md)** — карта всех документов, промпты, разделение задач на 2 человек  
2. **[`КОНЦЕПЦИЯ-ИГРЫ.md`](КОНЦЕПЦИЯ-ИГРЫ.md)** — комнаты, двери, Route Map, ТЗ для Unity  
3. **[`GDD-кратко.md`](GDD-кратко.md)** — pitch, враги, scope демки  

### 3. Unity (playable demo)

```bash
# Открыть в Unity Hub:
Unity/UnstableExperiment
```

Menu: **Unstable Experiment → Create Demo Scene** → Play.

Подробно: [`Unity/UnstableExperiment/README.md`](Unity/UnstableExperiment/README.md)

### 4. Разделение работы

| Dev — Code (Unity) | Dev — Art / Level |
|--------------------|-------------------|
| RoomLoader по `rooms_graph.json` | Tilemap комнат (32px PPU) |
| Двери, ключи, переходы | Unified sprite sheets |
| Lottery combat UI | Combat BG ×3 сектора |
| Route Map overlay (Tab) | Route Map UI из mockup |
| `tickets.json` + `unstable_rules.json` | Новые комнаты по графу |

**Sync point:** меняется `rooms_graph.json` или `КОНЦЕПЦИЯ-ИГРЫ.md` → второй dev подтягивает.

---

## Структура репозитория

```
├── НАЧАЛО-РАБОТЫ.md          ← вход для команды
├── КОНЦЕПЦИЯ-ИГРЫ.md         ← master game design
├── GDD-кратко.md
├── demo/                     ← HTML-галерея ассетов
├── игра/
│   ├── data/rooms_graph.json ← граф мира (source of truth)
│   ├── rooms/sector-a/       ← комнаты (a_plaza готова)
│   └── ui/                   ← Route Map mockup
├── герои/subject-07/         ← ГГ
├── враги/                    ← 7 врагов + boss
├── окружение/                ← фоны боя + top-down props
├── ui-карты/                 ← lottery combat, chits, JSON
├── зоны/                     ← lore секторов A/B/C
└── prompts/                  ← промпты стиля арта (F&H v5)
```

---

## Бой — lottery, не deck builder

- **Ticket Roll:** 12 слотов на run (Red, Blue, Purge, Steady, Wild, Blank)
- **Рука:** 4 билета, **Energy:** 3
- **REDEEM** → случайный исход **A / B / C** из пула типа
- Билеты **расходуются** за бой — нельжен бесконечный Strike/Defend
- Визуал: **protocol chits** (термобумага, цензура `[███]`), не casino tickets

Подробно: [`ui-карты/lottery-combat.md`](ui-карты/lottery-combat.md)

---

## Мир — комнаты и двери

Сектор = **5–8 отдельных комнат**, не одна карта 40×30.

**Sector A (пример):**

```
        [Колодец]
            |
[Ворота]—[Площадь]—[Переулок]
            |
          [Дом] → ключ + карта маршрута
```

- Старт: `a_plaza` — hub с 4 дверями  
- В доме: **Rust Key** + **Sector Map A** → открывается Route Map (Tab)  
- Ворота locked до ключа → переход в Sector B  

Данные: [`игра/data/rooms_graph.json`](игра/data/rooms_graph.json)

---

## Арт-стиль

**Fear & Hunger hand-drawn** — единый на персонажах и окружении:

- Толстый чёрный контур, desaturated grey/brown  
- Mottled skin, block shadows, лёгкий soft blur  
- **Не pixel art**, не cobblestone stipple на коже  

Эталон: [`prompts/MASTER-unified-sheet.md`](prompts/MASTER-unified-sheet.md)

---

## Технические параметры (Unity)

| Параметр | Значение |
|----------|----------|
| Tile size | 32×32 px, PPU 32 |
| Движение | 4 направления, top-down |
| Комнаты | Отдельная scene/prefab на `room_id` |
| Данные | JSON → ScriptableObjects |
| Combat | Overlay поверх overworld |

---

## Git workflow

```bash
git checkout -b feature/unity-room-loader   # код
git checkout -b art/room-a-home             # арт
# merge → main через PR или локально
git pull origin main
```

`.gitignore` уже настроен под Unity (`Library/`, `Temp/`, …).

---

## Roadmap

- [x] Game design + room graph + combat data  
- [x] Character & environment art (v5)  
- [x] Room `a_plaza` + Route Map mockup (Sector A)  
- [x] **Unity demo scaffold** → [`Unity/UnstableExperiment/`](Unity/UnstableExperiment/)  
- [ ] Tilemap art import (replace procedural placeholders)  
- [ ] Route Map UI polish  
- [ ] Full Sector B/C playable  
- [ ] Boss + endings  

---

## Лицензия и статус

Проект в активной разработке. Демо ~10–15 минут геймплея.  
Вопросы по дизайну → `КОНЦЕПЦИЯ-ИГРЫ.md`. По онбордингу → `НАЧАЛО-РАБОТЫ.md`.
