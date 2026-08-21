"""Pack grimdark 2.5D ziggurat buildings onto magenta."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "ziggurat_25d_grim"
DST.mkdir(parents=True, exist_ok=True)
CANVAS = 1024
DISPLAY = 420
FILES = [f"ziggurat_grim_{i:02d}.png" for i in range(1, 4)]
LABELS = ["ziggurat 1", "ziggurat 2", "ziggurat 3"]
PINK_T = tuple(int(c) for c in m.PINK)


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr).copy()
    r = arr[:, :, 0].astype(np.int16)
    g = arr[:, :, 1].astype(np.int16)
    b = arr[:, :, 2].astype(np.int16)
    grass = (g > r + 18) & (g > b + 12) & (g > 70) & (r < 140)
    arr[grass] = m.PINK
    # leftover chroma in auras: never keep magenta on the prop
    mag = m.magenta_mask(arr)
    pinkish = (r > 150) & (b > 95) & (g < 120) & (r + b > g + 90) & (~mag)
    if pinkish.any():
        redder = pinkish & (r >= b)
        bluer = pinkish & (b > r)
        arr[redder, 2] = np.clip(g[redder] + 8, 0, 255).astype(np.uint8)
        arr[bluer, 0] = np.clip(g[bluer] + 8, 0, 255).astype(np.uint8)
    arr = m.snap_flat_magenta(arr)
    mag = m.magenta_mask(arr)
    return m.snap_flat_magenta(m.despill(arr, mag))


def catalog(tiles: list[Image.Image]) -> Image.Image:
    face = m.font(18)
    n = len(tiles)
    cols = 3
    rows = (n + cols - 1) // cols
    gap_x, gap_y, label_h, margin = 48, 56, 28, 40
    cell_w = DISPLAY + gap_x
    cell_h = DISPLAY + label_h + gap_y
    sheet = Image.new(
        "RGB",
        (margin * 2 + cols * cell_w - gap_x, margin * 2 + rows * cell_h - gap_y + 8),
        PINK_T,
    )
    draw = ImageDraw.Draw(sheet)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, cols)
        x = margin + col * cell_w
        y = margin + row * cell_h
        cell = tile.resize((DISPLAY, DISPLAY), Image.Resampling.LANCZOS)
        cell = Image.fromarray(finish(np.array(cell.convert("RGB"))))
        sheet.paste(cell, (x, y))
        name = LABELS[i]
        bbox = draw.textbbox((0, 0), name, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (DISPLAY - tw) // 2, y + DISPLAY + 8), name, fill=(28, 28, 28), font=face)
    return sheet


def main() -> None:
    crops = []
    for name in FILES:
        src = ASSETS / name
        if not src.exists():
            raise SystemExit(f"MISSING {name}")
        arr = finish(np.array(Image.open(src).convert("RGB")))
        mag = m.magenta_mask(arr)
        crop, mag_c = m.tight(arr, mag)
        crops.append((crop, mag_c))
        print(name, "crop", crop.shape[1], crop.shape[0])

    max_w = max(c.shape[1] for c, _ in crops)
    max_h = max(c.shape[0] for c, _ in crops)
    shared = min((CANVAS - 48) / max(max_w, 1), (CANVAS - 64) / max(max_h, 1))

    tiles = []
    for i, (crop, mag_c) in enumerate(crops, start=1):
        fitted = finish(m.fit(crop, mag_c, CANVAS, shared))
        im = Image.fromarray(fitted)
        out = DST / f"ziggurat_grim_{i:02d}.png"
        im.save(out, optimize=True)
        tiles.append(im)
        print("saved", out.name)

    sheet = catalog(tiles)
    sheet.save(DST / "ziggurat_25d_grim_sheet_blocks.png", optimize=True)
    sheet.save(m.OUT / "ziggurat_25d_grim_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size, "folder", DST)


if __name__ == "__main__":
    main()
