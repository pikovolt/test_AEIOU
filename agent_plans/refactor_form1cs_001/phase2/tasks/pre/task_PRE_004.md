# task_PRE_004: CellValueNeeded導入前設計（移行元: task_VM2_002）

## 目的
`CellValueNeeded` 導入前に、VirtualMode表示に必要な読み出し経路と境界条件を設計として固定化する。

## 実施内容
- `CellValueNeeded` に集約する参照経路を整理する。
- 行列インデックス境界の防御観点を定義する。
- 既存表示ロジックとの差分確認観点を列挙する。

## 着手判定ユニット（独立運用）
- `U-PRE004-CONTRACT`: `値返却契約（CellValueNeeded）` の確定。
- `U-PRE004-TRYGET`: `TryGetCellValue` 戻り契約（インターフェース） の確定。
- `U-PRE004-BOUNDARY`: `責務境界（再掲）` の確定。

運用ルール:
- 3ユニットはレビュー順序を固定しない。VM2要件に合わせて必要な着手判定ユニットから先に完了可能。
- IMPL側は PRE-004 全体完了ではなく、必要な着手判定ユニットの完了有無で着手可否を判定する。

## 値返却契約（CellValueNeeded）
`CellValueNeeded` の値返却は、以下の契約に従って実装・レビューする。

| 入力状態 | 返却値 | 例外有無 | UI挙動 |
|---|---|---|---|
| 正常セル（行・列ともに有効範囲、モデル初期化済み、バインド一致） | 対象セルの表示文字列（`string`、型正規化済み） | 例外なし | 該当セルを通常描画する |
| 空セル（値未設定 / `null`） | `string.Empty`（失敗時値: フォールバック空文字） | 例外なし | 空文字として描画し、編集継続可能 |
| 行/列境界外（`rowIndex` / `columnIndex` が範囲外） | `string.Empty`（失敗時値: フォールバック空文字） | 例外なし | 空表示で継続し、グリッド描画を中断しない |
| モデル未初期化（データソース未ロード / 解放済み） | `string.Empty`（失敗時値: フォールバック空文字） | 例外なし | 空表示で継続し、画面操作を阻害しない |
| バインド不一致（列定義とモデルプロパティが不整合） | `string.Empty`（失敗時値: フォールバック空文字） | 例外なし | 空表示で継続し、異常はログ/診断経路で検知する |

### 列型ごとの返却ポリシー
現行フェーズ（PRE-004）では **正規セル値型を「表示文字列（`string`）」に統一** する。

- 文字列列: モデル値を表示文字列として返却し、`null` は `string.Empty` に正規化する。
- 数値列: 列定義に基づいて数値として解釈した後、表示文字列へ正規化して返却する（内部で数値型を保持しても `CellValueNeeded` 返却値は文字列に統一）。
- 日時列: 列定義に基づいて日時として解釈した後、表示文字列へ正規化して返却する（内部で `DateTime` を扱っても返却値は文字列に統一）。
- いずれの列型でも未設定値（`null`）は `string.Empty` に統一し、`DBNull.Value` は返却しない。
- 上記の正規化は `TryGetCellValue` で実施し、`CellValueNeeded` 側で追加の型変換や例外吸収を行わない。

### 編集中セルの優先値ルール
- 対象セルが編集中の場合、表示値の解決順序は「編集中バッファ値 > モデル保持値」とする。
- 編集中バッファが取得できる場合は、その値を `CellValueNeeded` の返却値として最優先採用する。
- 編集中バッファが未確定・取得不可の場合のみ、モデル値（`TryGetCellValue` の通常経路）にフォールバックする。
- 編集確定（コミット）後はモデル値を唯一の正とし、バッファ値は参照しない。

### フォールバック時の診断ログ必須項目
フォールバック返却（`string.Empty` 等）が発生した場合、診断ログに以下を必須出力する。

- `rowIndex`
- `columnIndex`
- `columnName`（列定義名 / `DataPropertyName`）
- `reasonCategory`（例: `OutOfRange`, `ModelUninitialized`, `BindingMismatch`, `NullValue`）
- `occurrenceCount`（同一カテゴリ・同一列での発生回数）

ログは UI 継続性を優先して非例外で記録し、`CellValueNeeded` の制御フローを中断させない。

### フォールバック仕様（失敗時統一）
- 成功値型は `string`（表示文字列）に固定し、`CellValueNeeded` は成功時にそのまま表示へ渡す。
- `TryGetCellValue == false` の場合、失敗時値は常に `string.Empty`（フォールバック空文字）とする。
- 型不一致時動作は `TryGetCellValue == false`、`failureReason = CellValueFailureReason.TypeMismatch`、`CellValueNeeded` は `string.Empty` を返却、の組み合わせに統一する。
- 上記失敗系では例外を送出せず、診断ログ必須項目を記録して UI 継続を優先する。

### `TryGetCellValue` 戻り契約（インターフェース）
`TryGetCellValue` は以下の戻り契約を満たすインターフェースで扱う。

```csharp
public interface ICellValueResolver
{
    bool TryGetCellValue(
        int rowIndex,
        int columnIndex,
        out object value,
        out CellValueFailureReason failureReason);
}

public enum CellValueFailureReason
{
    None,
    OutOfRange,
    ModelUninitialized,
    BindingMismatch,
    TypeMismatch,
    NullValue,
    Unknown
}
```

- `bool` は成功可否を示し、`true` の場合のみ `value` を有効値として扱う。
- 成功値型は `string`（表示文字列）に固定し、`out object value` はインターフェース互換性維持のための受け口として継続する。
- .NET 3.5 / C# 3 相当の記法を前提とし、型記法は `object` を使用する（null許容は運用上許可する）。
- `TryGetCellValue` は成功判定前に、以下の順で列メタ情報から型を確定する。
  1. `columnIndex` から列定義を特定する。
  2. 列定義の `DataPropertyName` / 列種別 / `ValueType` から期待型を決定する。
  3. モデル値が期待型として解釈可能かを検証し、表示文字列へ正規化する。
  4. 正規化完了時のみ `true` とし、`value` に正規化済み文字列を設定する。
- 型確定または正規化に失敗した場合は `false` を返し、`value = null`（運用上の失敗値）、`failureReason = CellValueFailureReason.TypeMismatch`（型不一致時動作）に統一する。
- `failureReason` は失敗理由を必ず返し、成功時は `CellValueFailureReason.None` とする。
- `CellValueNeeded` は `TryGetCellValue == false` の場合に失敗時値 `string.Empty`（フォールバック空文字）を返し、同時に診断ログ必須項目を記録する。

### 型不一致時の統一処理（DataError 非依存）
- 型不一致（列メタ情報で確定した期待型とモデル値が整合しない、または表示文字列へ正規化できない）は、必ず `TryGetCellValue == false` で返す。
- 失敗理由は `CellValueFailureReason.TypeMismatch` に一本化し、`CellValueNeeded` は例外を投げず `string.Empty` を返す。
- 同時に診断ログ必須項目（`rowIndex` / `columnIndex` / `columnName` / `reasonCategory` / `occurrenceCount`）を記録する。
- `DataError` イベントでの後段吸収や再解釈には依存しない。

### 例外方針
- `CellValueNeeded` では例外を投げない。
- `DataError` イベントへの依存で吸収する設計を禁止する。

### 責務境界（再掲）
- `CellPainting`: 描画装飾のみを担当し、値取得ロジックを持たない。
- `KeyDown` / `KeyPress`: 入力制御のみを担当し、値解決・参照責務を持たない。
- 値取得経路は `TryGetCellValue` 一択とし、`CellValueNeeded` から直接的に同経路を呼び出す。

## 完了条件
- `CellValueNeeded` 実装前提となる設計観点が明記されている。
- IMPLタスクの入出力前提に引き継げる。
- 本ファイル内の `DONE_TEMPLATE_PRE 記入` セクションに、共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。

## 参照入力（依存最小化）
- `task_PRE_003.md` の `実装ゲートチェックリスト`（必要時のみ参照）

> PRE-003 の完了待ちは不要。`CellValueNeeded` 設計に必要な範囲のみ参照して確定する。

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## DONE_TEMPLATE_PRE 記入
- 対象範囲（今回扱ったファイル/機能/イベント）:
  - `CellValueNeeded` 導入前の値返却契約、`TryGetCellValue` 戻り契約、境界防御と責務境界（CellPainting/KeyDown/KeyPressとの分離）。
- 非対象（今回やらないこと）:
  - `CellValueNeeded` 本実装、`TryGetCellValue` 具象クラス追加、DataGridViewイベント実配線変更、実機での性能最適化。
- 証跡リンク（調査メモ、設計資料、関連Issue/PRなど）:
  - 本ファイル（`agent_plans/refactor_form1cs_001/phase2/tasks/pre/task_PRE_004.md`）の `値返却契約` / `フォールバック仕様` / `TryGetCellValue 戻り契約` セクション。
- 現状分析サマリ（現行構造/課題/制約）:
  - 値解決経路が分散したまま VirtualMode を導入すると、型不一致や境界外参照時の挙動が不統一になり、描画例外・空白表示の再現性が高まる。
- 設計方針（抽出方針、責務分割、インターフェース案）:
  - 値取得を `TryGetCellValue` に一本化し、`CellValueNeeded` は契約どおり返却のみ担当、失敗時は `string.Empty`＋診断ログ記録で UI 継続を優先する。
- 互換性/回帰リスク列挙（最小3観点）:
  - リスク1: 列型正規化ルールの不統一で、表示値が既存挙動と乖離する。
  - リスク2: 編集中バッファ優先ルールの欠落で、編集中セルに古いモデル値が表示される。
  - リスク3: `TryGetCellValue` 失敗時の理由分類が不足し、障害解析時に原因特定が遅延する。
- 実装フェーズへの引き継ぎ事項（前提条件/未確定事項）:
  - 失敗理由 `CellValueFailureReason` の列挙値を実装側で固定し、`CellValueNeeded` 非例外方針と診断ログ必須項目（row/column/reason/count）をレビュー観点に含める。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。

この値返却契約を満たした状態をもって PRE 完了とする。
