# refactor_form1cs_001 運用規約（Phase3）

## 目的
このディレクトリは `Form1.cs` リファクタリング計画の公式ソースとし、Phase3（描画分離＋入力分離）の作業計画・タスク・チェックリスト・意思決定履歴を一元管理する。

## 運用原則
- **1タスク = 1PR** を厳守する。
- 1タスクの上限は「変更ファイル数 3〜5」「主要Gate 1つ」「回帰観点 1チェックリストで完結」を満たす。
- 実装前に対象タスクの完了条件・回帰観点を確認する。
- PR では対象タスクIDを明記し、スコープ外変更を含めない。

## ドキュメント構成（Phase3 正本）
- `phase3/overview.md`: Phase3 の目的・依存関係・優先順位。
- `phase3/tasks/README.md`: タスク分割方針と運用概要。
- `phase3/tasks/impl/`: IMPLレーンの最小実行単位タスク。
- `phase3/checklists/smoke_phase3.md`: Phase3 回帰確認観点。
- `phase3/reports/impl_done/`: IMPL完了報告。
- `phase3/decision_log.md`: Phase3 の方針変更記録。

## 履歴資産（参照のみ）
- `phase2/` は履歴資産として保持する。
- Phase2 への追記は、証跡整合に必要な修正（誤記修正・判定整合）に限定する。

## `plans/` 側の取り扱い
`plans/` 以下には重複記述を置かず、`agent_plans/` 側への参照リンクのみを配置する。
