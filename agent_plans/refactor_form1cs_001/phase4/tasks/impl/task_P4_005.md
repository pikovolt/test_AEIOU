# task_P4_005

## 目的
数字/空セル/加減算入力の実行責務を `Form1` から専用コマンドへ移し、値入力経路を明確化する。

## 対象
- `AEIOU/WindowsFormsApplication1/GridValueInputCommand.cs`（新規）
- `AEIOU/WindowsFormsApplication1/Form1.cs`
- `AEIOU/WindowsFormsApplication1/Form1.Shortcut.cs`
- `AEIOU/WindowsFormsApplication1/AEIOU.csproj`
- `agent_plans/refactor_form1cs_001/phase4/tasks/impl/task_P4_005.md`

## 非対象
- BackSpace/Delete の削除責務
- `*` / `/` の範囲選択責務
- 修飾キー判定の単一路線化（`P4-006`）
- 値入力の既存仕様変更

## 依存タスク
- `P4-004`（完了済）

## 完了条件（Gate）
- 数字、空セル、加算、減算の入力が `GridValueInputCommand` 経由で実行される。
- 上段/テンキー数字の正規化、入力後の選択移動、スクロール、および `IsKaraNoMove` の既存挙動が維持される。

## 回帰観点
- `../../checklists/smoke_phase4.md` の「入力経路」「値入力」「移動/選択」。

## 実装方針
- `GridCellValueService` はセル値変更に専念させ、キー正規化と入力後の選択移動を `GridValueInputCommand` に集約する。
- `Form1.Shortcut` はフォーム状態の受け渡しと編集フラグ更新のみを担当する。
- `P4-004` で導入した移動コマンドおよび Router/Dispatcher の経路は変更しない。
