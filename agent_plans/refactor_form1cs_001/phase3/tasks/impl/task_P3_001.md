# task_P3_001

## 目的
`Form1.cs` からフレーム番号描画とヘッダ描画の責務を分離し、CellPainting の見通しを改善する。

## 対象
- `Form1.cs`
- `GridCellRenderer.cs`
- 新規 `GridFrameHeaderPainter.cs`
- 新規 `GridFrameLabelFormatter.cs`

## 非対象
- 入力系ロジックの変更
- 境界線判定ロジックの移管

## 依存タスク
- なし（起点）

## 完了条件（Gate）
- `Form1.cs` 内のフレーム番号描画責務が専用クラスへ移管される。
- 既存表示仕様を維持できる。

## 回帰観点
- `../../checklists/smoke_phase3.md` の描画系1,2
