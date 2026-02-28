# Decision Log (方針変更のみ)

## 記録ルール
- 実装詳細ではなく、計画・方針の変更のみ記録する。
- 各項目に日付、変更内容、理由、影響範囲を含める。
- 各判断エントリには、必須欄として以下を必ず明記する。
  - `lane: pre|impl`
  - `affects: task_PRE_xxx or task_VM2_xxx`

## テンプレート
### YYYY-MM-DD: タイトル
- lane: pre|impl
- affects: task_PRE_xxx or task_VM2_xxx
- 変更内容:
- 理由:
- 影響範囲:
- フォローアップ:

### 2026-02-28: ID衝突解消と命名規約変更
- lane: pre|impl
- affects: task_PRE_003, task_PRE_004, task_VM2_001
- 変更内容:
  - 旧 `task_VM2_001.md` / `task_VM2_002.md` を PRE レーンへ移動し、`task_PRE_003.md` / `task_PRE_004.md` に改名した。
  - `tasks/impl/` には新しい `task_VM2_001.md`（`VirtualMode=true` 起動成立）を再作成した。
  - pre/impl の命名規約を明文化し、移行期間は PRE レーンで VM2 番号を新規発行しない暫定ルールを追加した。
- 理由:
  - PRE と IMPL で同一番号帯が混在すると、依存関係とレビュー対象の識別が困難になるため。
- 影響範囲:
  - `overview.md` のレーン定義、着手条件、依存ルール。
  - `tasks/pre/` および `tasks/impl/` のタスクファイル命名と参照。
- フォローアップ:
  - 文書内リンクで旧 `tasks/impl/task_VM2_001.md` / `task_VM2_002.md` 参照が残っていないかを継続点検する。

### 2026-02-28: VM2-002実装タスク欠落の是正
- lane: impl
- affects: task_VM2_002
- 変更内容:
  - `tasks/impl/task_VM2_002.md` を復元し、`CellValueNeeded` read配線タスクを IMPL レーンに再配置した。
  - 移行中ルールへ「`task_VM2_002.md` は IMPL 実装タスクとして維持する」旨を追記した。
- 理由:
  - IMPLレーン定義に VM2-002 が存在する一方でファイルが欠落しており、計画と実体の不整合が発生していたため。
- 影響範囲:
  - `overview.md` の移行中ルール。
  - `tasks/impl/task_VM2_002.md` のタスク実体。
- フォローアップ:
  - レーン定義（overview）と実ファイル（tasks配下）の差分チェックを更新時に必須化する。

