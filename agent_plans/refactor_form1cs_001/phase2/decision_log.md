# Decision Log (方針変更のみ)

## 記録ルール
- 実装詳細ではなく、計画・方針の変更のみ記録する。
- 各項目に日付、変更内容、理由、影響範囲を含める。
- 各判断エントリには、必須欄として以下を明記する。
  - `lane: impl`
  - `affects: task_VM2_xxx`

## テンプレート
### YYYY-MM-DD: タイトル
- lane: impl
- affects: task_VM2_xxx
- 変更内容:
- 理由:
- 影響範囲:
- フォローアップ:

### 2026-02-28: VM2-002実装タスク欠落の是正
- lane: impl
- affects: task_VM2_002
- 変更内容:
  - `tasks/impl/task_VM2_002.md` を復元し、`CellValueNeeded` read配線タスクを IMPL レーンに再配置した。
- 理由:
  - IMPLレーン定義に VM2-002 が存在する一方でファイルが欠落しており、計画と実体の不整合が発生していたため。
- 影響範囲:
  - `overview.md` の実装レーン定義。
  - `tasks/impl/task_VM2_002.md` のタスク実体。
- フォローアップ:
  - レーン定義（overview）と実ファイル（tasks配下）の差分チェックを更新時に必須化する。

### 2026-02-28: PRE旧定義の削除とIMPL定義の単純化
- lane: impl
- affects: task_VM2_001
- 変更内容:
  - PRE旧定義への依存記述を `task_VM2_001.md` から削除した。
  - overviewからPRE関連の依存・運用記述を削除し、IMPL中心の定義へ単純化した。
- 理由:
  - PRE定義の関連性は別途再検討とし、現時点での混在参照を排除するため。
- 影響範囲:
  - `tasks/impl/task_VM2_001.md`
  - `overview.md`
- フォローアップ:
  - PRE定義の新規連携方針が確定した時点で、IMPL着手条件を再定義する。
