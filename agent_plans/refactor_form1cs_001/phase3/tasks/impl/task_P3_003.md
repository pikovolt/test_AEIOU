# task_P3_003

## 目的
`dataGridView1_KeyDown` の前段分岐をルータ化し、入力処理の責務分離を開始する。

## 対象
- `Form1.cs`
- `GridInputInterpreter.cs`
- 新規 `GridShortcutRouter.cs`
- `KeyBinds.cs`

## 非対象
- 選択移動ロジックの全面移管
- 数値入力処理の全面移管

## 依存タスク
- なし（入力レーン起点）

## 完了条件（Gate）
- `Form1.cs` の KeyDown 前段分岐がルータ経由になる。
- 主要ショートカットの動作が維持される。

## 回帰観点
- `../../checklists/smoke_phase3.md` の入力系1
