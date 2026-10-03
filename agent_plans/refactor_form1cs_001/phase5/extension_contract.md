# Automation Extension Contract（設計案）

## 1. 境界原則
- 拡張は `Form1`、`DataGridView`、`TimingSheetModel`、`Settings`、`UndoManager` を参照しない。
- 入力は読み取り専用スナップショット、出力は変更要求の一覧とする。
- 変更の適用、Undo group、使用数再計算、継続表示の再計算、Invalidate は host だけが行う。
- コマンド実行中にシートを変更しない。成功結果を検証した後、1回の write group で原子的に反映する。

## 2. アセンブリ構成案
### `AEIOU.Automation.Abstractions.dll`
.NET Framework 3.5 を対象とする小さな公開契約。次だけを含める。

- `IAutomationCommand`
- `AutomationCommandDescriptor`
- `AutomationParameterDefinition`
- `AutomationRequest`
- `AutomationSelection`
- `AutomationCell` / 読み取り専用セル集合
- `AutomationChange`
- `AutomationResult`
- 契約バージョン定数

### 本体アセンブリ
- `AutomationHost`: 入力収集、検証、適用、Undo、エラー表示。
- `AutomationRegistry`: 組み込み/外部コマンドの登録と ID 一意性保証。
- `BuiltIn.*Command`: 既存6経路の純粋な変換実装。
- `ExtensionLoader`: DLL 探索と型生成。P5-006 までは導入しない。

## 3. コマンド面（疑似コード）
```csharp
public interface IAutomationCommand
{
    AutomationCommandDescriptor Descriptor { get; }
    AutomationResult Execute(AutomationRequest request);
}
```

`AutomationRequest` に含める情報:
- シートの列数・行数。
- 選択矩形。
- 選択範囲内のセル値スナップショット。
- コマンドのパラメーター値（文字列辞書）。
- 必要最小限の読み取り専用設定値。初期対象ではカラセル文字列のみ。

`AutomationResult` に含める情報:
- 成功/失敗。
- `AutomationChange(row, column, value)` の一覧。
- 利用者向け検証エラー。例外を通常の入力エラー表現として使わない。
- 任意の結果メッセージ。

## 4. パラメーター UI
外部拡張に Form を生成させず、descriptor のパラメーター定義から host が入力 UI を構築する。

初版で許可する型:
- `String`
- `Int32`
- `Boolean`
- 選択肢（固定文字列集合）

定義には安定 ID、表示名、既定値、必須、最小/最大を持たせる。複雑な独自 UI は Phase5 の対象外とする。既存 `InputBox1` / `RepeatInputBox` は移行中の adapter として残してよいが、コマンド本体は各 Form 型を知らないこと。

## 5. Host の必須検証
結果適用前に全変更を検査し、1件でも不正なら全件拒否する。

- row/column がシート範囲内か。
- 同じセルへの変更が重複していないか。
- 値が null でないか。
- 変更件数が host 上限以下か。
- コマンド ID と契約バージョンが有効か。
- 実行開始後に対象シート/選択の世代が変わっていないか。

拡張の `Execute` 例外はコマンド ID と共に記録し、UI には失敗を通知する。例外発生前後でセルを変更しない設計にする。

## 6. 現行6経路の契約化メモ
| コマンド | 選択 | パラメーター | 固定すべき現行挙動 |
|---|---|---|---|
| 四則演算 | 複数列・複数行 | 演算子、整数 | 空セル/カラセルを無視。0セルの乗除算、0除算の現状を P5-001 で明文化してから変更判断 |
| 反転 | 1列 | なし | 空セル位置を維持し、非空値だけ逆順にする |
| 置換 | 1列 | 置換前、置換後 | 完全一致のみ。空の検索/置換値は拒否 |
| 連番作成 | 1列 | 開始、ステップ、番号を飛ばす | `step` の正負と `skip` の意味、`step=0` の扱いを固定 |
| 繰り返し入力 | 複数列 | 開始、終了、行間隔、ループ、スキップ、挿入値 | 番号カウンターは列をまたいで継続。選択範囲クリアと入力を1 Undo groupにする |
| 繰り返し起動 | UIのみ | 上記入力 | モデルレスで再実行時に直前結果を Undo する現行操作を host session として表現 |

曖昧点は「ついでに修正」しない。P5-001 で現行互換を Gate にするか、仕様変更タスクへ分ける。

## 7. 外部 DLL discovery
- 既定では実行ファイル横の `Extensions` 直下だけを探索する。再帰探索しない。
- DLL ごとのロード失敗は記録して次へ進み、本体起動を妨げない。
- public かつ非 abstract、引数なし constructor を持つ `IAutomationCommand` 実装だけを候補とする。
- command ID は永続的な ASCII ID とし、組み込みを含め重複時は外部側を無効化する。
- 契約 major 不一致はロード拒否、minor は後方互換範囲のみ許可する。
- 読み込み結果は起動時に固定し、実行中の再ロード/アンロードは行わない。

## 8. 互換性と配布上の注意
- 対象ランタイムは本体と同じ .NET Framework 3.5 とする。
- 契約アセンブリへ型を追加するときは既存 constructor/interface を壊さない。
- 拡張は信頼済みコードであり、ファイル/ネットワーク等の OS 権限を本体と共有することを利用者へ明記する。
- 障害調査用に DLL パス、command ID、契約バージョン、例外概要をローカルログへ残す。セル内容は既定でログへ出さない。
