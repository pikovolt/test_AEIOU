# task_P3_004

## 目的
選択移動処理をコマンド化し、KeyDown分岐から直接操作呼び出しを減らす。

## 対象
- `Form1.cs`
- `GridSelectionService.cs`
- 新規 `GridMoveSelectionCommand.cs`
- 新規 `GridKeyCommandDispatcher.cs`

## 非対象
- 数値入力・空セル入力処理の分離
- 描画ロジックの変更

## 依存タスク
- P3-003

## 完了条件（Gate）
- 選択移動処理が dispatcher/command 経由で実行される。
- キーボード移動と範囲選択が既存仕様どおり動作する。

## 回帰観点
- `../../checklists/smoke_phase3.md` の入力系1, 操作系1
