# Phase5 Smoke Checklist

## A. 分離前 characterization（P5-001）
- [x] 連番: 正/負 step、skip on/off、空セル上書き、選択末尾。
- [x] 置換: 一致/不一致/空セル、空の置換前・置換後。
- [x] 反転: 全入力、空セル混在、入力1件、入力0件。
- [x] 四則演算: `+ - * /`、負数、カラセル、空セル、セル値0、除数0、非数値セル。
- [x] 繰り返し: 挿入値あり/なし、skip、loop、複数列、シート末尾。
- [x] モデルレス繰り返し: 初回、連続実行、閉じて再起動、Undo/Redo。

P5-001 の期待値、再現手順、および互換維持/別修正の判断は
[`../characterization_p5_001.md`](../characterization_p5_001.md) に固定した。

## B. Automation host
- [x] 正常な変更セットが1 write groupで全件適用される（in-memory prototype）。
- [x] 空の変更セットで Undo 履歴と編集状態を不要に変更しない（in-memory prototype）。
- [x] 範囲外セルを1件含む結果を全件拒否する（in-memory prototype）。
- [x] 同一セルへの重複変更を全件拒否する（in-memory prototype）。
- [x] null値、上限超過、シート世代不一致を全件拒否する（in-memory prototype）。
- [x] command の例外発生時にセル変更が0件である（in-memory prototype）。
- [ ] 適用後のセル使用数、継続表示、再描画が既存経路と一致する。

## C. 組み込み6機能
- [ ] 各メニューの表示名、ショートカット、ダイアログ初期値が変わらない。
- [ ] 置換、反転、四則演算が registry/host 経由で動く。
- [ ] 連番、繰り返しが registry/host 経由で動く。
- [ ] 各操作の Undo 1回/Redo 1回で表示とモデルが一致する。
- [ ] 繰り返し再実行が、当該 session の直前結果だけを置き換える。

## D. 外部拡張
- [ ] `Extensions` が存在しない/空でも正常起動する。
- [ ] 正常 DLL の command が1回だけ登録される。
- [ ] 非 DLL、非実装型、abstract型を無視する。
- [ ] 契約 major 不一致を拒否する。
- [ ] 組み込み/外部および外部同士の重複 ID を拒否する。
- [ ] 依存 DLL 不足、constructor 例外、実行例外が他コマンドへ波及しない。
- [ ] 不正な変更セットを返す外部 command がシートを変更できない。
- [ ] 外部 DLL を除去して再起動すると登録が消え、組み込み機能は維持される。

## E. 非退行
- [ ] STS 読込/保存。
- [ ] AE 通常/ダイレクト/スクリプトコピーと AE ペースト。
- [ ] キー入力、移動、範囲選択、行削除。
- [ ] Debug x86 / Release x86 ビルド。
