# PRE Worklog（pre_v2専用）

参照SSOT: `PRE_SSOT.md`

## 記録ルール
- 実行順序は原則 `R-P0 -> R-P1 -> R-P2`。
- 各項目に `status(PASS/FAIL/BLOCKED)` と証跡を残す。
- BLOCKEDは理由と解除条件を必須記録する。

## 実行チェックリスト（要件順）

### W-01: R-P0-01 起動表示成立
- status: PASS
- evidence:
  - ファイル証跡:
    - `AEIOU/WindowsFormsApplication1/Form1.Designer.cs` で `dataGridView1.VirtualMode = true;` が設定され、`CellValueNeeded` / `CellValuePushed` がイベント配線されている。
    - `AEIOU/WindowsFormsApplication1/Form1.cs` で `dataGridView1_CellValueNeeded` ハンドラが実装され、有効セル範囲で `TryGetCellValue` を経由して表示値を返却している。
  - ログ証跡:
    1) `rg -n "VirtualMode = true|CellValueNeeded \+=|CellValuePushed \+=" AEIOU/WindowsFormsApplication1/Form1.Designer.cs`
    2) `rg -n "dataGridView1_CellValueNeeded\(|TryGetCellValue\(e\.ColumnIndex, e\.RowIndex" AEIOU/WindowsFormsApplication1/Form1.cs`
  - 再現手順:
    1) 上記2コマンドを実行し、DesignerでVirtualMode有効化とイベント配線を確認する。
    2) `CellValueNeeded` 実装で `TryGetCellValue` が呼ばれていることを確認する。
    3) `R-P0-01` の前提（VirtualMode=true起動経路と値供給イベント）が成立していると判定する。
- note:
  - 旧記録は実装前スナップショットであり、現行コードとの差分により再採取結果はPASSへ更新。

### W-02: R-P0-02 値取得契約
- status: PASS
- evidence:
  - ファイル証跡:
    - `AEIOU/WindowsFormsApplication1/Form1.cs` に `private bool TryGetCellValue(int col, int row, out string value, out string failureReason)` が実装されている。
    - `dataGridView1_CellValueNeeded` が `TryGetCellValue(e.ColumnIndex, e.RowIndex, out value)` を一意の値取得経路として利用している。
    - 3引数版 `TryGetCellValue` は4引数版へ委譲し、`failureReason` 契約を保持している。
  - ログ証跡:
    1) `rg -n "dataGridView1_CellValueNeeded\(|TryGetCellValue\(int col, int row, out string value, out string failureReason\)|TryGetCellValue\(int col, int row, out string value\)" AEIOU/WindowsFormsApplication1/Form1.cs`
    2) `rg -n "failureReason" AEIOU/WindowsFormsApplication1/Form1.cs`
  - 再現手順:
    1) 上記コマンドで `CellValueNeeded` と `TryGetCellValue` 両シグネチャを確認する。
    2) `CellValueNeeded` が `TryGetCellValue` を呼ぶ実装を確認する。
    3) 4引数版で `failureReason` を返却し、3引数版が同契約へ委譲していることを確認し、`R-P0-02` 達成と判定する。
- note:
  - 旧記録時点では `failureReason` 契約未導入だったが、現行コードでは導入済み。

### W-03: R-P0-03 責務境界固定
- 関連ユニット: `unit_PRE_P0_03_boundary_separation.md`（実施記録は本セクションを正本とする）
- status: PASS
- evidence:
  - ファイル証跡:
    - `AEIOU/WindowsFormsApplication1/Form1.cs` の `checkContinuty` は `continuityStateService.GetContinuityFlag` の参照のみで、セル値取得 (`TryGetCellValue`) を実行しない。
    - `dataGridView1_CellPainting` は `checkContinuty` の結果と描画サービス (`gridCellStyleResolver` / `gridCellRenderer`) を使った描画処理に限定されている。
  - ログ証跡:
    1) `rg -n "checkContinuty\(|GetContinuityFlag|dataGridView1_CellPainting\(" AEIOU/WindowsFormsApplication1/Form1.cs`
    2) `awk 'NR>=1267 && NR<=1316 {print}' AEIOU/WindowsFormsApplication1/Form1.cs | rg -n "TryGetCellValue"`
       - 結果: no match（exit code 1）
  - 再現手順:
    1) 1)の `rg` で `checkContinuty` が事前計算済み継続状態参照のみであることを確認する。
    2) 2)の `awk|rg` で `CellPainting` 本体に `TryGetCellValue` 呼び出しがないことを確認する。
    3) `CellPainting -> checkContinuty -> TryGetCellValue` 連鎖が解消され、`R-P0-03`（描画専任）が成立していると判定する。
- note:
  - 旧記録は `checkContinuty` が直接値取得していた時期の情報であり、現行では事前計算参照方式に置換済み。

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
- P0完了: Yes
- P1完了: No
- PRE完了（R-P0+R-P1全PASS）: No
- 残課題（R-P2含む）:
  - R-P1-01: イベント割当マップ正本の作成（対象/対象外理由付き）。
  - R-P1-02: 3サービス実体の作成と責務マトリクス化。
  - R-P1-03: smoke checklist 実施結果の記録。
  - R-P1-04: Done報告（未解決事項ID、再現手順、証跡リンク）の整備。
  - R-P2: ログフォーマット・チケット命名・補助テンプレート運用の統一。
