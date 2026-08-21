# Re-pack bush/tree variants: keep dark bark, magenta only as background, add outline.
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
PINK = np.array([255, 0, 255], dtype=np.uint8)
BARK = np.array([26, 18, 12], dtype=np.uint8)
OUTLINE = np.array([6, 4, 3], dtype=np.uint8)
SEED_T = 12
BARK_T = 1
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
SETS = [
    {
        "slug": "dead_bush",
        "prefix": "dead_bush",
        "canvas": 512,
        "display": 220,
        "outline": 6,
        "rows": ["dead_bush_var_row_a.png", "dead_bush_var_row_b.png", "dead_bush_var_row_c.png"],
    },
    {
        "slug": "dead_tree",
        "prefix": "dead_tree",
        "canvas": 512,
        "display": 220,
        "outline": 6,
        "rows": ["dead_tree_var_row_a.png", "dead_tree_var_row_b.png", "dead_tree_var_row_c.png"],
    },
    {
        "slug": "big_tree",
        "prefix": "big_tree",
        "canvas": 768,
        "display": 240,
        "outline": 8,
        "rows": ["big_tree_var_row_a.png", "big_tree_var_row_b.png", "big_tree_var_row_c.png"],
    },
]


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def dilate(mask: np.ndarray, times: int) -> np.ndarray:
    if times <= 0:
        return mask
    im = Image.fromarray((mask.astype(np.uint8) * 255), "L")
    for _ in range(times):
        im = im.filter(ImageFilter.MaxFilter(3))
    return np.array(im) > 0


def erode(mask: np.ndarray, times: int) -> np.ndarray:
    if times <= 0:
        return mask
    im = Image.fromarray((mask.astype(np.uint8) * 255), "L")
    for _ in range(times):
        im = im.filter(ImageFilter.MinFilter(3))
    return np.array(im) > 0


def fill_holes(mask: np.ndarray) -> np.ndarray:
    h, w = mask.shape
    outside = np.zeros((h, w), dtype=bool)
    q = deque()

    def push(y, x):
        if not mask[y, x] and not outside[y, x]:
            outside[y, x] = True
            q.append((y, x))

    for x in range(w):
        push(0, x)
        push(h - 1, x)
    for y in range(h):
        push(y, 0)
        push(y, w - 1)
    while q:
        y, x = q.popleft()
        for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
            if 0 <= ny < h and 0 <= nx < w:
                push(ny, nx)
    return mask | (~outside)


def tree_mask(arr: np.ndarray) -> np.ndarray:
    lum = arr.max(axis=2)
    seed = lum > SEED_T
    cand = lum >= BARK_T
    mask = seed.copy()
    for _ in range(64):
        nxt = dilate(mask, 1) & cand
        if nxt.sum() == mask.sum():
            break
        mask = nxt
    mask = dilate(mask, 2)
    mask = erode(mask, 2)
    return fill_holes(mask)


def split_by_gaps(im: Image.Image, n: int) -> list[Image.Image]:
    arr = np.array(im.convert("RGB"))
    col = (arr.max(axis=2) > SEED_T).any(axis=0)
    runs = []
    start = None
    for i, filled in enumerate(col):
        if filled and start is None:
            start = i
        elif not filled and start is not None:
            runs.append((start, i))
            start = None
    if start is not None:
        runs.append((start, arr.shape[1]))
    runs = [(a, b) for a, b in runs if (b - a) >= 16]
    if len(runs) > n:
        runs = sorted(sorted(runs, key=lambda r: r[1] - r[0], reverse=True)[:n], key=lambda r: r[0])
    h, w = im.size[1], im.size[0]
    if len(runs) != n:
        cw = w // n
        return [im.crop((i * cw, 0, (i + 1) * cw if i < n - 1 else w, h)) for i in range(n)]
    return [im.crop((max(0, a - 6), 0, min(w, b + 6), h)) for a, b in runs]


def extract(im: Image.Image) -> tuple[Image.Image, Image.Image]:
    arr = np.array(im.convert("RGB"))
    mask = tree_mask(arr)
    if not mask.any():
        return im, Image.fromarray((mask.astype(np.uint8) * 255), "L")
    ys, xs = np.where(mask)
    pad = 14
    y0 = max(0, int(ys.min()) - pad)
    x0 = max(0, int(xs.min()) - pad)
    y1 = min(arr.shape[0], int(ys.max()) + 1 + pad)
    x1 = min(arr.shape[1], int(xs.max()) + 1 + pad)
    rgb = Image.fromarray(arr[y0:y1, x0:x1])
    m = Image.fromarray((mask[y0:y1, x0:x1].astype(np.uint8) * 255), "L")
    return rgb, m


def fit_shared(crops: list[Image.Image], size: int) -> list[tuple[Image.Image, Image.Image]]:
    pairs = [extract(im) for im in crops]
    max_w = max(rgb.size[0] for rgb, _ in pairs)
    max_h = max(rgb.size[1] for rgb, _ in pairs)
    scale = min((size - 48) / max_w, (size - 56) / max_h)
    out = []
    for rgb, mask in pairs:
        nw = max(1, int(rgb.size[0] * scale))
        nh = max(1, int(rgb.size[1] * scale))
        rgb_r = rgb.resize((nw, nh), Image.Resampling.LANCZOS)
        mask_r = mask.resize((nw, nh), Image.Resampling.NEAREST)
        canvas_rgb = Image.new("RGB", (size, size), (0, 0, 0))
        canvas_m = Image.new("L", (size, size), 0)
        xy = ((size - nw) // 2, size - nh - 16)
        canvas_rgb.paste(rgb_r, xy)
        canvas_m.paste(mask_r, xy)
        out.append((canvas_rgb, canvas_m))
    return out


def paint(rgb: Image.Image, mask_im: Image.Image, outline_w: int) -> Image.Image:
    arr = np.array(rgb.convert("RGB"))
    mask = np.array(mask_im) > 127
    mask = fill_holes(mask)
    ring = dilate(mask, outline_w) & (~mask)
    out = np.empty_like(arr)
    out[:, :] = PINK
    out[mask] = arr[mask]
    missing = mask & (arr.max(axis=2) < 8)
    out[missing] = BARK
    out[ring] = OUTLINE
    return Image.fromarray(out)


def catalog(tiles: list[Image.Image], prefix: str, display: int) -> Image.Image:
    face = font(18)
    n = len(tiles)
    cols = min(3, n)
    rows = (n + cols - 1) // cols
    gap_x, gap_y, label_h, margin = 48, 56, 28, 40
    cell_w = display + gap_x
    cell_h = display + label_h + gap_y
    sheet = Image.new(
        "RGB",
        (margin * 2 + cols * cell_w - gap_x, margin * 2 + rows * cell_h - gap_y + 8),
        CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, cols)
        x = margin + col * cell_w
        y = margin + row * cell_h
        sheet.paste(tile.resize((display, display), Image.Resampling.LANCZOS), (x, y))
        name = f"{prefix} {i + 1}"
        bbox = draw.textbbox((0, 0), name, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (display - tw) // 2, y + display + 8), name, fill=LABEL, font=face)
    return sheet


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for spec in SETS:
        crops = []
        for name in spec["rows"]:
            src = ASSETS / name
            im = Image.open(src).convert("RGB")
            parts = split_by_gaps(im, 3)
            print(f"{name}: {len(parts)} sizes {[p.size for p in parts]}")
            crops.extend(parts)
        fitted = fit_shared(crops, spec["canvas"])
        tiles = [paint(rgb, mask, spec["outline"]) for rgb, mask in fitted]
        folder = OUT / spec["slug"]
        folder.mkdir(exist_ok=True)
        for old in folder.glob(f"{spec['slug']}_*.png"):
            old.unlink()
        for i, tile in enumerate(tiles, start=1):
            tile.save(folder / f"{spec['slug']}_{i:02d}.png")
        catalog(tiles, spec["prefix"], spec["display"]).save(OUT / f"{spec['slug']}_sheet_blocks.png")
        print(f"saved {len(tiles)} -> {folder}")


if __name__ == "__main__":
    main()
