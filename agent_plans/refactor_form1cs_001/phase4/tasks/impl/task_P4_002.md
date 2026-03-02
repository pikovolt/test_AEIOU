# task_P4_002

## 目的
入力コマンド面の定義を固定し、`P4-003` 以降の実装で解釈差が出ない状態を先に確立する。

## 対象
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_002.md`
- `agent_plans/refactor_form1cs_001/phase4/checklists/smoke_phase4.md`
- `agent_plans/refactor_form1cs_001/phase4/decision_log.md`
- `agent_plans/refactor_form1cs_001/phase4/reports/impl_done/P4_002.md`

## 非対象
- アプリ実装コード（`AEIOU/WindowsFormsApplication1/`）の変更
- `P4-003` 以降（Dispatcher/Command導入）の先行実装
- 性能KPI（P95）の計測・判定

## 依存タスク
- `P4-001`（完了済）

## 完了条件（Gate）
- 入力コマンド面の定義（キー入力系の責務境界・回帰必須観点）が文書として固定されている。
- 修飾キー判定の扱いが「チャネル別運用」として明記され、`P4-006` での単一路線化判断へ接続されている。
- `P4-003` に渡す前提（`GridShortcutRouter` の段階的置換方針）が `decision_log.md` に記録されている。

## 回帰観点
- `../../checklists/smoke_phase4.md` の「移動/選択」「値入力」必須観点。
