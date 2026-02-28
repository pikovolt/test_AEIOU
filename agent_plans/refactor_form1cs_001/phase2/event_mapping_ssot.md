# Event Mapping SSOT（Phase2）

Phase2 で扱う主要イベントの担当層と IMPL タスク対応を単一正本として定義する。

| イベント名 | 担当層 (Form1 or Service) | 対応IMPLタスクID | 対象外理由 |
| --- | --- | --- | --- |
| CellPainting | Form1 | VM2-006 | - |
| CellValueNeeded | Service | VM2-002 | - |
| CellValuePushed | Service | VM2-003, VM2-007 | - |
| KeyDown | Form1 | VM2-005 | - |
| KeyPress | Form1 | - | VirtualMode 直接配線の主対象外。既存ショートカット互換の監視のみ（必要時は VM2-005 で評価）。 |

## 運用ルール
- PRE 判定（R-P1-01）と IMPL タスク着手時のイベント割当確認は本書を唯一の参照元とする。
- 変更時は `unit_PRE_P1_01_event_mapping_consistency.md` と `PRE_worklog.md` の参照整合も同時に更新する。
