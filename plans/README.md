# plans ディレクトリ運用ガイド

このディレクトリは、機能追加・リファクタリング・調査の**実行計画ドキュメント**を集約するための場所です。

> 最上位SSOTはリポジトリ直下の `architecture.md`。`plans/` はその具体化・実行レイヤとする。

## 推奨ディレクトリ構成

```text
plans/
  README.md
  <workstream_key>/
    architecture.md
    plan_YYYYMMDD_NNN.md
    notes_YYYYMMDD.md
    decision_log.md
```

- `workstream_key`: `<type>_<target>_<id>`（例: `refactor_form1cs_001`）
- `plan_YYYYMMDD_NNN.md`: 起点日 + 改稿番号（3桁）
- `architecture.md` は背景/目的/方針、`notes` は一時メモ、`decision_log.md` は採用/却下判断を扱う。

## 命名・改稿ルール（最小）

- IDは固定幅3桁（`001` 開始）。欠番は許容し、再利用しない（例: `feature_virtualmode_002`）。
- plan の軽微修正は同一ファイル更新。
- plan の `NNN` を繰り上げるのは、スコープ変更・フェーズ再編・完了条件再定義などの大改稿時のみ。
- 複数 plan がある場合、**最大 `NNN` を最新版**として扱う。
- 改稿理由は `decision_log.md` に残す（理由・影響範囲・旧版との差分要約を1行で可）。

## 記載粒度ルール（plan / decision_log / worklog）

### plan（`plan_YYYYMMDD_NNN.md`）
- 中心は **Exit Gates（完了条件）**。
- 経路の詳細（細かい手順・試行錯誤）は書きすぎない。

### decision_log（`decision_log.md`）
- **迷いがあった設計判断のみ**記録する。
- 最低限: 「候補」「採用案」「理由」「影響範囲」。

### worklog（`worklog_YYYYMMDD_NNN.md`）
- 原則作成しない（任意）。
- 日々の作業履歴は Git コミット履歴を正とする。
- 例外は、障害再現手順などコミットだけでは追えない事実に限定する。

## ドキュメント同期タイミング

- 小PR方針は維持する。
- `plan` / `decision_log` の更新は、**設計または完了条件が変わった時**を基本とする。
- 実装中の細かな進捗に合わせて機械的に毎回更新しない。
- PR本文には更新対象の plan ファイル名（例: `plan_20260228_001.md`）を明記する。

## ID管理の正式仕様（定義重複防止）

- ID定義元は1箇所に固定する。
  - Workstream ID: ディレクトリ名
  - Plan ID: `plan_YYYYMMDD_NNN.md` のファイル名
  - Decision ID: `decision_log.md` の連番（`D-001`, `D-002` ...）
- 他文書では再定義せず、既存IDを参照のみ。
- 同一対象へ別名ID（例: `001` や `Plan-A`）を付与しない。
- PRレビュー時に、同一対象への重複ID新設がないか確認する。


## 運用背景（短縮版）

- 目的は、実行記録の過剰化を防ぎつつ、判断理由と完了条件だけを監査可能に残すこと。
- 迷った時だけ `decision_log` を残すことで、変更履歴はGit、設計判断は文書という役割分担を維持する。

## SSOT優先順

1. `architecture.md`（root）
2. `plans/<workstream>/architecture.md`
3. `plan_YYYYMMDD_NNN.md`（最大番号を最新版として扱う）
4. `decision_log.md`

## `agent_plans/.../README.md` 向けタスク分割基準

- 1タスクあたりの上限は以下を満たすこと。
  - 変更ファイル数: **3〜5ファイル以内**
  - 完了判定: **主要Gateは1つのみ**
  - 回帰観点: **1チェックリストで完結**
  - 想定実装時間: **半日〜1日**
- 上限超過が見込まれた時点で、対象 `task_VM2_xxx` を分割する。
- 分割時は、親タスク側に子タスクへの依存関係を追加し、実行順序を明示する。
