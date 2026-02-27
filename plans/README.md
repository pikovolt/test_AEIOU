# plans ディレクトリ運用ガイド

このディレクトリは、機能追加・リファクタリング・調査の**実行計画ドキュメント**を集約するための場所です。

> 設計・仕様の最上位SSOTはリポジトリ直下の `architecture.md` とし、`plans/` はそれを具体化・実行するレイヤとする。

## 推奨ディレクトリ構成

```text
plans/
  README.md
  <workstream_key>/
    architecture.md
    plan_YYYYMMDD_001.md
    plan_YYYYMMDD_002.md
    notes_YYYYMMDD.md
    decision_log.md
```

- `workstream_key`: 計画対象を短く表す識別子（例: `refactor_form1cs_001`）
- `architecture.md`: 該当タスクの背景・目的・設計方針
- `plan_YYYYMMDD_001.md`: 実装・検証手順を記載した実行計画（同日複数なら連番を上げる）
- `notes_YYYYMMDD.md`: 調査メモや一時的な検証結果
- `decision_log.md`: 採用/却下した設計判断を時系列で管理

## 命名規約（推奨）

### 1) ワークストリームディレクトリ名

`<type>_<target>_<id>` 形式を推奨。

- `type`: `refactor` / `feature` / `investigation` など
- `target`: 対象を簡潔に（例: `form1cs`, `virtualmode`）
- `id`: 3桁連番（`001`, `002` ...）

例:
- `refactor_form1cs_001`
- `feature_virtualmode_002`

### 2) 実行計画ファイル名

`plan_YYYYMMDD_NNN.md`

- `YYYYMMDD`: 作成日
- `NNN`: 同日内の連番

例:
- `plan_20260227_001.md`
- `plan_20260227_002.md`

## 運用ルール（軽量）

1. 新しい実行を始めるときは、既存のワークストリームを再利用するか、新規作成するかを先に判断する。
2. 仕様/設計の土台が変わる場合は `architecture.md` を更新し、計画ファイル本文から参照する。
3. 作業中の判断は `decision_log.md` に追記し、後から読み返せる状態にする。
4. 計画を実施したら、`plan_...md` のチェック項目を更新して進捗を見える化する。

## 一般的なベストプラクティス（要点）

- **時系列とテーマの二軸で整理**: 「どの案件か」と「いつの計画か」の両方で追える命名にする。
- **設計（architecture）と実行（plan）を分離**: 設計の長期安定性と、実行計画の短期更新性を分ける。
- **意思決定ログを独立**: 「なぜそうしたか」を残すと、将来の再リファクタ時に役立つ。
- **連番を固定幅にする**: ソートで自然順になり、CLI上で扱いやすい。


## SSOTとの関係（重要）

- 全体原則・共通非機能要件: ルート `architecture.md` を正とする
- 個別案件の設計具体化: `plans/<workstream>/architecture.md`
- 日々の実行手順: `plan_YYYYMMDD_NNN.md`
- 判断履歴: `decision_log.md`

重複が発生した場合の優先順は次の通りです。

1. `architecture.md`（root）
2. `plans/<workstream>/architecture.md`
3. `plan_YYYYMMDD_NNN.md`
4. `decision_log.md`
