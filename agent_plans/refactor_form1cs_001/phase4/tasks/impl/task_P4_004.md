# task_P4_004

## 目的
`GridMoveSelectionCommand` を導入し、移動/範囲選択の実行責務を `Form1` からコマンド層へ段階移管する。

## 対象
- `AEIOU/WindowsFormsApplication1/GridMoveSelectionCommand.cs`（新規）
- `AEIOU/WindowsFormsApplication1/Form1.cs`
- `AEIOU/WindowsFormsApplication1/GridShortcutRouter.cs`
- `AEIOU/WindowsFormsApplication1/AEIOU.csproj`
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_004.md`

## 非対象
- 数字/空セルなど値入力処理のコマンド分離（`P4-005`）
- 修飾キー判定の単一路線化（`P4-006`）
- マウス経路（`CellMouseDown/Move/Up`）の責務再編

## 依存タスク
- `P4-003`（完了済）

## 完了条件（Gate）
- 矢印/Enter/Home/PageUp/PageDown 等の移動系入力が `GridMoveSelectionCommand` 経由で実行され、`Form1` の直接分岐が縮小されている。

## 回帰観点
- `../../checklists/smoke_phase4.md` の「移動/選択」「入力経路」。

## 見通し（実装前の確認ポイント）
- `GridShortcutRouter` の移動系ハンドラ（`OnLeftArrow` など）を `GridMoveSelectionCommand` へ委譲し、値入力系（`OnNumberKey` 等）は現行維持で境界を明確化する。
- `Ctrl` / `Shift` 付き移動（連続選択・範囲拡張）に退行リスクがあるため、`Form1` 側の既存フラグ連携（`selectRange` / `isFirstEdit` 周辺）との接続点を先に固定する。
- 1タスク1PR・変更ファイル3〜5の制約を守るため、P4-004ではキーボード移動系に限定し、値入力分離は `P4-005` に送る。

## 実装着手の入口条件
- `P4-003` の Done 証跡（`phase4/reports/impl_done/P4_003.md`）を前提として、`../../checklists/preflight_P4_004.md` を [x] にしてから着手する。
- 着手前ベースラインは `../../checklists/smoke_phase4.md` の「入力経路」「移動/選択」を基準に固定する。

## 未解決課題（後段TODO化）
- `Shift+Delete`（行削除）で 1入力あたり約1秒の遅延が観測される。
- 本タスクでは挙動整合を優先し、性能対策は後段へ分離する。
- 後段候補（`P4-006` 以降で実施想定）
	- バッチ書き込み中の `CellValueChanged -> RecalculateColumn` の抑止と再計算集約
	- `InvalidateCell` のセル単位多発を抑止し、再描画を集約
	- 前後比較計測（単発/連続）を取り、改善結果を `reports/` に記録
