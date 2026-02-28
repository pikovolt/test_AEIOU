# refactor_form1cs_001 運用規約

## 目的
このディレクトリは `Form1.cs` リファクタリング計画の公式ソースとし、作業計画・タスク・チェックリスト・意思決定履歴を一元管理する。

## 運用原則
- **1タスク = 1PR** を厳守する。
- タスクはレーン別に以下の形式で最小実行単位に分割する。
  - PRE: `phase2/tasks/pre_v2/unit_PRE_*.md`
  - IMPL: `phase2/tasks/impl/task_VM2_*.md`
- 実装前に対象タスクの完了条件・回帰観点を確認する。
- PR では対象タスクIDを明記し、スコープ外変更を含めない。

## ドキュメント構成
- `phase2/overview.md`: Phase 2 の目的・依存関係・優先順位。
- `phase2/tasks/pre_v2/README.md`: PRE v2 の分割方針と運用概要。
- `phase2/tasks/pre_v2/PRE_SSOT.md`: PRE の不変要件・必須要件の正本。
- `phase2/tasks/pre_v2/PRE_decision_log.md`: PRE定義変更の記録。
- `phase2/tasks/pre_v2/PRE_worklog.md`: PRE実行記録・判定記録。
- `phase2/tasks/pre_v2/unit_PRE_*.md`: PREレーンの最小実行単位タスク。
- `phase2/tasks/impl/`: IMPLレーンの最小実行単位タスク。
- `phase2/definitions/DONE_TEMPLATE_IMPL.md`: IMPL完了報告テンプレート。
- `phase2/checklists/smoke_virtualmode.md`: 回帰確認観点。
- `phase2/decision_log.md`: IMPL側の方針変更記録。

## `plans/` 側の取り扱い
`plans/` 以下には重複記述を置かず、`agent_plans/` 側への参照リンクのみを配置する。
