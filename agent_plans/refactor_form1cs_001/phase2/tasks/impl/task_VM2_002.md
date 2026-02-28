# task_VM2_002: CellValueNeeded read配線（表示成立）

## 目的
VirtualMode表示に必要な読み出し経路を `CellValueNeeded` へ集約し、表示成立を担保する。

## 実施内容
- `CellValueNeeded` で参照データを返す実装へ切り替える。
- 行列インデックス境界の防御を入れる。
- 既存表示ロジックとの差異を確認する。

## 完了条件
- 表示データが `CellValueNeeded` 経由で取得される。
- スクロール/再描画時に表示欠落が発生しない。
- `VM2-003` 実装に必要な入出力前提が明記されている。

## 依存タスク
- VM2-001

## Done定義参照
- 共通定義 `DONE_TEMPLATE_VM2.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_VM2.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
