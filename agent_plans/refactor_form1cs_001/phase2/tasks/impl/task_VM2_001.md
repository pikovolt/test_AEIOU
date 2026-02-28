# task_VM2_001: VirtualMode=true 起動成立（表示崩壊なし）

## 目的
`DataGridView.VirtualMode` を有効化しても、初期表示と基本操作が崩れない状態を確立する。

## 実施内容
- `VirtualMode=true` を適用する。
- 起動時の表示崩壊（空白表示、例外、描画乱れ）がないことを確認する。
- 既存イベント配線との干渉箇所を洗い出す。

## 完了条件
- VirtualMode有効で起動できる。
- 初期表示が既存期待値を満たす。
- 次タスク（`CellValueNeeded`）への前提条件が明記されている。

## 依存ユニット（PRE見出し単位）
- `U-PRE001-EVT`（`task_PRE_001.md`）
- `U-PRE003-GATE` / `U-PRE003-HANDOVER`（`task_PRE_003.md`）

参照（着手時点で必須ではない）:
- `U-PRE004-CONTRACT` / `U-PRE004-TRYGET` / `U-PRE004-BOUNDARY`（`task_PRE_004.md`）

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
