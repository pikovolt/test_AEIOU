# done_VM2_012

- 対応タスク: [task_VM2_012](../../tasks/impl/task_VM2_012.md)
- 判定: Done

## 共通必須欄
- 対象範囲（今回変更したファイル/機能/イベント）:
  - `dataGridInitialize` 内の全セル初期化ループの要否評価。
  - `InitializeWork(InitializeTarget.Timing)` 内の重複 `SetCellValue` ループの確認。
  - `TimingSheetModel` コンストラクタの初期化状態の確認。
- 非対象（今回やらないこと）:
  - 初期化ループの削除・最適化実施（Phase 3 スコープとして持ち越し）。
  - 起動パフォーマンスの定量計測。
- 証跡リンク（実行ログ、確認メモ、関連Issue/PRなど）:
  - [task_VM2_012](../../tasks/impl/task_VM2_012.md)
  - `Form1.cs:668-670` — `dataGridInitialize` 内の全セル初期化ループ
  - `Form1.cs:517-574` — `InitializeWork(InitializeTarget.Timing)` 内の追加 SetCellValue ループ
  - `TimingSheetModel.cs:18-27` — コンストラクタでの全セル `""` 初期化
  - `Form1.cs:528` — `gridViewManager.InitializeWork(dataGridView1, timingSheetModel)` （view/model バインド）
  - `GridViewManager.cs:220-234` — `SetCellDisplayValue` の VirtualMode 分岐（InvalidateCell 発行箇所）

## IMPL判定項目（動作成立・回帰確認・非対象明記）
- 動作成立（成功条件と確認結果）:
  - **問題あり（パフォーマンス乖離・Phase 3 持ち越し）**。根拠は下記のとおり。
  - `TimingSheetModel.cs:18-27` のコンストラクタでコレクション全セルが `""` で初期化済み。
  - `dataGridInitialize` の `SetCellValue("")` ループはモデルをさらに `""` で上書きする（完全に冗長）。
  - VirtualMode では `CellValueNeeded` イベントが表示時に都度値を供給するため、初期化時の一括 `InvalidateCell` は不要。
  - 実際の影響:
    - `InitializeWork(bool isBoot)` 初回起動時: `gridViewManager._view` が null のため `SetCellDisplayValue` が即 return。→ **0件の InvalidateCell**（実害なし）
    - `InitializeWork(bool isBoot)` 再初期化時（loadSTS経由）: `_view = dataGridView1`（設定済み）、`Model != timingSheetModel`（新規作成直後）→ else分岐で `SetCellDisplayValue` → `InvalidateCell` x **13,000件**
    - `InitializeWork(InitializeTarget.Timing)`（resize経由）: `dataGridInitialize` で 13,000件 + 行:570-574 の重複ループで 13,000件 → 計 **26,000件**
  - 起動直後は表示崩れ・例外は発生しておらず、Phase 2 の動作成立条件を侵害しない。
  - ただし VirtualMode 設計との乖離があり、Phase 3 のパフォーマンス改善スコープとして持ち越す。
- 回帰確認（最小2観点）:
  - 観点1: 初期化後の表示崩れなしを確認（既存 VM2-001/004 Done 証跡より）。InvalidateCell の発行数が多くても最終的な表示結果に影響しない。
  - 観点2: 起動・ファイル読み込み時に例外が発生しないことを確認（既存 smoke_virtualmode チェックリストの起動系項目が [x] であることより）。
- 未解決事項/フォローアップ（必要時）:
  - VM2-012-R01 / 影響: VirtualMode 設計との乖離。`dataGridInitialize` の `SetCellValue` ループと `InitializeWork(InitializeTarget.Timing)` の重複ループを削除または `InvalidateAll()` 1回に集約することで起動・リサイズ時の描画コストを削減できる / 暫定評価: Phase 3 スコープ（描画分離時に同時対応）。

## 検証セット
- セット1
  - 前提/操作/期待/実結果:
  - 前提: コードの静的解析。VirtualMode=true 環境。
  - 操作: `TimingSheetModel.cs:18-27`・`Form1.cs:668-670`・`Form1.cs:517-574`・`GridViewManager.cs:220-234` を順に確認する。
  - 期待: TimingSheetModel コンストラクタで全セルが "" 初期化済みであり、`SetCellValue("")` ループは冗長。VirtualMode 時の SetCellDisplayValue が InvalidateCell を発行する経路が存在する。
  - 実結果: TimingSheetModel コンストラクタ (`TimingSheetModel.cs:18-27`) で全セル `""` 初期化済みを確認。`dataGridInitialize` の SetCellValue ループは冗長。`InitializeWork(InitializeTarget.Timing)` では重複して同ループが存在し最大 26,000 件の InvalidateCell が発行される経路を確認。起動・表示崩れなし・例外なしはVM2-001/004 証跡で担保済み。性能問題は Phase 3 持ち越しとして記録した。
  - 証跡リンク: `Form1.cs:668-670`、`TimingSheetModel.cs:18-27`、`GridViewManager.cs:220-234`
