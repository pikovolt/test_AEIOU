# Preflight Checklist: P4-004（MoveSelectionCommand導入前）

## 前提確認
- [x] `P4-003` Done証跡（`../reports/impl_done/P4_003.md`）を確認済み
- [x] `task_P4_004.md` の対象/非対象に合意済み（値入力系は `P4-005` へ分離）
- [x] 変更ファイル数 3〜5（1タスク1PR）で進める前提を確認済み

## 入力経路（着手前ベースライン）
- [x] `KeyDown -> Interpreter -> Router/Dispatcher -> Command -> Service` 経路を現状把握済み
- [x] メニューショートカット優先の既存挙動が維持対象であることを確認済み

## 移動/選択（P4-004 主対象）
- [x] キーボード移動（上下左右）の期待動作を確認済み
- [x] `Shift+↑/↓/←/→` の範囲拡縮期待動作を確認済み
- [x] `*` / `/` の縦方向範囲拡縮期待動作を確認済み
- [x] 行列境界（先頭/末尾）での移動・選択期待動作を確認済み

## 境界条件（退行リスク）
- [x] `Ctrl` / `Shift` 付き移動時の `selectRange` 連携ポイントを確認済み
- [x] `isFirstEdit` 更新タイミングに影響しない分離方針を確認済み
- [x] マウス経路（`CellMouseDown/Move/Up`）は今回非対象であることを確認済み

## 実施メモ
- 実施日: 2026-03-02
- 実施者: Copilot
- 備考: 文書・コード読解による着手前確認を完了。実機回帰は `P4-004` 実装後に `smoke_phase4.md` で実施する。

## 参照
- `smoke_phase4.md`（Phase4共通の回帰観点）
- `../tasks/impl/task_P4_004.md`
