"""Pack 9 enclosure fences: same layout, different materials/board sizes."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

# reuse kit assembly helpers
import make_fence_25d_variants as v

DST = m.OUT / "fence_25d"
DST.mkdir(parents=True, exist_ok=True)
PINK_T = tuple(int(c) for c in m.PINK)
TILE = 320


def enclosure_tile(arr: np.ndarray) -> Image.Image:
    arr = v.finish(arr)
    mag = m.magenta_mask(arr)
    ys, xs = np.where(~mag)
    crop = arr[int(ys.min()) : int(ys.max()) + 1, int(xs.min()) : int(xs.max()) + 1]
    h, w = crop.shape[:2]
    s = min((TILE - 8) / w, (TILE - 8) / h)
    nw, nh = max(1, int(w * s)), max(1, int(h * s))
    resized = v.scale_rgb(crop, nw, nh)
    cell = np.full((TILE, TILE, 3), m.PINK, dtype=np.uint8)
    x = (TILE - nw) // 2
    y = (TILE - nh) // 2
    cell[y : y + nh, x : x + nw] = resized
    return Image.fromarray(v.finish(cell))


def load_gen(name: str) -> np.ndarray:
    p = ASSETS / name
    return v.finish(np.array(Image.open(p).convert("RGB")))


def thicken_up(up: np.ndarray) -> np.ndarray:
    """Three fat boards instead of four pickets, same rails."""
    p = v.PICKETS
    fat = v.erase_picket_body(up, *p[1])  # drop second, keep 0,2,3 then widen
    # rebuild: take pickets 0,2,3, scale x, place evenly
    canvas = np.full_like(up, m.PINK)
    # keep rails from a gapped version
    rails_only = up.copy()
    for a, b in p:
        rails_only = v.erase_picket_body(rails_only, a, b)
    canvas[:] = rails_only
    strips = []
    for i in (0, 2, 3):
        a, b = p[i]
        strip = up[:, a:b].copy()
        nh, nw = strip.shape[0], int(round(strip.shape[1] * 1.55))
        strips.append(v.scale_rgb(strip, nw, nh))
    total_w = sum(s.shape[1] for s in strips)
    span = p[3][1] - p[0][0]
    gap = max(8, (span - total_w) // 4)
    x = p[0][0]
    for s in strips:
        v.paste_over(canvas, s, x, 0)
        x += s.shape[1] + gap
    return v.finish(canvas)


def main() -> None:
    left, right = v.load("left"), v.load("right")
    up_b, down_b = v.load("up"), v.load("down")
    ups = v.make_up_variants(up_b)
    downs = v.make_up_variants(down_b)

    frames = []
    # 1 original wood
    frames.append(v.assemble(left, right, ups[0], downs[0]))
    # 2 missing boards
    frames.append(v.assemble(left, right, ups[1], downs[1]))
    # 3 tree logs
    frames.append(load_gen("fence_style_logs.png"))
    # 4 wide boards
    up_w = thicken_up(up_b)
    down_w = thicken_up(down_b)
    frames.append(v.assemble(left, right, up_w, down_w))
    # 5 thin palings
    frames.append(load_gen("fence_style_thin.png"))
    # 6 iron
    frames.append(load_gen("fence_style_iron.png"))
    # 7 moss wood
    frames.append(v.assemble(left, right, ups[2], downs[2]))
    # 8 broken heights
    frames.append(v.assemble(left, right, ups[5], downs[5]))
    # 9 rusty iron / weathered metal
    frames.append(load_gen("fence_style_rusty.png"))

    tiles = []
    for i, fr in enumerate(frames, start=1):
        tile = enclosure_tile(fr)
        path = DST / f"fence_25d_{i:02d}.png"
        tile.save(path, optimize=True)
        tiles.append(tile)
        print("saved", path.name, tile.size)

    sheet = v.catalog(tiles)
    sheet.save(m.OUT / "fence_25d_sheet_blocks.png", optimize=True)
    sheet.save(DST / "fence_25d_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size)


if __name__ == "__main__":
    main()
