# UI — lottery combat (Protocol Chits)

> Механика: `lottery-combat.md` · Визуал v2: `protocol-chits-visual.md` · Данные: `data/tickets.json`

## Формат

| Поле | Размер |
|------|--------|
| Chit в руке | **128×192** px |
| Окно исхода α/β/γ | 32×32 |
| Intent врага | 64×64 |

**Стиль v2:** термобумага, **цензура `[███]`**, штамп LOT-R/B/G…, **REDEEM**, уколы ●●○ = cost.  
**Не** casino scratch-card.

## Файлы

| Файл | Статус |
|------|--------|
| `sprites/protocol_chits_sheet.png` | **актуальный** |
| `data/tickets.json` | без изменений |

## HUD (термины)

- Кнопка **REDEEM** вместо SCRATCH
- Flash **SPECIMEN MISMATCH** (Sector C forged chit)
