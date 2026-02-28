# Phase 2 Overview

## 全体目的
`Form1.cs` の責務を分割し、VirtualMode 対応時の変更容易性・回帰安全性・レビュー容易性を高める。

## PREレーン（準備）
- **PRE-001**: 既存挙動の可視化（現行イベント/描画/編集フローの把握）
- **PRE-002**: 小さな抽出単位の定義（UIイベント処理、グリッド操作、データ更新）
- **PRE-003**: VirtualMode起動前チェック（移行元: task_VM2_001）
- **PRE-004**: CellValueNeeded導入前設計（移行元: task_VM2_002）

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

## 着手条件
- IMPLレーンの着手条件は、必須PREタスク（PRE-001〜PRE-004）の完了とする。
- VM2ごとの必須PRE判定は [`pre_impl_traceability.md`](./pre_impl_traceability.md) の「VM2ごとの必須PREチェックリスト（機械判定用）」に従う。
- 例外的に先行検証が必要な場合も、正式着手（タスク開始宣言・PR作成）は必須PRE完了後に行う。

## 移行中ルール（pre/impl 命名規約）
- 旧 `task_VM2_001.md` / `task_VM2_002.md` は、PREレーンへ移動し `task_PRE_003.md` / `task_PRE_004.md` として扱う。
- `tasks/impl/task_VM2_001.md` は新規作成した IMPL 起点タスクとして運用する。
- `tasks/impl/task_VM2_002.md` は IMPL レーンの実装タスクとして維持する（削除しない）。
- **移行期間中の暫定ルール: PREレーンでは VM2 番号を新規発行しない。**
- IMPLタスクは `task_VM2_xxx.md`、PREタスクは `task_PRE_xxx.md` を使用する。

## 依存ルール
- PREタスクはPREレーン内でのみ依存を張る。
- IMPLタスクはIMPLレーン内でのみ依存を張る。
- レーン間依存は **PRE→IMPL の一方向依存のみ許可** する。
- **VM2番号はIMPL専用、PRE番号はPRE専用** とし、同一番号帯の二重利用を禁止する。

## 完了条件
- `phase2/tasks/pre/` と `phase2/tasks/impl/` 配下タスクが順次完了し、各PRで回帰確認済み。
- 重大な方針変更は `decision_log.md` に記録済み。
