# task_VM2_010: R-P0-03スコープ不整合の解消（GAP-H2対応）

## 目的
PRE SSOT R-P0-03「CellPainting は描画責務のみ」「KeyDown は入力制御責務のみ」の基準が、
実装上は Phase 3（描画分離）・Phase 4（入力分離）のスコープに属する。
Phase 2 において PASS と判定されているが、実態は `calcBorderState()`（100行超）等が
Form1 の CellPainting ハンドラに残存しており乖離がある。
この不整合を正式に記録し、Phase 2 / Phase 3 のスコープ境界を明確化する。

## 根拠
- GAP-H2（精査報告 2026-03-01）
- PRE SSOT R-P0-03
- architecture.md Phase 3「描画分離」定義

## 関連サービス
- なし（方針決定・ドキュメントタスク）

## 実施内容
- 以下を確認し、決定内容を `decision_log.md` に記録する。
  - R-P0-03「CellPainting は描画責務のみ」の Phase 2 における達成範囲を明文化する。
  - `calcBorderState()` / `drawFrameNumber()` 等の Form1 残存ロジックを
    「Phase 3 スコープ」として正式に分類・列挙する。
  - PRE SSOT R-P0-03 の判定基準に「Phase 2 適用範囲」注記を追加するか、
    または別 ID として分離するかを決定する。
- `PRE_SSOT.md` に Phase 2 適用範囲の注記を追加する（変更する場合）。
- `PRE_decision_log.md` に本変更の経緯を記録する。

## 完了条件
- R-P0-03 の Phase 2 における達成範囲が文書上で明確になっている。
- Form1 残存ロジック（`calcBorderState` 等）の Phase 3 スコープ帰属が記録済み。
- `decision_log.md` または `PRE_decision_log.md` に変更根拠が記録されている。

## 依存タスク
- VM2-008

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
