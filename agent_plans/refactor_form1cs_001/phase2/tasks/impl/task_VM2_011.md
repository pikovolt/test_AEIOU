# task_VM2_011: ApplyTimingCellState null条件検証（GAP-H3対応）

## 目的
`GridCellRenderer.ApplyTimingCellState` が `_view[col, row] as TimingCell` を
null チェックしてサイレントリターンする実装になっており、
VirtualMode 環境で `TimingCell` にキャストできない場合に
基準線・カラセルマーク・継続記号がすべて無音で消失するリスクがある。
発生条件を確認し、問題があれば修正する。

## 根拠
- GAP-H3（精査報告 2026-03-01）

## 関連サービス
- `GridCellRenderer`（`ApplyTimingCellState`）
- `TimingColumn`（`CellTemplate` の型）

## 実施内容
- `TimingColumn.CellTemplate` の型を確認し、`TimingCell` と一致することを検証する。
- VirtualMode で `dataGridView1[col, row]` が `TimingCell` 型として返されることを確認する。
- null リターン条件が実際に発生するシナリオ（行追加直後・初期化直後等）を確認する。
- null ルートが発生した場合の影響（描画欠落の実態）を記録する。
- 問題がある場合は以下のいずれかで対処する。
  - `TimingColumn.CellTemplate` の型を保証する修正を加える。
  - null 時の防御的ログ（DEBUG）を追加して無音失敗を検知可能にする。
  - 問題なし（null は実際には到達しない）であれば、その根拠を記録する。

## 完了条件
- null リターン条件の発生有無が実測・コード調査で確定している。
- 問題あり: 修正またはログ追加が完了し、再現なしを確認している。
- 問題なし: 「到達不能」の根拠（TimingColumn.CellTemplate確認）が記録されている。
- いずれの場合も、回帰観点2件が記録されている。

## 依存タスク
- VM2-002

## Done定義参照
- 共通定義 `DONE_TEMPLATE_IMPL.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。
