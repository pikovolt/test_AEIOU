# P5-007 完了報告 — 回帰・配布手順

## 実施内容

- 最小サンプル拡張の Release x86 ビルド、配置、検出、実行、Undo、除去手順を具体化した。
- 利用者向けに拡張の標準配置、ログ採取、全除去と安全な切り戻しを文書化した。
- 契約不一致、重複 ID、依存不足、constructor 例外、実行例外・不正結果ごとの復旧手順を文書化した。
- Phase 5 smoke の自動検証済み項目と Windows UI で確認を要する項目を区別した。

## 2026-10-03 検証結果

| 確認 | 結果 | 備考 |
|---|---|---|
| Git 差分検査 | 成功 | whitespace error なし |
| サンプル/運用文書の相互リンク | 成功 | 配置・除去・復旧への導線を確認 |
| Debug x86 ビルド | 未実施 | 実行環境に MSBuild/Mono/.NET Framework がない |
| Release x86 ビルド | 未実施 | 実行環境に MSBuild/Mono/.NET Framework がない |
| Automation tests | 未実施 | テスト実行バイナリをビルドできない |
| Windows UI smoke | 未実施 | Linux 非 GUI 環境のため、メニュー/Undo/STS/AE 連携を確認できない |

## Gate 判定

運用文書と再現手順の作成は完了した。P5-007 の最終 Gate である「クリーン環境でサンプル配置→検出→実行→除去」は、Windows + .NET Framework 3.5 のクリーン環境で [`extension_operations.md`](extension_operations.md) のリリース前チェックを実行後に完了とする。未実施の手動項目を推測で完了扱いにはしない。
