# unit_PRE_P1_02_service_extraction_architecture

## 目的
抽出先サービスの責務重複を防ぎ、実装順序を明確化する。

## 必須確認
- `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` の責務が重複しない。
- `Form1` はイベント中継と依存注入に責務を限定する。

## サービス名確定（既存拡張 or 新規導入）

| サービス名 | 方針 | 根拠（現行実装） | 担当責務 |
| --- | --- | --- | --- |
| `GridInputInterpreter` | **新規導入** | `Form1` の `dataGridView1_KeyDown` / `dataGridView1_KeyPress` が入力解釈と操作実行を同時に担っているため、入力解釈を独立させる。 | キー入力・編集入力の解釈、入力コマンド化、入力由来の境界判定。 |
| `GridViewUpdater` | **既存 `GridCellRenderer` 拡張で導入** | `GridCellRenderer` がセル描画状態適用を保持しているため、表示更新の集約先として拡張する。 | 描画状態適用、`Invalidate` 境界制御、選択表示・再描画更新。 |
| `GridDataSyncService` | **既存 `GridViewManager` 拡張で導入** | `GridViewManager` が `RowCount` / `ColumnCount` / モデル値アクセスを管理しているため、同期責務を一元化しやすい。 | 行列件数同期、`CellValueNeeded/CellValuePushed` 連携、モデル-表示の更新同期。 |

## `AEIOU/WindowsFormsApplication1` 責務分離マッピング

### `Form1` に残す責務（薄いUI層）
- イベント中継: WinFormsイベント受信後、各サービス呼び出しへ中継する。
- DI/Composition: `Form1` 初期化時に `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` を組み立てる。
- 画面境界責務: メニュー、ダイアログ、フォームライフサイクル、エラー通知などUI境界のみ保持する。

### `Form1` から外出しする責務
- 入力解釈: キー種別判定、ショートカット解決、編集系コマンド解決（`GridInputInterpreter`）。
- 表示更新: セル表示状態反映、再描画境界、選択描画更新（`GridViewUpdater`）。
- 同期: 行列件数、VirtualMode値読書き、モデルと表示の更新整合（`GridDataSyncService`）。

## PRE/IMPL 用語統一
- PRE・IMPLとも、責務軸は `入力解釈 / 表示更新 / 同期` を正本語彙とする。
- サービス名は `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` に固定する。
- IMPLタスクでは以下の対応で記述する。
  - VM2-002: `GridDataSyncService`（`CellValueNeeded` read）
  - VM2-003: `GridDataSyncService`（`CellValuePushed` write）
  - VM2-004: `GridDataSyncService`（Row/Column同期）
  - VM2-006: `GridViewUpdater`（`Invalidate` 境界最適化）

## 完了判定
- 2項目ともPASS。
