# task_PRE_001: Form1.cs のイベント処理責務の棚卸し

## 目的
`Form1.cs` 内イベントハンドラを分類し、抽出候補を明確化する。

## 実施内容
- イベントハンドラ一覧を作成する。
- 各ハンドラの責務（UI制御/モデル更新/描画更新）をラベル付けする。
- 複合責務ハンドラを抽出候補として列挙する。

## 着手判定ユニット（独立運用）
- `U-PRE001-EVT`: `Phase2対象（必須レビュー）` の確認完了。

運用ルール:
- 本PREは他PREのファイル完了を前提にしない。必要な着手判定ユニットのみ先行で完了してよい。
- IMPL側は PRE-001 全体完了ではなく `U-PRE001-EVT` の完了有無で着手可否を判定する。

## 完了条件
- 分類結果が本ファイルに記載されている。
- 次タスクへ引き継ぐ抽出候補が明記されている。
- 本ファイル内の `DONE_TEMPLATE_PRE 記入` セクションに、共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。

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

## イベント棚卸し（Phase2で触る可能性が高いものに限定）

### Phase2対象（必須レビュー）
| イベント | 主責務 | 副責務 | 識別子ベース探索情報（Form1.cs） | 副作用分類チェック | 抽出候補（最小関数レベル）と初手IMPL |
| --- | --- | --- | --- | --- | --- |
| `dataGridView1_KeyDown` | キーボード入力解釈（移動/編集/ショートカット） | 選択範囲更新、スクロール、書き込み系処理の起動 | メソッド: `dataGridView1_KeyDown`<br>関連呼び出し: `deleteRect_with_backspace`, `calcRect_with_enter`, `gridSelectionService.MoveSelection`, `flushUndoHistory`, `dataGridView1.Invalidate`<br>特徴コメント: `// 画面2/3より下に移動した場合の画面送り` | データ更新: ☑<br>選択変更: ☑<br>Invalidate: ☑<br>Undo記録: ☑<br>外部UI更新: ☐ | `MoveSelection` 前後の「範囲計算/境界補正」切り出し → **VM2-005**<br>入力種別ごとの「編集書き込み分岐」切り出し → **VM2-003**<br>`Invalidate` 呼び出し条件の集約 → **VM2-006** |
| `dataGridView1_KeyPress` | 直接入力文字の受理/変換（現状は空実装） | セル値反映トリガ（将来拡張余地） | メソッド: `dataGridView1_KeyPress`<br>関連呼び出し: なし（`return` のみ）<br>特徴コメント: `// (なにもしない)` | データ更新: ☐<br>選択変更: ☐<br>Invalidate: ☐<br>Undo記録: ☐<br>外部UI更新: ☐ | 文字入力の「受理判定（許可/拒否）」関数化 → **VM2-003**<br>`KeyDown` との責務境界（イベント分配）定義 → **VM2-005** |
| `dataGridView1_CellPainting` | セル描画（表示ルール適用） | フレーム番号等の補助描画、再描画条件分岐 | メソッド: `dataGridView1_CellPainting`<br>関連呼び出し: `drawFrameNumber`, `checkContinuty`, `gridCellStyleResolver.ResolveBackColor`, `calcBorderState`, `gridCellRenderer.ApplyTimingCellState`, `gridCellRenderer.PaintCell`<br>特徴コメント: `// (仮)フレーム数の表示 [ここから]` | データ更新: ☐<br>選択変更: ☐<br>Invalidate: ☐（本体からは要求しない）<br>Undo記録: ☐<br>外部UI更新: ☑（描画出力） | 背景色解決の「入力値→色」関数化 → **VM2-002**<br>ボーダー判定の「描画境界解決」関数化 → **VM2-006**<br>ヘッダ列描画の「フレーム番号描画判定」切り出し → **VM2-002** |
| `pasteFromAEToolStripMenuItem_Click` | AE由来データの貼り付け制御 | クリップボード解析、範囲書き込み、Undo文脈管理 | メソッド: `pasteFromAEToolStripMenuItem_Click`<br>関連呼び出し: `Clipboard.GetText`, `ApplyCellWrites`, `dataGridView1.Invalidate`, `flushUndoHistory`<br>解析ブロック補助キー: `Units Per Second`, `Time Remap`, `ApplyCellWrites("AEペースト", writes)`<br>特徴コメント: `// "Time Remap"で始まる行を探す` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☑<br>Undo記録: ☑（履歴フラッシュ含む）<br>外部UI更新: ☑（FPSメニュー反映） | クリップボード行の「ヘッダ/FPS解析」切り出し → **VM2-003**<br>AE値の「秒→フレーム変換」関数化 → **VM2-003**<br>書き込み後の「UI反映境界（Invalidate/メニュー同期）」整理 → **VM2-006**<br>AE貼り付けの「Undo境界/履歴確定」分離 → **VM2-007** |
| `insertCellToolStripMenuItem_Click` | セル挿入操作の実行 | 既存データシフト、複数セル更新、再描画 | メソッド: `insertCellToolStripMenuItem_Click`<br>関連呼び出し: `resizeDataGridView1`, `adjustWindowSize`, `CopyColumn`, `ClearColumn`, `flushUndoHistory`, `InitializeWork`<br>特徴コメント: `// カレントセルの位置を空ける` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☐（サイズ変更側に委譲）<br>Undo記録: ☑（履歴フラッシュ）<br>外部UI更新: ☑（ウィンドウ調整） | 列シフトの「移動元/移動先インデックス解決」関数化 → **VM2-004**<br>空列初期化の「挿入後クリア処理」分離 → **VM2-003**<br>サイズ変更と表示同期の境界整理 → **VM2-004** |
| `deleteCellToolStripMenuItem_Click` | セル削除操作の実行 | データ詰め処理、複数セル更新、再描画 | メソッド: `deleteCellToolStripMenuItem_Click`<br>関連呼び出し: `CopyColumn`, `resizeDataGridView1`, `adjustWindowSize`, `flushUndoHistory`, `InitializeWork`<br>特徴コメント: `// カレントセルの位置を詰める` | データ更新: ☑<br>選択変更: ☐<br>Invalidate: ☐（サイズ変更側に委譲）<br>Undo記録: ☑（履歴フラッシュ）<br>外部UI更新: ☑（ウィンドウ調整） | 列詰めの「シフト終端計算」関数化 → **VM2-004**<br>削除後サイズ反映の「Row/Column同期」分離 → **VM2-004**<br>削除操作の履歴境界（Undo粒度）整理 → **VM2-007** |

### 対象外（参照のみ）
| イベント | 1行サマリ | 抽出優先度 |
| --- | --- | --- |
| `dataGridView1_KeyUp` | 入力後処理の補助で、主ロジックは `KeyDown/KeyPress` 側に寄る。 | Low |
| `dataGridView1_CellMouseDown/Move/Up` | マウス選択操作の追従が中心で、Phase2の主要抽出軸（入力処理/描画/貼り付け）からは外れる。 | Low |
| `dataGridView1_CellDoubleClick` | 個別編集開始の入口であり、広域なデータ変換責務は小さい。 | Low |
| `Form1_FormClosing` | 終了時の状態保存系で、VirtualMode対応の中核イベントではない。 | Low |
| `undoToolStripMenuItem_Click` / `redoToolStripMenuItem_Click` | 履歴実行の呼び出し窓口で、分離対象は履歴実装側。 | Low |

### Phase2対象の根拠（VirtualMode/責務分離との関係）
- VirtualMode導入時は、`KeyDown/KeyPress` の「入力イベント→データ供給/更新要求」境界を明確にする必要がある。
- `CellPainting` は表示問い合わせ頻度が高く、描画責務を分離しないとデータ更新ロジックと相互依存しやすい。
- `pasteFromAEToolStripMenuItem_Click` と挿入/削除系は、複数セル更新・Undo・再描画を同時に扱うため、責務分離の優先度が高い。
- 上記6イベントを先行抽出することで、UIイベント層とデータ操作層の依存をPhase2内で段階的に解ける。
