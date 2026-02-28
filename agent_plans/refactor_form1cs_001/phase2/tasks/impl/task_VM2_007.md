# task_VM2_007: Undo/Redo最小回帰（単一・範囲）

## 目的
VirtualMode配線後も Undo/Redo の最小保証（単一セル・範囲編集）を維持する。

## 実施内容
- Undo/Redoの記録単位と復元単位を確認する。
- 単一セル編集と範囲編集で回帰確認する。
- `CellValuePushed` 経由時の履歴整合を点検する。

## 完了条件
- Undo/Redo が主要ケースで成立する。
- 履歴破損や復元漏れがない。
- Gate判定向けの確認結果が整理されている。

## 依存タスク
- VM2-003
- VM2-005

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
