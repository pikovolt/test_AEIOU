# Phase 6 Overview — シート編集計算の分離

## 目的
`Form1` に残るシート編集処理から、WinForms や Undo を知らなくても成立する「どのセルをどの値へ変更するか」という計算を段階的に分離する。

最初の題材は次の4経路とする。

- `insertToAllCell`（全列に対する行挿入）
- `cutToAllCell`（全列に対する行削除）
- `insertCellToolStripMenuItem_Click`（列挿入）
- `deleteCellToolStripMenuItem_Click`（列削除）

Phase6 は、これらを一度に `SheetEditService` へ移す計画ではない。まず入力スナップショットから変更一覧を作る純粋な計算を抽出し、`Form1` には UI 入力、サイズ変更、Undo group、変更適用、再描画などの orchestration を残す。

## 現状と最初の境界
行挿入・削除は既に次の共通経路を持つ。

```text
insertToAllCell / cutToAllCell
  -> ExecuteWriteGroup
  -> QueueShiftWrites
  -> QueueCellWrite
  -> GridViewManager / Model
```

ただし `QueueShiftWrites` は値の読み取りと書き込みを同時に行い、`Form1` の private メソッドへ依存している。第一段階ではこれを次の形へ変える。

```text
Form1（入力、Undo、適用、再描画）
  -> SheetRowEditCalculator（入力スナップショットから変更一覧を計算）
  -> CellWriteEntry[]
  -> Form1 / GridViewManager（検証済み変更を適用）
```

変更一覧は Phase5 Automation の `Command -> AutomationChange[] -> Host` と同じ考え方を用いるが、外部拡張契約へシート内部編集を無理に載せない。Phase6 の型は本体内部に置き、契約共有が本当に必要になった時点で別タスクとして判断する。

## 責務境界

### 計算側へ移すもの
- 行挿入・削除時の source/destination 座標計算。
- 下方向シフトを末尾から、上方向シフトを先頭から生成する順序。
- 挿入領域または削除後の末尾領域を空文字にする変更。
- 列挿入・削除時の列コピー先とクリア対象の計算（行編集が安定した後）。
- 範囲外、0件、負数など入力条件の検証結果。

### Form1 / host 側に残すもの
- 現在セル・選択範囲・ダイアログなど UI 入力の取得。
- `resizeDataGridView1`、`adjustWindowSize` と列ヘッダーの反映。
- `ExecuteWriteGroup`、Undo/Redo、`flushUndoHistory`。
- `GridViewManager` への変更適用、継続表示再計算、Invalidate。
- `delRange` / `addRange` の更新とコピーバッファ初期化。

`Form1` の private 状態をまとめて calculator/service の constructor へ渡すことは禁止する。新しい計算クラスの入力は、行列数、対象位置・件数、読み取り専用のセル値だけに限定する。

## 段階導入
1. **P6-001 現行挙動の固定** — 4経路の境界、書き込み順、Undo/履歴、ヘッダー、関連範囲への影響を記録する。
2. **P6-002 変更一覧の内部契約** — `CellWriteEntry` を `Form1` の private 型から切り離し、読み取り専用入力と計算結果の最小境界を作る。
3. **P6-003 行編集計算の抽出** — `insertToAllCell` / `cutToAllCell` のシフトとクリア計算を `SheetRowEditCalculator` へ移す。
4. **P6-004 行編集の適用経路統一** — `Form1` を「計算結果を1 groupで適用して後処理する」形へ縮小する。
5. **P6-005 列編集計算の抽出** — 列挿入・削除の値/ヘッダー移動計算を抽出し、サイズ変更との実行順を明示する。
6. **P6-006 回帰と次段階判定** — smoke と実測を完了し、適用側を service 化するかを証拠に基づいて判断する。

## 非対象
- `Form1` の全メソッドを別クラスへ移すこと。
- 設定ダイアログなど UI そのものの抽出。
- Automation 公開契約の変更または外部 DLL API の拡張。
- `TimingSheetModel`、`GridViewManager`、UndoManager の同時再設計。
- 行・列編集の利用者向け仕様変更や、既知不具合の便乗修正。

## Phase5 からの持ち越し
Phase5 は運用文書まで作成済みだが、Windows + .NET Framework 3.5 環境を要する次の確認は未完了のまま残す。

- Debug/Release x86 ビルド。
- Automation tests の実行。
- 組み込み処理、外部拡張、Undo、STS/AE 連携の Windows UI smoke。
- クリーン環境でのサンプル拡張の配置、検出、実行、除去。

これらは Phase5 の checklist/完了報告で追跡し、Phase6 の完了数に算入しない。Phase6 で Automation 関連ファイルへ変更が及んだ場合のみ、該当する Phase5 smoke を追加 Gate とする。

## Phase6 完了条件
- 行挿入・削除の変更一覧が WinForms、`Form1`、`DataGridView`、Undo 実装に依存せず計算できる。
- 列挿入・削除では、純粋計算と UI/サイズ変更/履歴初期化の境界がコード上で分かれている。
- `Form1` は変更先座標の二重ループを持たず、計算結果の検証・適用と UI 後処理を担当する。
- 空シフト、先頭/末尾、全行相当、最小列数付近を含む境界入力で範囲外変更を生成しない。
- 既存の Undo/Redo、`delRange` / `addRange`、ヘッダー、コピーバッファ、再描画の挙動を維持する。
- `checklists/smoke_phase6.md` の必須項目が完了している。
