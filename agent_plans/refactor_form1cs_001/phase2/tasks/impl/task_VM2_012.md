# task_VM2_012: 初期化ループ全セルInvalidate評価（GAP-H4対応）

## 目的
`dataGridInitialize` 内で全セルに対して `SetCellValue("", ...)` → `InvalidateCell()` を
逐次発行しており、VirtualMode の本来の設計（CellValueNeeded で都度取得）と相反する。
26列×500行規模で 13,000件の `InvalidateCell()` が初期化時に走ることになる。
この実態を評価し、問題があれば対処する。

## 根拠
- GAP-H4（精査報告 2026-03-01）
- VM2-004（Row/Column同期）の見落とし観点

## 関連サービス
- `GridViewManager`（`SetCellDisplayValue` / `InvalidateCell`）
- `TimingSheetModel`（`SetCell`）

## 実施内容
- `dataGridInitialize` での全セル初期化ループを確認し、以下を評価する。
  - `SetCellValue(j, i, "")` が VirtualMode で必要かどうかを確認する。
    （TimingSheetModel のコンストラクタで "" 初期化済みであれば不要な可能性がある）
  - 全セル分の `InvalidateCell()` 発行が起動時パフォーマンスに与える影響を確認する。
  - 初期化ループを削除または最適化できるかを判断する。
- 問題がある場合は以下のいずれかで対処する。
  - 全セル初期化ループを削除し、VirtualMode の初期描画を `CellValueNeeded` に委ねる。
  - または問題なし（初期化上必要）の根拠を記録する。

## 完了条件
- 全セル初期化ループの要否が判定されている。
- 問題あり: 最適化または削除が完了し、起動表示崩れなしを確認している。
- 問題なし: 「必要」の根拠が記録されている。
- いずれの場合も、初期化後の表示崩れなし・起動例外なしを回帰確認している。

## 依存タスク
- VM2-004

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
