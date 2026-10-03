# ランダム整数サンプル拡張

外部 DLL discovery の最小サンプルです。選択範囲を、指定した最小値から最大値まで（両端を含む）のランダム整数で埋めます。ステップ数は値を書き込むフレームの間隔で、`1` ならすべてのフレーム、`2` なら1フレーム飛ばしで、選択された各列を処理します。

## 前提

- Visual Studio または MSBuild で .NET Framework 3.5 をビルドできる Windows 環境
- 本体と同じ構成・プラットフォーム（通常は `Release|x86`）

## ビルドと配置

1. Developer Command Prompt でリポジトリの `AEIOU` ディレクトリへ移動し、次を実行します。

   ```bat
   msbuild AEIOU.sln /t:Rebuild /p:Configuration=Release /p:Platform=x86
   ```

2. AEIOU を終了します。
3. `AEIOU.exe` と同じディレクトリに `Extensions` ディレクトリを作成します。
4. `AEIOU.Automation.Sample.Random\bin\x86\Release\AEIOU.Automation.Sample.Random.dll` **だけ**を `Extensions` 直下へコピーします。
5. `AEIOU.exe` と同じディレクトリに、同じビルドで生成された `AEIOU.Automation.Abstractions.dll` があることを確認します。異なるビルドの契約 DLL を混在させないでください。
6. AEIOU を起動し、右クリックメニューの **拡張自動処理 > ランダム整数** を選びます。
7. 最小値、最大値、1以上のステップ数を入力して実行します。選択範囲の各列で指定間隔のセルだけが範囲内の整数になり、Undo 1回で実行前へ戻ることを確認します。

`AEIOU.Automation.Abstractions.dll` は本体と同じ場所にあるものを利用するため、`Extensions` にはサンプル DLL だけを配置してください。拡張 DLL は本体と同じ権限で実行される信頼済みローカルコードとして扱われます。

この実装は `Form1` や WinForms を参照せず、公開契約のスナップショットから変更候補だけを返します。独自の拡張を作る際は、公開された引数なしコンストラクターと一意な小文字 ASCII command ID を維持してください。

## 除去

1. AEIOU を終了します（実行中の DLL は再読み込み/アンロードされません）。
2. `Extensions\AEIOU.Automation.Sample.Random.dll` を削除または `Extensions` の外へ退避します。
3. AEIOU を再起動し、**拡張自動処理 > ランダム整数** が消え、組み込み自動処理が引き続き利用できることを確認します。

検出されない場合や起動後に問題が出た場合は、Phase 5 の
[`extension_operations.md`](../../agent_plans/refactor_form1cs_001/phase5/extension_operations.md)
にあるログ確認・切り戻し手順を参照してください。
