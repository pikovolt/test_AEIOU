# task_PRE_002: グリッド更新ロジックの抽出設計

## 目的
`Form1.cs` 内のグリッド更新処理をサービス層へ分離する準備を行う。

## 実施内容
- 更新処理の入口/出口を整理する。
- 依存オブジェクトを明確化する。
- 抽出先クラス案（責務・公開メソッド）を定義する。

## 着手判定ユニット（独立運用）
- `U-PRE002-ARCH`: `抽出候補責務（3系統）` と `抽出先クラス案` のレビュー完了。
- `U-PRE002-RISK`: `互換性/回帰リスク` のレビュー完了。

運用ルール:
- `U-PRE002-ARCH` と `U-PRE002-RISK` は相互依存なしで完了可能。
- IMPL側は PRE-002 全体完了ではなく必要な着手判定ユニットの完了有無で着手可否を判定する。

## 抽出候補責務（3系統）
1. 入力解釈系
   - 画面イベント（セル編集、ボタン押下、選択変更）から、更新対象行・更新種別・検証ルールを解釈する。
2. グリッド更新系
   - `DataGridView` への表示更新（値反映、行追加/削除、再描画トリガ）を一元化する。
3. データ反映系
   - 画面上の変更をデータモデルへ反映し、必要に応じて Undo スタックへ変更履歴を記録する。

## 抽出先クラス案

### 1) `GridInputInterpreter`
- 責務境界
  - UIイベント情報をドメイン寄りの更新要求へ変換する。
  - 変換と入力妥当性判定までを担当し、描画更新は担当しない。
- 公開メソッド
  - `GridUpdateRequest InterpretCellEdit(CellEditContext context)`
  - `GridUpdateRequest InterpretSelectionChange(SelectionContext context)`
- 戻り値
  - `GridUpdateRequest`（対象行ID、対象列キー、更新値、更新種別、検証結果）
- 例外方針
  - 想定可能な入力不備は例外を投げず `ValidationResult` に格納。
  - 想定外（null依存、壊れた状態）は `InvalidOperationException` を送出。

### 2) `GridViewUpdater`
- 責務境界
  - `GridUpdateRequest` を受け取り、`DataGridView` 表示を更新する。
  - データ永続化や Undo 記録は担当しない。
- 公開メソッド
  - `GridUpdateResult ApplyUpdate(GridUpdateRequest request)`
  - `void RefreshRow(int rowIndex)`
- 戻り値
  - `GridUpdateResult`（更新成否、影響行、再描画要否、警告メッセージ）
- 例外方針
  - UI操作不能（行インデックス不正など）は `ArgumentOutOfRangeException`。
  - 一時的不整合は `GridUpdateResult.Warning` で返却して継続。

### 3) `GridDataSyncService`
- 責務境界
  - グリッド更新結果をデータモデルへ反映し、Undo 管理へ変更を記録する。
  - 画面イベント解釈や直接のセル描画は担当しない。
- 公開メソッド
  - `DataSyncResult Commit(GridUpdateRequest request)`
  - `void RollbackLastChange()`
- 戻り値
  - `DataSyncResult`（反映成否、モデル更新件数、Undo記録ID）
- 例外方針
  - ドメイン制約違反は `DomainValidationException`（業務エラーとして扱う）。
  - 永続化・整合性エラーは `DataSyncException` でラップして上位へ通知。

## 実装受け渡し最小情報（Phase2 IMPL向け）

### A. 3クラスの配置先と namespace（確定）
- `GridInputInterpreter`
  - 配置先: `AEIOU/WindowsFormsApplication1/Application/Grid/GridInputInterpreter.cs`
  - namespace: `AEIOU.Application.Grid`
- `GridViewUpdater`
  - 配置先: `AEIOU/WindowsFormsApplication1/Application/Grid/GridViewUpdater.cs`
  - namespace: `AEIOU.Application.Grid`
- `GridDataSyncService`
  - 配置先: `AEIOU/WindowsFormsApplication1/Application/Grid/GridDataSyncService.cs`
  - namespace: `AEIOU.Application.Grid`

補足:
- 既存実装（`Form1`, `GridViewManager` など）が `namespace AEIOU` を使用しているため、`Form1.cs` 側では `using AEIOU.Application.Grid;` を追加して段階導入する。

### B. `Form1` からの呼び出し入口（イベントハンドラ）マッピング
- `GridInputInterpreter`（入力イベント解釈）
  - `dataGridView1_KeyDown`
  - `dataGridView1_KeyPress`
  - `dataGridView1_CellDoubleClick`
  - `dataGridView1_CellMouseUp`
- `GridViewUpdater`（表示反映）
  - `pasteToolStripMenuItem_Click`
  - `insertCellToolStripMenuItem_Click`
  - `deleteCellToolStripMenuItem_Click`
  - `dataGridView1_ColumnHeaderMouseClick`
- `GridDataSyncService`（モデル反映/Undo）
  - `setNakanukiToolStripMenuItem_Click`
  - `setKiribariToolStripMenuItem_Click`
  - `cancelNakanukiToolStripMenuItem_Click`
  - `cancelKiribariToolStripMenuItem_Click`
  - `sequentialNumberToolStripMenuItem_Click`
  - `repeatNumberToolStripMenuItem_Click`
  - `replaceToolStripMenuItem_Click`
  - `reverseToolStripMenuItem_Click`
  - `fourArithmeticOperationToolStripMenuItem_Click`
  - `duplicateToolStripMenuItem_Click`

### C. 既存 `TryGetCellValue` / `SetCellValue` / Undo処理との接続責務（確定）
- 契約の正本（優先仕様）
  - `TryGetCellValue` 契約は **`task_PRE_004.md` の「TryGetCellValue 戻り契約（インターフェース・正本）」を正** とする。
  - 本セクションは接続責務の整理を目的とし、型/シグネチャ/失敗契約の定義は PRE-004 正本へ従属する。
- `TryGetCellValue`（`Form1` の既存ヘルパ）
  - 直接利用者: `GridInputInterpreter`
  - 接続方針: `ICellValueResolver`（`bool TryGetCellValue(int rowIndex, int columnIndex, out object value, out CellValueFailureReason failureReason)`）を介して注入。初期実装は `Form1` アダプタで既存メソッドを委譲する。
  - 契約要点（PRE-004に統一）:
    - 引数順は `rowIndex` → `columnIndex` に固定する。
    - 成功値は表示文字列（`string`）に固定し、インターフェースは互換目的で `out object value` を採用する。
    - 失敗時は `false` を返し、`failureReason` を **必須返却** とする（成功時は `CellValueFailureReason.None`）。
    - 診断ログは `CellValueNeeded` 側で記録し、`TryGetCellValue` は理由分類の返却に責務を限定する。
- `SetCellValue`（`Form1` の既存ヘルパ）
  - 直接利用者: `GridViewUpdater`
  - 接続方針: `IGridCellWriter`（`void SetCellValue(int columnIndex, int rowIndex, string value)`）を介して注入。`GridViewUpdater` は値比較（`SetCellValueIfChanged` 相当）まで担当する。
  - 備考: `SetCellValue` は既存呼び出し互換の都合で `columnIndex` → `rowIndex` を維持し、`TryGetCellValue` 契約とは分離して扱う。
- Undo関連（`gridViewManager.Undo/Redo`, `flushUndoHistory`, `OperationGroup`）
  - 直接利用者: `GridDataSyncService`
  - 接続方針: `IUndoGateway` を新設し、`BeginGroup/EndGroup/Undo/Redo/Flush` を集約。`Form1` はUIイベントからUndoコマンドを受けた時のみゲートウェイを呼び、実際の記録境界の決定は `GridDataSyncService.Commit` 側に寄せる。

### D. 例外型の定義方針（未定義禁止のため確定）
- `DomainValidationException`
  - **新規作成する**。
  - 配置先: `AEIOU/WindowsFormsApplication1/Application/Grid/Exceptions/DomainValidationException.cs`
  - namespace: `AEIOU.Application.Grid.Exceptions`
  - 継承: `Exception`
  - 用途: 入力値・業務制約違反（ユーザー修正可能）を表現。
- `DataSyncException`
  - **新規作成する**。
  - 配置先: `AEIOU/WindowsFormsApplication1/Application/Grid/Exceptions/DataSyncException.cs`
  - namespace: `AEIOU.Application.Grid.Exceptions`
  - 継承: `Exception`
  - 用途: モデル反映失敗、Undo記録失敗、整合性破綻などのシステム系異常をラップ。

実装メモ:
- 既存の `InvalidOperationException` / `ArgumentOutOfRangeException` はUI層（`GridInputInterpreter`/`GridViewUpdater`）のガード用途で継続利用し、上記2例外は `GridDataSyncService` の業務境界でのみ送出する。

### E. 非SDKプロジェクト前提の `.csproj` 更新手順（必須）
- 対象プロジェクト: `AEIOU/WindowsFormsApplication1/AEIOU.csproj`
- 本プロジェクトは **SDK形式ではない** 前提で、`<ItemGroup>` に `Compile Include` を明示追加する。
- 以下の追加を **必須作業** とする（未登録の場合のみ追記し、重複登録はしない）。
  - `<Compile Include="Application\\Grid\\GridInputInterpreter.cs" />`
  - `<Compile Include="Application\\Grid\\GridViewUpdater.cs" />`
  - `<Compile Include="Application\\Grid\\GridDataSyncService.cs" />`
  - `<Compile Include="Application\\Grid\\Exceptions\\DomainValidationException.cs" />`
  - `<Compile Include="Application\\Grid\\Exceptions\\DataSyncException.cs" />`
- 追加後に、上記ファイルが `AEIOU.csproj` に登録されておりビルド対象になっていることを確認する。

## 依存オブジェクト一覧と注入方向
- `DataGridView`（UIコンポーネント）
  - 注入方向: `Form1` → `GridViewUpdater`（コンストラクタ注入）
- データモデル（例: `BindingList<RowModel>` / `IGridRepository`）
  - 注入方向: `Form1`（またはComposition Root）→ `GridDataSyncService`（インターフェース注入）
- Undo管理（例: `IUndoManager`）
  - 注入方向: `Form1` → `GridDataSyncService`（コンストラクタ注入）
- バリデータ（例: `IGridInputValidator`）
  - 注入方向: `Form1` → `GridInputInterpreter`（コンストラクタ注入）
- ロガー（例: `ILogger`）
  - 注入方向: `Form1` → 各抽出クラス（任意注入）

## 互換性/回帰リスク（検知方法・暫定回避策付き）
- リスク1: 既存イベント順序（CellValueChanged → SelectionChanged）依存が崩れ、更新タイミングが変わる。
  - 検知方法: 代表操作のイベント発火順ログを既存実装と比較する。
  - 暫定回避策: 既存順序を維持するアダプタ層を `Form1` 側に残し、段階的に切替する。
- リスク2: 行インデックス/モデルIDのマッピングずれで誤行更新が発生する。
  - 検知方法: 行並び替え・フィルタ適用時の更新先一致を自動テストで検証する。
  - 暫定回避策: 一時的にインデックス更新を禁止し、IDベース更新のみ許可する。
- リスク3: Undo記録の粒度変更により取り消し/やり直し結果が変化する。
  - 検知方法: 既存シナリオ（連続編集→Undo/Redo）を回帰テストで再生比較する。
  - 暫定回避策: 旧Undo記録形式との互換ラッパを `GridDataSyncService` に暫定実装する。

## 完了条件
- 抽出設計が本ファイルに記載されている。
- 実装時の互換性リスクが列挙されている。
- 本ファイル内の `DONE_TEMPLATE_PRE 記入` セクションに、共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。
- （IMPL完了条件追加）新規ファイルが `AEIOU.csproj` へ登録され、ビルド対象化されていること。

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## DONE_TEMPLATE_PRE 記入
- 対象範囲（今回扱ったファイル/機能/イベント）:
  - `Form1.cs` のグリッド更新ロジック抽出に向けた責務分割設計、依存関係整理、回帰リスクの事前列挙。
- 非対象（今回やらないこと）:
  - `Form1.cs` 本体の実装修正、抽出クラスの新規作成、ユニットテスト追加、DIコンテナ設定変更。
- 証跡リンク（調査メモ、設計資料、関連Issue/PRなど）:
  - 本ファイル（`agent_plans/refactor_form1cs_001/phase2/tasks/pre/task_PRE_002.md`）を設計記録として採用。
- 現状分析サマリ（現行構造/課題/制約）:
  - `Form1.cs` が入力解釈・表示更新・データ反映を同時に担う前提で、変更影響範囲が広く回帰リスクが高い。
- 設計方針（抽出方針、責務分割、インターフェース案）:
  - 入力解釈/表示更新/データ反映を3クラスに分離し、`Form1` はイベント中継と依存注入に責務を限定する。
- 互換性/回帰リスク列挙（最小3観点）:
  - リスク1: 既存イベント順序依存が崩れる。
  - リスク2: 行インデックスとモデルIDの不一致で誤更新が起きる。
  - リスク3: Undo/Redoの操作単位が変化し、利用者体験が変わる。
- 実装フェーズへの引き継ぎ事項（前提条件/未確定事項）:
  - `Form1` のイベントハンドラ一覧と更新経路の実測ログを先に取得し、抽出時はアダプタ層で段階移行する。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
