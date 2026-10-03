# Smoke Checklist: Phase4（入力分離仕上げ）

## 入力経路
- [x] `KeyDown -> Interpreter -> Router/Dispatcher -> Command -> Service` 経路で例外なく処理される
- [x] 修飾キー判定は入力チャネルに応じて一貫する（キーボード: イベント由来 / マウスドラッグ: 現在押下状態）

## 移動/選択
- [x] キーボード移動（上下左右）が既存仕様どおり機能する
- [x] `Shift+↑/↓/←/→` の範囲拡縮が既存仕様どおり機能する
- [x] `*` / `/` による縦方向範囲拡縮が既存仕様どおり機能する
- [x] 行列境界（先頭/末尾）で移動・選択が破綻しない

## 値入力
- [x] 上段数字 `0..9` 入力が既存仕様どおり反映される（テンキーなし環境含む）
- [x] テンキー数字 `0..9` 入力が既存仕様どおり反映される
- [x] 空セル入力（`.`）と縦方向拡縮（`*` `/`）が非退行である

## 回帰観点
- [x] `BackSpace` / `Delete` / `Undo` / `Redo` 往復後に表示とデータが一致する
- [x] マウス選択・ドラッグ操作が退行していない
- [x] 保存/再読込で表示内容が再現される

## 実施記録
- 実施日: 2026-10-03
- 実施者: ユーザー手動確認
- 結果: 入力経路、移動/選択、値入力、回帰観点のすべてで従来からの変化なし。

## 調査メモ
- [x] 旧実装基準コミット `2d0e4cff4e2b36f035cfb016303311b8e0ff4ad0` を確認（`Form1.cs` 単体実装時代）
- [x] 旧実装合わせとして、`Delete` は `Shift+Delete`（範囲削除）/ `Delete`（内容削除）へ復元

## 既知課題（後段対応）
- [ ] `Shift+Delete` の体感遅延（1入力あたり約1秒）
	- 観測条件: 行削除（`Shift+Delete`）時
	- 推定ホットパス:
		- `cutToAllCell -> QueueShiftWrites -> QueueCellWrite(SetValueOperation)` の大量実行
		- 各セル更新で `CellValueChanged` が発火し、`RecalculateColumn` が列全走査で再実行される
		- `InvalidateCell` がセル単位で多数発生する
	- 影響: 入力レスポンス悪化（P95 25ms目標を満たしにくい）

## 後段TODO（性能）
- [ ] TODO-PERF-01: バッチ書き込み中の `CellValueChanged` 起因 `RecalculateColumn` を抑止し、終了時に変更列のみ再計算する
- [ ] TODO-PERF-02: バッチ書き込み中の `InvalidateCell` 連発を抑止し、終了時に集約再描画（列/全体）へ切替える
- [ ] TODO-PERF-03: `Shift+Delete` の前後で処理時間計測（最低: 1回平均 / 10回平均）を取得し、改善量を記録する
