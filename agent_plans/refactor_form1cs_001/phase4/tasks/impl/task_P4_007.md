# task_P4_007

## 目的
`Shift+Delete`（行削除）で顕在化した性能課題（体感遅延）を、既存仕様を変えずに改善する。

## 対象
- `AEIOU/WindowsFormsApplication1/Form1.cs`
- `AEIOU/WindowsFormsApplication1/Form1.Shortcut.cs`
- `AEIOU/WindowsFormsApplication1/GridViewManager.cs`
- `AEIOU/WindowsFormsApplication1/ContinuityStateService.cs`
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_007.md`

## 非対象
- キー機能仕様の追加・変更（既存仕様維持）
- 移動/選択ロジックの大規模再設計
- 修飾キー判定の単一路線化（`P4-006` の担当範囲）

## 依存タスク
- `P4-006`（完了後に着手）

## 完了条件（Gate）
- `Shift+Delete` 実行時のバッチ更新で、以下が集約されている。
  - `CellValueChanged -> RecalculateColumn` の列全走査をセル単位で繰り返さない
  - `InvalidateCell` のセル単位連発を抑止し、再描画を集約する
- 体感遅延の改善を計測記録で確認できる（単発/連続の前後比較）。

## 回帰観点
- `../../checklists/smoke_phase4.md` の「入力経路」「移動/選択」「回帰観点」。
- 特に `BackSpace` / `Delete` / `Undo` / `Redo` 往復で表示とデータが一致すること。

## 実施TODO（本タスク内）
- TODO-PERF-01: バッチ書き込み中の `CellValueChanged` 起因 `RecalculateColumn` を抑止し、終了時に変更列のみ再計算する。
- TODO-PERF-02: バッチ書き込み中の `InvalidateCell` 連発を抑止し、終了時に集約再描画へ切替える。
- TODO-PERF-03: `Shift+Delete` の前後で処理時間計測（1回平均 / 10回平均）を取得し、`reports/` に記録する。