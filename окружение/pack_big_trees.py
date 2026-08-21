# Split large-tree strips into 9 labeled blocks (same catalog as dead_bush / water_deep).
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
CANVAS = 768
BG = (0, 0, 0)
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
BLACK_T = 18
DISPLAY = 240
GAP_X, GAP_Y = 48, 56
LABEL_H = 28
MARGIN = 40
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
ROWS = [
    "big_tree_var_row_a.png",
    "big_tree_var_row_b.png",
    "big_tree_var_row_c.png",
]
SLUG = "big_tree"
PER_ROW = 3


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def content_mask(arr: np.ndarray) -> np.ndarray:
    return arr.max(axis=2) > BLACK_T


def split_by_gaps(im: Image.Image, n: int) -> list[Image.Image]:
    arr = np.array(im.convert("RGB"))
    col = content_mask(arr).any(axis=0)
    runs = []
    start = None
    for i, filled in enumerate(col):
        if filled and start is None:
            start = i
        elif not filled and start is not None:
            runs.append((start, i))
            start = None
    if start is not None:
        runs.append((start, len(col)))
    runs = [(a, b) for a, b in runs if (b - a) >= 16]
    if len(runs) > n:
        runs = sorted(sorted(runs, key=lambda r: r[1] - r[0], reverse=True)[:n], key=lambda r: r[0])
    h, w = im.size[1], im.size[0]
    if len(runs) != n:
        cw = w // n
        return [im.crop((i * cw, 0, (i + 1) * cw if i < n - 1 else w, h)) for i in range(n)]
    out = []
    for a, b in runs:
        out.append(im.crop((max(0, a - 6), 0, min(w, b + 6), h)))
    return out


def tight(im: Image.Image) -> Image.Image:
    arr = np.array(im.convert("RGB"))
    mask = content_mask(arr)
    if not mask.any():
        return im
    ys, xs = np.where(mask)
    pad = 6
    y0 = max(0, int(ys.min()) - pad)
    x0 = max(0, int(xs.min()) - pad)
    y1 = min(arr.shape[0], int(ys.max()) + 1 + pad)
    x1 = min(arr.shape[1], int(xs.max()) + 1 + pad)
    return Image.fromarray(arr[y0:y1, x0:x1])


def fit_shared(crops: list[Image.Image], size: int = CANVAS) -> list[Image.Image]:
    boxes = [tight(im) for im in crops]
    max_w = max(im.size[0] for im in boxes)
    max_h = max(im.size[1] for im in boxes)
    scale = min((size - 28) / max_w, (size - 36) / max_h)
    out = []
    for im in boxes:
        nw = max(1, int(im.size[0] * scale))
        nh = max(1, int(im.size[1] * scale))
        resized = im.resize((nw, nh), Image.Resampling.LANCZOS)
        canvas = Image.new("RGB", (size, size), BG)
        canvas.paste(resized, ((size - nw) // 2, size - nh - 10))
        out.append(canvas)
    return out


def catalog(tiles: list[Image.Image], prefix: str) -> Image.Image:
    face = font(18)
    n = len(tiles)
    cols = min(3, n)
    rows = (n + cols - 1) // cols
    cell_w = DISPLAY + GAP_X
    cell_h = DISPLAY + LABEL_H + GAP_Y
    sheet = Image.new(
        "RGB",
        (MARGIN * 2 + cols * cell_w - GAP_X, MARGIN * 2 + rows * cell_h - GAP_Y + 8),
        CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, cols)
        x = MARGIN + col * cell_w
        y = MARGIN + row * cell_h
        sheet.paste(tile.resize((DISPLAY, DISPLAY), Image.Resampling.LANCZOS), (x, y))
        name = f"{prefix} {i + 1}"
        bbox = draw.textbbox((0, 0), name, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (DISPLAY - tw) // 2, y + DISPLAY + 8), name, fill=LABEL, font=face)
    return sheet


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    crops = []
    for name in ROWS:
        src = ASSETS / name
        im = Image.open(src).convert("RGB")
        parts = split_by_gaps(im, PER_ROW)
        print(f"{name}: {len(parts)}  sizes {[p.size for p in parts]}")
        crops.extend(parts)
    tiles = fit_shared(crops)
    folder = OUT / SLUG
    folder.mkdir(exist_ok=True)
    for old in folder.glob(f"{SLUG}_*.png"):
        old.unlink()
    for i, tile in enumerate(tiles, start=1):
        tile.save(folder / f"{SLUG}_{i:02d}.png")
    catalog(tiles, "big_tree").save(OUT / f"{SLUG}_sheet_blocks.png")
    print(f"saved {len(tiles)} -> {folder}")


if __name__ == "__main__":
    main()
