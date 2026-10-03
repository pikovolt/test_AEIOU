# refactor_form1cs_001 運用規約（Phase6 計画）

## 目的
このディレクトリは `Form1.cs` リファクタリング計画の公式ソースとする。Phase5 までの成果と未完了項目を履歴として保持し、現在は Phase6（シート編集計算の分離）の計画を正本とする。

## 運用原則
- **1タスク = 1PR** を厳守する。
- 1タスクの上限は「変更ファイル数 3〜5」「主要Gate 1つ」「回帰観点 1チェックリストで完結」を満たす。
- 実装前に対象タスクの完了条件・回帰観点を確認する。
- PR では対象タスクIDを明記し、スコープ外変更を含めない。

## ドキュメント構成（Phase6 正本）
- `phase6/overview.md`: Phase6 の目的、責務境界、段階導入方針、Phase5 からの持ち越し。
- `phase6/tasks/README.md`: 実装前 characterization を含むタスク分割と Gate。
- `phase6/checklists/smoke_phase6.md`: 行・列編集、Undo、関連範囲の回帰観点。

## Phase5 の扱い
- Phase5 は作業を一旦終了し、`phase5/` を実装成果および残件の参照元として保持する。
- Windows 実機で未実施のビルド、Automation tests、UI smoke、クリーン環境での拡張配置確認は完了扱いにしない。詳細は `phase5/p5_007_completion.md` と `phase5/checklists/smoke_phase5.md` を参照する。
- Phase6 の変更が Automation host/registry に触れる場合は Phase5 の契約と smoke を回帰条件に含めるが、Phase5 残件の解消を Phase6 着手の前提にはしない。

## Phase4 完了資産
- `phase4/overview.md`: Phase4 の目的・依存関係・優先順位。
- `phase4/tasks/README.md`: タスク分割方針と運用概要。
- `phase4/tasks/impl/`: IMPLレーンの最小実行単位タスク。
- `phase4/checklists/smoke_phase4.md`: Phase4 回帰確認観点。
- `phase4/reports/impl_done/`: IMPL完了報告。
- `phase4/decision_log.md`: Phase4 の方針変更記録。

## 履歴資産（参照のみ）
- `phase2/`, `phase3/`, `phase4/`, `phase5/` は履歴資産として保持する。
- Phase2〜Phase5 への追記は、証跡整合、残件の実施記録、誤記修正・判定整合に限定する。

## `plans/` 側の取り扱い
`plans/` 以下には重複記述を置かず、`agent_plans/` 側への参照リンクのみを配置する。

## 履歴注記（Phase3）
以下は Phase3 運用中の不具合対応メモとして履歴保持する。

### 2026-03-02 入力不具合対応メモ
- 対象事象: `Shift` 範囲選択が効かない / 上段数字入力が `・` になる。
- 影響範囲: Phase3 入力分離後の `KeyDown -> GridShortcutRouter -> Form1.Shortcut -> GridCellValueService` 経路。
- 原因要約:
	- `Shift+矢印` の分岐が矢印ハンドラ側で未実装となり、単純移動へフォールバックしていた。
	- 数字入力がテンキー前提の変換式 + 932文字変換に依存し、上段数字で文字化けした。
- 修正方針:
	- `Shift+↑/↓` を既存の縦方向範囲拡縮処理へ接続。
	- `Shift+→/←` を横方向の範囲拡縮（右拡大 / 右端縮小）として実装。
	- 数字入力は `0..9` に正規化して `'0' + digit` で文字生成（エンコーディング依存を排除）。
- 最低確認観点:
	- `Shift+↑/↓/←/→` の範囲拡縮。
	- 上段数字 `0..9` 入力（テンキーなし環境）。
	- 既存 `*` `/` の縦方向拡縮が非退行であること。

### TODO（2026-03-02 追加）
- キーボード移動時の背景更新は、当面「前回アクティブ列 + 現在アクティブ列」の `InvalidateColumn` で対応。
- 将来的に、選択範囲更新（上下移動時）の最小再描画範囲を評価し、必要なら `Invalidate` 粒度をさらに最適化する。
