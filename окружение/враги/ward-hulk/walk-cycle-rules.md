# Walk cycle rules — Ward Hulk

## Проблема (merged v1)

| Ряд | Баг |
|-----|-----|
| WALK FRONT | Всегда одна нога (viewer right / char left) впереди — **нет чередования** |
| WALK BACK | То же — одна нога «ведёт» |
| WALK RIGHT/LEFT | Проверить contact/passing |

## Правило — чередование

**8 кадров = 4 шага (R L R L):**

| Frame | WALK FRONT (к камере) | WALK BACK (от камеры) | WALK RIGHT (side) |
|-------|----------------------|----------------------|-------------------|
| 1 | **R** foot forward, L back | **R** back, L forward (вид сзади) | R contact down |
| 2 | passing | passing | R up |
| 3 | **L** foot forward, R back | **L** back, R forward | L contact down |
| 4 | passing | passing | L up |
| 5 | **R** forward | **R** | R contact |
| 6 | passing | passing | R up |
| 7 | **L** forward | **L** | L contact |
| 8 | passing | passing | L up |

**Character right** = cleaver hand side in FRONT view.

## Unity

```csharp
// Animator: WalkFront, WalkBack, WalkRight, WalkLeft
// 8 frames @ 8fps, loop
```

## Файлы

- `ward_hulk_merged_sheet_v2.png` — fix
- `prompt_merged_sheet.txt` — обновлён
