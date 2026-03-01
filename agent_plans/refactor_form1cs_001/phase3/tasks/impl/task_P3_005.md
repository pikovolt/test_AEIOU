# task_P3_005

## 目的
数値入力/空セル入力の操作を `Form1.cs` から分離し、入力責務分離を完了する。

## 対象
- `Form1.cs`
- `GridViewOperation.cs`
- `GridViewManager.cs`
- `OperationGroup.cs`

## 非対象
- 新規機能追加
- 永続化仕様の変更

## 依存タスク
- P3-004

## 完了条件（Gate）
- 数値入力/空セル入力の主要経路が専用操作経由になる。
- Undo/Redo整合と保存/再読込再現性が維持される。

## 回帰観点
- `../../checklists/smoke_phase3.md` の入力系2,3 とデータ整合性2
