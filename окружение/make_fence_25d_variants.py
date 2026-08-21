"""9 different 2.5D fence tiles from fence_kit_25d, cobble catalog on magenta."""
from pathlib import Path

import importlib.util
import numpy as np
from PIL import Image, ImageDraw, ImageEnhance

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

SRC = m.OUT / "fence_kit_25d"
DST = m.OUT / "fence_25d"
DST.mkdir(parents=True, exist_ok=True)
PINK_T = tuple(int(c) for c in m.PINK)

PICKETS = [(70, 234), (310, 473), (549, 711), (778, 941)]
RAILS = [(256, 370), (538, 652)]
GAP_X = [270, 511, 744]
TILE = 320
DISPLAY = 220
GAP_X_S, GAP_Y, LABEL_H, MARGIN = 48, 56, 28, 40
COLS, ROWS = 3, 3
OLIVE = np.array([86, 102, 34], dtype=np.float32)


def load(name: str) -> np.ndarray:
    return m.snap_flat_magenta(np.array(Image.open(SRC / f"fence_{name}.png").convert("RGB")))


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr)
    return m.snap_flat_magenta(m.despill(arr, m.magenta_mask(arr)))


def wood(arr: np.ndarray) -> np.ndarray:
    return ~m.magenta_mask(arr)


def paste_over(dst: np.ndarray, src: np.ndarray, x: int, y: int) -> None:
    sm = m.magenta_mask(src)
    sh, sw = src.shape[:2]
    dh, dw = dst.shape[:2]
    x0, y0 = max(0, x), max(0, y)
    x1, y1 = min(dw, x + sw), min(dh, y + sh)
    if x1 <= x0 or y1 <= y0:
        return
    sx, sy = x0 - x, y0 - y
    patch = src[sy : sy + (y1 - y0), sx : sx + (x1 - x0)]
    pmag = sm[sy : sy + (y1 - y0), sx : sx + (x1 - x0)]
    region = dst[y0:y1, x0:x1]
    region[~pmag] = patch[~pmag]
    dst[y0:y1, x0:x1] = region


def scale_rgb(arr: np.ndarray, nw: int, nh: int) -> np.ndarray:
    im = Image.fromarray(arr).resize((max(1, int(nw)), max(1, int(nh))), Image.Resampling.LANCZOS)
    return finish(np.array(im.convert("RGB")))


def crop_content(arr: np.ndarray) -> np.ndarray:
    mag = m.magenta_mask(arr)
    ys, xs = np.where(~mag)
    return arr[int(ys.min()) : int(ys.max()) + 1, int(xs.min()) : int(xs.max()) + 1]


def picket_top(arr: np.ndarray, x0: int, x1: int) -> int:
    w = wood(arr)
    ys, xs = np.where(w[:, x0:x1])
    return int(ys.min()) if len(ys) else 0


def erase_picket_body(arr: np.ndarray, x0: int, x1: int) -> np.ndarray:
    out = arr.copy()
    mag = m.magenta_mask(arr)
    rail_m = np.zeros(arr.shape[:2], dtype=bool)
    for a, b in RAILS:
        rail_m[a:b, :] = True
    gap = GAP_X[1]
    for x in range(x0, x1):
        for y in range(out.shape[0]):
            if mag[y, x]:
                continue
            if rail_m[y, x]:
                out[y, x] = arr[y, gap]
            else:
                out[y, x] = m.PINK
    return out


def break_picket(arr: np.ndarray, x0: int, x1: int, drop: int = 70) -> np.ndarray:
    out = arr.copy()
    top = picket_top(arr, x0, x1)
    cut0 = top + drop
    rng = np.random.RandomState(x0 * 13 + drop)
    for x in range(x0, x1):
        jagged = cut0 + int(rng.randint(-7, 8))
        y0 = top
        y1 = min(RAILS[0][0] - 4, jagged)
        if y1 <= y0:
            continue
        out[y0:y1, x] = m.PINK
        if y1 < out.shape[0] and not m.magenta_mask(out[y1 : y1 + 1, x : x + 1])[0, 0]:
            out[min(y1, out.shape[0] - 1), x] = np.array([12, 8, 6], dtype=np.uint8)
    return out


def shorten_picket(arr: np.ndarray, x0: int, x1: int, drop: int) -> np.ndarray:
    return break_picket(arr, x0, x1, drop=drop)


def moss_picket(arr: np.ndarray, x0: int, x1: int, depth: int = 110) -> np.ndarray:
    out = arr.copy().astype(np.float32)
    w = wood(arr)
    top = picket_top(arr, x0, x1)
    for y in range(top, min(arr.shape[0], top + depth)):
        fade = 1.0 - (y - top) / max(1, depth)
        amt = min(0.92, 0.88 * fade + 0.12)
        sl = out[y, x0:x1]
        wm = w[y, x0:x1] & (sl.sum(1) > 40)
        sl[wm] = sl[wm] * (1 - amt) + OLIVE * amt
        hi = wm & (sl.sum(1) > 90)
        sl[hi, 1] = np.clip(sl[hi, 1] + 28, 0, 255)
        sl[hi, 0] = np.clip(sl[hi, 0] - 10, 0, 255)
        out[y, x0:x1] = sl
    return np.clip(out, 0, 255).astype(np.uint8)


def jagged_bottoms(arr: np.ndarray, which: list[int], seed: int = 3) -> np.ndarray:
    out = arr.copy()
    rng = np.random.RandomState(seed)
    bottom_start = RAILS[1][1] + 2
    h = out.shape[0]
    for i in which:
        x0, x1 = PICKETS[i]
        for x in range(x0, x1):
            extra = int(rng.randint(18, 55))
            y0 = max(bottom_start, h - extra)
            out[y0:h, x] = m.PINK
            if y0 - 1 >= 0 and not m.magenta_mask(out[y0 - 1 : y0, x : x + 1])[0, 0]:
                out[y0 - 1, x] = np.array([10, 7, 5], dtype=np.uint8)
    return out


def darken_wood(arr: np.ndarray, factor: float) -> np.ndarray:
    out = arr.copy().astype(np.float32)
    w = wood(arr)
    out[w] *= factor
    return np.clip(out, 0, 255).astype(np.uint8)


def make_up_variants(base: np.ndarray) -> list[np.ndarray]:
    p = PICKETS
    v = [None] * 9
    v[0] = base.copy()
    v[1] = erase_picket_body(base, *p[3])
    mossed = base.copy()
    for a, b in p:
        mossed = moss_picket(mossed, a, b, depth=120)
    v[2] = mossed
    v[3] = break_picket(base, *p[0], drop=95)
    hole = erase_picket_body(base, *p[1])
    hole = erase_picket_body(hole, *p[2])
    v[4] = hole
    stair = base.copy()
    stair = shorten_picket(stair, *p[1], drop=48)
    stair = shorten_picket(stair, *p[2], drop=88)
    stair = shorten_picket(stair, *p[3], drop=128)
    v[5] = stair
    half = erase_picket_body(base, *p[2])
    half = erase_picket_body(half, *p[3])
    v[6] = half
    rot = jagged_bottoms(base, [0, 1, 2, 3], seed=21)
    rot = break_picket(rot, *p[1], drop=60)
    rot = break_picket(rot, *p[3], drop=42)
    v[7] = rot
    wreck = erase_picket_body(base, *p[2])
    wreck = break_picket(wreck, *p[0], drop=90)
    wreck = moss_picket(wreck, *p[3], depth=130)
    wreck = darken_wood(wreck, 0.78)
    v[8] = wreck
    return [finish(x) for x in v]


def assemble(left: np.ndarray, right: np.ndarray, up: np.ndarray, down: np.ndarray) -> np.ndarray:
    FRAME = 820
    margin = 18
    overlap = 16
    lc, rc, uc, dc = map(crop_content, (left, right, up, down))
    lh, lw = lc.shape[0], lc.shape[1]
    side_h = FRAME - margin * 2
    side_w = max(70, int(round(lw * side_h / lh)))
    left_s = scale_rgb(lc, side_w, side_h)
    right_s = scale_rgb(rc, side_w, side_h)
    inner_w = FRAME - margin * 2 - side_w * 2 + overlap * 2
    uh = int(round(uc.shape[0] * inner_w / uc.shape[1]))
    uh = min(uh, int(FRAME * 0.36))
    up_s = scale_rgb(uc, inner_w, uh)
    down_s = scale_rgb(dc, inner_w, uh)
    canvas = np.full((FRAME, FRAME, 3), m.PINK, dtype=np.uint8)
    paste_over(canvas, left_s, margin, margin)
    paste_over(canvas, right_s, FRAME - margin - side_w, margin)
    xU = margin + side_w - overlap
    paste_over(canvas, up_s, xU, margin)
    paste_over(canvas, down_s, xU, FRAME - margin - uh)
    return finish(canvas)


def to_tile(arr: np.ndarray) -> Image.Image:
    im = Image.fromarray(arr).resize((TILE, TILE), Image.Resampling.LANCZOS)
    return Image.fromarray(finish(np.array(im.convert("RGB"))))


def catalog(tiles: list[Image.Image]) -> Image.Image:
    face = m.font(18)
    cell_w = DISPLAY + GAP_X_S
    cell_h = DISPLAY + LABEL_H + GAP_Y
    sheet = Image.new(
        "RGB",
        (MARGIN * 2 + COLS * cell_w - GAP_X_S, MARGIN * 2 + ROWS * cell_h - GAP_Y + 8),
        PINK_T,
    )
    draw = ImageDraw.Draw(sheet)
    for i, tile in enumerate(tiles):
        row, col = divmod(i, COLS)
        x = MARGIN + col * cell_w
        y = MARGIN + row * cell_h
        cell = tile.resize((DISPLAY, DISPLAY), Image.Resampling.LANCZOS)
        cell = Image.fromarray(finish(np.array(cell.convert("RGB"))))
        sheet.paste(cell, (x, y))
        label = f"fence {i + 1}"
        bbox = draw.textbbox((0, 0), label, font=face)
        tw = bbox[2] - bbox[0]
        draw.text((x + (DISPLAY - tw) // 2, y + DISPLAY + 8), label, fill=(28, 28, 28), font=face)
    return sheet


def main() -> None:
    left, right = load("left"), load("right")
    up_b, down_b = load("up"), load("down")
    ups = make_up_variants(up_b)
    downs = make_up_variants(down_b)
    tiles = []
    for i in range(9):
        L, R = left.copy(), right.copy()
        if i == 8:
            L, R = darken_wood(left, 0.78), darken_wood(right, 0.78)
        frame = assemble(L, R, ups[i], downs[i])
        tile = to_tile(frame)
        path = DST / f"fence_25d_{i + 1:02d}.png"
        tile.save(path, optimize=True)
        tiles.append(tile)
        print("saved", path.name)

    sheet = catalog(tiles)
    sheet.save(m.OUT / "fence_25d_sheet_blocks.png", optimize=True)
    sheet.save(DST / "fence_25d_sheet_blocks.png", optimize=True)
    print("sheet", sheet.size)


if __name__ == "__main__":
    main()
