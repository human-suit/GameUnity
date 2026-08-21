"""Pack 2.5D cliff edge tiles (ring a floor island) onto magenta."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "cliff_25d"
DST.mkdir(parents=True, exist_ok=True)
CANVAS = 1024
DISPLAY = 280
PINK_T = tuple(int(c) for c in m.PINK)

# 3x3 catalog matches cobble/fence: corners + edges + extra south face
ITEMS = [
    ("cliff_nw.png", "cliff nw", "nw"),
    ("cliff_n.png", "cliff n", "n"),
    ("cliff_ne.png", "cliff ne", "ne"),
    ("cliff_w.png", "cliff w", "w"),
    ("cliff_s.png", "cliff s", "s"),
    ("cliff_e.png", "cliff e", "e"),
    ("cliff_sw.png", "cliff sw", "sw"),
    ("cliff_se.png", "cliff se", "se"),
    ("cliff_s2.png", "cliff s2", "s"),
]


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr).copy()
    r, g, b = arr[:, :, 0].astype(np.int16), arr[:, :, 1].astype(np.int16), arr[:, :, 2].astype(np.int16)
    grass = (g > r + 18) & (g > b + 12) & (g > 70) & (r < 140)
    arr[grass] = m.PINK
    arr = m.snap_flat_magenta(arr)
    mag = m.magenta_mask(arr)
    return m.snap_flat_magenta(m.despill(arr, mag))


def xy_for(align: str, size: int, nw: int, nh: int, pad: int = 18) -> tuple[int, int]:
    """Keep the walkable dirt against the floor-neighboring edges."""
    cx = (size - nw) // 2
    cy = (size - nh) // 2
    left, right = pad, size - nw - pad
    top, bottom = pad, size - nh - pad
    table = {
        "n": (cx, bottom),
        "s": (cx, top),
        "w": (right, cy),
        "e": (left, cy),
        "nw": (right, bottom),
        "ne": (left, bottom),
        "sw": (right, top),
        "se": (left, top),
    }
    x, y = table[align]
    return max(0, min(x, size - nw)), max(0, min(y, size - nh))


def fit_align(arr: np.ndarray, mag: np.ndarray, size: int, align: str) -> np.ndarray:
    h, w = arr.shape[:2]
    scale = min((size - 40) / max(w, 1), (size - 40) / max(h, 1))
    nw, nh = max(1, int(w * scale)), max(1, int(h * scale))
    rgb = np.array(Image.fromarray(arr).resize((nw, nh), Image.Resampling.LANCZOS))
    rgb = m.snap_flat_magenta(rgb)
    canvas = np.full((size, size, 3), m.PINK, dtype=np.uint8)
    x, y = xy_for(align, size, nw, nh)
    canvas[y : y + nh, x : x + nw] = rgb
    return canvas


def catalog(pairs: list[tuple[str, Image.Image]]) -> Image.Image:
    face = m.font(18)
    cols, rows = 3, 3
    gap_x, gap_y, label_h, margin = 40, 52, 28, 36
    cell_w = DISPLAY + gap_x
    cell_h = DISPLAY + label_h + gap_y
    sheet = Image.new(
        "RGB",
        (margin * 2 + cols * cell_w - gap_x, margin * 2 + rows * cell_h - gap_y + 8),
        PINK_T,
    )
    draw = ImageDraw.Draw(sheet)
    for i, (label, tile) in enumerate(pairs):
        row, col = divmod(i, cols)
        x = margin + col * cell_w
        y = margin + row * cell_h
        cell = tile.resize((DISPLAY, DISPLAY), Image.Resampling.LANCZOS)
        cell = Image.fromarray(finish(np.array(cell.convert("RGB"))))
        sheet.paste(cell, (x, y))
        bbox = draw.textbbox((0, 0), label, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (DISPLAY - tw) // 2, y + DISPLAY + 8), label, fill=(28, 28, 28), font=face)
    return sheet


def main() -> None:
    pairs = []
    for src_name, label, align in ITEMS:
        src = ASSETS / src_name
        if not src.exists():
            print("MISSING", src_name)
            continue
        arr = finish(np.array(Image.open(src).convert("RGB")))
        mag = m.magenta_mask(arr)
        crop, mag_c = m.tight(arr, mag)
        fitted = finish(fit_align(crop, mag_c, CANVAS, align))
        im = Image.fromarray(fitted)
        out = DST / src_name
        im.save(out, optimize=True)
        pairs.append((label, im))
        print("saved", out.name, crop.shape[1], crop.shape[0], "->", CANVAS)

    if len(pairs) != 9:
        raise SystemExit(f"expected 9 cliff tiles, got {len(pairs)}")

    sheet = catalog(pairs)
    sheet.save(DST / "cliff_25d_sheet_blocks.png", optimize=True)
    sheet.save(m.OUT / "cliff_25d_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size, "folder", DST)


if __name__ == "__main__":
    main()
