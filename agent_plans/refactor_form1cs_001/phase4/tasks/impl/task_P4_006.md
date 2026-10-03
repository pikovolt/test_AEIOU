# task_P4_006

## 目的
キーボード経路の修飾キー判定を `KeyEventArgs` 由来の情報へ単一路線化し、イベント処理中にグローバルな押下状態を再取得する経路をなくす。

## 対象
- `AEIOU/WindowsFormsApplication1/GridKeyCommandDispatcher.cs`
- `AEIOU/WindowsFormsApplication1/GridShortcutRouter.cs`
- `AEIOU/WindowsFormsApplication1/Form1.Shortcut.cs`
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_006.md`
- `agent_plans/refactor_form1cs_001/phase4/reports/impl_done/P4_006.md`

## 非対象
- マウスドラッグ中の `Ctrl` 判定（現在押下状態を継続利用する）
- `Shift+Delete` の性能改善（`P4-007`）
- キーバインド変換仕様の変更

## 依存タスク
- `P4-005`

## 完了条件（Gate）
- 矢印キーの `Shift` 判定が `KeyDown` イベント由来の値として Dispatcher、Router、Handler の一本道で伝搬し、`Form1.Shortcut` が `Control.ModifierKeys` を参照しない。

## 回帰観点
- `../../checklists/smoke_phase4.md` の「入力経路」「移動/選択」。
