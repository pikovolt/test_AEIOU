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


## 2026-02-27 Phase 1 C区分監査（統合）

- 決定: AE貼り付け～入力有無判定までをモデル基準に統一する。
  - `pasteFromAEToolStripMenuItem_Click` の書込を `SetCellValue` 経由に変更。
  - AE貼り付け時は `TryGetCellValue` で範囲外フレームをスキップ。
  - `checkCellValue` の範囲判定を `X < timingSheetModel.ColumnCount` / `Y < timingSheetModel.RowCount` に是正。
- 理由: Phase 1 方針（モデル正・範囲外は `Try*` 吸収）に対する移行漏れと判定不整合を同時に解消するため。
- 影響:
  - ファイルI/O経路でもモデルとビューの更新口が統一される。
  - 範囲外データ混入時の処理継続性が向上する。
  - `checkCellValue` が意図通りの「有効範囲かつ非空」判定として機能する。

## 2026-02-27 Phase 1 B区分着手（更新口の集約）

- 決定: `GridViewOperation` から `Model` / `View` 直接更新を減らし、`GridViewManager` のセル更新API経由へ統一する。
- 実装内容:
  - `GridViewManager` に `GetCellValue` / `SetCellValue` / `SetCellValueWithUndo` / `ApplyUndoCellValue` / `ApplyRedoCellValue` を追加。
  - `SetValueOperation`, `PasteOperation`, `CutOperation`, `DeleteOperation`, 共通ヘルパ (`CopyToBuffer` / `PasteFromBuffer` / `ClearSelection`) のセル更新を新APIへ置換。
- 理由: Phase 1 の同期戦略（モデル正 + 反映口集約）を B区分（copy/delete 系一括操作）に拡張し、Phase 2 で `CellValueNeeded/Pushed` 接続点をさらに絞るため。
- 影響:
  - Undo/Redo を含むセル更新経路が `GridViewManager` に寄るため、同期漏れの検出/修正が容易になる。
  - 既存の画面反映タイミングは維持され、挙動差分を最小化できる。

## 2026-02-27 Phase 1 B区分修正（null正規化の統一）

- 決定: `GridViewManager` の集約APIで、セル値更新時の `null` を空文字へ正規化してから Model/View に反映する。
- 理由: モデル正（空文字を正規値）とビュー表示値の不整合を防ぎ、更新口集約後の一貫性を確保するため。
- 影響: `SetCellValue*` / `Apply*CellValue` 経路で `null` 混入時も Model/View が同一値（`""`）で同期される。

## 2026-02-27 Phase 1 B区分追補（Paste Undo/Redo の決定性）

- 決定: `PasteOperation` の Undo/Redo を `manager.CopyBuffer` の現在値に依存させず、実行時スナップショット（貼り付け前値/貼り付け値）で再適用する。
- 理由: 貼り付け後にコピーバッファが変更・クリアされても、Undo/Redo の結果を決定的に保つため。
- 影響:
  - Undo は `_oldValues`、Redo は `_newValues` を使って復元される。
  - `PasteOperation` の履歴再生がバッファ状態変化から独立し、操作単位の再現性が向上する。

## 2026-02-27 Phase 1 Step 5着手（ヘッダ更新口の集約）

- 決定: 列ヘッダの参照/更新を `GridViewManager` 経由に集約し、`Form1` の `GetHeaderValue` / `SetHeaderValue` は manager API優先で使用する。
- 実装内容:
  - `GridViewManager` に `GetHeaderValue` / `SetHeaderValue` を追加。
  - `dataGridInitialize` の初期ヘッダ設定を `SetHeaderValue` 経由へ切替。
  - `SetHeaderValue` 内で `null` を空文字へ正規化し、Model/View 同期の一貫性を維持。
- 理由: Phase 1 の更新口統一方針をヘッダ操作にも適用し、Phase 2 での VirtualMode 接続時に同期境界を明確化するため。
- 影響:
  - 列名編集・列挿入削除後のヘッダ復元が同一APIで扱える。
  - ヘッダ値に `null` が混入した場合でもモデルと表示が同一値で維持される。

## 2026-02-27 Phase 1 Step 5移行ミス修正（モデル参照ずれ防止）

- 決定: `Form1` のヘッダ read/write で `GridViewManager.Model != null` だけを条件に manager API を使う判定を廃止し、`gridViewManager.Model == timingSheetModel` の一致判定に変更する。
- 理由: `InitializeWork(InitializeTarget.Timing)` で `timingSheetModel` を再生成した直後は manager が旧モデル参照を保持しうるため、非一致状態で manager API を使うとヘッダ更新先が旧モデルへずれるリスクがある。
- 影響:
  - モデル再生成直後でも、ヘッダ read/write は常に現行 `timingSheetModel` を参照できる。
  - manager と現行モデルが一致した後は従来どおり manager API 経由で更新口を統一できる。

## 2026-02-27 Phase 1継続（SetCellValue の adapter 経由化）

- 決定: `Form1.SetCellValue` は `GridViewManager` が現行 `TimingSheetModel` にバインド済みの場合、`gridViewManager.SetCellValue` を優先して呼ぶ。
- 理由: Phase 1 の同期戦略（モデル正 + 反映口の集約）に合わせ、`DataGridView` 直接反映を `Form1` 内で分散させないため。
- 影響: 単一セル更新・列シフト・AE貼り付け等、`SetCellValue` を経由する書込が adapter 経由に統一される。

## 2026-02-27 Phase 1移行ミス修正（Timing初期化時のmanager再バインド）

- 決定: `InitializeWork(InitializeTarget.Timing)` で `timingSheetModel` 再生成直後に、`gridViewManager.InitializeWork(dataGridView1, timingSheetModel)` を実行して manager の参照先を即時更新する。
- 理由: 直前PRでは `Form1.SetCellValue` が manager 経由を優先するため、Timing再初期化直後に manager が旧モデルを保持しているとフォールバック経路に依存しやすく、移行意図（adapter 経路の一貫化）とずれるため。
- 影響: リサイズ/再初期化中も manager と現行モデルの整合が保たれ、`SetCellValue`・`SetHeaderValue` の更新口が安定する。

## 2026-02-27 Phase 1再点検（Timing初期化時のUndo履歴保持）

- 決定: `InitializeWork(InitializeTarget.Timing)` では `gridViewManager.InitializeWork(...)` を呼ばず、`gridViewManager.Model = timingSheetModel` の再バインドのみ行う。
- 理由: `InitializeWork(...)` は Undo/Redo 履歴・コピー状態を再初期化する副作用があり、Timing再初期化だけを意図した経路で履歴が失われる移行ミスになるため。
- 影響: モデル参照ずれは防ぎつつ、`InitializeTarget.Timing` 単体呼び出し時の Undo 履歴ポリシーを従来どおり維持できる。


## 2026-02-27 PR再監査（追加フォローアップの明文化）

- 決定: 移行ミス修正後の残課題として、read経路境界・Timing初期化時copy buffer方針・検証証跡運用をフォローアップ項目として plan/worklog に明示する。
- 理由: Phase 1 の実装差分は小さいが、Phase 2 接続時に曖昧さが残ると再発しやすいため、早期に監査観点を固定する。
- 影響: 次PRで「何を追加確認すべきか」を事前共有でき、差分レビューの観点がぶれにくくなる。



## 2026-02-27 Phase 1 Step 4着手（複製操作のUndoグループ化）

- 決定: `duplicateToolStripMenuItem_Click` で `SetValueOperation` を複数発行する前に `gridViewManager.BeginGroup("複製")` を開始し、末尾の `EndGroup()` と対になるよう修正する。
- 理由: 一括操作（B区分）のUndo単位を操作論理に合わせるため。グループ開始なしで `EndGroup()` のみ呼ぶ実装では履歴がまとまらず、操作再現性が低下する。
- 影響:
  - 複製操作が単一のUndo単位として記録され、既存の一括操作ポリシー（Begin/Endで囲む）と整合する。
  - モデル更新経路は既存の `SetValueOperation`（= manager 経由更新）を維持し、Phase 1 の更新口統一方針に一致する。

## 2026-02-27 PR再精査（複製Undoグループの例外安全性）

- 決定: `duplicateToolStripMenuItem_Click` の `BeginGroup("複製")` 導入箇所を `try/finally` で囲み、例外発生時でも `EndGroup()` が必ず実行されるように修正する。
- 理由: 移行前はグループ未使用だったため、途中例外でも Undo グループスタック破損は起きなかった。移行後は `BeginGroup` 追加により、`EndGroup` 未到達時にグループが積み残るリスクが新規発生するため。
- 影響:
  - 複製中に範囲外アクセス等の例外が起きても `GridViewManager` のグループスタック整合性を維持できる。
  - 既存の複製ロジック（`SetValueOperation` 経由更新）は維持され、Phase 1 方針との整合は保たれる。

## 2026-02-27 PR再精査フォローアップ（複製先の範囲外書込防止）

- 決定: `duplicateToolStripMenuItem_Click` に、複製先（`Row + Len` 以降）が `setting.RowLength` を超える場合の上限計算を追加し、範囲内に収まる件数だけ複製する。
- 理由: 直前PRは Undo グループ整合性を改善したが、選択位置によっては複製先インデックスが行数上限を超え、`SetValueOperation` 実行中に例外が起きる余地が残っていたため。
- 影響:
  - 複製操作の行末近傍での実行時に、範囲外アクセス例外を回避できる。
  - 実際に複製できた件数（`copyLength`）に合わせて選択範囲の高さを更新し、表示上の選択状態と実データを一致させる。

## 2026-02-27 Phase 1読取経路の統一（GridViewManager優先）

- 決定: `Form1.GetCellValue` / `Form1.TryGetCellValue` は、`GridViewManager` が現行 `TimingSheetModel` にバインド済みの場合に manager 経由を優先する。
- 理由: Phase 1 の同期戦略（adapter 経由で反映口を限定）に合わせ、読取側も同一境界へ寄せるため。
- 影響: `Form1` のセル値アクセスは read/write ともに manager 優先の対称構造となり、Phase 2（VirtualMode 接続）での参照口一本化を進めやすくなる。

## 2026-02-27 Phase 1 Step4継続（B区分の参照置換 + 列インデックス是正）

- 決定: 一括操作のうち `replace` / `fourArithmeticOperation` のセル値参照を `TryGetCellValue` 優先へ段階移行する。
- 理由: 同一セルの `GetCellValue` 多重呼び出しを減らし、モデル参照口を Phase 2 前に統一するため。
- 追加修正:
  - `deleteRect_with_backspace` の `SetValueOperation` 列指定を `rect.X + i` から `i` へ修正。
- 影響:
  - B区分の read 経路がより一貫化され、モデル経由置換の適用範囲が拡大。
  - バックスペース削除時に列ずれ書込が起きる不具合を回避。

## 2026-02-27 PR再精査（四則演算エラー時Undo境界の補強）

- 決定: `fourArithmeticOperationToolStripMenuItem_Click` で変換エラーが発生した際、同操作内で更新が1件も無い場合は `Undo()` を実行しない。
- 理由: `BeginGroup` 後に操作が push されていない状態で `Undo()` を呼ぶと、直前の別操作を取り消すリスクがあるため。
- 影響:
  - 四則演算の途中失敗時に「今回分だけを巻き戻す」境界が明確化される。
  - Phase 1 移行中の Undo/Redo 整合性を維持しやすくなる。

## 2026-02-27 PRコメント対応（backspace削除の列インデックス説明をコード化）

- 決定: `deleteRect_with_backspace` のループ変数を `col` に改め、絶対列インデックスを直接走査していることをコメントで明示する。
- 理由: `GetCellValue(i, rect.Y)` と `SetValueOperation(..., i, ...)` の組み合わせが、相対インデックスと誤解されやすかったため。
- 影響:
  - `rect.X + i` を使わない設計意図（`i`/`col` がすでに絶対列）がコード上で追跡しやすくなる。
  - 仕様変更はなく、可読性とレビュー容易性のみを改善。

## 2026-02-27 PRコメント対応（複数列選択時の挙動説明を補強）

- 決定: `deleteRect_with_backspace` のコメントに、複数列選択時でも `rect.Left`～`rect.Right` を絶対列として順次処理するため列ずれが起きないことを追記する。
- 理由: 列インデックス修正の妥当性を、レビュー時にコードだけで検証しやすくするため。
- 影響: 動作変更はなく、意図の可視化のみ。

## 2026-02-27 Phase 1 Step4継続（B区分の再描画トリガ集約）

- 決定: `copyToCell` / `cutToBuf` / `deleteRect` に `shouldInvalidate` 引数を追加し、複合操作（行挿入/削除・全体挿入/削除・ドラッグ移動）では末尾で1回だけ `Invalidate()` する。
- 理由: Step4 対象の一括操作は内部で delete/cut/paste を連鎖実行しており、途中の都度再描画が多重に発生すると応答性低下と描画チラつきの要因になるため。
- 影響:
  - 単体操作（通常のコピー貼付け・削除）は従来どおり即時再描画を維持。
  - 複合操作では Undo/Redo 操作単位を変えず、描画更新のみ末尾集約となる。

## 2026-02-27 PR再精査（移行ミス: optional parameter 互換性）

- 決定: `copyToCell` / `cutToBuf` / `deleteRect` の `shouldInvalidate = true`（optional parameter）を廃止し、`(引数なし)` + `(bool 引数あり)` のオーバーロード構成へ変更する。
- 理由: 本プロジェクトは `TargetFrameworkVersion v3.5` かつ旧ツールチェーン互換を維持しており、optional parameter 導入はビルド環境によってはコンパイル不能となる移行ミスになり得るため。
- 影響:
  - 呼び出し側の既定挙動は維持（引数なし呼び出しは従来どおり即時 `Invalidate()`）。
  - 一括操作側の `false` 指定による再描画集約も維持される。

## 2026-02-27 PR再精査フォローアップ（再描画二重発火の残件解消）

- 決定: 前PRの再描画集約後も残っていた二重 `Invalidate()` 呼び出し箇所を追加で整理する。
- 対象:
  - ドラッグ移動の Ctrl+コピー経路（`copyToCell`）
  - メニュー操作の Cut/Paste 経路
  - Deleteキー経路（`deleteRect` 後の明示 `Invalidate()`）
  - 繰り返し入力の範囲消去（`deleteRect` をグループ内で実行）
- 理由: 集約の目的は「複合操作あたり再描画1回」にあるため、経路ごとの取りこぼしを残すと効果が不均一になる。
- 影響:
  - 値更新・Undo/Redo・選択更新ロジックは変更なし。
  - 再描画タイミングのみを末尾集約に寄せ、重複発火を削減する。

## 2026-02-27 Phase 1 Step4継続（連番/反転の読取経路統一）

- 決定: `sequentialNumberToolStripMenuItem_Click` / `reverseToolStripMenuItem_Click` の空セル判定を `TryGetCellValue` ベースに寄せ、`GetCellValue(...) == ""` 直比較を削減する。
- 理由: B区分（一括操作）の read 経路を model adapter 経由に統一し、範囲外や未初期化の分岐を同一ルールで扱うため。
- 影響:
  - 連番作成の使用数カウント判定は `IsCellEmpty` ヘルパー経由で統一される。
  - 反転処理の値収集/適用時に `TryGetCellValue` を利用し、読取境界が明確化される。

## 2026-02-27 PR再精査（reverse の読取/適用判定の同一化）

- 決定: `reverseToolStripMenuItem_Click` の2ndパス（適用側）でも `TryGetCellValue` を直接使い、1stパス（収集側）と同一条件で空判定する。
- 理由: 将来の判定条件変更時に `IsCellEmpty` と分岐が乖離すると、収集件数と適用件数の不整合が起きるリスクがあるため。
- 影響:
  - 収集/適用の判定基準が同一化され、`temp[--targetCount]` の境界を保守しやすくなる。
  - 防御的に `targetCount <= 0` ガードを追加し、想定外の差異が出ても範囲外アクセスを回避する。

## 2026-02-27 PR再精査フォローアップ（checkContinuty の読取一本化）

- 決定: 描画ホットパスの `checkContinuty` で同一セルへの二重 `GetCellValue` 呼び出しをやめ、`TryGetCellValue` 1回 + ローカル変数判定へ置換する。
- 理由: 読取経路を model adapter 側へ揃えつつ、CellPainting 経路での重複読取を減らし判定条件の一貫性を高めるため。
- 影響:
  - 空セル/範囲外は `TryGetCellValue == false or ""` として同一扱い。
  - KaraCell 判定と通常入力判定は従来仕様を維持。

## 2026-02-27 Phase 1継続（CellPainting読取のモデル経由化）

- 決定: `GridCellRenderer` のセル値読取を `DataGridViewCell.Value` 直参照から、`Form1.GetCellValue` 経由（= `GridViewManager` / `TimingSheetModel` 参照口）へ切り替える。
- 理由: Phase 1 の「読取経路をモデルAPIへ寄せる」方針を描画判定にも適用し、`CellPainting` での参照口を将来の VirtualMode 接続点に揃えるため。
- 影響:
  - `IsKaraCell` / `IsContinuousLine` 判定が、入力ホットパスと同じ参照境界で評価される。
  - `GridCellRenderer` は `Func<int,int,string>` を受け取る構成となり、値取得の依存が明示化される。

## 2026-02-27 PR再精査（移行ミス修正: throw expression 互換性）

- 決定: `GridCellRenderer` コンストラクタの null チェックを `?? throw` から従来構文（`if (...) throw`）へ修正する。
- 理由: 本リポジトリは .NET 3.5 / 旧 C# コンパイラ互換を前提にしており、throw expression（C# 7 以降）はビルド不能となる移行ミスのため。
- 影響:
  - 実行時挙動（null 時に `ArgumentNullException` を送出）は維持。
  - 言語バージョン依存を除去し、既存ツールチェーン互換を回復。

## 2026-02-27 PRコメント対応（worklog採番の整合回復）

- 決定: `worklog_20260227_001.md` の 18番セクション配下見出しを `22-1` / `22-2` から `18-1` / `18-2` に修正する。
- 理由: セクション本文は18番の内容であり、`22` は編集時の採番ずれで説明コストを増やすため。
- 影響:
  - 記録内容自体は不変。
  - 履歴参照時の章番号整合性を回復し、レビュー指摘の再発を抑止。

## 2026-02-27 Phase 1 Step4継続（B区分の書込ループ境界をモデルサイズ基準へ）

- 決定: 行挿入/削除系（`insertCell`, `deleteCell`, `insertToAllCell`, `cutToAllCell`）のループ境界と削除矩形計算を、`setting.ColLength` / `setting.RowLength` 直参照から `TimingSheetModel` サイズ参照（`GetSheetColumnCount` / `GetSheetRowCount`）へ置換する。
- 理由: Phase 1 方針の「モデル正」を徹底し、設定値と実モデルサイズの乖離が起きる経路で範囲計算が不一致になるリスクを下げるため。
- 影響:
  - B区分（一括操作）の更新処理で、サイズ取得口がモデル基準に統一される。
  - VirtualMode 移行時にも、行列サイズの参照先を追加変更せずに流用しやすくなる。
