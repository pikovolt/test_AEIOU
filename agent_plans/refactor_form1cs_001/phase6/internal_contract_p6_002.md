# P6-002 — 内部変更一覧契約

## 契約

- 計算側は `CellWriteEntry` の順序付き一覧を返す。座標の引数順は `row, column` とし、
  一覧順をそのまま書き込み順として扱う。
- 読み取り入力は `CellValueReader(int row, int column)` に限定する。calculator はこの
  delegate を通して文字列値だけを読み、`Form1`、WinForms control、model、Undo の参照を受け取らない。
- `CellWriteEntry` は受け取った null を空文字へ正規化する。これにより計算結果と既存シートの
  空セル表現を同じ非 null 文字列にする。
- 適用側は Undo group を開始する前に変更一覧全体を検証する。シート範囲外の座標または同じ
  `row, column` の重複があれば例外で拒否し、一件も適用しない。空の一覧は従来どおり no-op とする。
- 行列数、対象位置、件数など calculator 固有の入力条件は各 calculator が一覧生成前に検証する。
  P6-003 の行 calculator は characterization の 8.2 節を入力条件とする。

## Automation DTO と分離する理由

`AutomationChange` は外部拡張 DLL と host の公開契約であり、null を不正値として拒否する。
一方、シート編集の内部契約は既存の空セル表現に合わせて null を空文字へ正規化し、書き込み順も
シフト処理の意味を持つ。内部実装の変更を外部 API の互換性問題にしないため、P6-002 では型を
共有しない。

将来、両方の利用者で null、座標、重複、順序、バージョニングの規則が一致し、公開契約を変更する
便益が互換性コストを上回る場合に限り、共通 DTO への統合を別タスクで判断する。

## 適用経路

`Form1.ApplyCellWrites` は空一覧を除き、`CellWriteBatch.Validate` を通過した一覧だけを既存の
`ExecuteWriteGroup` / `QueueCellWrites` へ渡す。このため既存の書き込み結果と Undo group の構造を
保ちながら、calculator が返す一覧を同じ経路で消費できる。
