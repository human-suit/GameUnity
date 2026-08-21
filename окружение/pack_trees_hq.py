# Pack high-res generated props: magenta chroma with despill, no jaggy morphology.
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
OUT = Path(r"C:\all\Концепции\ассеты\окружение\blocks_filled")
CAT_BG = (28, 28, 28)
LABEL = (235, 235, 235)
PINK = np.array([255, 0, 255], dtype=np.uint8)
FONT_PATHS = [
    Path(r"C:\Windows\Fonts\consola.ttf"),
    Path(r"C:\Windows\Fonts\cour.ttf"),
    Path(r"C:\Windows\Fonts\arial.ttf"),
]
SETS = [
    {"slug": "dead_bush", "prefix": "dead_bush", "canvas": 1024, "display": 512, "prefix_file": "dead_bush_hq", "shared": False},
    {"slug": "dead_tree", "prefix": "dead_tree", "canvas": 1024, "display": 512, "prefix_file": "dead_tree_hq", "shared": False},
    {"slug": "big_tree", "prefix": "big_tree", "canvas": 1280, "display": 560, "prefix_file": "big_tree_hq", "shared": False},
    {"slug": "fence_v", "prefix": "fence_v", "canvas": 1024, "display": 512, "prefix_file": "fence_v_big", "shared": True, "keep_frame": True},
    {"slug": "green_tree", "prefix": "green_tree", "canvas": 1024, "display": 512, "prefix_file": "green_tree_hq", "shared": True},
]


def font(size: int):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def magenta_mask(arr: np.ndarray) -> np.ndarray:
    r = arr[:, :, 0].astype(np.int16)
    g = arr[:, :, 1].astype(np.int16)
    b = arr[:, :, 2].astype(np.int16)
    return (r > 170) & (b > 170) & (g < 140) & (r + b > g + 110)


def despill(arr: np.ndarray, mag: np.ndarray) -> np.ndarray:
    """Pull leftover magenta fringe toward neighboring bark, keep anti-alias."""
    out = arr.copy()
    mag_u8 = (mag.astype(np.uint8) * 255)
    dil = np.array(Image.fromarray(mag_u8, "L").filter(ImageFilter.MaxFilter(3))) > 0
    fringe = dil & (~mag)
    if not fringe.any():
        return out
    r, g, b = out[:, :, 0].astype(np.int16), out[:, :, 1].astype(np.int16), out[:, :, 2].astype(np.int16)
    # reduce magenta: clamp R/B toward G on fringe
    over = np.minimum(r, b) - g
    pull = np.clip(over, 0, 255)
    r2 = np.clip(r - pull, 0, 255)
    b2 = np.clip(b - pull, 0, 255)
    out[fringe, 0] = r2[fringe].astype(np.uint8)
    out[fringe, 2] = b2[fringe].astype(np.uint8)
    return out


def tight(arr: np.ndarray, mag: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    tree = ~mag
    if not tree.any():
        return arr, mag
    ys, xs = np.where(tree)
    pad = 18
    y0 = max(0, int(ys.min()) - pad)
    x0 = max(0, int(xs.min()) - pad)
    y1 = min(arr.shape[0], int(ys.max()) + 1 + pad)
    x1 = min(arr.shape[1], int(xs.max()) + 1 + pad)
    return arr[y0:y1, x0:x1], mag[y0:y1, x0:x1]


def snap_flat_magenta(arr: np.ndarray) -> np.ndarray:
    """Unify only near-pure chroma pixels; leave anti-aliased edges alone."""
    r, g, b = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2]
    flat = (r > 200) & (b > 180) & (g < 60)
    out = arr.copy()
    out[flat] = PINK
    return out


def fit(arr: np.ndarray, mag: np.ndarray, size: int, scale: float | None = None) -> np.ndarray:
    h, w = arr.shape[:2]
    if scale is None:
        scale = min((size - 48) / max(w, 1), (size - 64) / max(h, 1))
    nw, nh = max(1, int(w * scale)), max(1, int(h * scale))
    rgb = np.array(Image.fromarray(arr).resize((nw, nh), Image.Resampling.LANCZOS))
    rgb = snap_flat_magenta(rgb)
    canvas = np.full((size, size, 3), PINK, dtype=np.uint8)
    x = (size - nw) // 2
    y = size - nh - 20
    canvas[y : y + nh, x : x + nw] = rgb
    return canvas


def catalog(tiles: list[Image.Image], prefix: str, display: int) -> Image.Image:
    face = font(22)
    n = len(tiles)
    cols = min(3, n)
    rows = (n + cols - 1) // cols
    gap_x, gap_y, label_h, margin = 36, 52, 32, 36
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


def load_src(prefix_file: str, i: int) -> Image.Image:
    name = f"{prefix_file}_{i:02d}.png"
    p = ASSETS / name
    if not p.exists():
        matches = list(ASSETS.glob(f"{prefix_file}_{i:02d}*.png"))
        if not matches:
            raise FileNotFoundError(name)
        p = matches[0]
    return Image.open(p).convert("RGB")


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for spec in SETS:
        tiles = []
        folder = OUT / spec["slug"]
        folder.mkdir(exist_ok=True)
        for old in folder.glob(f"{spec['slug']}_*.png"):
            old.unlink()
        crops = []
        for i in range(1, 10):
            arr = snap_flat_magenta(np.array(load_src(spec["prefix_file"], i)))
            mag = magenta_mask(arr)
            if spec.get("keep_frame"):
                crops.append((arr, mag))
            else:
                crop, mag_c = tight(arr, mag)
                crops.append((crop, mag_c))
        shared_scale = None
        if spec.get("shared"):
            max_w = max(c.shape[1] for c, _ in crops)
            max_h = max(c.shape[0] for c, _ in crops)
            shared_scale = min(
                (spec["canvas"] - 48) / max(max_w, 1),
                (spec["canvas"] - 64) / max(max_h, 1),
            )
        for i, (crop, mag_c) in enumerate(crops, start=1):
            if spec.get("keep_frame"):
                size = spec["canvas"]
                fitted = np.array(Image.fromarray(crop).resize((size, size), Image.Resampling.LANCZOS))
                fitted = snap_flat_magenta(fitted)
            else:
                fitted = fit(crop, mag_c, spec["canvas"], shared_scale)
            im = Image.fromarray(fitted)
            im.save(folder / f"{spec['slug']}_{i:02d}.png", optimize=True)
            tiles.append(im)
            print(spec["slug"], i, crop.shape[:2], "->", spec["canvas"])
        catalog(tiles, spec["prefix"], spec["display"]).save(
            OUT / f"{spec['slug']}_sheet_blocks.png", optimize=True
        )
        print("sheet", spec["slug"])


if __name__ == "__main__":
    main()
