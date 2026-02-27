# Phase 1 分割メモ（モデル分離）

最終更新: 2026-02-27

## 1. 目的

- Phase 1 を「短い plan 契約」と「実装詳細メモ」に分離し、追跡性を維持したまま肥大化を防ぐ。

## 2. 未了項目の列挙（棚卸し）

### 2.1 API/責務の未完了

1. `TimingSheetModel` 最小 API の完了定義
   - `GetCell` / `TryGetCell` / `SetCell`
   - `ColumnCount` / `RowCount`
   - `GetHeader` / `SetHeader`
2. Undo/Redo 連携 API の境界固定
   - `SetCellWithUndo`
   - `ApplyUndoCell`
   - `ApplyRedoCell`
3. モデル責務ガードレール（業務ロジック非移管）の継続監視

### 2.2 B区分（Step4）の未完了

1. `copyToCell` の write 経路モデル化
2. `deleteRect` の write 経路モデル化
3. 連番/置換/四則演算の write 経路モデル化
4. 行列シフト系の write 経路モデル化

### 2.3 検証とゲートの未完了

1. PR単位の検証証跡テンプレート固定
2. 直参照残件数（read/write別）の定点観測
3. B区分カバレッジの定点観測
4. Undo/Redo 回帰（単一セル、複数セル、失敗時ロールバック）の継続記録

## 3. 段階切り分け（Phase 1 内のサブフェーズ）

### P1-A: write 経路の土台固定（1PR）

- 対象: `copyToCell`, `deleteRect`
- Exit:
  - read/write の参照口が manager/model へ統一
  - 既存の `Invalidate` 挙動を維持

### P1-B: B区分 write 置換の拡張（1PR）

- 対象: 連番 / 置換 / 四則演算 / 行列シフト
- Exit:
  - B区分の write 経路でモデル API 50%以上
  - Undo/Redo の操作境界に差分なし

### P1-C: Exit判定と Phase 2 接続性確認（1PR）

- 対象: 検証証跡と移行判定
- Exit:
  - 手動スモークの結果を記録
  - `CellValueNeeded/Pushed` 接続時に API 追加不要を確認

## 4. ログ整理ルール（再確認）

- plan: 「未了項目」「exit条件」「直近3PR」だけを残す。
- decision_log: なぜその順序/制約にしたかを残す。
- worklog: 何を変更し、どう検証したかを PR 単位で残す。
