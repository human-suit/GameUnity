# STYLE LOCK — Ogre / Ward Hulk (эталон #1)

> Единственный референс рисовки: `окружение/fh-style/references/combat_ogre_master.png`  
> Структура листа: Subject 03 sheet layout.

---

## LOCK — персонаж

| Параметр | Значение |
|----------|----------|
| Имя | WARD HULK |
| Тело | Огромный, hunched, **асимметричная** мускулатура |
| Кожа | Бледная + **красновато-коричневые пятна/синяки**, blob-mottled |
| Текстура | **НЕ** cobblestone-stipple, **НЕ** fine engraving |
| Тени | Крупные чёрные блоки + грязные пятна highlight |
| Контур | Толстый чёрный, неровный, sketchy |
| Голова | Маленькая, лысая, **впалые тёмные глаза**, один глаз припухший |
| Глаза | **НЕ** белые без зрачков |
| Руки | **ОБЕ руки** — правая массивная/длинная, **cleaver в ЛЕВОЙ** |
| Stump | **НЕТ** (stump = другой дизайн, не эталон) |
| Одежда | Leather X-harness, metal ring, тёмный pleated kilt |
| Ноги | Бинты/тряпки на голенях, босиком |
| Оружие | Прямоугольный meat cleaver, кровь по краю |
| Blur | Лёгкий soft focus, не crisp pixel |

## LOCK — палитра (7 swatches)

`#2A2520` `#4A4038` `#6A5A4A` `#9A8070` `#C4A898` `#8B4A3A` `#1A1A1A`

## LOCK — лист (layout)

```
HEALTH 28 | palette
FRONT + BACK                    ← реф. static (карт.2)
BODY PARTS + EXPRESSIONS        ← реф. static (карт.2)
COMBAT CARDS + ITEMS

IDLE 6 | side
WALK RIGHT 8 | WALK LEFT 8      ← реф. anim (карт.1)
WALK FRONT 6 | WALK BACK 6      ← NEW 4-dir
ACTION 4 | STAGGER 3 | DEATH 5  ← реф. anim (карт.1)
```

## ANTI-patterns (запрещено)

- Cobblestone/dot skin all over
- **Разный стиль FRONT vs BACK / parts / anims** ← частая ошибка
- White pupilless eyes
- Bloody arm stump
- Clean smooth gradients
- Sharp pixel grid
- White glow outline around character

---

## Master prompt (copy for generation)

```
Hand-drawn Fear and Hunger grimdark character asset sheet on black background.

STYLE LOCK from combat reference: thick rough black sketchy outlines, desaturated muddy grey brown palette, pale skin with REDDISH-BROWN mottled bruise patches and blobby highlights, large block black shadows NOT fine stipple cobblestone texture NOT engraved cross-hatch skin.

WARD HULK character EXACT design: massive hunched asymmetric mutant BOTH ARMS NO STUMP, right arm extremely massive long, LEFT hand holds large rectangular meat cleaver with blood, small bald head sunken DARK eyes one eye swollen shut NOT white eyes, leather X harness metal ring, dark pleated kilt, bandages on shins barefoot.

Full labeled sprite sheet: HEALTH 28 palette 7 swatches, FRONT BACK large, BODY PARTS, EXPRESSIONS Blank Hostile Chop Tired, side animations IDLE 6 WALK 8 ACTION 4 STAGGER 3 DEATH 5, COMBAT CARDS Cleave Chop Brace, ITEMS. Slight soft blur atmospheric. One image.
```
