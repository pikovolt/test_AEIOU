# Phase5 tasks — 実行計画

## 運用
- 1タスク = 1PR、主要 Gate は1つ、原則3〜5変更ファイルを維持する。
- 各実装 PR は `../checklists/smoke_phase5.md` の該当項目を更新する。
- P5-001 完了前に挙動変更を行わない。外部 DLL ロードは P5-006 まで導入しない。

## P5-001: 現行挙動 characterization
**目的:** 6経路の暗黙仕様と既知の異常系を、分離前に再現可能にする。

**作業:**
- 純粋化できる計算例を表形式またはテスト fixture 化する。
- 単一/複数列、空セル混在、カラセル、範囲端、負数、ゼロ、モデルレス繰り返し再実行を記録する。
- 特に `step=0`、除数0、セル値0の乗除算、範囲外になる繰り返し入力を「互換維持」か「別修正」か決定する。

**Gate:** 6経路すべてに正常系、境界、異常系、Undo期待値があり、仕様変更候補が実装移管と分離されている。

## P5-002: Abstractions と host 検証プロトタイプ
**目的:** UI 非依存の最小公開契約と、変更セットを安全に拒否できる host 境界を確定する。

**作業:**
- `AEIOU.Automation.Abstractions` プロジェクトと DTO/interface を追加する。
- in-memory の fake command で正常、範囲外、重複セル、例外を検証する。
- パラメーター定義は初期6機能に必要な型だけに限定する。

**Gate:** Abstractions が WinForms と本体型を参照せず、不正結果時に書き込みが0件である。

## P5-003: 単純変換コマンド移管
**目的:** 置換、反転、四則演算を同じコマンド実行面へ移す。

**作業:**
- `ReplaceCommand`、`ReverseCommand`、`ArithmeticCommand` を実装する。
- 既存イベントはパラメーター取得と host 呼び出しへ縮小する。
- 3処理の write group 名と `FinishWriteOperation(true)` 相当を維持する。

**Gate:** P5-001 fixture と UI smoke が移管前後で一致する。

## P5-004: 連番・繰り返しコマンド移管
**目的:** 状態と Undo 規則が複雑な連番系を、計算と session orchestration に分ける。

**作業:**
- `SequentialNumberCommand` と `RepeatNumberCommand` を実装する。
- `RepeatInputBox` の値を DTO 化し、`HandleRepeatInput` から Form cast とセル計算を除く。
- 「再実行前に直前結果だけを Undo」「範囲クリア+入力が1 group」を `AutomationSession` の明示仕様にする。
- ダイアログの多重起動、閉じた後のイベント解除を確認する。

**Gate:** 初回、連続2回、ダイアログを閉じて再起動の各 Undo/Redo 往復が一致する。

## P5-005: Registry と UI adapter 統一
**目的:** 組み込み処理を安定 ID で登録し、将来の外部処理と同じ呼び出し経路にする。

**作業:**
- `AutomationRegistry` と descriptor ベースの parameter adapter を導入する。
- 既存メニューは ID でコマンドを解決する。
- 不明 ID、重複 ID、無効パラメーターを host で扱う。

**Gate:** 外部 DLL が0件でも全組み込み機能が registry 経由で動き、直接 new/直接 Execute がイベント側に残らない。

## P5-006: 外部 DLL discovery
**目的:** 信頼済みローカル DLL からコマンドを追加できるようにする。

**作業:**
- `Extensions` 直下の探索、型検出、契約バージョン/ID 検査を実装する。
- 壊れた DLL、依存不足、constructor 例外、実行例外を拡張単位で隔離する。
- 外部コマンド一覧を既存の自動処理メニュー配下へ追加する。

**Gate:** 正常サンプル1件は実行でき、失敗サンプルを同居させても起動と組み込み6機能が継続する。

## P5-007: 回帰・配布手順確定
**目的:** 拡張作成者と利用者が契約を再現でき、障害時に復旧できる状態にする。

**作業:**
- 最小サンプル拡張とビルド/配置手順を追加する。
- 拡張全除去、契約不一致、重複 ID、実行例外の復旧手順を記載する。
- Phase5 smoke と Release ビルド結果を完了報告へ残す。

**Gate:** クリーン環境でサンプル配置→検出→実行→除去を再現できる。
