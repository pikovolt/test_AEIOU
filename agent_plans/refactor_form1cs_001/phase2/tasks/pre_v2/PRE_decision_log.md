# PRE Decision Log（pre_v2専用）

## 記録ルール
- PRE定義（SSOT/ユニット/判定運用）の変更のみ記録する。
- 実装詳細は記録しない。
- 1エントリにつき、`decisionId/date/summary/reason/impact/followup/ssotRefs` を必須とする。

## テンプレート
### PREV2-DEC-XXX
- date:
- summary:
- reason:
- impact:
- followup:
- ssotRefs:
  - R-Px-yy

## Entries

### PREV2-DEC-001
- date: 2026-02-28
- summary: PREの不変要件を `PRE_SSOT.md` に一本化した。
- reason:
  - ユニット化後も、前提事実と必須要件の正本が分離しており、解釈差の余地があったため。
- impact:
  - PRE判定の基準を `R-P0/R-P1/R-P2` で統一。
  - 各ユニットの役割をSSOT要件に対応付けて追跡可能にした。
- followup:
  - 新規ユニット追加時は、SSOT対応表の更新を必須化する。
- ssotRefs:
  - R-P0-01
  - R-P0-02
  - R-P0-03
  - R-P1-01
  - R-P1-02
  - R-P1-03
  - R-P1-04

### PREV2-DEC-002
- date: 2026-02-28
- summary: PRE記録を「方針（decision）」と「実行（worklog）」に分離した。
- reason:
  - 変更判断と実施証跡が混在すると、監査時に履歴追跡コストが高くなるため。
- impact:
  - 方針変更は `PRE_decision_log.md`、実行結果は `PRE_worklog.md` に分離。
- followup:
  - レビュー時は decision/worklog の両方を確認対象にする。
- ssotRefs:
  - R-P1-04

### PREV2-DEC-003
- date: 2026-02-28
- summary: R-P1-02 のサービス名称・導入方針と PRE/IMPL 用語を固定した。
- reason:
  - サービス名の導入方式（新規/既存拡張）と責務語彙が文書間で暗黙になっており、レビュー判定で揺れが生じる恐れがあったため。
- impact:
  - `GridInputInterpreter`（新規導入）、`GridViewUpdater`（`GridCellRenderer`拡張）、`GridDataSyncService`（`GridViewManager`拡張）を正本化。
  - `Form1` の責務をイベント中継・依存注入・UI境界に限定する方針を明記。
  - IMPLタスク（VM2-002/003/004/006）へ関連サービス語彙を反映し、PREとの用語一致を確保。
- followup:
  - VM2実装時は、各PRで「関連サービス」節の語彙と実装上の責務配置が一致することを確認する。
- ssotRefs:
  - R-P1-02
