# task-D-2: 「返済しない」という判断の扱い

## ステータス
- **状態**: 完了
- **更新日**: 2026-03-07

## タスク概要
元文書は「ツールの寿命が返済の判断基準」として `single-shot` なら返済不要と明記。v1.1 では Tool Lifecycle の `debt_policy` に反映されている。しかし損益分岐点テーブルと `debt_policy` の関係が暗黙的。`single-shot` なら LoDD が過剰 → そもそも LoDD を使わない → `debt_policy` が参照されない、という循環がある。

## 詳細分析

### 現状の問題
- **暗黙的な関係**: 損益分岐点テーブル（LoDD を使うべきか否かの判断テーブル）と `debt_policy` の参照関係が v1.1 に明示されていない
- **循環の問題**: 論理的には「`single-shot` → LoDD 不要 → `debt_policy` 参照不要」となるが、この流れが v1.1 に明示されていない。結果として `debt_policy` の記述が宙に浮いた状態になっている
- **実運用での混乱リスク**: LoDD の採用判断ステップと `debt_policy` の参照タイミングが不明瞭なため、実運用時に判断が迷う

### 影響範囲
- `LoDD_Reference_v1.1.md` の Tool Lifecycle セクション（`debt_policy` の定義・参照タイミング）
- 損益分岐点テーブルと `debt_policy` の関係性の記述
- LoDD 採用判断フローの明確化

## 対応方針

### 確認項目
1. 元文書「AI Debt and LoDD Schedule Design.md」の損益分岐点テーブルの内容を確認
   - `single-shot` の定義と返済不要の根拠
   - LoDD 採用判断の条件
2. v1.1 の `debt_policy` の定義箇所と損益分岐点テーブルの位置を確認
3. 「LoDD を使う・使わない」の判断フローにおける `debt_policy` の位置付けを整理
4. 循環を解消するために注記・フロー図・参照リンクのいずれが適切かを判断

### 修正対象
- `LoDD_Reference_v1.1.md` の Tool Lifecycle セクションおよび損益分岐点テーブル周辺
- 損益分岐点テーブルと `debt_policy` の関係を明示する注記または参照

### 完了条件
- [x] 損益分岐点テーブルと `debt_policy` の関係が v1.1 に明示された
- [x] `single-shot` の場合に `debt_policy` が参照されない理由が明確に記述された
- [x] LoDD 採用判断フローにおける `debt_policy` の位置付けが整理された

## 関連文書
- 統合元: `considerations/refactor_LoDD_001/AI Debt and LoDD Schedule Design.md`
- 正本: `considerations/refactor_LoDD_001/LoDD_Reference_v1.1.md`
- 親タスク: なし
- 依存タスク: なし

## 備考
「循環」自体は論理的に正しい（single-shot なら LoDD を使わないので debt_policy は関係ない）が、その前提が v1.1 に明示されていないことが問題。短い注記を損益分岐点テーブルに追加するだけで解消できる可能性が高い。

## 対応記録

### 分析結果
損益分岐点テーブルと Tool Lifecycle / `debt_policy` の関係を v1.1 内で追跡した結果、以下の流れが暗黙的であることを確認した:

1. 損益分岐点テーブルで LoDD 採用を判断（`single-shot` → LoDD 過剰 → 不採用）
2. LoDD を採用した場合のみ `architecture.md` に Tool Lifecycle を明記
3. Tool Lifecycle の `debt_policy` に従って返済ポリシーが決まる

この「LoDD 不採用の場合は `debt_policy` が参照されない」という前提が明示されていなかった。

### 修正内容
`LoDD_Reference_v1.1.md` の損益分岐点テーブル直下に注記（blockquote）を追加し、以下を明示した:
- 「LoDDが過剰」と判断された場合、LoDD 自体を採用しないため `debt_policy` は参照されない
- `debt_policy` は **LoDD 採用後** に機能する仕組みである
- 損益分岐点テーブル → Tool Lifecycle 明記 → `debt_policy` 適用という順序関係

### 判断
備考の想定どおり、短い注記の追加で循環の暗黙性を解消できた。v1.1 の読みやすさへの影響は最小限。
