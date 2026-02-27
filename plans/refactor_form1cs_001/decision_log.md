# decision_log

## 2026-02-27

- 初期配置として `architecture.md` をリポジトリ直下から `plans/refactor_form1cs_001/` に移動。
- 設計（architecture）と実行計画（plan）を分離する運用に統一。

## 2026-02-27 (update)

- ルート `architecture.md` を SSOT として再定義し、全体設計原則の一次情報を復活。
- `plans/` は SSOT の下位にある実行計画レイヤとして位置付け、優先順位を明文化。

## 2026-02-27 A.先行分離（初回）

- 決定: まず `Form1` から **状態を持たない/持ち方が限定的な処理** をサービスへ抽出する。
  - `GridSelectionService`（選択矩形計算と範囲再構成）
  - `GridScrollService`（画面送り/ページ移動）
  - `GridCellStyleResolver`（背景色判定）
- 理由: `KeyDown` 本体やモデル更新を先に動かすと差分が大きく回帰リスクが高いため。
- 影響: `Form1` 側はイベント中継に寄り始めるが、セル値更新ロジックは現時点で未変更。

## 2026-02-27 plan改稿番号ルール再定義

- 決定: `plan_YYYYMMDD_NNN.md` の改稿番号ルールを採用する。
  - 通常更新は現行 plan を更新
  - ドラスティックな変更時のみ `NNN` を繰り上げる
  - 複数存在時は最大番号を最新版（正）とする
- メリット:
  - 大幅改稿の履歴境界が残る
  - 過去計画との差分比較がしやすい
- デメリット:
  - 古い plan の誤参照リスク
  - 更新先分散リスク（運用徹底が必要）
- 対策: 改稿時は必ず decision_log に理由・影響範囲を記録し、PR本文で最新版 plan ファイル名を明記する。

## 2026-02-27 移譲ロジック再確認（選択クリア責務）

- 決定: `ClearSelection()` は `GridSelectionService` ではなく `Form1.calcRect_with_enter` 呼び出し側に置く。
- 理由: 元実装では len算出前に常時 `ClearSelection()` しており、移譲後も同じタイミングを維持するため。
- 影響: `len <= 0` 時を含め、従来通り選択解除が先に走る挙動へ復帰。

## 2026-02-27 フェーズ遷移整理（001正本で継続）

- 決定: A.先行分離（A-1〜A-5）完了後も、運用ルールに従い `plan_20260227_001.md` を正本として通常更新を継続する。
- 変更点:
  - `plan_20260227_001.md` の A 親項目を完了状態に更新。
  - 進捗反映は 001 に集約し、改稿番号の繰上げはドラスティック変更時のみとする。
- 理由: A完了はフェーズ遷移上の進捗更新であり、スコープ変更・フェーズ再編・完了条件再定義には該当しないため。
- 影響: 次フェーズ（モデル分離 Phase 1）への移行記録を維持しつつ、計画正本の分散を防ぐ。

## 2026-02-27 Phase 1実装計画の詳細化

- 決定: `plan_20260227_001.md` の「3. モデル分離（Phase 1）」に詳細タスクを追加し、対象範囲・最小API・置換順序・完了条件を明文化。
- 追加方針:
  - 入力ホットパス（KeyDown系）を Phase 1 の必須対象に固定。
  - `TimingSheetModel` の責務を「セル値/列名/行列サイズ」に限定し、UI制御責務を除外。
  - VirtualMode OFF のままモデル正の同期戦略を採用し、Phase 2 接続点（CellValueNeeded/Pushed）を先に固定。
- 理由: 実装順序を明確化しないまま置換を進めると、差分拡大と回帰判定の困難化を招くため。
- 影響: Phase 1 完了判定が具体化され、次フェーズ移行可否（VirtualMode 着手条件）を判断しやすくなる。

## 2026-02-27 Phase 1計画の整合性再点検（レビュー反映）

- 決定: Phase 1 計画に以下を追加し、`worklog_20260227_001.md` の B-3（Undo/Redo整合）と一致させる。
  - `SetValueOperation` 互換を維持したまま、セル値更新先をモデルへ寄せるための Undo/Redo 連携 API を最小APIへ追加。
  - `GetCell` を O(1) 応答前提で定義し、Phase 2 `CellValueNeeded` ホットパスの布石を明文化。
  - `copyToCell` / `deleteRect` を一括操作置換の優先対象へ明示。
  - Model の責務上限を「データ入れ物 + 行列管理 + Undo/Redo連携」に固定し、業務ロジック移管を禁止。
- 理由: 先行レビューで指摘された「Undo/Redo影響」「Model肥大化回避」「VirtualMode性能前提」の3点を、実装着手前に計画へ埋め込むため。
- 影響: Phase 1 の実装順序と Done 判定がより具体化され、Phase 2 への移行時の追加設計コストを抑制できる。

## 2026-02-27 Phase 1実装着手（モデル正の同期導入）

- 決定: `TimingSheetModel` を新規導入し、`GridViewManager` と `SetValueOperation` 系をモデル同期対応に変更する。
- 実装方針:
  - `Form1` では `GetCellValue`/`SetCellValue` を導入し、入力ホットパスのセル読取をモデル経由へ置換。
  - `GridViewOperation` では Undo/Redo を含む値更新時に `TimingSheetModel` を同時更新し、表示反映は従来どおり `DataGridView` へ行う。
  - VirtualMode は OFF のまま維持し、Phase 2 で `CellValueNeeded/Pushed` に接続可能な更新口を先に固定する。
- 理由: Phase 1 の必須要件（入力ホットパスの直参照削減・Undo/Redo互換維持）を小差分で満たすため。
- 影響: `Form1` でのセル参照がモデル経由に寄り、今後の一括操作置換（B区分）を段階的に適用しやすくなる。

## 2026-02-27 Phase 1移行点検（追補）

- 決定: 初回PRの移行差分を再点検し、モデル同期の抜けを小修正する。
- 修正内容:
  - `SetValueOperation.Redo` が `Execute` 再呼び出しで `oldValue` を再取得してしまう経路を是正し、`ApplyRedoCell` 経由で newValue を再適用する形へ統一。
  - `dataGridInitialize` の初期化ループを引数 `columnCount` / `rowCount` 基準へ統一し、モデル・ビューのサイズ整合を明示。
  - ヘッダ復元時の反映を `SetHeaderValue` 経由に寄せ、更新口を一本化。
- 理由: VirtualMode 前段の Phase 1 では「モデル正」の更新口を固定し、再実行経路での値取り違えリスクを最小化するため。


## 2026-02-27 Phase 1継続（ファイルI/O書込経路のモデル経由化）

- 決定: `pasteFromAEToolStripMenuItem_Click` のセル更新を `DataGridViewCell.Value` 直書きから `SetCellValue` 呼び出しへ切り替える。
- 理由: Phase 1 方針（モデル正）に沿って、C区分（ファイルI/O）の接続点を先にモデルAPIへ寄せるため。
- 影響:
  - 貼り付け処理でも `TimingSheetModel` と `DataGridView` の同期経路が統一される。
  - `aryCellUsedCount` 更新判定は `GetCellValue` で実施し、既存の空判定仕様を維持する。


## 2026-02-27 Phase 1補強（AE貼り付けの範囲外入力ガード）

- 決定: AE貼り付け時の空判定を `TryGetCellValue` ベースに変更し、範囲外フレームはスキップする。
- 理由: `TimingSheetModel` の例外方針（範囲外は `Try*` で吸収）に合わせ、ファイルI/O経路の移行漏れを防ぐため。
- 影響:
  - 範囲外キーを含む貼り付けデータでも処理継続できる。
  - 範囲内セルの更新経路は引き続き `SetCellValue` に統一される。


## 2026-02-27 追加点検（`checkCellValue` の範囲判定是正）

- 決定: `checkCellValue` の範囲判定を `X < ColumnCount` / `Y < RowCount` の正方向判定へ修正し、判定基準をモデルサイズに揃える。
- 理由: 追加点検で、既存条件が比較方向誤り（`ColumnCount < X`）になっており、実質的に有効範囲判定として機能しない移行リスクを確認したため。
- 影響:
  - `checkCellValue` の戻り値が期待どおり「有効セルかつ非空セル」で評価される。
  - Phase 1 方針（モデル正）に合わせ、範囲判定が `TimingSheetModel` 基準に統一される。
