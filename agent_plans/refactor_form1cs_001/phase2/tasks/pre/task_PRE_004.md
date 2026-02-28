# task_PRE_004: CellValueNeeded導入前設計（移行元: task_VM2_002）

## 目的
`CellValueNeeded` 導入前に、VirtualMode表示に必要な読み出し経路と境界条件を設計として固定化する。

## 実施内容
- `CellValueNeeded` に集約する参照経路を整理する。
- 行列インデックス境界の防御観点を定義する。
- 既存表示ロジックとの差分確認観点を列挙する。

## 値返却契約（CellValueNeeded）
`CellValueNeeded` の値返却は、以下の契約に従って実装・レビューする。

| 入力状態 | 返却値 | 例外有無 | UI挙動 |
|---|---|---|---|
| 正常セル（行・列ともに有効範囲、モデル初期化済み、バインド一致） | 対象セルの表示値（型変換済みの `object`） | 例外なし | 該当セルを通常描画する |
| 空セル（値未設定 / `null`） | `string.Empty`（または表示上の空値） | 例外なし | 空文字として描画し、編集継続可能 |
| 行/列境界外（`rowIndex` / `columnIndex` が範囲外） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、グリッド描画を中断しない |
| モデル未初期化（データソース未ロード / 解放済み） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、画面操作を阻害しない |
| バインド不一致（列定義とモデルプロパティが不整合） | `string.Empty`（フォールバック） | 例外なし | 空表示で継続し、異常はログ/診断経路で検知する |

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

## 依存タスク
- PRE-003

## Done定義参照
- 共通定義 `DONE_TEMPLATE_PRE.md` を参照する。
- 配置場所: `agent_plans/refactor_form1cs_001/phase2/definitions/DONE_TEMPLATE_PRE.md`
- 完了報告は、上記定義の必須欄をすべて埋めること。

## PRルール
- 本タスクのみを変更対象とする（1タスク=1PR）。

この値返却契約を満たした状態をもって PRE 完了とする。
