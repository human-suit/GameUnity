"""Pack Subject 07 visual-novel standing sprites onto magenta."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "subject07_vn"
DST.mkdir(parents=True, exist_ok=True)
CANVAS = 1024
DISPLAY = 280
PINK_T = tuple(int(c) for c in m.PINK)

ITEMS = [
    ("subject07_vn_front.png", "front"),
    ("subject07_vn_back.png", "back"),
    ("subject07_vn_threequarter.png", "3/4"),
    ("subject07_vn_side.png", "side"),
    ("subject07_vn_paper.png", "paper"),
    ("subject07_vn_injured.png", "injured"),
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
    crops = []
    for src_name, label in ITEMS:
        src = ASSETS / src_name
        if not src.exists():
            raise SystemExit(f"MISSING {src_name}")
        arr = finish(np.array(Image.open(src).convert("RGB")))
        mag = m.magenta_mask(arr)
        crop, mag_c = m.tight(arr, mag)
        crops.append((src_name, label, crop, mag_c))
        print(src_name, "crop", crop.shape[1], crop.shape[0])

    max_w = max(c.shape[1] for _, _, c, _ in crops)
    max_h = max(c.shape[0] for _, _, c, _ in crops)
    shared = min((CANVAS - 48) / max(max_w, 1), (CANVAS - 64) / max(max_h, 1))

    pairs = []
    for i, (src_name, label, crop, mag_c) in enumerate(crops, start=1):
        fitted = finish(m.fit(crop, mag_c, CANVAS, shared))
        im = Image.fromarray(fitted)
        out = DST / src_name
        im.save(out, optimize=True)
        pairs.append((label, im))
        print("saved", out.name)

    sheet = catalog(pairs)
    sheet.save(DST / "subject07_vn_sheet_blocks.png", optimize=True)
    sheet.save(m.OUT / "subject07_vn_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size, "folder", DST)


if __name__ == "__main__":
    main()
