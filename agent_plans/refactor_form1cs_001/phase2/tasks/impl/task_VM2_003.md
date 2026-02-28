# task_VM2_003: CellValuePushed write配線（編集反映成立）

## 目的
編集結果を `CellValuePushed` へ集約し、VirtualMode環境での値反映を成立させる。

## 実施内容
- `CellValuePushed` で編集値をデータソースへ反映する。
- 変換失敗時の扱い（バリデーション/フォールバック）を定義する。
- 編集コミット時の反映タイミングを確認する。

## 完了条件
- 編集した値が期待どおり保存される。
- 編集キャンセル時の挙動が既存仕様と整合する。
- `VM2-004` 以降で必要な更新通知条件が明記されている。

## 依存タスク
- VM2-002

## Done定義参照
- 共通定義 `DONE_TEMPLATE_VM2.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_VM2.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
