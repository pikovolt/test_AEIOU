# Smoke Checklist: Phase6（シート編集計算の分離）

## 自動検証 — 変更一覧
- [x] 行挿入: 先頭/中間/末尾、1行/複数行で期待する座標・値・順序になる（calculator fixture）
- [x] 行削除: 先頭/中間/末尾、1行/複数行で期待する座標・値・順序になる（calculator fixture）
- [x] 移動長0では不要なシフトを書き出さず、必要なクリアだけを生成する（calculator fixture）
- [x] null は境界仕様どおり正規化または拒否される（calculator fixture）
- [x] 重複座標・範囲外座標を適用前に拒否し、部分更新しない（batch validation fixture）
- [x] calculator は WinForms、`Form1`、`DataGridView`、Undo 実装を参照しない（project/source dependency check）
- [x] 列挿入: 先頭/中間/末尾で値、ヘッダー、使用数、適用順、挿入列のクリアが一致する（calculator fixture）
- [x] 列削除: 先頭/中間/末尾で値、ヘッダー、使用数、適用順が一致する（calculator fixture）
- [x] 列 calculator は不正入力を snapshot 読み取り前に拒否し、null 文字列を空文字へ正規化する（calculator fixture）

## 行編集 — UI/Undo
- [ ] 行挿入を Undo/Redo して値、空白領域、表示が往復する
- [ ] 行削除を Undo/Redo して値、末尾空白、表示が往復する
- [ ] メニューおよび `Shift+Insert` / `Shift+Delete` の既存経路が動作する
- [ ] 切り貼り範囲の設定/解除後に `addRange` が正しい位置・長さになる
- [ ] 中抜き範囲を伴う編集後に `delRange` が正しい位置・長さになる
- [x] 継続記号、選択範囲、current cell の表示が退行しない

2026-10-04 の実機確認では、行挿入・削除の先頭/中間/末尾/複数行と
`Shift+Delete` が正常だった。通常の UI 経路は編集後に履歴を flush する既存仕様のため、
Undo/Redo 項目は未確認のままとする。`Shift+Insert`、`addRange`、`delRange` も未実施。

## 列編集
- [ ] 先頭/中間/末尾への列挿入で列数、値、ヘッダー、使用数が一致する
- [ ] 先頭/中間/末尾の列削除で列数、値、ヘッダー、使用数が一致する
- [x] 列挿入・削除後のウィンドウサイズと current cell が既存仕様どおりである
- [ ] 列挿入・削除後に Undo 履歴と copy buffer が既存仕様どおり初期化される
- [ ] 列数の下限/上限付近で範囲外アクセスまたは不正な列数にならない

2026-10-04 の実機確認では、列挿入・削除の先頭/中間/末尾で値の移動が正常で、
Undo 履歴の初期化も確認した。ヘッダー、使用数、copy buffer は未確認なので、これらを
含む複合項目は完了扱いにしない。

## 保存・周辺機能
- [ ] 編集後の保存/再読込で値とヘッダーが再現する
- [ ] Automation の組み込みコマンドが編集後のシートに対して従来どおり動作する
- [ ] STS 読み書きおよび AE コピーの対象範囲が退行しない

## 性能
- [ ] 大量行の挿入・削除について移管前後の条件、変更件数、単発/連続時間を記録する
- [ ] 変更一覧の一時メモリが許容不能な場合は測定値を残し、適用方式変更を別タスク化する

## 実施記録
- 実施日: 2026-10-04
- 実施者: kanbara
- 環境: Windows 11 25H2 / Visual Studio 2022
- 対象コミット: `e881d40238795c41efcca7c442f2b238a9f613a5`
- ビルド結果: Debug x86 / Release x86 ともに PASS（既存警告あり、エラーなし）。
- 自動テスト結果: Debug x86 / Release x86 ともに
  `All automation host tests passed.`
- Windows UI smoke 結果: 行の先頭/中間/末尾/複数行の挿入・削除、
  `Shift+Delete`、継続記号、選択範囲/current cell、列の先頭/中間/末尾の挿入・削除、
  ウィンドウサイズ/current cell、列編集後の Undo 履歴初期化は PASS。
- 未実施項目と理由: `Shift+Insert`、`addRange`、`delRange`、列ヘッダー/使用数、
  copy buffer、保存/再読込、Automation UI、STS、AE コピー、性能、列数下限/上限は未実施。
  行編集の Undo/Redo は通常 UI 経路で既存仕様どおり履歴が flush されることを観測したが、
  group 単体の往復は未確認。
