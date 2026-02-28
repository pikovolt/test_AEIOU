# Phase 2 Overview

## 全体目的
`Form1.cs` の責務を分割し、VirtualMode 対応時の変更容易性・回帰安全性・レビュー容易性を高める。

## PRE（事前定義）
- PRE定義は `phase2/tasks/pre_v2/` を正本ディレクトリとして運用する。
- 必須要件の正本は `phase2/tasks/pre_v2/PRE_SSOT.md` とする。
- PREの方針変更履歴は `phase2/tasks/pre_v2/PRE_decision_log.md`、実行記録は `phase2/tasks/pre_v2/PRE_worklog.md` を参照する。

## IMPLレーン（実装）
1. **VM2-001**: `VirtualMode=true` 起動成立（表示崩壊なし）  
   依存（前提タスク）: なし（IMPL起点タスク）
2. **VM2-002**: `CellValueNeeded` read配線（表示成立）  
   依存（前提タスク）: VM2-001
3. **VM2-003**: `CellValuePushed` write配線（編集反映成立）  
   依存（前提タスク）: VM2-002
4. **VM2-004**: Row/Column同期一本化（件数ズレ防止）  
   依存（前提タスク）: VM2-002, VM2-003
5. **VM2-005**: CurrentCell/Selection整合（移動回帰）  
   依存（前提タスク）: VM2-004
6. **VM2-006**: Invalidate境界最適化（過剰再描画抑制）  
   依存（前提タスク）: VM2-004, VM2-005
7. **VM2-007**: Undo/Redo最小回帰（単一・範囲）  
   依存（前提タスク）: VM2-003, VM2-005
8. **VM2-008**: Phase2 Gate判定（証跡集約）  
   依存（前提タスク）: VM2-001〜VM2-007

## 依存ルール
- タスク依存は IMPL タスク（`VM2-xxx`）同士で管理する。
- 着手可否は、各タスクの完了条件と引き継ぎ情報の充足で判定する。

### 暫定運用（PRE-IMPL関係）
- 現時点では IMPL タスク着手判定は、各タスクの完了条件・実測結果を優先する。
- PRE基準の参照先は `phase2/tasks/pre_v2/PRE_SSOT.md` とする。
- PRE-IMPL の厳密なゲート同期規則は別途方針決定とする。

## 完了条件
- PRE判定は `phase2/tasks/pre_v2/PRE_SSOT.md` の R-P0/R-P1 を基準とし、R-P2は改善管理対象（非ブロッカー）とする。
- `phase2/tasks/impl/` 配下タスクが順次完了し、各PRで回帰確認済み。
- 重大な方針変更は `decision_log.md` に記録済み。
