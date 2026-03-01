# task_P3_002

## 目的
`calcBorderState` 周辺の境界判定ロジックを分離し、描画判定の責務境界を明確化する。

## 対象
- `Form1.cs`
- 新規 `GridBorderStateCalculator.cs`
- `Enums.cs`

## 非対象
- KeyDown系の入力処理分離
- Undo/Redoの挙動変更

## 依存タスク
- P3-001

## 完了条件（Gate）
- 境界判定ロジックが `Form1.cs` から専用クラスへ移管される。
- 連続状態境界の表示が既存仕様と一致する。

## 回帰観点
- `../../checklists/smoke_phase3.md` の描画系3
