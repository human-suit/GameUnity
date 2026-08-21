# Split fence object strips into separate sprites (last-photo style, magenta key).
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
MAGENTA = (255, 0, 255)
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
DISPLAY = 220
GAP_X, GAP_Y = 48, 56
LABEL_H = 28
MARGIN = 40
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
SETS = [
    {
        "slug": "fence_h",
        "prefix": "fence_h",
        "rows": ["fence_obj_h_row_a.png", "fence_obj_h_row_b.png", "fence_obj_h_row_c.png"],
    },
    {
        "slug": "fence_v",
        "prefix": "fence_v",
        "rows": ["fence_side_row_a.png", "fence_side_row_b.png", "fence_side_row_c.png"],
    },
]


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def is_magenta(arr: np.ndarray) -> np.ndarray:
    r, g, b = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2]
    return (r > 180) & (b > 180) & (g < 140) & (r + b > g + 180)


def content_mask(arr: np.ndarray) -> np.ndarray:
    return ~is_magenta(arr) & (arr.max(axis=2) > 18)


def runs_from(fill, min_len: int, gap: int):
    raw = []
    start = None
    for i, v in enumerate(fill):
        if v and start is None:
            start = i
        elif not v and start is not None:
            raw.append((start, i))
            start = None
    if start is not None:
        raw.append((start, len(fill)))
    if not raw:
        return []
    merged = [list(raw[0])]
    for a, b in raw[1:]:
        if a - merged[-1][1] <= gap:
            merged[-1][1] = b
        else:
            merged.append([a, b])
    return [(a, b) for a, b in merged if (b - a) >= min_len]


def split_three(im: Image.Image) -> list[Image.Image]:
    arr = np.array(im.convert("RGB"))
    m = content_mask(arr)
    cols = runs_from(m.mean(0) > 0.02, 24, 12)
    if len(cols) > 3:
        cols = sorted(sorted(cols, key=lambda r: r[1] - r[0], reverse=True)[:3], key=lambda r: r[0])
    h, w = arr.shape[:2]
    if len(cols) != 3:
        cw = w // 3
        cols = [(i * cw, (i + 1) * cw if i < 2 else w) for i in range(3)]
    out = []
    for a, b in cols:
        tile = arr[:, max(0, a - 4) : min(w, b + 4)]
        cm = content_mask(tile)
        if not cm.any():
            continue
        ys, xs = np.where(cm)
        pad = 6
        y0 = max(0, int(ys.min()) - pad)
        x0 = max(0, int(xs.min()) - pad)
        y1 = min(tile.shape[0], int(ys.max()) + 1 + pad)
        x1 = min(tile.shape[1], int(xs.max()) + 1 + pad)
        crop = tile[y0:y1, x0:x1]
        rgba = np.zeros((crop.shape[0], crop.shape[1], 4), np.uint8)
        rgba[..., :3] = crop
        rgba[..., 3] = np.where(is_magenta(crop), 0, 255)
        out.append(Image.fromarray(rgba, "RGBA"))
    return out


def catalog(items: list[tuple[str, Image.Image]]) -> Image.Image:
    face = font(18)
    n = len(items)
    cols = 3
    rows = (n + cols - 1) // cols
    cell_w = DISPLAY + GAP_X
    cell_h = DISPLAY + LABEL_H + GAP_Y
    sheet = Image.new(
        "RGB",
        (MARGIN * 2 + cols * cell_w - GAP_X, MARGIN * 2 + rows * cell_h - GAP_Y + 8),
        CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    for i, (name, im) in enumerate(items):
        row, col = divmod(i, cols)
        x = MARGIN + col * cell_w
        y = MARGIN + row * cell_h
        shown = im.convert("RGBA")
        shown.thumbnail((DISPLAY - 8, DISPLAY - 8), Image.Resampling.LANCZOS)
        cell = Image.new("RGBA", (DISPLAY, DISPLAY), (*CAT_BG, 255))
        cell.alpha_composite(
            shown,
            ((DISPLAY - shown.size[0]) // 2, DISPLAY - shown.size[1] - 8),
        )
        sheet.paste(cell.convert("RGB"), (x, y))
        bbox = draw.textbbox((0, 0), name, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + max(0, (DISPLAY - tw) // 2), y + DISPLAY + 8), name, fill=LABEL, font=face)
    return sheet


def pack(spec: dict) -> None:
    crops = []
    for name in spec["rows"]:
        src = ASSETS / name
        parts = split_three(Image.open(src))
        print(f"{name}: {len(parts)} {[p.size for p in parts]}")
        crops.extend(parts)
    folder = OUT / spec["slug"]
    folder.mkdir(parents=True, exist_ok=True)
    for old in folder.glob("*.png"):
        old.unlink()
    items = []
    for i, im in enumerate(crops, start=1):
        path = folder / f"{spec['slug']}_{i:02d}.png"
        im.save(path)
        items.append((f"{spec['prefix']} {i}", im))
    catalog(items).save(OUT / f"{spec['slug']}_sheet_blocks.png")
    print("saved", len(crops), folder)


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    pack(SETS[1])


if __name__ == "__main__":
    main()
