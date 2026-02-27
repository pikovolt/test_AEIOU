# Form1.cs リファクタリング設計（DataGridView VirtualMode 対応）

## 1. 背景と目的

`AEIOU/WindowsFormsApplication1/Form1.cs` は、以下の責務が 1 ファイルに集中している。

- データ管理（セル値の読取/書込、行列操作）
- 描画（`CellPainting` で色、罫線、フレーム番号、記号描画）
- キー入力（`KeyDown` で移動・編集・ショートカット・範囲選択）
- UI 制御（スクロール、選択、フォーカス）

この状態では、機能追加時に副作用が発生しやすく、テストしづらい。そこで次の 3 方針で分割する。

1. `DataGridView` を `VirtualMode = true` に切り替える。  
2. データを `Model` 層へ分離する。  
3. 描画・キー入力をそれぞれ別コンポーネントへ分離する。  

また、本ツールは「タイムシートをテンキーのみで打ち込む」運用が中心であり、
表計算ソフトの入力体験に近い軽快さが必須である。したがって、
**キー入力から画面反映まで 25ms 以内**を性能目標（P95）として設計に組み込む。

---

## 2. 非機能要件（入力レスポンス）

- 主指標: キー入力イベント受理 (`KeyDown`) からセル表示更新完了までの時間
- 目標値: **P95 <= 25ms**（通常編集操作、単一セル入力時）
- 許容上限: P99 <= 40ms（スパイク監視用）
- 計測対象外: クリップボード大量貼付、行列一括挿入/削除などバッチ系処理

### 2.1 計測契約（Measurement Contract）

計測の解釈ブレを防ぐため、以下で固定する。

- 開始点（T0）: `dataGridView1_KeyDown` 受理直後
- 終了点（T1）: 入力対象セルの `CellPainting` が完了した時点
- 計測値: `latency_ms = T1 - T0`
- 対象セル: `CurrentCell`（範囲入力時は左上セル）
- 集計窓: 直近 N=200 イベント（移動平均）

> 注: `InvalidateCell` 呼び出し時刻ではなく、実描画完了までを含める。

### 2.2 標準計測シナリオ（回帰判定用）

性能判定は次の 4 シナリオで行う。

1. 単一セル連続入力（テンキー 0-9 を連打）
2. Enter 移動付き入力（入力 + 下方向移動）
3. 空セル入力（KaraCell 設定値含む）
4. スクロール境界付近（表示 2/3 付近）での連続入力

各シナリオで P95/P99 を採取し、**いずれか 1 つでも P95 > 25ms なら回帰**と判定する。

### 2.3 計測実装メモ

- `GridInputController.HandleKeyDown` の入口で `Stopwatch.GetTimestamp()` を取得
- `GridCellRenderer.Paint` 終了時に該当セルなら計測終了
- デバッグビルドでのみ計測ログを有効化（`#if DEBUG`）
- ログは CSV（timestamp, key, row, col, latency_ms）で出力可能にする

### 2.4 25ms 目標を満たすための実装原則

- 1 キー入力で再描画する範囲を最小化（`InvalidateCell` / `InvalidateColumn`）
- 文字列解析・状態判定は `GridCellStyleResolver` で軽量化し、不要な `ToString()` を抑制
- 入力処理（Controller）中に I/O（ファイル、外部プロセス呼び出し）を行わない
- 反復操作は都度 GC 負荷を増やす一時オブジェクト生成を避ける

---

## 3. 目標アーキテクチャ

### 3.1 レイヤ構成

- **UI（WinForms）**
  - `Form1`（薄いコンテナ）
  - `GridViewAdapter`（DataGridView 依存操作の集約）
- **Application / Controller**
  - `GridInputController`（キー入力・ショートカット解釈）
  - `SelectionController`（選択範囲・カーソル移動）
- **Presentation**
  - `GridCellStyleResolver`（色/罫線など見た目判定）
  - `GridCellRenderer`（`CellPainting` 実処理）
- **Domain / Model**
  - `TimingSheetModel`（セル内容・列名・各種設定）
  - `TimingCellModel`（単一セル状態）
  - `SheetOperationService`（挿入/削除/貼付/連番など操作）

### 3.2 依存ルール

- `Form1` は **モデルを直接編集しない**。Controller 経由のみ。
- Renderer は **モデルを read-only 参照**。
- Controller は **UI 部品の詳細を知らない**。`IGridView` インターフェース越しに操作。
- `DataGridViewCell` 派生クラスへ状態を持たせない。描画状態は `GridCellStyle` DTO で渡す。

### 3.3 インターフェース契約（最小）

#### IGridView（Controller から利用）

- `CellPosition CurrentCell { get; set; }`
- `SelectionRange CurrentSelection { get; set; }`
- `void InvalidateCell(int col, int row)`
- `void EnsureVisible(int row)`
- `void BeginBatchUpdate()` / `void EndBatchUpdate()`

不変条件:

- `CurrentCell` は常に有効範囲内
- `EndBatchUpdate` 後は UI 状態が一貫していること（選択とカーソルの整合）

#### GridCellStyle DTO（Renderer へ渡す）

- `bool IsActiveColumn`
- `bool IsSelected`
- `bool IsKaraCell`
- `bool IsContinuousLine`
- `SheetBorder BorderState`
- `string DisplayText`

---

## 4. VirtualMode 導入方針

## 4.1 基本設定

`Form1` 初期化で以下を設定する。

- `dataGridView1.VirtualMode = true`
- `RowCount` / `ColumnCount` はモデル件数と同期
- `AllowUserToAddRows = false`

## 4.2 必須イベント

VirtualMode では値の実体をグリッドに持たないため、次を実装する。

- `CellValueNeeded`
  - `model.GetDisplayValue(col, row)` を返す
- `CellValuePushed`（編集を許可する場合）
  - `controller.ApplyCellEdit(col, row, value)` を呼ぶ
- `NewRowNeeded` は今回不要（固定行数運用）

## 4.3 キャッシュ戦略

- 初期段階は **モデルを唯一の真実源（SoT）** とし、UI キャッシュなし。
- パフォーマンス問題が出たら、`VisibleRange` 単位の読み取りキャッシュを `GridViewAdapter` に追加。

---

## 5. コンポーネント設計

## 5.1 Model

### TimingSheetModel

責務:

- セル値（`string`）
- 列ヘッダ名
- fps、開始フレーム、カラセル文字など表示関連設定
- 行/列挿入削除 API

代表 API 例:

- `string GetCellValue(int col, int row)`
- `void SetCellValue(int col, int row, string value)`
- `void InsertRows(int at, int count)`
- `void DeleteRows(int at, int count)`

### SheetOperationService

責務:

- 連番、繰り返し、置換、四則演算、コピー/カット/貼付など
- Undo/Redo 履歴作成

## 5.2 Input

### GridInputController

責務:

- `KeyDown` の分岐をコマンドへ変換
- 単一セル入力／範囲入力／特殊キー（Enter, BS, Delete, +/- など）の統一処理
- モデル更新後に必要な UI 更新通知を発行

I/F 例:

- `void HandleKeyDown(GridKeyEvent e)`
- `void ApplyCellEdit(int col, int row, string value)`

### SelectionController

責務:

- 現在セル、選択矩形、アンカー位置の管理
- キー移動時のスクロール追従判定

## 5.3 Rendering

### GridCellStyleResolver

責務:

- 偶数/奇数列色
- アクティブ列・選択領域・基準線判定
- カラセル、継続記号、通常値の種別判定

### GridCellRenderer

責務:

- `CellPainting` 内部ロジックを集約
- `GridCellStyle` を受けて描画（背景、罫線、文字、補助情報）

---

## 6. Form1 の責務縮小

`Form1` に残す責務は以下のみ。

- DI 相当の組み立て
  - `model`, `controller`, `renderer`, `adapter` の生成と接続
- WinForms イベントの中継
  - `KeyDown -> controller.HandleKeyDown`
  - `CellPainting -> renderer.Paint`
  - `CellValueNeeded -> model.GetDisplayValue`
- メニュー/ダイアログ呼び出しの入口

> 目安: `Form1.cs` の業務ロジックを段階的に 70〜80% 以上外部化する。

---

## 7. 移行手順（段階的）

## Phase 0: 現状固定

- 既存挙動のスモークテスト項目を作る（手動で可）
  - セル編集
  - 範囲選択
  - コピー/貼付
  - キー移動
  - 基準線描画
- 現行実装で性能ベースラインを取得（P50/P95/P99）
  - 対象は「2.2 標準計測シナリオ」の 4 ケース

## Phase 1: モデル抽出

- `TimingSheetModel` を追加し、`dataGridView1[col,row].Value` 直参照箇所をモデル経由へ置換。
- この段階では VirtualMode はまだ OFF でもよい。

## Phase 2: VirtualMode 化

- `CellValueNeeded / CellValuePushed` 実装。
- `Rows/Columns` への直接値代入コードを除去。

## Phase 3: 描画分離

- `CellPainting` の条件判定と描画処理を `GridCellStyleResolver` + `GridCellRenderer` に移す。

## Phase 4: 入力分離

- `KeyDown` の switch 群を `GridInputController` に移す。
- ショートカット定義を `KeyBindingMap` として独立させる。

## Phase 5: 最適化

- 必要に応じて描画キャッシュ・可視範囲更新最適化を導入。
- P95 > 25ms の場合はホットパスプロファイルを取得し、要因別に改善。

---

## 8. リスクと対策

- **リスク**: VirtualMode で `SelectedCells` / `CurrentCell` 周辺の挙動差異が出る  
  **対策**: `IGridView` 経由で選択 API を統一し、既存ショートカットの回帰テストを作成。

- **リスク**: `CellPainting` 分離後に見た目が変わる  
  **対策**: 主要状態（通常/選択/アクティブ/カラセル）の描画比較チェックリストを作成。

- **リスク**: 大量データ時の更新頻度増大  
  **対策**: `InvalidateCell` / `InvalidateColumn` 単位で再描画を限定。

- **リスク**: テンキー入力時の遅延が 25ms 目標を超える  
  **対策**: レンダリング/入力処理の計測ログを常時取得し、P95 超過時は回帰として扱う。

---

## 9. 完了条件（Definition of Done）

- `Form1` の `KeyDown` 本体が「イベント中継 + 最小限の UI 制御」のみ。
- `CellPainting` 本体が `renderer` 呼び出しのみ。
- セルデータの読み書きがすべて `TimingSheetModel` 経由。
- `dataGridView1.VirtualMode = true` で既存主要操作が破綻しない。
- 「2.2 標準計測シナリオ」全ケースで入力→表示反映が **P95 25ms 以内**。
- 計測ログ（P50/P95/P99）を成果物として添付。
- 手動スモークテスト項目を全通過。
