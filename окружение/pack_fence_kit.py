# Slice the 3x3 modular fence-kit sheet and pull back/left/right pieces from kit 1.
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
SRC = ASSETS / "fence_kit_9frames.png"
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
DISPLAY = 220
GAP_X, GAP_Y = 48, 56
LABEL_H = 28
MARGIN = 40
COLS, ROWS = 3, 3
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
CELLS = [
    (30, 30, 308, 334),
    (375, 30, 655, 334),
    (711, 30, 989, 334),
    (30, 368, 305, 664),
    (375, 368, 650, 664),
    (709, 368, 987, 664),
    (30, 702, 309, 998),
    (375, 702, 650, 998),
    (711, 702, 990, 998),
]


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def wood_mask(arr: np.ndarray) -> np.ndarray:
    mx = arr.max(axis=2).astype(np.int16)
    mn = arr.min(axis=2).astype(np.int16)
    return (mx - mn > 12) & (arr[:, :, 0] > arr[:, :, 2] + 4)


def to_rgba(tile: np.ndarray) -> Image.Image:
    m = wood_mask(tile)
    rgba = np.zeros((tile.shape[0], tile.shape[1], 4), np.uint8)
    rgba[..., :3] = tile
    rgba[..., 3] = np.where(m, 255, 0)
    # keep a little anti-alias: mid-luma near wood
    near = (~m) & (tile.max(axis=2) < 240) & (tile.max(axis=2) > 40)
    rgba[near, 3] = 180
    rgba[near, :3] = tile[near]
    return Image.fromarray(rgba, "RGBA")


def tight_rgba(im: Image.Image, pad: int = 4) -> Image.Image:
    arr = np.array(im)
    a = arr[:, :, 3] > 16
    if not a.any():
        return im
    ys, xs = np.where(a)
    y0 = max(0, int(ys.min()) - pad)
    x0 = max(0, int(xs.min()) - pad)
    y1 = min(arr.shape[0], int(ys.max()) + 1 + pad)
    x1 = min(arr.shape[1], int(xs.max()) + 1 + pad)
    return Image.fromarray(arr[y0:y1, x0:x1])


def catalog(items: list[tuple[str, Image.Image]]) -> Image.Image:
    face = font(18)
    n = len(items)
    cell_w = DISPLAY + GAP_X
    cell_h = DISPLAY + LABEL_H + GAP_Y
    sheet = Image.new(
        "RGB",
        (MARGIN * 2 + COLS * cell_w - GAP_X, MARGIN * 2 + ROWS * cell_h - GAP_Y + 8),
        CAT_BG,
    )
    draw = ImageDraw.Draw(sheet)
    for i, (name, im) in enumerate(items):
        row, col = divmod(i, COLS)
        x = MARGIN + col * cell_w
        y = MARGIN + row * cell_h
        shown = im.convert("RGBA")
        shown.thumbnail((DISPLAY, DISPLAY), Image.Resampling.LANCZOS)
        cell = Image.new("RGBA", (DISPLAY, DISPLAY), (*CAT_BG, 255))
        cell.alpha_composite(
            shown,
            ((DISPLAY - shown.size[0]) // 2, (DISPLAY - shown.size[1]) // 2),
        )
        sheet.paste(cell.convert("RGB"), (x, y))
        bbox = draw.textbbox((0, 0), name, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (DISPLAY - tw) // 2, y + DISPLAY + 8), name, fill=LABEL, font=face)
    return sheet


def split_ur(kit: np.ndarray) -> dict[str, Image.Image]:
    m = wood_mask(kit)
    h, w = m.shape
    row_f = m.mean(1)
    # top bar: first band with high fill
    top_end = 8
    for y in range(h):
        if row_f[y] > 0.35:
            top_end = y
            break
    for y in range(top_end, h):
        if row_f[y] < 0.18 and y > top_end + 18:
            top_end = y
            break
    top_end = min(h, top_end + 6)
    col_f = m.mean(0)
    # left post width
    left_w = max(int(w * 0.22), 24)
    for x in range(w // 3, 8, -1):
        if col_f[x] < 0.08:
            left_w = x + 4
            break
    right_x = w - max(int(w * 0.22), 24)
    for x in range(w * 2 // 3, w - 8):
        if col_f[x] < 0.08:
            right_x = x - 4
            break
    parts = {
        "back": kit[:top_end, :],
        "left": kit[:, :left_w],
        "right": kit[:, right_x:],
    }
    return {k: tight_rgba(to_rgba(v)) for k, v in parts.items()}


def main() -> None:
    arr = np.array(Image.open(SRC).convert("RGB"))
    folder = OUT / "fence_kit"
    folder.mkdir(parents=True, exist_ok=True)
    for old in folder.glob("*.png"):
        old.unlink()
    items = []
    kits = []
    for i, (x0, y0, x1, y1) in enumerate(CELLS, start=1):
        cell = arr[y0:y1, x0:x1]
        im = tight_rgba(to_rgba(cell))
        im.save(folder / f"fence_kit_{i:02d}.png")
        items.append((f"fence_kit {i}", im))
        kits.append(cell)
        print(f"kit {i} {im.size}")
    catalog(items).save(OUT / "fence_kit_sheet_blocks.png")
    Image.fromarray(arr).save(OUT / "fence_kit_9frames.png")
    parts = split_ur(kits[0])
    for name, im in parts.items():
        im.save(folder / f"fence_{name}.png")
        print("part", name, im.size)


if __name__ == "__main__":
    main()
