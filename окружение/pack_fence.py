# Pack 9 fence profiles into a dirt-style labeled 3x3 sheet.
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
CANVAS = 640
BG = (0, 0, 0)
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
BLACK_T = 20
DISPLAY = 220
GAP_X, GAP_Y = 48, 56
LABEL_H = 28
MARGIN = 40
COLS, ROWS_N = 3, 3
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
STRIPS = [
    "fence_profile_row_a.png",
    "fence_profile_row_b.png",
    "fence_profile_row_c.png",
]
SLUG = "fence"


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def content_mask(arr: np.ndarray) -> np.ndarray:
    return arr.max(axis=2) > BLACK_T


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


def main_band(im: Image.Image) -> Image.Image:
    arr = np.array(im.convert("RGB"))
    mask = content_mask(arr)
    rows = runs_from(mask.mean(1) > 0.02, 40, 8)
    if not rows:
        return im
    y0, y1 = max(rows, key=lambda r: r[1] - r[0])
    pad = 8
    y0 = max(0, y0 - pad)
    y1 = min(arr.shape[0], y1 + pad)
    return Image.fromarray(arr[y0:y1])


def split_by_gaps(im: Image.Image, n: int) -> list[Image.Image]:
    arr = np.array(im.convert("RGB"))
    cols = runs_from(content_mask(arr).mean(0) > 0.03, 24, 10)
    if len(cols) > n:
        cols = sorted(sorted(cols, key=lambda r: r[1] - r[0], reverse=True)[:n], key=lambda r: r[0])
    h, w = arr.shape[:2]
    if len(cols) != n:
        cw = w // n
        return [im.crop((i * cw, 0, (i + 1) * cw if i < n - 1 else w, h)) for i in range(n)]
    out = []
    for a, b in cols:
        out.append(im.crop((max(0, a - 4), 0, min(w, b + 4), h)))
    return out


def tight(im: Image.Image) -> Image.Image:
    arr = np.array(im.convert("RGB"))
    mask = content_mask(arr)
    if not mask.any():
        return im
    ys, xs = np.where(mask)
    pad = 4
    y0 = max(0, int(ys.min()) - pad)
    x0 = max(0, int(xs.min()) - pad)
    y1 = min(arr.shape[0], int(ys.max()) + 1 + pad)
    x1 = min(arr.shape[1], int(xs.max()) + 1 + pad)
    return Image.fromarray(arr[y0:y1, x0:x1])


def fit_shared(crops: list[Image.Image], size: int = CANVAS) -> list[Image.Image]:
    boxes = [tight(im) for im in crops]
    max_w = max(im.size[0] for im in boxes)
    max_h = max(im.size[1] for im in boxes)
    scale = min((size - 32) / max_w, (size - 48) / max_h)
    out = []
    for im in boxes:
        nw = max(1, int(im.size[0] * scale))
        nh = max(1, int(im.size[1] * scale))
        resized = im.resize((nw, nh), Image.Resampling.LANCZOS)
        canvas = Image.new("RGB", (size, size), BG)
        canvas.paste(resized, ((size - nw) // 2, size - nh - 16))
        out.append(canvas)
    return out


def catalog(tiles: list[Image.Image], prefix: str) -> Image.Image:
    face = font(18)
    cell_w = DISPLAY + GAP_X
    cell_h = DISPLAY + LABEL_H + GAP_Y
    sheet = Image.new(
        "RGB",
        (MARGIN * 2 + COLS * cell_w - GAP_X, MARGIN * 2 + ROWS_N * cell_h - GAP_Y + 8),
        CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, COLS)
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
    for name in STRIPS:
        im = main_band(Image.open(ASSETS / name).convert("RGB"))
        parts = split_by_gaps(im, 3)
        print(f"{name}: {len(parts)} {[p.size for p in parts]}")
        crops.extend(parts)
    tiles = fit_shared(crops)
    folder = OUT / SLUG
    folder.mkdir(exist_ok=True)
    for old in folder.glob(f"{SLUG}_*.png"):
        old.unlink()
    for i, tile in enumerate(tiles, start=1):
        tile.save(folder / f"{SLUG}_{i:02d}.png")
    catalog(tiles, "fence").save(OUT / f"{SLUG}_sheet_blocks.png")
    print("saved", len(tiles), folder)


if __name__ == "__main__":
    main()
