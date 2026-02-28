# task_PRE_001: Form1.cs のイベント処理責務の棚卸し

## 目的
`Form1.cs` 内イベントハンドラを分類し、抽出候補を明確化する。

## 実施内容
- イベントハンドラ一覧を作成する。
- 各ハンドラの責務（UI制御/モデル更新/描画更新）をラベル付けする。
- 複合責務ハンドラを抽出候補として列挙する。

## 完了条件
- 分類結果が本ファイルに記載されている。
- 次タスクへ引き継ぐ抽出候補が明記されている。
- 共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。

## イベント棚卸し（Phase2で触る可能性が高いものに限定）

### Phase2対象（必須レビュー）
| イベント | 主責務 | 副責務 | 抽出優先度 |
| --- | --- | --- | --- |
| `dataGridView1_KeyDown` | キーボード入力解釈（移動/編集/ショートカット） | 選択範囲更新、スクロール、書き込み系処理の起動 | High |
| `dataGridView1_KeyPress` | 直接入力文字の受理/変換 | セル値反映トリガ | High |
| `dataGridView1_CellPainting` | セル描画（表示ルール適用） | フレーム番号等の補助描画、再描画条件分岐 | High |
| `pasteFromAEToolStripMenuItem_Click` | AE由来データの貼り付け制御 | クリップボード解析、範囲書き込み、Undo文脈管理 | High |
| `insertCellToolStripMenuItem_Click` | セル挿入操作の実行 | 既存データシフト、複数セル更新、再描画 | Med |
| `deleteCellToolStripMenuItem_Click` | セル削除操作の実行 | データ詰め処理、複数セル更新、再描画 | Med |

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
