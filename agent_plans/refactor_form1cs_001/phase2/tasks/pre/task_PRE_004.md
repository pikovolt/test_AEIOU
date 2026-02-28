# task_PRE_004: CellValueNeeded導入前設計（移行元: task_VM2_002）

## 目的
`CellValueNeeded` 導入前に、VirtualMode表示に必要な読み出し経路と境界条件を設計として固定化する。

## 実施内容
- `CellValueNeeded` に集約する参照経路を整理する。
- 行列インデックス境界の防御観点を定義する。
- 既存表示ロジックとの差分確認観点を列挙する。

## 完了条件
- `CellValueNeeded` 実装前提となる設計観点が明記されている。
- IMPLタスクの入出力前提に引き継げる。
- 共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。

## 依存タスク
- PRE-003

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
