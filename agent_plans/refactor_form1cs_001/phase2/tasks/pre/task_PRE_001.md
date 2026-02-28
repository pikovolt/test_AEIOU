# task_PRE_001: Form1.cs のイベント処理責務の棚卸し

## 目的
`Form1.cs` のうち、Phase2実装に必要な最小限のイベント責務を分類し、抽出候補を明確化する。

## 実施内容
- 実装着手に必要な最小限のイベント棚卸しを行い、Phase2対象/今フェーズ詳細化しない対象に分類する。
- 各ハンドラの責務（UI制御/モデル更新/描画更新）をラベル付けする。
- 複合責務ハンドラを抽出候補として列挙する。

## 着手判定ユニット（独立運用）
- `U-PRE001-EVT`: `Phase2対象（必須レビュー）` の確認完了。

運用ルール:
- 本PREは他PREのファイル完了を前提にしない。必要な着手判定ユニットのみ先行で完了してよい。
- IMPL側は PRE-001 全体完了ではなく `U-PRE001-EVT` の完了有無で着手可否を判定する。

## 完了条件
- 分類結果（最低限、Phase2対象イベントと今フェーズ詳細化しないイベント）が本ファイルに記載されている。
- 次タスクへ引き継ぐ抽出候補が明記されている。
- 本ファイル内の `DONE_TEMPLATE_PRE 記入` セクションに、共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。
- Phase2実装着手に必要な確認項目（入力処理/描画/貼り付け・挿入削除の責務境界）が不足なく記載されている。
- PRE-001とPRE-002でイベント割当が一致していること。

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## DONE_TEMPLATE_PRE 記入
- 対象範囲（今回扱ったファイル/機能/イベント）:
  - `Form1.cs` のイベントハンドラ棚卸し、責務ラベル付け、抽出候補（VM2-002〜006 への接続）の整理。
- 非対象（今回やらないこと）:
  - `Form1.cs` の実装修正、イベント配線変更、VirtualMode有効化、各VM2タスクの実装着手。
- 証跡リンク（調査メモ、設計資料、関連Issue/PRなど）:
  - 本ファイル（`agent_plans/refactor_form1cs_001/phase2/tasks/pre/task_PRE_001.md`）の `イベント棚卸し` セクションを証跡として採用。
- 現状分析サマリ（現行構造/課題/制約）:
  - 単一ハンドラ内に入力解釈・データ更新・描画更新が混在し、副作用境界が不明瞭なため、変更時の影響範囲が読み取りにくい。
- 設計方針（抽出方針、責務分割、インターフェース案）:
  - イベントを「入力解釈」「描画」「データ更新」に分解して最小関数単位で抽出候補化し、VM2実装は候補単位で段階分離する。
- 互換性/回帰リスク列挙（最小3観点）:
  - リスク1: イベント分割時に既存の発火順序依存（KeyDown→更新→Invalidate）が崩れる。
  - リスク2: 副作用の切り出し漏れにより Undo 履歴記録や選択状態更新が抜け落ちる。
  - リスク3: 描画系と更新系の責務境界変更で、表示更新タイミングの差異が生じる。
- 実装フェーズへの引き継ぎ事項（前提条件/未確定事項）:
  - 各候補関数の入出力（依存メソッド・副作用）をVM2タスクの受け口に固定し、先に自動/手動でイベント順序のベースラインを取得する。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。

## 棚卸し方針（軽量運用）
- 本PREは「全イベントの網羅」を目的とせず、Phase2実装に直結する責務境界の確認を最優先とする。
- 追加調査は、実装中に不足が判明したイベントのみ都度追記する。

## イベント棚卸し

### Phase2イベント責務マトリクス（正本）
> PRE-002 から参照する正本テーブル。イベント割当の差分記載は本表を更新して反映する。

| イベント名 | 優先度 | 担当サービス候補 | 接続VM2 | 備考 |
| --- | --- | --- | --- | --- |
| `dataGridView1_KeyDown` | High（Phase2対象） | `GridInputInterpreter` | VM2-005 / VM2-003 | 入力解釈の主入口。 |
| `dataGridView1_KeyPress` | Mid（Phase2対象） | `GridInputInterpreter` | VM2-003 / VM2-005 | 現状空実装だが入力受理境界の定義対象。 |
| `dataGridView1_CellPainting` | High（Phase2対象） | `GridViewUpdater` | VM2-002 / VM2-006 | 描画責務分離の主対象。 |
| `pasteFromAEToolStripMenuItem_Click` | High（Phase2対象） | `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` | VM2-003 / VM2-006 / VM2-007 | 解析・表示反映・Undo境界を分担する複合イベント。 |
| `insertCellToolStripMenuItem_Click` | High（Phase2対象） | `GridViewUpdater` | VM2-004 | 列シフトと表示同期を分離。 |
| `deleteCellToolStripMenuItem_Click` | High（Phase2対象） | `GridViewUpdater` | VM2-004 / VM2-007 | 削除シフトと履歴境界の整理対象。 |
| `dataGridView1_CellMouseUp` | Low（対象外） | なし（PRE-001対象外） | - | マウス選択系のため本フェーズでは詳細化しない。 |
| `dataGridView1_ColumnHeaderMouseClick` | Low（対象外） | なし（PRE-001対象外） | - | 列ヘッダ操作は本フェーズの主要抽出軸外。 |

### 層1: 実装可否判断に必要な最小イベント一覧（イベント名・1行責務・優先度）
| イベント名 | 1行責務 | 優先度 |
| --- | --- | --- |
| `dataGridView1_KeyDown` | キーボード入力の解釈と移動/編集起点を担う。 | High（Phase2対象） |
| `dataGridView1_KeyPress` | 直接文字入力の受理窓口（現状は空実装）を担う。 | Mid（Phase2対象） |
| `dataGridView1_CellPainting` | セル描画ルールの適用と描画出力を担う。 | High（Phase2対象） |
| `pasteFromAEToolStripMenuItem_Click` | AE形式テキストの解析と貼り付け実行を担う。 | High（Phase2対象） |
| `insertCellToolStripMenuItem_Click` | カレント位置へのセル挿入と後続データ移動を担う。 | High（Phase2対象） |
| `deleteCellToolStripMenuItem_Click` | カレント位置のセル削除と前詰め更新を担う。 | High（Phase2対象） |
| `dataGridView1_KeyUp` | 入力後の補助処理を担う。 | Low（対象外） |
| `dataGridView1_CellMouseDown/Move/Up` | マウスドラッグ選択の開始/追従/確定を担う。 | Low（対象外） |
| `dataGridView1_CellDoubleClick` | 個別編集開始の入口を担う。 | Low（対象外） |
| `Form1_FormClosing` | 終了時の状態保存フローを担う。 | Low（対象外） |
| `undoToolStripMenuItem_Click` / `redoToolStripMenuItem_Click` | Undo/Redoの実行トリガを担う。 | Low（対象外） |

### 層2: Phase2対象（必須レビュー）の詳細表
| イベント | 主責務 | 副責務 | 識別子ベース探索情報（Form1.cs） | 副作用分類チェック | 抽出候補（最小関数レベル）と初手IMPL |
| --- | --- | --- | --- | --- | --- |
| `dataGridView1_KeyDown` | キーボード入力解釈（移動/編集/ショートカット） | 選択範囲更新、スクロール、書き込み系処理の起動 | メソッド: `dataGridView1_KeyDown`<br>関連呼び出し: `deleteRect_with_backspace`, `calcRect_with_enter`, `gridSelectionService.MoveSelection`, `flushUndoHistory`, `dataGridView1.Invalidate`<br>特徴コメント: `// 画面2/3より下に移動した場合の画面送り` | データ更新: ☑<br>選択変更: ☑<br>Invalidate: ☑<br>Undo記録: ☑<br>外部UI更新: ☐ | `MoveSelection` 前後の「範囲計算/境界補正」切り出し → **VM2-005**<br>入力種別ごとの「編集書き込み分岐」切り出し → **VM2-003**<br>`Invalidate` 呼び出し条件の集約 → **VM2-006** |
| `dataGridView1_KeyPress` | 直接入力文字の受理/変換（現状は空実装） | セル値反映トリガ（将来拡張余地） | メソッド: `dataGridView1_KeyPress`<br>関連呼び出し: なし（`return` のみ）<br>特徴コメント: `// (なにもしない)` | データ更新: ☐<br>選択変更: ☐<br>Invalidate: ☐<br>Undo記録: ☐<br>外部UI更新: ☐ | 文字入力の「受理判定（許可/拒否）」関数化 → **VM2-003**<br>`KeyDown` との責務境界（イベント分配）定義 → **VM2-005** |
| `dataGridView1_CellPainting` | セル描画（表示ルール適用） | フレーム番号等の補助描画、再描画条件分岐 | メソッド: `dataGridView1_CellPainting`<br>関連呼び出し: `drawFrameNumber`, `checkContinuty`, `gridCellStyleResolver.ResolveBackColor`, `calcBorderState`, `gridCellRenderer.ApplyTimingCellState`, `gridCellRenderer.PaintCell`<br>特徴コメント: `// (仮)フレーム数の表示 [ここから]` | データ更新: ☐<br>選択変更: ☐<br>Invalidate: ☐（本体からは要求しない）<br>Undo記録: ☐<br>外部UI更新: ☑（描画出力） | 背景色解決の「入力値→色」関数化 → **VM2-002**<br>ボーダー判定の「描画境界解決」関数化 → **VM2-006**<br>ヘッダ列描画の「フレーム番号描画判定」切り出し → **VM2-002** |
| `pasteFromAEToolStripMenuItem_Click` | AE由来データの貼り付け制御 | クリップボード解析、範囲書き込み、Undo文脈管理 | メソッド: `pasteFromAEToolStripMenuItem_Click`<br>関連呼び出し: `Clipboard.GetText`, `ApplyCellWrites`, `dataGridView1.Invalidate`, `flushUndoHistory`<br>解析ブロック補助キー: `Units Per Second`, `Time Remap`, `ApplyCellWrites("AEペースト", writes)`<br>特徴コメント: `// "Time Remap"で始まる行を探す` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☑<br>Undo記録: ☑（履歴フラッシュ含む）<br>外部UI更新: ☑（FPSメニュー反映） | クリップボード行の「ヘッダ/FPS解析」切り出し → **VM2-003**<br>AE値の「秒→フレーム変換」関数化 → **VM2-003**<br>書き込み後の「UI反映境界（Invalidate/メニュー同期）」整理 → **VM2-006**<br>AE貼り付けの「Undo境界/履歴確定」分離 → **VM2-007** |
| `insertCellToolStripMenuItem_Click` | セル挿入操作の実行 | 既存データシフト、複数セル更新、再描画 | メソッド: `insertCellToolStripMenuItem_Click`<br>関連呼び出し: `resizeDataGridView1`, `adjustWindowSize`, `CopyColumn`, `ClearColumn`, `flushUndoHistory`, `InitializeWork`<br>特徴コメント: `// カレントセルの位置を空ける` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☐（サイズ変更側に委譲）<br>Undo記録: ☑（履歴フラッシュ）<br>外部UI更新: ☑（ウィンドウ調整） | 列シフトの「移動元/移動先インデックス解決」関数化 → **VM2-004**<br>空列初期化の「挿入後クリア処理」分離 → **VM2-003**<br>サイズ変更と表示同期の境界整理 → **VM2-004** |
| `deleteCellToolStripMenuItem_Click` | セル削除操作の実行 | データ詰め処理、複数セル更新、再描画 | メソッド: `deleteCellToolStripMenuItem_Click`<br>関連呼び出し: `CopyColumn`, `resizeDataGridView1`, `adjustWindowSize`, `flushUndoHistory`, `InitializeWork`<br>特徴コメント: `// カレントセルの位置を詰める` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☐（サイズ変更側に委譲）<br>Undo記録: ☑（履歴フラッシュ）<br>外部UI更新: ☑（ウィンドウ調整） | 列詰めの「シフト終端計算」関数化 → **VM2-004**<br>削除後サイズ反映の「Row/Column同期」分離 → **VM2-004**<br>削除操作の履歴境界（Undo粒度）整理 → **VM2-007** |

### 対象外（棚卸し済み・今フェーズでは詳細化しない）
※ 以下は実装可否判断に必要な範囲で棚卸し済みであり、本フェーズでは詳細表の作成対象外とした項目。

| イベント | 1行サマリ | 抽出優先度 |
| --- | --- | --- |
| `dataGridView1_KeyUp` | 入力後処理の補助で、主ロジックは `KeyDown/KeyPress` 側に寄る。 | Low |
| `dataGridView1_CellMouseDown/Move/Up` | マウス選択操作の追従が中心で、Phase2の主要抽出軸（入力処理/描画/貼り付け）からは外れる。 | Low |
| `dataGridView1_ColumnHeaderMouseClick` | 列ヘッダ操作の補助的UIイベントで、Phase2の主要抽出軸からは外れる。 | Low |
| `dataGridView1_CellDoubleClick` | 個別編集開始の入口であり、広域なデータ変換責務は小さい。 | Low |
| `Form1_FormClosing` | 終了時の状態保存系で、VirtualMode対応の中核イベントではない。 | Low |
| `undoToolStripMenuItem_Click` / `redoToolStripMenuItem_Click` | 履歴実行の呼び出し窓口で、分離対象は履歴実装側。 | Low |

### Phase2対象の根拠（VirtualMode/責務分離との関係）
- VirtualMode導入時は、`KeyDown/KeyPress` の「入力イベント→データ供給/更新要求」境界を明確にする必要がある。
- `CellPainting` は表示問い合わせ頻度が高く、描画責務を分離しないとデータ更新ロジックと相互依存しやすい。
- `pasteFromAEToolStripMenuItem_Click` と挿入/削除系は、複数セル更新・Undo・再描画を同時に扱うため、責務分離の優先度が高い。
- 上記6イベントを先行抽出することで、UIイベント層とデータ操作層の依存をPhase2内で段階的に解ける。
