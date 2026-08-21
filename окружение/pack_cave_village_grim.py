"""Pack cave village props remade in our grim setting."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "cave_village_grim"
DST.mkdir(parents=True, exist_ok=True)
CANVAS = 768
DISPLAY = 220
PINK_T = tuple(int(c) for c in m.PINK)

ITEMS = [
    ("grim_hut_01.png", "hut 1"),
    ("grim_hut_02.png", "hut 2"),
    ("grim_hut_03.png", "hut 3"),
    ("grim_hut_04.png", "hut 4"),
    ("grim_fence_logs.png", "fence logs"),
    ("grim_fence_rail.png", "fence rail"),
    ("grim_platform.png", "platform"),
    ("grim_ladder.png", "ladder"),
    ("grim_hoist.png", "hoist"),
    ("grim_barrel.png", "barrel"),
    ("grim_crate.png", "crate"),
    ("grim_pots.png", "pots"),
    ("grim_torch.png", "torch"),
    ("grim_rug.png", "rug"),
    ("grim_stool.png", "stool"),
    ("grim_bones.png", "bones"),
]


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr).copy()
    r, g, b = arr[:, :, 0].astype(np.int16), arr[:, :, 1].astype(np.int16), arr[:, :, 2].astype(np.int16)
    grass = (g > r + 18) & (g > b + 12) & (g > 70) & (r < 140)
    arr[grass] = m.PINK
    arr = m.snap_flat_magenta(arr)
    mag = m.magenta_mask(arr)
    return m.snap_flat_magenta(m.despill(arr, mag))


def catalog(pairs: list[tuple[str, Image.Image]]) -> Image.Image:
    face = m.font(16)
    n = len(pairs)
    cols = 4
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
    pairs = []
    for src_name, label in ITEMS:
        src = ASSETS / src_name
        if not src.exists():
            print("MISSING", src_name)
            continue
        arr = finish(np.array(Image.open(src).convert("RGB")))
        mag = m.magenta_mask(arr)
        crop, mag_c = m.tight(arr, mag)
        fitted = finish(m.fit(crop, mag_c, CANVAS))
        im = Image.fromarray(fitted)
        slug = Path(src_name).stem.replace("grim_", "cave_")
        im.save(DST / f"{slug}.png", optimize=True)
        pairs.append((label, im))
        print("saved", slug)

    sheet = catalog(pairs)
    sheet.save(DST / "cave_village_grim_sheet_blocks.png", optimize=True)
    sheet.save(m.OUT / "cave_village_grim_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size, "n", len(pairs), "folder", DST)


if __name__ == "__main__":
    main()
