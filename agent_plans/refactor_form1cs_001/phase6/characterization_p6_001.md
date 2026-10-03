# P6-001 現行挙動 characterization

## 1. この文書の位置付け

Phase 6 でシート編集計算を分離する前の、現在の `Form1` の挙動を固定する。
ここでいう「互換維持」は望ましい仕様という意味ではなく、P6-002 以降で計算を移す際に
意図せず変えないための基準である。範囲外入力など安全性に関わる項目は
**仕様変更候補** として分け、calculator 抽出と同じ変更に混ぜない。

座標は `(row, col)=値`、行数を `R`、列数を `C` と書く。例では 2 列 5 行の入力を
`A=[A0,A1,A2,A3,A4]`, `B=[B0,B1,B2,B3,B4]` とする。`""` は空セルである。
値の書き込み順は `QueueShiftWrites` が作る `SetValueOperation` の順を指し、
範囲クリアは末尾の `DeleteOperation` 1 件として表す。

## 2. 共通境界

- 行挿入・削除はシートの行数・列数を変更しない。全列の値だけを移動し、対象領域を空にする。
- シフトは列優先である。下方向は各列の末尾から先頭へ、上方向は各列の先頭から末尾へ書く。
  これにより読み取り元と書き込み先が重なっても、まだ読んでいない値を上書きしない。
- シフトの `length <= 0` または空の列範囲ではシフト書き込みを作らない。ただし呼び出し側の
  `deleteRect` は省略されず、挿入領域または削除後の末尾領域をクリアする。
- 行編集は `ExecuteWriteGroup` により、シフトの全 `SetValueOperation` と最後の
  `DeleteOperation` をそれぞれ「行の挿入」「行の削除」という 1 group にする。
  `deleteRect(..., false)` も group 内に入り、そこで `isFirstEdit = true` になる。
- `insertToAllCell` は group 後に `Invalidate` だけを呼ぶ。`cutToAllCell` は
  `FinishWriteOperation(true)` を呼ぶため、明示的に `isFirstEdit = true` と `Invalidate` を行う。
- `deleteRect` の範囲クリアは `null` を生成せず `""` を設定する。既存セル読み取りも
  `GetCellValue` 境界では文字列として扱われる。calculator の入力値と出力値は
  **非 null 文字列（null は `""` に正規化）** を契約とする。

## 3. 行挿入 (`insertToAllCell`)

計算式は `movableLength = R - (Row + Count)`、読み取り開始は `Row`、書き込み開始は
`Row + Count` である。移動後に全列の `[Row, Row + Count)` を空にする。

| ID | 入力 (`R=5`, `C=2`) | シフト書き込み順（その後に記載範囲をクリア） | 最終列 A | Undo |
|---|---|---|---|---|
| RI-01 | 先頭 `Row=0, Count=1` | `(4,0)=A3,(3,0)=A2,(2,0)=A1,(1,0)=A0`, 次に B も同順。row 0 を全列クリア | `[,A0,A1,A2,A3]` | 「行の挿入」1 group |
| RI-02 | 中間 `Row=2, Count=1` | `(4,0)=A3,(3,0)=A2`, 次に B。row 2 を全列クリア | `[A0,A1,,A2,A3]` | 同上 |
| RI-03 | 末尾 `Row=4, Count=1` | シフトなし。row 4 を全列クリア | `[A0,A1,A2,A3,]` | クリアを含む1 group |
| RI-04 | 中間複数 `Row=1, Count=2` | `(4,0)=A2,(3,0)=A1`, 次に B。rows 1..2 を全列クリア | `[A0,,,A1,A2]` | 1 group |
| RI-05 | 末尾複数 `Row=3, Count=2` | シフトなし。rows 3..4 を全列クリア | `[A0,A1,A2,,]` | クリアを含む1 group |

Undo は group 内の逆順でクリア前の値を戻してからシフト前の値を戻し、1 回で入力全体へ戻る。
Redo も 1 回で表の最終値を再現する。ただし後述の呼び出し元が直後に履歴を flush する場合、
利用者はこの group を Undo/Redo できない。

## 4. 行削除 (`cutToAllCell`)

計算式は `sourceStartRow = Row + Count`、`movableLength = R - sourceStartRow`、
書き込み開始は `Row` である。移動後に全列の `[R - Count, R)` を空にする。

| ID | 入力 (`R=5`, `C=2`) | シフト書き込み順（その後に記載範囲をクリア） | 最終列 A | Undo |
|---|---|---|---|---|
| RD-01 | 先頭 `Row=0, Count=1` | `(0,0)=A1,(1,0)=A2,(2,0)=A3,(3,0)=A4`, 次に B。row 4 を全列クリア | `[A1,A2,A3,A4,]` | 「行の削除」1 group |
| RD-02 | 中間 `Row=2, Count=1` | `(2,0)=A3,(3,0)=A4`, 次に B。row 4 を全列クリア | `[A0,A1,A3,A4,]` | 同上 |
| RD-03 | 末尾 `Row=4, Count=1` | シフトなし。row 4 を全列クリア | `[A0,A1,A2,A3,]` | クリアを含む1 group |
| RD-04 | 中間複数 `Row=1, Count=2` | `(1,0)=A3,(2,0)=A4`, 次に B。rows 3..4 を全列クリア | `[A0,A3,A4,,]` | 1 group |
| RD-05 | 全行 `Row=0, Count=5` | シフトなし。rows 0..4 を全列クリア | `[,,,,]` | クリアを含む1 group |

Undo/Redo の単位と、呼び出し元の flush による差は行挿入と同じである。

## 5. 行編集の呼び出し経路と関連範囲

| 経路 | 値編集後の処理 | `delRange` / `addRange` | 選択範囲 | 履歴・描画 |
|---|---|---|---|---|
| `Insert`（`OnInsert`） | `insertToAllCell(top,height)` | 両リストについて、`range.Top >= top` の範囲だけ Top/Bottom に `+height` | 元の選択を高さ分だけ下へ移動 | 行 group 作成後に履歴を flush。挿入内で描画 |
| `Shift+Delete`（`OnDelete`） | `cutToAllCell(top,height)` | 同条件の範囲だけ Top/Bottom から `height` を引く | `max(0, Y-height)` へ移動 | 行 group 作成後に履歴を flush。削除内で描画 |
| 切り貼り範囲設定 | 重なる既存 `addRange` を行削除し、選択位置へ行挿入 | 各削除/挿入の直後に両リストを補正し、最後に選択範囲を `addRange` へ追加 | 明示移動なし | 一連の終了時に履歴を flush、`isFirstEdit=true`、再描画 |
| 切り貼り範囲解除 | 該当 `addRange` を行削除 | 削除直後に両リストを補正し、該当 range を除去 | 明示移動なし | 終了時に履歴を flush、`isFirstEdit=true`、再描画 |

範囲補正は「編集範囲との交差」を計算せず、`range.Top >= top` だけを条件とする。
そのため編集位置をまたぐ範囲や、削除により負座標になる範囲を特別扱いしない。この挙動は
抽出時の互換対象であり、範囲の clamp/統合/除去は別修正とする。

## 6. 列挿入 (`insertCellToolStripMenuItem_Click`)

処理順は次のとおりである。

1. 操作開始時の current column を `col` として保存する。
2. 列数を `C + 1` に変更する。`resizeDataGridView1` は既存の値、ヘッダー、
   `aryCellUsedCount` を退避し、Timing/model/grid を作り直して同じ index へ復元する。
3. `adjustWindowSize` を呼ぶ。
4. 新しい末尾の一つ前から `col` まで降順に、列ごとに
   **使用数 → ヘッダー → row 0..R-1 の値** の順で `i` から `i+1` へコピーする。
5. `col` を **使用数 0 → ヘッダー `""` → row 0..R-1 の値 `""`** の順でクリアする。
6. Undo/Redo 履歴を flush し、copy buffer を初期化する。

| ID | 対象 | 入力列 `[header: values; used]` | 最終列 |
|---|---|---|---|
| CI-01 | 先頭 `col=0` | `A:[A0..;a], B:[B0..;b]` | `:[""..;0], A:[A0..;a], B:[B0..;b]` |
| CI-02 | 中間 `col=1` | 同上 | `A:[A0..;a], :[""..;0], B:[B0..;b]` |
| CI-03 | 末尾 `col=C-1` | 同上 | `A:[A0..;a], :[""..;0], B:[B0..;b]`（旧末尾 B は右へ移る） |

列数は常に 1 増え、行数は不変である。`resizeDataGridView1` と列コピーはいずれも
Undo 操作として積まれず、最後の flush 後は列挿入もそれ以前の編集も Undo/Redo できない。
`addRange` / `delRange` は変更しない。明示的な `Invalidate` はなく、サイズ変更と値設定に伴う
通常の DataGridView 更新に委ねる。

current cell は開始時に列 index だけ保存するが、処理後に設定し直していない。
また resize は DataGridView の列を作り直す。このため P6-005 で current cell を計算側の
出力に含めず、**UI 側が resize 後に現在保持している位置を現行値として維持する**。
特定 index への新しい移動保証を追加する場合は別仕様とする。

## 7. 列削除 (`deleteCellToolStripMenuItem_Click`)

処理順は次のとおりである。

1. 操作開始時の current column を `col` として保存する。
2. `col` から末尾の一つ前まで昇順に、列ごとに
   **使用数 → ヘッダー → row 0..R-1 の値** の順で `i+1` から `i` へコピーする。
3. 列数を `C - 1` に変更する。resize は先頭 `C-1` 列だけを退避・復元するため、
   コピー元だった旧末尾列はここで捨てられる。
4. `adjustWindowSize`、Undo/Redo 履歴の flush、copy buffer 初期化を順に行う。

| ID | 対象 | 入力列 `[header: values; used]` | 最終列 |
|---|---|---|---|
| CD-01 | 先頭 `col=0` | `A:[A0..;a], B:[B0..;b], C:[C0..;c]` | `B:[B0..;b], C:[C0..;c]` |
| CD-02 | 中間 `col=1` | 同上 | `A:[A0..;a], C:[C0..;c]` |
| CD-03 | 末尾 `col=C-1` | 同上 | `A:[A0..;a], B:[B0..;b]`（コピーなし） |

列数は常に 1 減り、行数は不変である。履歴、copy buffer、関連範囲、描画、current cell の
境界は列挿入と同じであり、current cell の明示復元はない。

## 8. 入力防止の現状と calculator 契約

### 8.1 現状の防止箇所

- 通常の行編集は `selectRange.Top` / `Height` または登録済み `Range` から入力される。
  `insertToAllCell` / `cutToAllCell` 自身は `Row` / `Count` を検証しない。
- 列編集は `dataGridView1.CurrentCell.ColumnIndex` を使うため通常 UI では既存列を指すが、
  handler 自身は current cell の null、列数の上限、削除後の列数下限を検証しない。
- 「セル枚数の指定」には `0 < count <= CellCountLimit` の検証がある一方、列挿入 handler は
  `CellCountLimit` を参照せず `C + 1`、列削除 handler は無条件に `C - 1` を resize へ渡す。

### 8.2 P6-002/P6-003 に渡す最小契約

行 calculator は `rowCount > 0`, `columnCount > 0`, `Row >= 0`, `Count > 0`,
`Row <= rowCount`, `Count <= rowCount - Row` を満たす入力だけを受ける。セル snapshot は
`0 <= col < columnCount`, `0 <= row < rowCount` の全座標を読み取り可能で、値は非 null とする。
違反時は変更一覧を一部も返さず、host は一件も適用しない。

`Row == rowCount` は `Count > 0` と両立しないため拒否する。移動長 0 は有効であり、
シフトを生成せずクリアだけを返す。列 calculator も同様に、正の行列数、既存の対象列、
挿入後/削除後の有効列数を前提とし、上限/下限の UI 方針は host 側で判定する。

## 9. 仕様変更候補（本タスクでは変更しない）

1. 不正な `Row` / `Count` が private メソッドへ到達すると、負の移動長、範囲外読み取り、
   範囲外 `Rect` のいずれかになり、事前に原子的拒否されない。calculator 導入時は 8.2 の
   契約で全件を検証してから適用する。
2. 列挿入は `CellCountLimit` を超え得る。列削除は最小列数で `C - 1` を要求し得る。
   UI で警告するか無操作にするかは別タスクで決める。
3. current cell を明示復元していないため、DataGridView 再構築への依存がある。
   対象列維持、挿入列選択、隣接列選択のどれを正式仕様にするかは別タスクで決める。
4. 行編集の Undo group は直後の `flushUndoHistory` により通常のショートカット/範囲経路では
   利用不能になる。抽出中はこの差を維持し、Undo 対応へ変える場合は独立した仕様変更とする。
5. `calcNakanukiRange` / `calcKiribariRange` は交差範囲、負座標、シート末尾超過を補正しない。
   range 正規化は行シフト計算から分離して検討する。

## 10. P6-002 以降への固定 Gate

1. RI-01〜RI-05、RD-01〜RD-05 の座標、値、列優先順、方向別順序、最後のクリアを一致させる。
2. 行編集の変更一覧を全件検証してから、既存と同名の 1 Undo group で適用する。
3. CI-01〜CI-03、CD-01〜CD-03 で値、ヘッダー、使用数、列数と処理順を一致させる。
4. UI host に残す resize、window 調整、履歴 flush、copy buffer 初期化、関連範囲補正、
   選択移動、再描画を calculator へ持ち込まない。
5. 9 節の候補を互換 fixture に黙って混ぜず、変更する場合は独立したタスクとテスト ID を持つ。

