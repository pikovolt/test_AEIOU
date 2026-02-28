# PRE→IMPL 対応マトリクス（Phase2）

## 判定ルール
- **前提**: 当該 VM2 タスクの正式着手前に、PRE 成果物の確認完了が必須。
- **参照**: 必須ではないが、設計・レビュー時に参照推奨。
- **非該当**: 直接の依存関係なし。

## PRE→IMPL マトリクス

| PRE \ VM2 | VM2-001 | VM2-002 | VM2-003 | VM2-004 | VM2-005 | VM2-006 | VM2-007 | VM2-008 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| PRE-001 | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） | 前提（`task_PRE_001.md`「Phase2対象（必須レビュー）」） |
| PRE-002 | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） | 前提（`task_PRE_002.md`「実施内容」） |
| PRE-003 | 前提（`task_PRE_003.md`「実装ゲートチェックリスト（task_VM2_001.md 引き継ぎ用）」） | 前提（`task_PRE_003.md`「実装ゲートチェックリスト（task_VM2_001.md 引き継ぎ用）」） | 参照 | 参照 | 参照 | 非該当 | 前提（`task_PRE_003.md`「実装ゲートチェックリスト（task_VM2_001.md 引き継ぎ用）」） | 前提（`task_PRE_003.md`「IMPL着手可否判定」） |
| PRE-004 | 非該当 | 前提（`task_PRE_004.md`「値返却契約（CellValueNeeded）」） | 前提（`task_PRE_004.md`「値返却契約（CellValueNeeded）」） | 前提（`task_PRE_004.md`「責務境界（再掲）」） | 前提（`task_PRE_004.md`「責務境界（再掲）」） | 参照 | 非該当 | 前提（`task_PRE_004.md`「完了条件」） |

## VM2ごとの必須PREチェックリスト（機械判定用）

判定仕様:
- 各 VM2 で **「前提」指定された PRE のみ** を必須とする。
- 各チェックは `DONE=Yes/No` で記録し、必須PREがすべて `Yes` のときに `START=Ready` と判定する。

### VM2-001 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-003 DONE=Yes
- 判定式: `START_VM2_001 = PRE001 && PRE002 && PRE003`

### VM2-002 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-003 DONE=Yes
- [ ] PRE-004 DONE=Yes
- 判定式: `START_VM2_002 = PRE001 && PRE002 && PRE003 && PRE004`

### VM2-003 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-004 DONE=Yes
- 判定式: `START_VM2_003 = PRE001 && PRE002 && PRE004`

### VM2-004 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-004 DONE=Yes
- 判定式: `START_VM2_004 = PRE001 && PRE002 && PRE004`

### VM2-005 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-004 DONE=Yes
- 判定式: `START_VM2_005 = PRE001 && PRE002 && PRE004`

### VM2-006 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- 判定式: `START_VM2_006 = PRE001 && PRE002`

### VM2-007 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-003 DONE=Yes
- 判定式: `START_VM2_007 = PRE001 && PRE002 && PRE003`

### VM2-008 必須PRE
- [ ] PRE-001 DONE=Yes
- [ ] PRE-002 DONE=Yes
- [ ] PRE-003 DONE=Yes
- [ ] PRE-004 DONE=Yes
- 判定式: `START_VM2_008 = PRE001 && PRE002 && PRE003 && PRE004`
