# P6-006 — 回帰確定と次段階判断

## ステータス

進行中。2026-10-04 の Windows 実機結果を記録したが、性能実測と必須 smoke の一部が
残っているため、Phase6 の Gate はまだ完了扱いにしない。

## 実機回帰結果

環境は Windows 11 25H2 / Visual Studio 2022、対象は
`e881d40238795c41efcca7c442f2b238a9f613a5`、実施者は kanbara。

| 確認 | 結果 | 備考 |
|---|---|---|
| Debug x86 ビルド | PASS | 既存警告あり、エラーなし |
| Release x86 ビルド | PASS | 既存警告あり、エラーなし |
| Automation.Tests Debug x86 | PASS | `All automation host tests passed.` |
| Automation.Tests Release x86 | PASS | `All automation host tests passed.` |
| 行挿入 | PASS（確認範囲） | 先頭/中間/末尾/複数行 |
| 行削除 | PASS（確認範囲） | 先頭/中間/末尾/複数行、`Shift+Delete` |
| 行編集表示 | PASS | 継続記号、選択範囲、current cell |
| 列挿入・削除 | PASS（確認範囲） | 先頭/中間/末尾の値、window size、current cell |
| 列編集後の履歴 | PASS | Undo 履歴の初期化を確認 |

行編集の Undo/Redo が利用できなかったことは、通常のメニュー/ショートカット/関連範囲経路が
行の write group 作成後に履歴を flush する P6-001 の characterization と一致する。
これは今回発見した退行とは扱わない。ただし calculator の結果が作る 1 group 自体の往復を
UI から分離して確認できていないため、checklist の Undo/Redo 項目は未完了のままとする。

## 未完了の回帰項目

- `Shift+Insert`、`addRange`、`delRange`。
- 列ヘッダー、使用数、copy buffer の初期化。
- 列数の下限/上限付近。
- 保存/再読込、編集後の Automation UI、STS、AE コピー。
- 大量行の挿入・削除について、移管前後の条件、変更件数、単発/連続時間。
- 一時変更一覧のメモリが許容範囲かの判定。

未実施項目を推測で PASS にせず、`checklists/smoke_phase6.md` に残す。

## 分離できた責務

### `Form1` から calculator へ移したもの

- 行挿入の下方向シフト座標、末尾からの生成順、挿入領域の空白化。
- 行削除の上方向シフト座標、先頭からの生成順、末尾領域の空白化。
- 行編集入力の事前検証と null 値の空文字正規化。
- 列挿入・削除の source/destination、値・ヘッダー・使用数の snapshot、適用順。
- 列編集入力の事前検証と null 値の空文字正規化。

これらは `SheetRowEditCalculator` / `SheetColumnEditCalculator` にあり、WinForms、`Form1`、
`GridViewManager`、Undo 実装を参照しない。自動 fixture は正常系、端、順序、不正入力を
Windows の Debug/Release x86 の双方で通過した。

### `Form1` に意図して残したもの

- シート寸法、current cell、セル値、ヘッダー、使用数を UI/model から読み取ること。
- calculator の結果を `GridViewManager` と model へ適用すること。
- 行編集の Undo group と、`isFirstEdit`、継続表示、Invalidate。
- 列編集前後の resize、window size 調整、Undo 履歴と copy buffer の初期化。
- `addRange` / `delRange`、選択範囲、保存、STS、AE コピーなどの orchestration。

UI 状態と操作順に依存するため、これらを calculator の constructor や入力へまとめて渡さない。

## 次段階の暫定判定

### `SheetEditService` 導入: 今は見送る

`Form1` の行経路は「入力取得 → calculator → batch 適用 → UI 後処理」、列経路は
「resize/snapshot 順序 → calculator → 適用 → UI 後処理」まで縮小済みである。
現時点では service を追加しても UI、Undo、resize への依存を別クラスへ移すだけになる可能性が
高く、実測前に導入する根拠がない。

### 適用 host 共通化: 条件付き候補

行は `CellWriteEntry` の原子的検証と Undo group、列は metadata を含む
`ColumnWriteEntry` の適用と resize を必要とし、契約が異なる。保存処理や別の編集経路で同じ
適用境界が必要になった場合に、UI 非依存の小さな host interface を先に定義して再評価する。

### 追加編集機能の移管: 個別タスク候補

関連範囲補正などの二重ループを機械的に移すのではなく、`addRange` / `delRange` の実機 smoke と
境界 fixture を先に用意する。列数上下限、行編集後の履歴 flush、current cell の正式仕様も
互換抽出とは分けた仕様変更タスクにする。

## Gate 判定

既存 calculator の回帰について、Debug x86、Debug/Release の Automation tests、主要な行列 UI
操作では退行を示す結果はない。一方で性能実測と未完了 smoke が残るため P6-006 は進行中である。
残件を完了または理由付きで引き渡し、性能比較を記録した後に最終判定を行う。
