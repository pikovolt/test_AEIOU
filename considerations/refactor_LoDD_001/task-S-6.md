# task-S-6: テスト種別テーブルの v1.1 への追記検討

## ステータス
- **状態**: 未了
- **更新日**: 2026-03-07

## タスク概要
S-4 の対応で「Contract Test の実装詳細は元文書を補足資料として参照」とした。ただし、テスト種別テーブル（Contract Test / Unit Test / Integration Test の使い分け）については、v1.1 の拡張パスセクションに1テーブルを追記するだけで元文書参照の必要がなくなる可能性がある（選択肢X）。この追記が妥当かを改めて検討する。

## 詳細分析

### 対象情報
元文書「Semantic Drift Issues in Agent Parallelization.md」に記載されている以下のテーブル:

| テスト種別 | 検証対象 | 実行タイミング |
|-----------|---------|--------------|
| Contract Test | interfaces/ の契約を満たしているか | 各Agentの成果物の統合前 |
| Unit Test | 各モジュールの個別機能 | 各Agent内のDone条件 |
| Integration Test | 統合後の結合動作 | 全Agent完了後 |

### 現状
- v1.1 の拡張パスセクションには「`tests/contracts/` を新設し統合前に検証する」という目的の記述のみ
- Contract Test・Unit Test・Integration Test の使い分けは v1.1 単体では判断できない

### 検討ポイント
- このテーブル1つの追記で v1.1 単体での運用が可能になるか
- v1.1 の叩き台としての規模感に対して追記量が許容範囲か
- 追記する場合、拡張パスセクションの何行目に挿入するか

## 対応方針

### 確認項目
1. 上記テーブルのみの追記で「Contract Test 着手時の元文書参照」が不要になるか判断する
2. v1.1 の記述量・粒度のバランスを確認し、追記の可否を決定する

### 修正対象（追記する場合）
- `considerations/refactor_LoDD_001/LoDD_Reference_v1.1.md` の拡張パスセクション（Contract Test 行の直下）
- `considerations/refactor_LoDD_001/overview.md` の SSOT 例外箇所テーブル（解消された場合は該当行を削除）

### 完了条件
- [ ] 追記の可否が判断された
- [ ] 追記する場合、v1.1 への反映が完了した
- [ ] 追記する場合、overview.md の SSOT 例外箇所テーブルから該当行を削除した

## 関連文書
- 統合元: `considerations/refactor_LoDD_001/Semantic Drift Issues in Agent Parallelization.md`
- 正本: `considerations/refactor_LoDD_001/LoDD_Reference_v1.1.md`
- 親タスク: なし
- 依存タスク: S-4（完了済み）・S-3（完了済み）
