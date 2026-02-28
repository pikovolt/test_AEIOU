# PRE v2 分割定義（検討結果）

## 背景
従来のPRE定義は1ファイルあたりの条件量が多く、レビュー・着手判定・保守の負荷が高い。
本v2は「必須確認を単一論点ごとに分割」するための再編案である。

## 分割原則
1. 1ファイル1判定（1つのYes/Noで完了判定できること）。
2. MUST（実装ブロッカー）だけをPRE本体に残す。
3. 運用詳細（ログ形式、命名規約、N/A承認運用など）は補助文書へ分離する。
4. PRE→IMPLの接続はユニットIDで機械判定可能にする。

## v2ユニット一覧（重要度順）
- P0-01: `unit_PRE_P0_01_virtualmode_startup.md`
- P0-02: `unit_PRE_P0_02_cellvalue_contract.md`
- P0-03: `unit_PRE_P0_03_boundary_separation.md`
- P1-01: `unit_PRE_P1_01_event_mapping_consistency.md`
- P1-02: `unit_PRE_P1_02_service_extraction_architecture.md`
- P1-03: `unit_PRE_P1_03_regression_risks.md`
- P1-04: `unit_PRE_P1_04_impl_handover_minimum.md`

## 運用方針
- 本ディレクトリをPRE定義の正本として運用する。
- 新規更新・追加レビューはPRE v2ユニットを優先する。


## SSOT / Log / Worklog
- SSOT: `PRE_SSOT.md`
- decision log: `PRE_decision_log.md`
- worklog: `PRE_worklog.md`
