# PRE Worklog（pre_v2専用）

参照SSOT: `PRE_SSOT.md`

## 記録ルール
- 実行順序は原則 `R-P0 -> R-P1 -> R-P2`。
- 各項目に `status(PASS/FAIL/BLOCKED)` と証跡を残す。
- BLOCKEDは理由と解除条件を必須記録する。

## 実行チェックリスト（要件順）

### W-01: R-P0-01 起動表示成立
- status: FAIL
- evidence:
  - ファイル証跡: `AEIOU/WindowsFormsApplication1/Form1.cs` に `VirtualMode` 設定および `CellValueNeeded` ハンドラ定義が存在しない（`rg -n "VirtualMode|CellValueNeeded" AEIOU/WindowsFormsApplication1/Form1.cs` の結果0件）。
  - ログ証跡: `rg -n "VirtualMode|CellValueNeeded" AEIOU/WindowsFormsApplication1/Form1.cs` -> no match（exit code 1）。
  - 再現手順:
    1) `rg -n "VirtualMode|CellValueNeeded" AEIOU/WindowsFormsApplication1/Form1.cs`
    2) 0件であることを確認。
    3) `R-P0-01` の前提（VirtualMode=true起動経路）が未配線と判定。
- note:
  - R-P0-01 未達。VirtualMode配線（設定+値供給イベント）を実装後に再判定。

### W-02: R-P0-02 値取得契約
- status: FAIL
- evidence:
  - ファイル証跡: `TryGetCellValue` は `private bool TryGetCellValue(int col, int row, out string value)` のみで、`failureReason` を返す契約が未実装（`AEIOU/WindowsFormsApplication1/Form1.cs`）。
  - ログ証跡:
    - `rg -n "TryGetCellValue\(|failureReason|CellValueNeeded" AEIOU/WindowsFormsApplication1/Form1.cs` -> `TryGetCellValue` は検出されるが `failureReason` / `CellValueNeeded` は0件。
  - 再現手順:
    1) `rg -n "TryGetCellValue\(|failureReason|CellValueNeeded" AEIOU/WindowsFormsApplication1/Form1.cs`
    2) `TryGetCellValue` シグネチャが out string のみであることを確認。
    3) `R-P0-02` 要件（`CellValueNeeded -> TryGetCellValue` 一意経路、failureReason返却）未充足と判定。
- note:
  - R-P0-02 未達。`CellValueNeeded` 経路固定と `failureReason` 返却方式の設計・実装が必要。

### W-03: R-P0-03 責務境界固定
- status: FAIL
- evidence:
  - ファイル証跡: `dataGridView1_CellPainting` 内から `checkContinuty` を経由して `TryGetCellValue` を参照しており、描画イベント内に値取得責務が混在（`AEIOU/WindowsFormsApplication1/Form1.cs`）。
  - ログ証跡: `rg -n "dataGridView1_CellPainting|checkContinuty|TryGetCellValue" AEIOU/WindowsFormsApplication1/Form1.cs` で同一責務経路を確認。
  - 再現手順:
    1) `rg -n "dataGridView1_CellPainting|checkContinuty|TryGetCellValue" AEIOU/WindowsFormsApplication1/Form1.cs`
    2) `CellPainting -> checkContinuty -> TryGetCellValue` の呼び出し連鎖を確認。
    3) `R-P0-03`（CellPainting描画専任）未達と判定。
- note:
  - R-P0-03 未達。描画判定用データを事前計算/キャッシュ化し、CellPaintingから値取得を排除すること。

### W-04: R-P1-01 イベント割当整合
- status: BLOCKED
- evidence:
  - ファイル証跡: PRE定義は存在するが、Phase2対象イベントの「単一定義（正本）」文書が未作成（`agent_plans/refactor_form1cs_001/phase2/tasks/pre_v2/unit_PRE_P1_01_event_mapping_consistency.md` は要件のみ）。
  - ログ証跡: `rg -n "イベント割当|mapping|CellValueNeeded|CellValuePushed|KeyDown|KeyPress|CellPainting" agent_plans/refactor_form1cs_001/phase2/tasks/pre_v2 agent_plans/refactor_form1cs_001/phase2/overview.md` で実体マッピング表を確認できず。
  - 再現手順:
    1) 上記 `rg` を実行。
    2) 要件文のみで、イベント割当の実体一覧（採用/対象外理由付き）がないことを確認。
- note:
  - BLOCKED理由（R-P1-01）: 判定対象となるイベント割当マップの正本が未整備。
  - 解除条件（R-P1-01）: `phase2` 配下に「対象イベント・割当先・対象外理由」を1表で管理するマッピング文書を追加し、参照先を `PRE_worklog` に記録する。

### W-05: R-P1-02 抽出設計整合
- status: BLOCKED
- evidence:
  - ファイル証跡: 要件で指定された `GridInputInterpreter` / `GridViewUpdater` / `GridDataSyncService` の実体ファイルが存在しない（`rg --files | rg "GridInputInterpreter|GridViewUpdater|GridDataSyncService"` 0件）。
  - ログ証跡: `rg -n "GridInputInterpreter|GridViewUpdater|GridDataSyncService" AEIOU/WindowsFormsApplication1 agent_plans/refactor_form1cs_001/phase2 -S` -> 要件文以外の実装参照なし。
  - 再現手順:
    1) `rg --files | rg "GridInputInterpreter|GridViewUpdater|GridDataSyncService"`
    2) 実体未作成を確認。
    3) 責務重複有無の比較母集団が不足し、判定不能とする。
- note:
  - BLOCKED理由（R-P1-02）: 対象サービスが未実装で責務比較ができない。
  - 解除条件（R-P1-02）: 3サービスのインターフェース/クラス定義（責務境界コメント付き）を追加し、責務マトリクスで重複なしを確認する。

### W-06: R-P1-03 回帰観点固定
- status: FAIL
- evidence:
  - ファイル証跡: 回帰観点の実運用チェックリストは存在するが全項目未チェック（`agent_plans/refactor_form1cs_001/phase2/checklists/smoke_virtualmode.md`）。
  - ログ証跡: `rg -n "\- \[ \]" agent_plans/refactor_form1cs_001/phase2/checklists/smoke_virtualmode.md` で未実施項目のみが列挙される。
  - 再現手順:
    1) `sed -n '1,220p' agent_plans/refactor_form1cs_001/phase2/checklists/smoke_virtualmode.md`
    2) すべて `[ ]` のままであることを確認。
    3) `R-P1-03` の「観点固定/記録済み」未達と判定。
- note:
  - R-P1-03 未達。イベント順序依存・Undo/Redo粒度・描画更新境界の各観点について、実施結果（PASS/FAIL）を記録すること。

### W-07: R-P1-04 引き継ぎ最小要件
- status: BLOCKED
- evidence:
  - ファイル証跡:
    - ひな形は存在（`agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`）。
    - ただし VM2-001〜VM2-008 の Done報告実体が未配置で、未解決事項ID/影響/暫定評価・再現手順（前提/操作/期待/実結果）・証跡リンクの三点セットを同時充足した記録を確認できない。
  - ログ証跡: `rg -n "# task_VM2_00[1-8]|未解決事項/フォローアップ|前提|操作|期待|実結果|証跡リンク" agent_plans/refactor_form1cs_001/phase2 -S` でテンプレート定義のみ確認。
  - 再現手順:
    1) `sed -n '1,220p' agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_IMPL.md`
    2) `agent_plans/refactor_form1cs_001/phase2/tasks/impl/` 配下にDone報告ファイルがないことを確認。
    3) `R-P1-04` 判定に必要な引き継ぎ記録が未整備と判定。
- note:
  - BLOCKED理由（R-P1-04）: 引き継ぎ記録の実体不足により判定不能。
  - 解除条件（R-P1-04）: VM2各タスクでDone報告を作成し、少なくとも1件は「未解決事項ID/影響/暫定評価 + 再現手順4点 + 証跡リンク」を満たしたサンプルを添付する。

## 判定サマリ
- P0完了: No
- P1完了: No
- PRE完了（R-P0+R-P1全PASS）: No
- 残課題（R-P2含む）:
  - R-P0-01: VirtualMode起動経路と表示成立の実測証跡を追加。
  - R-P0-02: `CellValueNeeded -> TryGetCellValue` 契約一本化と failureReason 返却仕様の明文化。
  - R-P0-03: CellPaintingから値取得責務を分離。
  - R-P1-01: イベント割当マップ正本の作成（対象/対象外理由付き）。
  - R-P1-02: 3サービス実体の作成と責務マトリクス化。
  - R-P1-03: smoke checklist 実施結果の記録。
  - R-P1-04: Done報告（未解決事項ID、再現手順、証跡リンク）の整備。
  - R-P2: ログフォーマット・チケット命名・補助テンプレート運用の統一。
