"""Pack original F&H-inspired mob sheets + magenta sprites."""
from pathlib import Path
import shutil

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "mobs_25d"
DST.mkdir(parents=True, exist_ok=True)
CANVAS = 1024
DISPLAY = 280
PINK_T = tuple(int(c) for c in m.PINK)

SHEETS = [
    "mob_cinder_gaoler_sheet.png",
    "mob_cistern_widow_sheet.png",
    "mob_psalm_tick_sheet.png",
]
SPRITES = [
    ("mob_cinder_gaoler_front.png", "gaoler F"),
    ("mob_cinder_gaoler_back.png", "gaoler B"),
    ("mob_cistern_widow_front.png", "widow F"),
    ("mob_cistern_widow_back.png", "widow B"),
    ("mob_psalm_tick_front.png", "tick F"),
    ("mob_psalm_tick_back.png", "tick B"),
]


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr).copy()
    mag = m.magenta_mask(arr)
    return m.snap_flat_magenta(m.despill(arr, mag))


def catalog(pairs: list[tuple[str, Image.Image]]) -> Image.Image:
    face = m.font(18)
    n = len(pairs)
    cols = 3
    rows = (n + cols - 1) // cols
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
    for name in SHEETS:
        src = ASSETS / name
        shutil.copy2(src, DST / name)
        shutil.copy2(src, m.OUT / name)
        print("sheet", name)

    crops = []
    for src_name, label in SPRITES:
        arr = finish(np.array(Image.open(ASSETS / src_name).convert("RGB")))
        mag = m.magenta_mask(arr)
        crop, mag_c = m.tight(arr, mag)
        crops.append((src_name, label, crop, mag_c))

    max_w = max(c.shape[1] for _, _, c, _ in crops)
    max_h = max(c.shape[0] for _, _, c, _ in crops)
    shared = min((CANVAS - 48) / max(max_w, 1), (CANVAS - 64) / max(max_h, 1))

    pairs = []
    for src_name, label, crop, mag_c in crops:
        fitted = finish(m.fit(crop, mag_c, CANVAS, shared))
        im = Image.fromarray(fitted)
        im.save(DST / src_name, optimize=True)
        pairs.append((label, im))
        print("saved", src_name)

    cat = catalog(pairs)
    cat.save(DST / "mobs_25d_sheet_blocks.png", optimize=True)
    cat.save(m.OUT / "mobs_25d_sheet_blocks.png", optimize=True)
    print("catalog", cat.size, "folder", DST)


if __name__ == "__main__":
    main()
