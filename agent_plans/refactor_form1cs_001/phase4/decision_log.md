# Phase4 Decision Log

## ルール
- 迷いがあった設計判断のみ記録する。
- 最低限「候補」「採用案」「理由」「影響範囲」を残す。

---

## P4-DEC-001
- 日付: 2026-03-02
- 候補:
  - A: 現行 Router + Shortcut 構成を維持し微修正のみ
  - B: コマンド層（Dispatcher + MoveSelectionCommand）を導入して入力分離を完了
- 採用案: B
- 理由: 入力責務の境界を明確化し、`Form1.Shortcut` の直接UI操作を段階的に縮小するため。
- 影響範囲: `phase4/tasks/impl/` のタスク分割、`Form1.cs`・入力系クラス。

## P4-DEC-002
- 日付: 2026-03-02
- 候補:
  - A: Phase4 Gateに性能KPI（P95<=25ms）を含める
  - B: Phase4 Gateは機能回帰中心とし、性能最適化は次段で扱う
- 採用案: B
- 理由: 本フェーズの目的を入力責務分離に集中させ、変更リスクを小さく分割するため。
- 影響範囲: `phase4/checklists/smoke_phase4.md` の必須観点、タスク完了条件。

## P4-DEC-003
- 日付: 2026-03-02
- 候補:
  - A: Phase3 の最終判定を `phase3/checklists/smoke_phase3.md` 側で正本化
  - B: Phase3 の最終判定を `phase3/reports/impl_done/smoke_phase3.md` 側で正本化
- 採用案: B
- 理由: 完了報告として最終確認項目が集約されており、Phase4 参照時の解釈を一意にしやすいため。
- 影響範囲: `phase3/checklists/smoke_phase3.md` の判定状態整合、`phase3/tasks/impl/task_P3_004.md` の繰越注記。

## P4-DEC-004
- 日付: 2026-03-02
- 候補:
  - A: `task_P3_004` の新規2ファイルを Phase3 実装済みとして扱う
  - B: 新規2ファイル未作成を明記し、Phase4 `P4-003` / `P4-004` へ移管する
- 採用案: B
- 理由: 実ファイル構成との不一致を解消し、タスク定義と実体の齟齬を残さないため。
- 影響範囲: Phase3 タスク定義、Phase4 実装順依存（`overview.md`）の参照整合。

## P4-DEC-005
- 日付: 2026-03-02
- 候補:
  - A: `P4-002` で最小コードフックまで実施する
  - B: `P4-002` は入力コマンド面の定義固定（文書更新）のみに限定する
- 採用案: B
- 理由: 依存順を守って `P4-003` 以降の実装前提を先に固定し、1タスク1PRの変更規模を維持するため。
- 影響範囲: `phase4/tasks/impl/task_P4_002.md`、`phase4/checklists/smoke_phase4.md`、`phase4/reports/impl_done/P4_002.md`。

## P4-DEC-006
- 日付: 2026-03-02
- 候補:
  - A: 修飾キー判定を全経路で単一情報源に即時統一する
  - B: `P4-002` 時点では入力チャネル別に判定源を定義し、`P4-006` で単一路線化を最終判断する
- 採用案: B
- 理由: Ctrlドラッグ中の押下/離しによるコピー/移動の即時切替を維持しつつ、キーボード経路の整理を段階実施するため。
- 影響範囲: `phase4/checklists/smoke_phase4.md` の入力経路観点、`P4-006` の設計論点。

## P4-DEC-007
- 日付: 2026-03-02
- 候補:
  - A: `GridShortcutRouter` を現状維持し、Dispatcherと長期併存する
  - B: `GridShortcutRouter` は段階的に置換し、Dispatcherへ責務移管する
- 採用案: B
- 理由: 入力解釈と実行責務をコマンド層へ寄せる Phase4 目的に一致し、`Form1.Shortcut` の縮小方針とも整合するため。
- 影響範囲: `P4-003`（KeyCommandDispatcher導入）、`P4-004`（MoveSelectionCommand導入）の実装境界。

## P4-DEC-008
- 日付: 2026-03-02
- 候補:
  - A: `P4-004` で移動系と値入力系を同時にコマンド化する
  - B: `P4-004` は移動/範囲選択系の責務移管に限定し、値入力系は `P4-005` に分離する
- 採用案: B
- 理由: 1タスク1PR・変更ファイル3〜5の運用制約を維持しつつ、`Ctrl` / `Shift` を伴う移動回帰の確認範囲を明確にするため。
- 影響範囲: `phase4/tasks/impl/task_P4_004.md` の対象/非対象、`P4-005` の着手境界、`phase4/checklists/smoke_phase4.md` の「移動/選択」確認順序。

## P4-DEC-009
- 日付: 2026-03-02
- 候補:
  - A: `P4-004` 着手前に追加の設計文書を作成してから実装へ進む
  - B: 既存SSOT + `preflight_P4_004.md` の確認完了をもって着手可と判定する
- 採用案: B
- 理由: 必要な境界条件（入力経路、移動/選択、`selectRange` / `isFirstEdit` 連携、非対象範囲）が文書上で確認済みであり、これ以上の事前分割は実装価値を増やさないため。
- 影響範囲: `phase4/checklists/preflight_P4_004.md`、`phase4/tasks/impl/task_P4_004.md`、`P4-004` 実装着手可否判断。

## P4-DEC-010
- 日付: 2026-03-02
- 候補:
  - A: `Shift+Delete` 遅延対策を `P4-006` に内包して同時対応する
  - B: `P4-006` は修飾キー判定単一路線化に集中し、性能改善は `P4-007` として分離する
- 採用案: B
- 理由: 1タスク1PR・主要Gate 1つの運用を維持し、仕様整合（入力判定）と性能改善（バッチ更新最適化）の検証軸を分離するため。
- 影響範囲: `phase4/overview.md` の実行順、`phase4/tasks/impl/task_P4_007.md` の新設、`phase4/checklists/smoke_phase4.md` の後段TODO参照。
