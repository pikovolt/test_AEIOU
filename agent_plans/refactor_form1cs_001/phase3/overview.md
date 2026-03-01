# Phase 3 Overview

## 全体目的
`Form1.cs` の残存責務を分離し、以下を達成する。

- 描画責務の分離（CellPainting系のForm1依存を削減）
- 入力責務の分離（KeyDown巨大分岐を段階分離）
- VirtualMode 前提の回帰安全性を維持したまま、保守容易性を高める

## Phase3 スコープ
- 対象:
  - `CellPainting` 周辺の描画オーケストレーション
  - `calcBorderState` / `drawFrameNumber` 周辺ロジック
  - `dataGridView1_KeyDown` の入力分岐とルーティング
- 非対象:
  - 大規模設計刷新（アプリ全体再設計）
  - 既存機能仕様の追加・変更

## 実行順（依存）
1. **P3-001**: 描画ヘッダ描画分離（起点）
2. **P3-002**: 境界判定分離（P3-001依存）
3. **P3-003**: KeyDown前段ルータ分離（起点）
4. **P3-004**: 選択移動コマンド化（P3-003依存）
5. **P3-005**: 数値入力/空セル入力の操作分離（P3-004依存）

## 依存ルール
- 1タスク = 1PR
- 1タスクの変更ファイル数は 3〜5 を上限とする
- 各タスクは主要Gateを1つだけ持つ
- 回帰確認は `checklists/smoke_phase3.md` に統一する

## 完了条件
- `tasks/impl/` 配下タスクが順次完了し、`reports/impl_done/` に Done 証跡が存在すること
- `smoke_phase3.md` の必須観点が [x] になること
- Phase3 の重大方針変更が `decision_log.md` に記録されていること
