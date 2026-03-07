# task-S-3: 元文書の位置付け

## ステータス
- **状態**: 完了
- **更新日**: 2026-03-07

## タスク概要
v1.1 に統合済みの場合、元文書は「議論経緯のアーカイブ」なのか「v1.1 と並行して参照すべき詳細資料」なのかが不明。SSOTの原則からすると正本は v1.1 のみとし、元文書はアーカイブ扱いが妥当だが、明示されていない。

## 詳細分析

### 現状の問題
- **位置付けの曖昧さ**: 元文書2点（「Semantic Drift Issues in Agent Parallelization.md」「AI Debt and LoDD Schedule Design.md」）が v1.1 に統合済みにもかかわらず、それらのドキュメントの扱いが明示されていない
- **SSOT 原則との矛盾**: LoDD では情報の単一真実源（SSOT）を重視する。正本が v1.1 であるなら、元文書は参照の対象外にすべきだが、実際には元文書を参照しないと判断できない箇所が存在する（task-S-4 参照）
- **参照先の混乱**: 作業者が「元文書 vs v1.1 のどちらが正しいか」を判断しなければならない状況が発生しうる

### 影響範囲
- `considerations/refactor_LoDD_001/` 階層内のファイル構成の整理
- `overview.md` での各ファイルの位置付け記述
- 作業者が参照すべきドキュメントの明確化

## 対応方針

### 確認項目
1. 元文書の内容が v1.1 に完全に統合されているか、または一部が未統合か確認（task-S-4 の結果も参照）  
   → **確認済み。主要論点は統合済み。Contract Test の実装詳細（コード例・配置構造・テスト種別テーブル）は元文書にのみ存在する（task-S-4 参照）。**
2. 未統合の情報がある場合、v1.1 への追記が必要か、または元文書を補足資料として明示するかを判断  
   → **判断済み。v1.1 への追記はしない。元文書を「限定的な補足資料」として overview.md に明示する。**
3. アーカイブ扱いにする場合、ファイルへ��注記や overview.md への記載方法を検討  
   → **アーカイブではなく補足資料として扱う。overview.md の位置付け表を更新する。**

### 修正対象
- `considerations/refactor_LoDD_001/overview.md` の各ファイル位置付け表

### 完了条件
- [x] 元文書2点の位置付け（アーカイブ vs 補足資料）が判断された → **補足資料（限定的参照）に決定。**
- [x] 判断内容が `overview.md` に明示された
- [x] SSOT として v1.1 のみを参照すれば作業可能な状態か、または例外箇所が明記された → **例外箇所（拡張パス着手時の Contract Test・Merge Gate 実装詳細）を overview.md に明記。**

## 関連文書
- 統合元: `considerations/refactor_LoDD_001/Semantic Drift Issues in Agent Parallelization.md`
- 統合元: `considerations/refactor_LoDD_001/AI Debt and LoDD Schedule Design.md`
- 正本: `considerations/refactor_LoDD_001/LoDD_Reference_v1.1.md`
- 親タスク: なし
- 依存タスク: S-4（完了済み）

## 備考
task-S-4 の結論が「v1.1 への追記なし・元文書を補足資料として明示」であったため、本タスクも同方針で対応。元文書のアーカイブ化は行わない。