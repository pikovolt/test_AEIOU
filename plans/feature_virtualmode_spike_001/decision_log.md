# VirtualMode スパイク decision log

- ワークストリーム: `feature_virtualmode_spike_001`
- 目的: 実装中の重要判断（採用/却下、順序変更、方針転換）の根拠を記録する。

## 2026-02-28

### D-001: ドキュメント分割運用に decision_log を追加
- 判断: SSOT (`plan_20260228_001.md`) / 詳細化 (`workplan`) / 実施記録 (`worklog`) に加え、判断根拠は `decision_log.md` に集約する。
- 理由: 進捗ログと判断根拠を分離し、後続フェーズで「なぜその順序・方針か」を追跡可能にするため。
- 影響範囲: `feature_virtualmode_spike_001` 配下の運用ドキュメント。
- 見直し条件: 実装開始後に判断の粒度が不足/過多と判明した場合、テンプレート項目を更新する。

### D-002: 重複抑制のため文書責務を再整理
- 判断: `workplan` は依存/順序/リスクのみ、`worklog` は実測・事実のみを記録し、目的/スコープ/KPI定義の重複を削除する。
- 理由: 情報重複による更新漏れと管理負荷の増大を防ぐため。
- 影響範囲: `workplan_20260228_001.md`, `worklog_20260228_001.md`, `plan_20260228_001.md` の運用記述。
- 見直し条件: 参照だけでは理解困難なレビュー指摘が継続した場合、最小限の要約追記を再検討する。

### D-003: PR精査時の編集整合を補正
- 判断: `worklog` の関連判断参照を `D-001` のみから `D-001, D-002` に修正する。
- 理由: 2026-02-28の実施内容には重複抑制の責務再整理（D-002）も含まれるため。
- 影響範囲: `worklog_20260228_001.md` の時系列ログ1件。
- 見直し条件: 参照ID運用を変更する場合は、過去ログを同一規約へ正規化する。

### D-004: 計測終了フックは `CellPainting` 側 + `Application.DoEvents` を採用
- 判断: 入力遅延の1サンプル単位を `KeyDown` 開始〜 `CellPainting` 到達後（`Application.DoEvents` 実行後）で計測する。
- 理由: Step 1の要件である「入力イベント開始〜画面更新完了寄りの地点」までを、既存ロジックへの侵襲を抑えて取得するため。
- 影響範囲: `Form1.cs` の `dataGridView1_KeyDown`, `dataGridView1_CellPainting`, `InputLatencyProbe`。
- 見直し条件: 手動計測でサンプル欠損・過剰が判明した場合、終了フック候補（`CellValueNeeded` 等）を再評価する。

### D-005: VirtualMode疎通はダミー2次元配列を中核に採用
- 判断: スパイク段階では `string[,]` を VirtualMode の唯一の読み書き源として扱い、`CellValueNeeded` / `CellValuePushed` で直接参照・更新する。
- 理由: SSOT Step 2-3の目的（接続性確認）を最小実装で満たし、実モデル結合の不確実性を切り離すため。
- 影響範囲: `Form1.cs` の VirtualMode設定、ダミーデータ準備処理、値取得/更新イベント。
- 見直し条件: Phase移行時に `TimingSheetModel` 直結へ進む場合、ダミーデータ層の撤去または `#if DEBUG` 化を判断する。


### D-006: 計測未開始時は `Application.DoEvents` をスキップ
- 判断: `TryEndInputLatencyMeasurement` に `HasPendingSample` 判定を追加し、未計測状態では `Application.DoEvents` を実行しない。
- 理由: 入力計測サンプルが存在しない描画サイクルで `DoEvents` が再入を誘発し、計測対象外の副作用を増やすリスクを抑えるため。
- 影響範囲: `Form1.cs` の `InputLatencyProbe` / `TryEndInputLatencyMeasurement`。
- 見直し条件: 手動検証で計測漏れが増える場合は終了フック設計（`CellPainting` 以外）を再評価する。
