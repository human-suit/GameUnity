# Subject 07 — главный герой

## Идентичность

| | |
|---|---|
| **Имя в UI** | Subject 07 |
| **Полное** | Объект №7 (имя стёрто протоколом) |
| **Роль** | Подопытный, «устойчивость в нестабильной среде» |
| **Не путать с** | Hollow Wanderer (меч, плащ — другой персona) |

## Визуал (Fear & Hunger pixel)

- Худой, **не** воин — жертва/выживший
- **Больничная рубашка** серая, номер **07** на груди (кровью или маркером)
- **Сорванный воротник-датчик** — провода торчат, 1–2 electrode patches на виске/шее
- Короткие тёмные волосы, **бритый затылок** или одна сторона короче (след эксперимента)
- Босиком или в тапочках/bandages на ногах
- Пустой взгляд → в EXPRESSIONS нарастает отчаяние
- Палитра: `#D8D4CC` рубашка, `#4A4A52` номер, `#C4A898` кожа, `#3D5A4A` провода, `#2E2A26` тени

## Silhouette

Узкий силуэт, плечи вперёд, легко отличить от Butcher (широкий) и от Wanderer (плащ+меч).

## Анимации (sprite sheet)

| Row | Назначение |
|-----|------------|
| TURN | front / side / back |
| IDLE | 6 |
| WALK | 8 (top-down или side — **side для Unity**) |
| ACTION | 4 — смотрит на «карту» в руке / протокол |
| STAGGER | 3 |
| FADE | 5 — падает, static на краю экрана |
| EXPRESSIONS | Neutral, Focus, Worried, Angry, Scared, Injured |
| BODY PARTS | head, torso, legs, smock, collar cable |

## Лор (1 абзац)

Проснулись в Sector A без памяти. На запястье бирка «07». Голос по динамику считает выживание. Карты в бою — обрывки **протокола**: Block, Strike, Purge Poison — не магия, а вшитые инструкции тела.

## Gameplay stats (демка)

| Stat | Value |
|------|-------|
| HP | 40 |
| Start deck | 8 protocol cards |
| Speed walk | normal |

## Файлы

- `sprite-sheet/subject_07_unified_sheet.png`
- `sprite-sheet/prompt_unified.txt`
