# unit_PRE_P0_03_boundary_separation

## 目的
入力・描画・値取得の責務混在を防ぎ、回帰範囲を限定する。

## 必須確認
- `CellPainting` は描画責務のみ。
- `KeyDown/KeyPress` は入力制御責務のみ。
- 値取得ロジックは `TryGetCellValue` に集約。

## 完了判定
- 上記3項目がすべてPASS。

## 完了判定への対応記録（PRE_P0_03）
- PASS: `dataGridView1_CellPainting` では `checkContinuty` から継続フラグ（事前計算済み）を受け取るだけにし、セル探索は実施しない構成へ変更。
- PASS: 値探索ロジックを `ContinuityStateService` へ移動し、`SetCellValue`/初期化時に列単位で再計算することで、描画外レイヤーでデータアクセスを実行。
- PASS: `TryGetCellValue` の集約方針は維持し、`ContinuityStateService` での値参照は `GetCellValue`（内部で `TryGetCellValue` 利用）経由に統一。
