# task_P3_004

## 目的
選択移動処理をコマンド化し、KeyDown分岐から直接操作呼び出しを減らす。

## 対象
- `Form1.cs`
- `GridSelectionService.cs`
- 繰越 `GridMoveSelectionCommand.cs`（Phase4 `P4-004` で導入）
- 繰越 `GridKeyCommandDispatcher.cs`（Phase4 `P4-003` で導入）

## 非対象
- 数値入力・空セル入力処理の分離
- 描画ロジックの変更

## 依存タスク
- P3-003

## 完了条件（Gate）
- Phase3 時点では既存経路で移動/範囲選択の動作成立を確認し、dispatcher/command 化は Phase4 に繰り越されている。
- キーボード移動と範囲選択が既存仕様どおり動作する。

## 回帰観点
- `../../checklists/smoke_phase3.md` の入力系1, 操作系1

## 整合注記（P4-001）
- `GridMoveSelectionCommand.cs` / `GridKeyCommandDispatcher.cs` は Phase3 では未作成であることを確認。
- 上記2点は Phase4 タスク `P4-003`, `P4-004` に移管して整合を確定する。
