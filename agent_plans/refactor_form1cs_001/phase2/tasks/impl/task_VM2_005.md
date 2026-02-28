# task_VM2_005: CurrentCell/Selection整合（移動回帰）

## 目的
CurrentCell と Selection の整合を保ち、移動・選択操作の回帰を抑止する。

## 実施内容
- CurrentCell更新タイミングを整理する。
- Selection更新と描画反映の順序を固定する。
- キーボード/マウス操作での移動回帰観点を確認する。

## 完了条件
- セル移動時の選択状態が破綻しない。
- 既存ショートカット操作が維持される。
- `VM2-006`/`VM2-007` に必要な状態遷移条件が明記されている。

## 依存タスク
- VM2-004

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
