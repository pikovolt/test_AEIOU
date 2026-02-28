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
| 正常セル（行・列ともに有効範囲、モデル初期化済み、バインド一致） | 対象セルの表示値（型変換済みの `object`） | 例外なし | 該当セルを通常描画する |
| 空セル（値未設定 / `null`） | `string.Empty`（または表示上の空値） | 例外なし | 空文字として描画し、編集継続可能 |
| 行/列境界外（`rowIndex` / `columnIndex` が範囲外） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、グリッド描画を中断しない |
| モデル未初期化（データソース未ロード / 解放済み） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、画面操作を阻害しない |
| バインド不一致（列定義とモデルプロパティが不整合） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、異常はログ/診断経路で検知する |

### 列型ごとの返却ポリシー
- 文字列列: モデル値を `string` として返却し、`null` は `string.Empty` に正規化する。
- 数値列: モデル値を各列が要求する数値型（`int` / `decimal` / `double` 等）のまま返却し、`null` は `DBNull.Value` ではなく表示用フォールバック（`string.Empty`）に統一する。
- 日時列: モデル値を `DateTime`（必要に応じて `DateTime?`）として返却し、未設定 (`null`) は空表示フォールバック（`string.Empty`）を返却する。
- 上記いずれも `CellValueNeeded` 内で例外変換を行わず、返却前に列定義と型の整合性を `TryGetCellValue` で検証する。

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

### `TryGetCellValue` 戻り契約（インターフェース）
`TryGetCellValue` は以下の戻り契約を満たすインターフェースで扱う。

```csharp
public interface ICellValueResolver
{
    bool TryGetCellValue(
        int rowIndex,
        int columnIndex,
        out object? value,
        out CellValueFailureReason failureReason);
}

public enum CellValueFailureReason
{
    None,
    OutOfRange,
    ModelUninitialized,
    BindingMismatch,
    NullValue,
    Unknown
}
```

- `bool` は成功可否を示し、`true` の場合のみ `value` を有効値として扱う。
- `value` は成功時に列型ポリシーに従った値を返し、失敗時は `null`（または呼び出し側で無視可能な値）とする。
- `failureReason` は失敗理由を必ず返し、成功時は `CellValueFailureReason.None` とする。
- `CellValueNeeded` は `TryGetCellValue == false` の場合にフォールバック値を返し、同時に診断ログ必須項目を記録する。

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
- 共通定義 `DONE_TEMPLATE_PRE.md` の必須欄がすべて記入済みである。

## 参照入力（依存最小化）
- `task_PRE_003.md` の `実装ゲートチェックリスト`（必要時のみ参照）

> PRE-003 の完了待ちは不要。`CellValueNeeded` 設計に必要な範囲のみ参照して確定する。

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。

この値返却契約を満たした状態をもって PRE 完了とする。
