#!/usr/bin/env python3
"""Outline + transparent background for character/UI sprites (not environment)."""

from __future__ import annotations

import sys
from pathlib import Path

from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
ASSETS_ROOT = Path(__file__).resolve().parents[3]
OUT_DIR = ROOT / "Assets" / "Resources" / "Art" / "_processed"

SKIP_PARTS = {
    "environment",
    "rooms",
    "tilesets",
    "library",
    "packagecache",
    "_processed",
}

SKIP_NAMES = {
    "combat_bg",
    "route_map",
    "a_plaza_room",
    "doors_sector",
    "topdown_sector",
}

SOURCE_DIRS = [
    ROOT / "Assets" / "Resources" / "Art",
    ROOT / "Assets" / "Animations" / "Player",
    ASSETS_ROOT / "враги",
    ASSETS_ROOT / "герои",
    ASSETS_ROOT / "ui-карты" / "sprites",
]


def is_magenta(r: int, g: int, b: int, tol: int = 85) -> bool:
    """Magenta / pink chroma key."""
    return r > 160 and b > 160 and g < min(r, b) - 30


def is_black_bg(r: int, g: int, b: int, icon_mode: bool) -> bool:
    if not icon_mode:
        return False
    return r < 20 and g < 20 and b < 20


def remove_background(img: Image.Image, icon_mode: bool) -> Image.Image:
    img = img.convert("RGBA")
    px = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if is_magenta(r, g, b) or is_black_bg(r, g, b, icon_mode):
                px[x, y] = (0, 0, 0, 0)
            elif is_magenta(r, g, b, tol=120):
                # soft pink fringe
                px[x, y] = (r, g, b, 0)
    return img


def add_outline(img: Image.Image, thickness: int = 2) -> Image.Image:
    img = img.convert("RGBA")
    alpha = img.split()[3]
    expanded = alpha
    for _ in range(thickness):
        expanded = expanded.filter(ImageFilter.MaxFilter(3))
    silhouette = Image.new("RGBA", img.size, (0, 0, 0, 255))
    silhouette.putalpha(expanded)
    return Image.alpha_composite(silhouette, img)


def should_process(path: Path) -> bool:
    low = str(path).lower()
    if any(part in low for part in SKIP_PARTS):
        return False
    name = path.stem.lower()
    if any(skip in name for skip in SKIP_NAMES):
        return False
    if path.suffix.lower() != ".png":
        return False
    if "_outlined" in name or "_transparent" in name:
        return False
    return True


def collect_files() -> list[Path]:
    found: set[Path] = set()
    for base in SOURCE_DIRS:
        if not base.exists():
            continue
        for p in base.rglob("*.png"):
            if should_process(p):
                found.add(p.resolve())
    return sorted(found)


def output_paths(src: Path) -> tuple[Path, Path]:
    rel = src.name
    parent_tag = src.parent.name
    if parent_tag not in ("Characters", "sprites", "Art"):
        rel = f"{parent_tag}_{src.name}"
    outlined = OUT_DIR / "outlined" / rel.replace(".png", "_outlined.png")
    transparent = OUT_DIR / "transparent" / rel.replace(".png", "_transparent.png")
    return outlined, transparent


def process_one(src: Path) -> tuple[Path, Path]:
    icon_mode = "icon" in src.stem.lower() or src.stem.lower() in {"player", "walk"}
    img = Image.open(src)
    transparent = remove_background(img, icon_mode)
    outlined = add_outline(transparent, thickness=2)

    out_outline, out_transparent = output_paths(src)
    out_outline.parent.mkdir(parents=True, exist_ok=True)
    out_transparent.parent.mkdir(parents=True, exist_ok=True)

    outlined.save(out_outline)
    transparent.save(out_transparent)
    return out_outline, out_transparent


def main() -> int:
    files = collect_files()
    if not files:
        print("No PNG files found.")
        return 1

    print(f"Processing {len(files)} files -> {OUT_DIR}")
    for src in files:
        try:
            o, t = process_one(src)
            print(f"OK  {src.name}")
            print(f"    {o.relative_to(ROOT)}")
            print(f"    {t.relative_to(ROOT)}")
        except Exception as exc:
            print(f"ERR {src}: {exc}", file=sys.stderr)
    print("Done.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
