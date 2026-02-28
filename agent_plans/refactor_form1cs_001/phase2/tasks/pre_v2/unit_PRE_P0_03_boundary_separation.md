# unit_PRE_P0_03_boundary_separation

## 目的
入力・描画・値取得の責務混在を防ぎ、回帰範囲を限定する。

## 必須確認
- `CellPainting` は描画責務のみ。
- `KeyDown/KeyPress` は入力制御責務のみ。
- 値取得ロジックは `TryGetCellValue` に集約。

## 完了判定
- 上記3項目がすべてPASS。
