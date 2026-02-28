# task_VM2_006: Invalidate境界最適化（過剰再描画抑制）

## 目的
無駄な再描画を抑えつつ表示整合を維持し、VirtualMode時の操作体感を安定させる。

## 関連サービス
- `GridViewUpdater`（表示更新）

## 実施内容
- `Invalidate` 呼び出し境界を見直す。
- `GridViewUpdater` を経由した描画更新境界へ整理する。
- 必要最小限の再描画範囲を定義する。
- スクロール・編集・選択変更時の描画負荷を比較する。

## 完了条件
- 過剰再描画が抑制されている。
- 描画欠落やちらつきが悪化していない。
- 最適化内容と副作用観点が明記されている。

## 依存タスク
- VM2-004
- VM2-005

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
