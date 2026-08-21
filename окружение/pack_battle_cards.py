# Composite square grimdark battle cards on magenta chroma.
from pathlib import Path
import importlib.util

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = m.ASSETS
PINK = tuple(int(x) for x in m.PINK)
SIZE = 1024
PAD = 48
CARD = SIZE - PAD * 2
RADIUS = 42

FONT_CYR = [
    Path(r"C:\Windows\Fonts\arialbd.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
    Path(r"C:\all\Концепции\ассеты\Unity\UnstableExperiment\Assets\Resources\Fonts\arial.ttf"),
]


def face(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    paths = FONT_CYR if not bold else FONT_CYR
    for path in paths:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return m.font(size)


def finish_art(path: Path) -> tuple[Image.Image, Image.Image]:
    arr = np.array(Image.open(path).convert("RGB"))
    arr = m.snap_flat_magenta(arr)
    mag = m.magenta_mask(arr)
    arr = m.snap_flat_magenta(m.despill(arr, mag))
    mag = m.magenta_mask(arr)
    arr, mag = m.tight(arr, mag)
    rgb = Image.fromarray(arr)
    alpha = Image.fromarray((~mag).astype(np.uint8) * 255, "L")
    return rgb, alpha


def gradient(size: tuple[int, int], top: tuple[int, int, int], bot: tuple[int, int, int]) -> Image.Image:
    w, h = size
    img = Image.new("RGB", size, top)
    px = img.load()
    for y in range(h):
        t = y / max(h - 1, 1)
        col = tuple(int(top[i] * (1 - t) + bot[i] * t) for i in range(3))
        for x in range(w):
            px[x, y] = col
    return img


def fit_text(draw: ImageDraw.ImageDraw, text: str, font_size: int, max_w: int) -> ImageFont.FreeTypeFont:
    size = font_size
    while size > 18:
        f = face(size, bold=True)
        bbox = draw.textbbox((0, 0), text, font=f)
        if bbox[2] - bbox[0] <= max_w:
            return f
        size -= 2
    return face(18, bold=True)


def wrap(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont, max_w: int) -> list[str]:
    words = text.split()
    lines: list[str] = []
    cur = ""
    for word in words:
        trial = word if not cur else f"{cur} {word}"
        bbox = draw.textbbox((0, 0), trial, font=font)
        if bbox[2] - bbox[0] <= max_w:
            cur = trial
        else:
            if cur:
                lines.append(cur)
            cur = word
    if cur:
        lines.append(cur)
    return lines or [text]


def octagon(cx: int, cy: int, r: int) -> list[tuple[int, int]]:
    k = int(r * 0.42)
    return [
        (cx - r + k, cy - r),
        (cx + r - k, cy - r),
        (cx + r, cy - r + k),
        (cx + r, cy + r - k),
        (cx + r - k, cy + r),
        (cx - r + k, cy + r),
        (cx - r, cy + r - k),
        (cx - r, cy - r + k),
    ]


RARITY_ROCKS = {
    "common": {
        "border": (132, 46, 34),
        "inner": (34, 30, 30),
        "ribbon": (92, 88, 86),
        "art_top": (62, 28, 18),
        "art_bot": (22, 12, 12),
        "tag": (168, 164, 160),
        "rare_line": (176, 132, 48),
        "epic_line": (96, 18, 18),
    },
    "rare": {
        "border": (128, 86, 28),
        "inner": (30, 26, 22),
        "ribbon": (78, 68, 42),
        "art_top": (72, 48, 16),
        "art_bot": (24, 16, 10),
        "tag": (196, 168, 92),
        "rare_line": (176, 132, 48),
        "epic_line": (96, 18, 18),
    },
    "epic": {
        "border": (18, 10, 10),
        "inner": (16, 12, 12),
        "ribbon": (52, 18, 18),
        "art_top": (48, 10, 12),
        "art_bot": (12, 6, 8),
        "tag": (168, 48, 42),
        "rare_line": (176, 132, 48),
        "epic_line": (96, 18, 18),
    },
}

RARITY_CISTERN = {
    "common": {
        "border": (42, 78, 72),
        "inner": (16, 22, 22),
        "ribbon": (48, 64, 62),
        "art_top": (22, 48, 46),
        "art_bot": (8, 14, 16),
        "tag": (148, 176, 168),
        "rare_line": (132, 168, 118),
        "epic_line": (72, 28, 28),
    },
    "rare": {
        "border": (48, 92, 68),
        "inner": (14, 22, 20),
        "ribbon": (36, 58, 48),
        "art_top": (28, 62, 44),
        "art_bot": (8, 16, 14),
        "tag": (168, 196, 140),
        "rare_line": (156, 196, 120),
        "epic_line": (72, 28, 28),
    },
    "epic": {
        "border": (10, 16, 16),
        "inner": (10, 14, 14),
        "ribbon": (28, 36, 34),
        "art_top": (18, 36, 32),
        "art_bot": (6, 8, 10),
        "tag": (120, 72, 58),
        "rare_line": (156, 196, 120),
        "epic_line": (86, 24, 24),
    },
}

PRESETS = {
    "rocks": {
        "out": "battle_cards",
        "art": "card_art_{slug}.png",
        "sheet": "battle_cards_sheet.png",
        "rarity": RARITY_ROCKS,
    },
    "cistern": {
        "out": "battle_cards_cistern",
        "art": "card_art_cistern_{slug}.png",
        "sheet": "battle_cards_cistern_sheet.png",
        "rarity": RARITY_CISTERN,
    },
}

CARDS = [
    {"slug": "knife", "title": "Knife", "cost": 1, "kind": "АТАКА", "text": "Наносит 7 урона.", "rarity": "common"},
    {"slug": "slash", "title": "Slash", "cost": 1, "kind": "АТАКА", "text": "Наносит 10 урона.", "rarity": "common"},
    {"slug": "quick_stab", "title": "Quick Stab", "cost": 0, "kind": "АТАКА", "text": "Наносит 4 урона.", "rarity": "common"},
    {"slug": "bleeding_cut", "title": "Bleeding Cut", "cost": 1, "kind": "АТАКА", "text": "6 урона + 2 Bleed.", "rarity": "common"},
    {"slug": "double_slash", "title": "Double Slash", "cost": 2, "kind": "АТАКА", "text": "2×7 урона.", "rarity": "rare"},
    {"slug": "executioner", "title": "Executioner", "cost": 2, "kind": "АТАКА", "text": "14 урона. Если враг <30% HP — ещё 15.", "rarity": "rare"},
    {"slug": "bloodletting", "title": "Bloodletting", "cost": 1, "kind": "АТАКА", "text": "18 урона. Получить 2 Bleed.", "rarity": "rare"},
    {"slug": "piercing_strike", "title": "Piercing Strike", "cost": 2, "kind": "АТАКА", "text": "12 урона, игнорирует Block.", "rarity": "rare"},
    {"slug": "decapitation", "title": "Decapitation", "cost": 3, "kind": "АТАКА", "text": "30 урона. Если враг <20% HP — убить.", "rarity": "epic"},
]


def draw_card(spec: dict, preset: dict) -> Image.Image:
    pal = preset["rarity"][spec["rarity"]]
    canvas = Image.new("RGB", (SIZE, SIZE), PINK)
    draw = ImageDraw.Draw(canvas)
    x0 = y0 = PAD
    x1 = y1 = PAD + CARD

    draw.rounded_rectangle((x0, y0, x1, y1), radius=RADIUS, fill=pal["border"])
    inset = 22
    ix0, iy0, ix1, iy1 = x0 + inset, y0 + inset, x1 - inset, y1 - inset
    draw.rounded_rectangle((ix0, iy0, ix1, iy1), radius=28, fill=pal["inner"])
    if spec["rarity"] == "epic":
        draw.rounded_rectangle(
            (ix0 + 4, iy0 + 4, ix1 - 4, iy1 - 4),
            radius=24,
            outline=pal["epic_line"],
            width=4,
        )
    elif spec["rarity"] == "rare":
        draw.rounded_rectangle(
            (ix0 + 4, iy0 + 4, ix1 - 4, iy1 - 4),
            radius=24,
            outline=pal["rare_line"],
            width=3,
        )

    ribbon_h = 72
    ry = iy0 + 18
    draw.rounded_rectangle((ix0 + 86, ry, ix1 - 18, ry + ribbon_h), radius=10, fill=pal["ribbon"])
    title_font = fit_text(draw, spec["title"], 44, ix1 - ix0 - 130)
    tb = draw.textbbox((0, 0), spec["title"], font=title_font)
    tw, th = tb[2] - tb[0], tb[3] - tb[1]
    draw.text(
        (ix0 + 86 + (ix1 - 18 - (ix0 + 86) - tw) // 2, ry + (ribbon_h - th) // 2 - 4),
        spec["title"],
        fill=(245, 245, 242),
        font=title_font,
    )

    art_top = ry + ribbon_h + 16
    art_h = 430
    ax0, ay0, ax1, ay1 = ix0 + 22, art_top, ix1 - 22, art_top + art_h
    draw.rounded_rectangle((ax0 - 6, ay0 - 6, ax1 + 6, ay1 + 6), radius=12, fill=(62, 58, 56))
    art_bg = gradient((ax1 - ax0, ay1 - ay0), pal["art_top"], pal["art_bot"])
    canvas.paste(art_bg, (ax0, ay0))

    rgb, alpha = finish_art(ASSETS / preset["art"].format(slug=spec["slug"]))
    box_w, box_h = ax1 - ax0 - 20, ay1 - ay0 - 16
    scale = min(box_w / rgb.width, box_h / rgb.height)
    nw, nh = max(1, int(rgb.width * scale)), max(1, int(rgb.height * scale))
    rgb = rgb.resize((nw, nh), Image.Resampling.LANCZOS)
    alpha = alpha.resize((nw, nh), Image.Resampling.LANCZOS)
    px = ax0 + (ax1 - ax0 - nw) // 2
    py = ay0 + (ay1 - ay0 - nh) // 2
    canvas.paste(rgb, (px, py), alpha)

    tag_w, tag_h = 168, 36
    tx = (ix0 + ix1 - tag_w) // 2
    ty = ay1 + 14
    draw.rounded_rectangle((tx, ty, tx + tag_w, ty + tag_h), radius=4, fill=pal["tag"])
    kind_font = face(20, bold=True)
    kb = draw.textbbox((0, 0), spec["kind"], font=kind_font)
    draw.text(
        (tx + (tag_w - (kb[2] - kb[0])) // 2, ty + (tag_h - (kb[3] - kb[1])) // 2 - 2),
        spec["kind"],
        fill=(18, 16, 16),
        font=kind_font,
    )

    body_font = face(28)
    lines = wrap(draw, spec["text"], body_font, ix1 - ix0 - 56)
    text_top = ty + tag_h + 28
    line_h = 36
    total_h = len(lines) * line_h
    start_y = text_top + max(0, (iy1 - 28 - text_top - total_h) // 2)
    for i, line in enumerate(lines):
        lb = draw.textbbox((0, 0), line, font=body_font)
        lw = lb[2] - lb[0]
        draw.text(((ix0 + ix1 - lw) // 2, start_y + i * line_h), line, fill=(236, 232, 228), font=body_font)

    gem_c = (ix0 + 8, iy0 + 18)
    r = 46
    gem = octagon(gem_c[0] + r, gem_c[1] + r, r)
    draw.polygon(gem, fill=(148, 22, 18))
    inner = octagon(gem_c[0] + r, gem_c[1] + r, r - 8)
    draw.polygon(inner, fill=(188, 42, 28), outline=(214, 168, 64), width=3)
    cost = str(spec["cost"])
    cost_font = face(46, bold=True)
    cb = draw.textbbox((0, 0), cost, font=cost_font)
    draw.text(
        (gem_c[0] + r - (cb[2] - cb[0]) // 2, gem_c[1] + r - (cb[3] - cb[1]) // 2 - 6),
        cost,
        fill=(255, 255, 252),
        font=cost_font,
    )
    return canvas


def catalog(tiles: list[Image.Image]) -> Image.Image:
    display = 420
    gap_x, gap_y, label_h, margin = 28, 48, 30, 28
    cols, rows = 3, 3
    sheet = Image.new(
        "RGB",
        (margin * 2 + cols * (display + gap_x) - gap_x, margin * 2 + rows * (display + label_h + gap_y) - gap_y),
        m.CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    lab = face(20)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, cols)
        x = margin + col * (display + gap_x)
        y = margin + row * (display + label_h + gap_y)
        sheet.paste(tile.resize((display, display), Image.Resampling.LANCZOS), (x, y))
        name = CARDS[i]["title"]
        bbox = draw.textbbox((0, 0), name, font=lab)
        draw.text((x + (display - (bbox[2] - bbox[0])) // 2, y + display + 6), name, fill=m.LABEL, font=lab)
    return sheet


def main() -> None:
    import sys

    name = sys.argv[1] if len(sys.argv) > 1 else "rocks"
    if name not in PRESETS:
        raise SystemExit(f"unknown set {name}, use: {', '.join(PRESETS)}")
    preset = PRESETS[name]
    out = m.OUT / preset["out"]
    out.mkdir(parents=True, exist_ok=True)
    tiles = []
    for spec in CARDS:
        card = draw_card(spec, preset)
        dest = out / f"card_{spec['slug']}.png"
        card.save(dest, optimize=True)
        tiles.append(card)
        print("saved", dest.name)
    sheet = catalog(tiles)
    sheet_path = out / "battle_cards_sheet.png"
    sheet.save(sheet_path, optimize=True)
    sheet.save(m.OUT / preset["sheet"], optimize=True)
    sheet.save(ASSETS / preset["sheet"], optimize=True)
    print("sheet", sheet_path)


if __name__ == "__main__":
    main()
