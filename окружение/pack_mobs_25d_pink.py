"""Copy pink-outline mob sheets into blocks_filled/mobs_25d_pink."""
from pathlib import Path
import shutil

import importlib.util
import numpy as np
from PIL import Image

ROOT = Path(r"C:\all\Концепции\ассеты\окружение")
spec = importlib.util.spec_from_file_location("pack_trees_hq", ROOT / "pack_trees_hq.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)

ASSETS = Path(r"C:\Users\vadfi\.cursor\projects\c-all-fix\assets")
DST = m.OUT / "mobs_25d_pink"
DST.mkdir(parents=True, exist_ok=True)

FILES = [
    "mob_cinder_gaoler_sheet_pink.png",
    "mob_psalm_tick_sheet_pink.png",
    "mob_cistern_widow_sheet_pink.png",
    "mob_nail_mastiff_sheet_pink.png",
    "mob_rust_chantry_sheet_pink.png",
    "mob_sulfur_adept_sheet_pink.png",
]


def finish(arr: np.ndarray) -> np.ndarray:
    arr = m.snap_flat_magenta(arr).copy()
    mag = m.magenta_mask(arr)
    return m.snap_flat_magenta(m.despill(arr, mag))


def main() -> None:
    for name in FILES:
        src = ASSETS / name
        arr = finish(np.array(Image.open(src).convert("RGB")))
        Image.fromarray(arr).save(DST / name, optimize=True)
        shutil.copy2(DST / name, m.OUT / name)
        print("saved", name, arr.shape[1], arr.shape[0])
    print("folder", DST)


if __name__ == "__main__":
    main()
