# P6-005 — 列編集計算の抽出

## 抽出した境界

`SheetColumnEditCalculator` は行数、変更前の列数、対象列と、セル値・ヘッダー・使用数の
読み取り delegate だけを受け取る。結果は destination 列ごとの `ColumnWriteEntry` であり、
使用数、ヘッダー、全行の値を一つの snapshot として保持する。`Form1`、WinForms、model、
Undo、設定値には依存しない。

列単位にまとめることで、host は従来どおり各 destination に
**使用数 → ヘッダー → row 0..R-1 の値**を適用できる。挿入は source 列を末尾から、削除は
先頭から列挙するため、列間の適用順も characterization の CI/CD fixture と一致する。
null のセル値とヘッダーは空文字へ正規化する。

## resize と snapshot の順序

### 挿入

1. 変更前の列数と対象列を保存する。
2. `resizeDataGridView1(C + 1, R)` を行う。resize は旧 `C` 列を同じ index へ復元する。
3. 復元済みの旧列から calculator が snapshot を作る。
4. 末尾から右シフトする結果と対象列の空 snapshot を適用する。
5. 履歴と copy buffer を従来どおり初期化する。

### 削除

1. resize 前の `C` 列が揃っている状態で calculator が snapshot を作る。
2. 対象列より右側を左詰めする結果を適用する。
3. `resizeDataGridView1(C - 1, R)` で旧末尾列を捨てる。
4. window 調整、履歴と copy buffer の初期化を従来どおり行う。

この順序により、挿入時は新しい destination が存在してから適用し、削除時は必要な source が
失われる前に読み取る。`adjustWindowSize`、current cell、履歴、copy buffer は UI orchestration
として `Form1` に残す。列数上限・下限の UI 方針も P6-001 の仕様変更候補のままとする。

## fixture

calculator fixture は先頭・中間・末尾について、destination の順序、値、ヘッダー、使用数、
挿入列の空白化、削除末尾で書き込みが不要なことを確認する。また、不正な行列数・対象列・
null reader が snapshot 読み取り前に拒否されることを確認する。
