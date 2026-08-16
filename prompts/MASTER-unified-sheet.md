# MASTER STYLE LOCK v5 — все правки сессии

> Эталон layout: `demo/ward_hulk_merged_sheet_v2.png`  
> Эталон рисовки: `окружение/fh-style/references/combat_ogre_master.png`  
> Walk: `враги/ward-hulk/walk-cycle-rules.md`  
> **Subject 07 не трогаем** — только враги.

---

## 1. Layout (каждый лист)

```
TOP LEFT:     HEALTH [N] + 7 palette swatches
TOP CENTER:   FRONT + BACK (large labeled)
TOP RIGHT:    BODY PARTS (labeled sprites)
MID:          EXPRESSIONS ×4 busts
              COMBAT CARDS ×3 (damage/effect text)
              ITEMS ×3 icons
BOTTOM:
  IDLE 6          side view right
  WALK RIGHT 8    alternating R-L-R-L
  WALK LEFT 8     mirror
  WALK FRONT 8    alternating — NOT same leg every frame
  WALK BACK 8     alternating from behind
  ACTION 4        signature attack
  STAGGER 3
  DEATH 5
```

---

## 2. Рисовка (F&H hand-drawn)

| Да | Нет |
|----|-----|
| Толстый чёрный sketchy контур | Crisp pixel grid |
| Mottled bruise/patch skin + block shadows | Cobblestone stipple на коже |
| Desaturated grey/brown/tan | Яркие полоски, neon |
| Тёмные впалые глаза (humanoid) | Белые глаза без зрачков (если не задумано) |
| Cross-hatch только в тени/одежде | Engraved texture на всей коже |
| Лёгкий soft blur / atmospheric | White glow outline |
| **Один стиль на ВСЁМ листе** | FRONT ok, BACK/parts другая техника |

---

## 3. Walk cycle (обязательно)

8 кадров = 4 шага **R → L → R → L**

| Кадр | WALK FRONT | WALK BACK | WALK RIGHT |
|------|------------|-----------|------------|
| 1 | R вперёд, L назад | R шаг | R contact |
| 2 | passing | passing | R up |
| 3 | **L** вперёд | **L** шаг | L contact |
| 4 | passing | passing | L up |
| 5–8 | повтор | повтор | повтор |

Feet on **baseline**. Side walk: plodding shuffle, тяжёлый шаг.

---

## 4. Static sections (качество «картинки 2»)

- FRONT/BACK: полный рост, читаемый силуэт
- BODY PARTS: отдельные спрайты с labels
- EXPRESSIONS: 4 bust круглых/квадратных иконок
- COMBAT CARDS: рамка + мини-иллюстрация + число

---

## 5. Animations (качество «картинки 1»)

- IDLE: subtle breathe, hunch
- ACTION: читаемая атака (не размыта)
- STAGGER: отшат назад
- DEATH: collapse face-down, hold last frame

---

## 6. Персонажи — визуал lock

### Ward Hulk (HP 28)
Обе руки, **нет stump**. Правая массивная. Cleaver в руке. Harness+ring. Kilt+loincloth. Бинты. Mottled bruise skin.

### Subject 03 (HP 20)
Грязная smock **03**, бирка, сильный hunch, без проводов. ACTION: слабый удар.

### Plague (HP 32)
Маска, зелёная слизь, плащ, green-gray skin. ACTION: cough vapor.

### Subject 12 (HP 44)
Rib mask, crystals, bone scythe arm, yellow pustules. ACTION: scythe slash.

### Patchwork Butcher (HP 85)
Самый широкий. Stitched patches, hook+scissors, stuffing, apron, button eye.

### Mask Wretch (HP 18)
Stone screaming mask, claws, thin, bare torso, rags.

### Pit Dweller (HP 14)
Low crawl, wet dark skin, white eyes (exception), small.

---

## 7. Master generation prompt

```
ONE unified character asset sheet black background.

STYLE: Fear and Hunger hand-drawn. Thick black sketchy outlines. Mottled bruise skin block shadows. NOT cobblestone stipple skin. NOT white glow outline. SAME style FRONT BACK parts ALL animation rows. Slight soft blur.

LAYOUT: [full layout from section 1]

WALK: 8 frames each direction. WALK FRONT and BACK MUST alternate legs frame1 R forward frame3 L forward frame5 R frame7 L frames 2 4 6 8 passing. NEVER same leg forward every frame.

CHARACTER: [name HP visual parts expressions cards items action]

Labels every section. One image.
```

---

## 8. Файлы (v5)

| ID | Output |
|----|--------|
| ward-hulk | ward_hulk_unified_v5.png |
| subject-03 | subject_03_unified_v5.png |
| plague | plague_unified_v5.png |
| subject-12 | subject_12_unified_v5.png |
| patchwork-butcher | patchwork_butcher_unified_v5.png |
| mask-wretch | mask_wretch_unified_v5.png |
| pit-dweller | pit_dweller_unified_v5.png |

Subject 07 — **без изменений** (старый unified ok).
