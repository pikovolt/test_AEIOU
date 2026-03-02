# task_P4_003

## 目的
`KeyDown` 経路に `GridKeyCommandDispatcher` を導入し、入力解釈と実行責務の境界を明確化する。

## 対象
- `AEIOU/WindowsFormsApplication1/GridKeyCommandDispatcher.cs`
- `AEIOU/WindowsFormsApplication1/Form1.cs`
- `AEIOU/WindowsFormsApplication1/AEIOU.csproj`
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_003.md`

## 非対象
- `MoveSelectionCommand` の導入（`P4-004`）
- 値入力コマンド分離（`P4-005`）
- 修飾キー判定の単一路線化（`P4-006`）

## 依存タスク
- `P4-002`（完了済）

## 完了条件（Gate）
- `dataGridView1_KeyDown` が `GridKeyCommandDispatcher` 経由で入力ディスパッチを実行する。
- メニューショートカット優先の既存挙動（`tryExecuteShortcut`）を維持する。
- 既存の `GridShortcutRouter` 連携が維持され、既存ショートカット動作が退行しない。

## 回帰観点
- `../../checklists/smoke_phase4.md` の入力経路、移動/選択、値入力。
