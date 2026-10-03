# Phase6 tasks — 実行計画

## 運用
- 1タスク = 1PR、主要 Gate は1つ、原則3〜5変更ファイルを維持する。
- 各実装 PR は `../checklists/smoke_phase6.md` の該当項目を更新する。
- 挙動変更を伴う修正は抽出と同じPRに混ぜず、現行互換の fixture を先に置く。
- calculator に `Form1`、WinForms control、`GridViewManager`、UndoManager、`Settings` を渡さない。

## P6-001: 行・列編集 characterization
**目的:** 抽出前の暗黙仕様を、計算結果と UI 後処理に分けて固定する。

**作業:**
- 行挿入・削除について、先頭/中間/末尾、件数1/複数、移動長0の変更座標・値・順序を記録する。
- 列挿入・削除について、値、ヘッダー、使用数、列数変更、current cell、履歴 flush、copy buffer の期待値を記録する。
- `delRange` / `addRange` から呼ばれる行編集と、メニュー/ショートカットから呼ばれる行編集を区別する。
- 不正な Row/Count が現状どこで防止されるかを確認し、calculator の入力契約を決める。

**Gate:** 4経路それぞれに正常系、境界、変更順、Undoまたは履歴初期化、再描画の期待値があり、仕様変更候補が別記されている。

## P6-002: 内部変更一覧契約
**目的:** 計算側が返し、適用側が消費できる本体内部の最小型を確定する。

**作業:**
- `CellWriteEntry` を `Form1` の private struct から独立した内部型へ移す。
- calculator が必要とする読み取り専用セル入力を、delegate または小さな snapshot として定義する。
- null 値の空文字正規化、重複座標、範囲外座標をどちらの境界で拒否するかを明文化する。
- Automation の公開 DTO を再利用しない理由と、将来統合の判断条件を残す。

**Gate:** 新しい内部型が WinForms と `Form1` を参照せず、既存の `QueueCellWrites` / `ApplyCellWrites` が同じ結果を適用できる。

## P6-003: 行編集 calculator 抽出
**目的:** 行挿入・削除のシフトおよび空白化を純粋計算にする。

**作業:**
- `SheetRowEditCalculator.CreateInsertRows` と `CreateDeleteRows` 相当を追加する（最終名称は実装時に確定）。
- 下方向シフトは末尾から、上方向シフトは先頭からという既存順序を fixture で固定する。
- 挿入範囲および削除後末尾のクリアも同じ変更一覧に含める。
- `QueueShiftWrites` の利用箇所を移行し、不要になった時点で削除する。

**Gate:** calculator 単体で先頭/中間/末尾、1行/複数行、移動長0を検証でき、生成座標が常にシート内である。

## P6-004: 行編集 orchestration の縮小
**目的:** `Form1` の行編集を「計算、1 group適用、UI後処理」に限定する。

**作業:**
- `insertToAllCell` / `cutToAllCell` が calculator の結果を `QueueCellWrites` 経由で適用するよう変更する。
- 変更一覧全体を適用前に検証し、不正時に部分適用しない。
- Undo group 名「行の挿入」「行の削除」、`FinishWriteOperation`、Invalidate の現行差を維持または characterization に基づいて統一する。
- `deleteRect` を同一 group 内で呼ぶ必要がなくなった場合も、`isFirstEdit` と継続表示への影響を確認する。

**Gate:** 行挿入・削除が1 Undo groupで往復し、関連範囲操作とショートカットを含む smoke が移管前後で一致する。

## P6-005: 列編集 calculator 抽出
**目的:** 列挿入・削除のデータ移動計算を UI の列数変更から分ける。

**作業:**
- 値とヘッダーの移動、挿入列のクリアに必要な変更を計算する。
- 挿入では resize 後、削除では resize 前に必要な snapshot/適用がある現行順序を明示する。
- `CopyColumn` / `ClearColumn` のうち計算に属するループを移し、使用数の更新主体を一つにする。
- `adjustWindowSize`、履歴 flush、copy buffer 初期化は `Form1` に残す。

**Gate:** 先頭/中間/末尾の列挿入・削除後に、列数、値、ヘッダー、使用数が一致し、保存再読込でも再現する。

## P6-006: 回帰確定と次段階判断
**目的:** 分離の成果を測定し、適用処理まで外へ出す価値があるかを決定する。

**作業:**
- Phase6 smoke、Debug/Release x86 build、利用可能な自動テスト結果を記録する。
- `Form1` から消えた計算依存と、意図して残した UI/orchestration 依存を一覧化する。
- 大量行の挿入・削除について移管前後の時間と変更件数を比較し、著しい退行がないことを確認する。
- `SheetEditService` 導入、適用 host 共通化、追加編集機能の移管は次Phase候補として判定し、Phase6へ便乗させない。

**Gate:** checklist の必須項目と実測記録が揃い、残件が完了扱いと混在せず次段階へ引き渡されている。
