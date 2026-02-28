# PRE SSOT（不変要件・事実の正本）

この文書は、PRE定義における**変更しない事実**と**実装前に満たすべき要件**の正本である。

## 1. 変更のない事実（Facts）

### F-01 対象UI基盤
- 対象は `Form1.cs` を中心とした `DataGridView` ベースUIである。

### F-02 段階導入
- Phase2 は段階実装であり、PREは実装前ゲートの役割を持つ。

### F-03 VirtualMode前提
- Phase2の主要論点は `VirtualMode` 対応時の表示/編集/同期の安定化である。

### F-04 責務分離軸
- PREで扱う責務分離軸は「入力」「描画」「値取得/同期」である。

### F-05 Undo/Redo重要性
- Undo/Redo整合は機能品質に直結するため、回帰観点から除外できない。

### F-06 実装着手判定の必要性
- IMPL着手には、事前確認済みであることを示す判定単位が必要である。

### F-07 文書運用の単純性
- PRE判定は、過密な複合条件より「小さい単位・明確な判定」が望ましい。

---

## 2. 必須要件（Requirements）

> 判定値: `PASS/FAIL`（原則2値）

### R-P0（Blocker）

#### R-P0-01 起動表示成立
- `VirtualMode=true` で初期表示崩壊がない。
- 0件データ時に例外なく継続動作する。
- 末尾スクロールで境界例外がない。

#### R-P0-02 値取得契約
- 値取得経路は `CellValueNeeded -> TryGetCellValue` で一意。
- `TryGetCellValue == false` のとき返却値は `string.Empty`。
- 失敗理由（failure reason）を返却可能である。

#### R-P0-03 責務境界固定
- `CellPainting` は描画責務のみ。
- `KeyDown/KeyPress` は入力制御責務のみ。
- 値取得責務は専用経路に集約される。

### R-P1（High）

#### R-P1-01 イベント割当整合
- Phase2対象イベントの割当が単一定義として矛盾しない。
- 対象外イベントの扱いが一貫している。

#### R-P1-02 抽出設計整合
- `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` の責務重複がない。
- UI層（`Form1`）の責務が肥大化しない。

##### R-P1-02 サービス導入方針（固定）
- `GridInputInterpreter`: **新規導入**（入力解釈専用）。
- `GridViewUpdater`: **既存 `GridCellRenderer` 拡張**として導入（表示更新専用）。
- `GridDataSyncService`: **既存 `GridViewManager` 拡張**として導入（値取得/同期専用）。
- `Form1` は「イベント中継・依存注入・UI境界」に限定し、入力解釈/表示更新/同期を保持しない。

#### R-P1-03 回帰観点固定
- イベント順序依存の回帰観点を持つ。
- Undo/Redo粒度の回帰観点を持つ。
- 描画更新境界の回帰観点を持つ。

#### R-P1-04 引き継ぎ最小要件
- 未解決事項（ID/影響/暫定評価）が記録済み。
- 再現手順（前提/操作/期待/実結果）が記録済み。
- 証跡リンク（ログ/記録パス）が追跡可能。

### R-P2（Advisory）
- ログフォーマット統一、チケット命名、補助テンプレート等の運用詳細。
- R-P2は品質向上要素であり、P0/P1の代替にはならない。

---

## 3. ユニット対応（pre_v2）

| SSOT要件 | 対応ユニット |
| --- | --- |
| R-P0-01 | `unit_PRE_P0_01_virtualmode_startup.md` |
| R-P0-02 | `unit_PRE_P0_02_cellvalue_contract.md` |
| R-P0-03 | `unit_PRE_P0_03_boundary_separation.md` |
| R-P1-01 | `unit_PRE_P1_01_event_mapping_consistency.md` |
| R-P1-02 | `unit_PRE_P1_02_service_extraction_architecture.md` |
| R-P1-03 | `unit_PRE_P1_03_regression_risks.md` |
| R-P1-04 | `unit_PRE_P1_04_impl_handover_minimum.md` |

---

## 4. 完了判定ルール
- PRE完了は **R-P0 + R-P1 がすべてPASS** のとき成立。
- R-P2は改善対象として別管理し、ブロッカーにはしない。

## 5. 変更管理ルール
- `unit_PRE_*.md` は要件・完了判定のみを記載し、実施ログ（PASS/FAIL/BLOCKED・変更記録）は `PRE_worklog.md` に記録する。
- 本SSOTの更新は、同ディレクトリの `PRE_decision_log.md` に記録する。
- 実行記録は `PRE_worklog.md` に記録する。

## 6. 用語正本（PRE/IMPL共通）
- 責務軸の正本語彙は `入力解釈 / 表示更新 / 同期` とする。
- サービス名の正本語彙は `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` とする。
- IMPLタスク対応:
  - VM2-002: `GridDataSyncService`（`CellValueNeeded` read配線）
  - VM2-003: `GridDataSyncService`（`CellValuePushed` write配線）
  - VM2-004: `GridDataSyncService`（Row/Column同期）
  - VM2-006: `GridViewUpdater`（`Invalidate`境界最適化）
