# PRE→IMPL 対応マトリクス（Phase2）

## 判定ルール
- 本書（特に「VM2ごとの必須ユニットチェックリスト（機械判定用）」）を、IMPL着手可否の**正規ルール（唯一の判定ソース）**とする。
- **前提**: 当該 VM2 タスクの正式着手前に、必要な PRE「見出しユニット」の確認完了が必須。
- **参照**: 必須ではないが、設計・レビュー時に参照推奨。
- **非該当**: 直接の依存関係なし。

### 運用注記（作業ブロック防止）
- PRE の完了判定は**ファイル全体完了を要求しない**。当該 VM2 の着手に必要な見出しユニットのみレビュー完了すれば着手可とする。
- 新しい PRE 番号や追加ファイルは作成せず、既存 PRE 本文内の既存見出しをユニットIDで運用する。
- PR 本文には「消化した PRE 見出しユニットID」を記載し、PRE 全体完了の宣言は不要とする。
- 依存判定は PRE 番号単位ではなく、見出しユニット単位で行う。これにより VM2-001 / VM2-002 / VM2-006 等の先行着手を阻害しない。

## PRE見出しユニット一覧（最小依存単位）

| ユニットID | 出典 | 見出し | 主用途 |
| --- | --- | --- | --- |
| U-PRE001-EVT | `task_PRE_001.md` | `Phase2対象（必須レビュー）` | 影響イベントと探索起点の固定 |
| U-PRE002-ARCH | `task_PRE_002.md` | `抽出候補責務（3系統）` + `抽出先クラス案` | 実装責務の分離方針 |
| U-PRE002-RISK | `task_PRE_002.md` | `互換性/回帰リスク` | 回帰観点と暫定回避 |
| U-PRE003-GATE | `task_PRE_003.md` | `実装ゲートチェックリスト` | 起動/表示成立の受け入れ基準 |
| U-PRE003-HANDOVER | `task_PRE_003.md` | `task_VM2_001.md` への引き継ぎ必須情報 | 未解決事項・再現手順の引き継ぎ |
| U-PRE004-CONTRACT | `task_PRE_004.md` | `値返却契約（CellValueNeeded）` | `CellValueNeeded` 返却仕様 |
| U-PRE004-TRYGET | `task_PRE_004.md` | `TryGetCellValue` 戻り契約（インターフェース・正本） | 値解決IFの固定（参照起点: `task_PRE_004.md` 同見出し） |
| U-PRE004-BOUNDARY | `task_PRE_004.md` | `責務境界（再掲）` | 値解決と描画責務の分離 |

## PRE見出しユニット→IMPL マトリクス

| ユニット \ VM2 | VM2-001 | VM2-002 | VM2-003 | VM2-004 | VM2-005 | VM2-006 | VM2-007 | VM2-008 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| U-PRE001-EVT | 前提 | 前提 | 参照 | 参照 | 参照 | 参照 | 前提 | 参照 |
| U-PRE002-ARCH | 参照 | 参照 | 前提 | 前提 | 前提 | 前提 | 参照 | 参照 |
| U-PRE002-RISK | 参照 | 参照 | 参照 | 参照 | 前提 | 前提 | 参照 | 前提 |
| U-PRE003-GATE | 前提 | 参照 | 非該当 | 非該当 | 非該当 | 参照 | 前提 | 前提 |
| U-PRE003-HANDOVER | 前提 | 前提 | 参照 | 参照 | 参照 | 参照 | 前提 | 前提 |
| U-PRE004-CONTRACT | 参照 | 前提 | 前提 | 参照 | 参照 | 非該当 | 非該当 | 前提 |
| U-PRE004-TRYGET | 参照 | 前提 | 前提 | 前提 | 前提 | 参照 | 非該当 | 前提 |
| U-PRE004-BOUNDARY | 参照 | 前提 | 参照 | 前提 | 前提 | 参照 | 非該当 | 前提 |

## VM2ごとの必須ユニットチェックリスト（機械判定用）

判定仕様:
- 各 VM2 で **「前提」指定された見出しユニットのみ** を必須とする。
- 各チェックは `DONE=Yes/No` で記録し、必須ユニットがすべて `Yes` のときに `START=Ready` と判定する。

### VM2-001 必須ユニット
- [ ] U-PRE001-EVT DONE=Yes
- [ ] U-PRE003-GATE DONE=Yes
- [ ] U-PRE003-HANDOVER DONE=Yes
- 判定式: `START_VM2_001 = U_PRE001_EVT && U_PRE003_GATE && U_PRE003_HANDOVER`

### VM2-002 必須ユニット
- [ ] U-PRE001-EVT DONE=Yes
- [ ] U-PRE003-HANDOVER DONE=Yes
- [ ] U-PRE004-CONTRACT DONE=Yes
- [ ] U-PRE004-TRYGET DONE=Yes
- [ ] U-PRE004-BOUNDARY DONE=Yes
- 判定式: `START_VM2_002 = U_PRE001_EVT && U_PRE003_HANDOVER && U_PRE004_CONTRACT && U_PRE004_TRYGET && U_PRE004_BOUNDARY`

### VM2-003 必須ユニット
- [ ] U-PRE002-ARCH DONE=Yes
- [ ] U-PRE004-CONTRACT DONE=Yes
- [ ] U-PRE004-TRYGET DONE=Yes
- 判定式: `START_VM2_003 = U_PRE002_ARCH && U_PRE004_CONTRACT && U_PRE004_TRYGET`

### VM2-004 必須ユニット
- [ ] U-PRE002-ARCH DONE=Yes
- [ ] U-PRE004-TRYGET DONE=Yes
- [ ] U-PRE004-BOUNDARY DONE=Yes
- 判定式: `START_VM2_004 = U_PRE002_ARCH && U_PRE004_TRYGET && U_PRE004_BOUNDARY`

### VM2-005 必須ユニット
- [ ] U-PRE002-ARCH DONE=Yes
- [ ] U-PRE002-RISK DONE=Yes
- [ ] U-PRE004-TRYGET DONE=Yes
- [ ] U-PRE004-BOUNDARY DONE=Yes
- 判定式: `START_VM2_005 = U_PRE002_ARCH && U_PRE002_RISK && U_PRE004_TRYGET && U_PRE004_BOUNDARY`

### VM2-006 必須ユニット
- [ ] U-PRE002-ARCH DONE=Yes
- [ ] U-PRE002-RISK DONE=Yes
- 判定式: `START_VM2_006 = U_PRE002_ARCH && U_PRE002_RISK`

### VM2-007 必須ユニット
- [ ] U-PRE001-EVT DONE=Yes
- [ ] U-PRE003-GATE DONE=Yes
- [ ] U-PRE003-HANDOVER DONE=Yes
- 判定式: `START_VM2_007 = U_PRE001_EVT && U_PRE003_GATE && U_PRE003_HANDOVER`

### VM2-008 必須ユニット
- [ ] U-PRE002-RISK DONE=Yes
- [ ] U-PRE003-GATE DONE=Yes
- [ ] U-PRE003-HANDOVER DONE=Yes
- [ ] U-PRE004-CONTRACT DONE=Yes
- [ ] U-PRE004-TRYGET DONE=Yes
- [ ] U-PRE004-BOUNDARY DONE=Yes
- 判定式: `START_VM2_008 = U_PRE002_RISK && U_PRE003_GATE && U_PRE003_HANDOVER && U_PRE004_CONTRACT && U_PRE004_TRYGET && U_PRE004_BOUNDARY`
