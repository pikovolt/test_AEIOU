# done_VM2_010

- 対応タスク: [task_VM2_010](../../tasks/impl/task_VM2_010.md)
- 判定: Done

## 共通必須欄
- 対象範囲（今回変更したファイル/機能/イベント）:
  - `PRE_SSOT.md` R-P0-03 への Phase 2 適用範囲注記の追加。
  - `PRE_decision_log.md` への PREV2-DEC-005 エントリの追加。
- 非対象（今回やらないこと）:
  - `calcBorderState()`・`drawFrameNumber()` の実装コード変更（Phase 3 スコープ）。
  - R-P0-03 の Phase 3 判定基準の確定（Phase 3 PRE で実施）。
- 証跡リンク（実行ログ、確認メモ、関連Issue/PRなど）:
  - [task_VM2_010](../../tasks/impl/task_VM2_010.md)
  - `PRE_SSOT.md:46-55` — R-P0-03 Phase 2 適用範囲注記（本タスクで追加）
  - `PRE_decision_log.md` — PREV2-DEC-005（本タスクで追加）

## IMPL判定項目（動作成立・回帰確認・非対象明記）
- 動作成立（成功条件と確認結果）:
  - R-P0-03 の Phase 2 における達成範囲が「CellPainting ハンドラが直接値取得・データ書込を行わない」として `PRE_SSOT.md` に明文化された。
  - `calcBorderState()`・`drawFrameNumber()` 等の Form1 残存ロジックが Phase 3（描画分離）スコープとして `PRE_decision_log.md` に正式記録された。
  - `KeyDown/KeyPress` の完全分離（`GridInputInterpreter` 委譲）は Phase 3〜4 スコープとして明記された。
- 回帰確認（最小2観点）:
  - 観点1: `PRE_SSOT.md` の R-P0-03 に「Phase 2 適用範囲注記」ブロックが追加され、Phase 2 PASS の意味が明確に定義されている。
  - 観点2: `PRE_decision_log.md` に PREV2-DEC-005 が追加され、変更根拠（GAP-H2対応）・影響・フォローアップが記録されている。
- 未解決事項/フォローアップ（必要時）:
  - VM2-010-F01 / Phase 3 PRE において R-P0-03 の完全分離条件（Form1 残存ロジック除去）を再評価 / 評価: Phase 3 着手時対応。

## 検証セット
- セット1
  - 前提/操作/期待/実結果:
  - 前提: VM2-008 完了済み、PRE_SSOT.md および PRE_decision_log.md が存在する。
  - 操作: PRE_SSOT.md の R-P0-03 と PRE_decision_log.md の最新エントリを確認する。
  - 期待: R-P0-03 に Phase 2 適用範囲が注記され、Form1 残存ロジックの Phase 3 帰属が記録されている。
  - 実結果: `PRE_SSOT.md:51-55` に Phase 2 適用範囲注記を追加。`PRE_decision_log.md` に PREV2-DEC-005（date: 2026-03-01）を追加。R-P0-03 の Phase 2/Phase 3 スコープ境界が文書上で明確になった。
  - 証跡リンク: `PRE_SSOT.md`（本タスクで更新）、`PRE_decision_log.md`（本タスクで更新）
