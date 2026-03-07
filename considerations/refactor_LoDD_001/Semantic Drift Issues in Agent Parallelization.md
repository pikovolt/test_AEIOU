
これは非常に本質的な問いです。LoDDの「局所封鎖」が効くのは、**タスクが直列で、人間がゲートキーパーとして介在する**からです。並列化+エージェント駆動にした瞬間、その前提が崩れます。

---

# エージェント並列化におけるセマンティックドリフト問題

## ドリフトの発生メカニズム

まず「何が起きるか」を構造的に整理します。

### 直列（現行LoDD）

```
Task-A → 人間確認 → Task-B → 人間確認 → Task-C
          ↑ここでドリフトを検知・修正できる
```

人間が毎回ゲートを通すので、意味のズレは1タスク分以上蓄積しません。

### 並列（エージェント駆動）

```
         ┌→ Agent-A (mesh処理) ──→ 成果物A
Task分解 ─┼→ Agent-B (material処理) ──→ 成果物B
         └→ Agent-C (export処理) ──→ 成果物C
                                       ↓
                                   統合時に初めて不整合が発覚
```

各Agentは自分の `Context Boundary` の中では「正しく」動いています。しかし、3つの成果物を統合した時に、**それぞれが `interfaces/` の同じ定義を微妙に違う解釈で実装している**という事態が起きます。

これがセマンティックドリフトです。

### ドリフトの3類型

| 類型 | 内容 | 例 |
|------|------|-----|
| **型のドリフト** | 同じ型名を異なる内部表現で扱う | Agent-Aは頂点をfloat配列、Agent-Bはtupleリストで扱っている |
| **意味のドリフト** | 同じ用語を異なる意味で解釈する | 「法線」がAgent-Aではface normal、Agent-Cではvertex normalを指している |
| **前提のドリフト** | 暗黙の前提条件が食い違う | Agent-Aは Y-up 前提、Agent-Cは Z-up 前提で書いている |

厄介なのは、**どれも構文レベルでは正しい**ことです。テストも個別には通ります。統合して初めて壊れます。

---

## 基本対策の考え方

### 原則: 「ドリフトを防ぐ」のではなく「ドリフトを検知可能にする」

並列作業でドリフトをゼロにすることは不可能です。人間同士の並列作業でも起きるものが、LLM同士で起きないはずがありません。

したがって対策の軸は、**ドリフトが統合時まで潜伏しない仕組み**を作ることです。

---

## 対策1: interfaces/ の解像度を上げる — 「解釈の余地」を潰す

現行の interfaces/ は人間がゲートキーパーである前提で、ある程度の暗黙知に頼れています。エージェント並列化では、**暗黙知がそのままドリフトの温床**になります。

### 現行（人間介在前提）

```markdown
## MeshData
- vertices: List[float] — 頂点座標
- normals: List[float] — 法線
```

### 並列対応（解釈の余地を排除）

```markdown
## MeshData
- vertices: List[float]
  - 格納順序: [x0, y0, z0, x1, y1, z1, ...]
  - 座標系: Y-up, 右手系
  - 単位: メートル
  - 空間: ワールド座標（kWorld相当）
- normals: List[float]
  - 種別: per-vertex normal（face normalではない）
  - 格納順序: vertices と同一インデックス対応
  - 正規化: 済み（長さ1.0を保証）
```

**追加すべき情報:**
- 座標系（Y-up / Z-up、左手系 / 右手系）
- 単位系（メートル / センチメートル）
- 空間（ローカル / ワールド）
- 格納順序（インターリーブか分離か）
- 種別の厳密な定義（同じ「法線」でも何を指すか）
- 不変条件（常に成り立つ条件）

これは記述量が増えますが、**並列化のコストとして不可避**です。interfaces/ の1行の曖昧さが、統合時に数時間のデバッグになって返ってきます。

---

## 対策2: Contract Test（契約テスト）の導入

interfaces/ の記述をどれだけ厳密にしても、Agentがそれを正しく解釈した保証はありません。**interfaces/ の記述を実行可能なテストに変換**し、各Agentの成果物を統合前に個別検証します。

```python
# tests/contracts/test_mesh_data_contract.py

def test_vertices_are_flat_float_array(mesh_data):
    """vertices が flat array であること"""
    assert isinstance(mesh_data.vertices, list)
    assert len(mesh_data.vertices) % 3 == 0
    assert all(isinstance(v, float) for v in mesh_data.vertices)

def test_normals_match_vertex_count(mesh_data):
    """normals が vertices と同数であること"""
    assert len(mesh_data.normals) == len(mesh_data.vertices)

def test_normals_are_normalized(mesh_data):
    """各法線ベクトルの長さが1.0であること"""
    for i in range(0, len(mesh_data.normals), 3):
        nx, ny, nz = mesh_data.normals[i:i+3]
        length = (nx**2 + ny**2 + nz**2) ** 0.5
        assert abs(length - 1.0) < 1e-6

def test_coordinate_system_is_y_up(mesh_data):
    """Y-up座標系であること（地面に置いたオブジェクトのY最小値が0付近）"""
    y_values = mesh_data.vertices[1::3]
    assert min(y_values) >= -0.001  # 地面より下に沈んでいない
```

### 配置と運用

```text
tests/
  ├ contracts/          # ← 新設：interfaces/ と1対1対応
  │  ├ test_mesh_data_contract.py
  │  └ test_material_contract.py
  ├ test_obj_exporter.py
  └ test_material_processor.py
```

| テスト種別 | 検証対象 | 実行タイミング |
|-----------|---------|--------------|
| Contract Test | interfaces/ の契約を満たしているか | 各Agentの成果物の統合前 |
| Unit Test | 各モジュールの個別機能 | 各Agent内のDone条件 |
| Integration Test | 統合後の結合動作 | 全Agent完了後 |

**Contract Testが並列化の鍵です。** 各Agentが「自分は正しい」と思っている成果物を、interfaces/ の契約に照らして**統合前に**検証することで、ドリフトの潜伏期間をゼロに近づけます。

---

## 対策3: Shared Constants（共有定数）の物理的分離

ドリフトの中でも最も頻発するのが、**「同じ値を複数箇所でハードコードして食い違う」**パターンです。

```python
# constants/spatial.py — 全Agent共通の参照点

COORDINATE_SYSTEM = "y-up-right-hand"
UNIT = "meter"
SPACE = "world"

# constants/mesh.py
VERTEX_STRIDE = 3  # x, y, z
NORMAL_TYPE = "per-vertex"  # "per-vertex" | "per-face"
```

これを各Agentの Context Boundary の **Read に必ず含める**ことで、「前提の食い違い」を構造的に防ぎます。

```markdown
## Context Boundary
- Read:
  - interfaces/mesh_export.md
  - constants/spatial.py           # 座標系・単位の共有定数
  - constants/mesh.py              # メッシュ仕様の共有定数
```

---

## 対策4: 統合ポイントの明示的設計 — Merge Gate

並列タスクの成果物を統合する地点を、**タスクとして明示的に定義**します。

```markdown
# Task-010: [Merge Gate] mesh + material + export の統合検証

## Status
Not Started

## Prerequisites
- Task-001 (mesh処理) = Done
- Task-002 (material処理) = Done
- Task-003 (export処理) = Done

## Context Boundary
- Read:
  - interfaces/mesh_data.md
  - interfaces/material_data.md
  - interfaces/export_pipeline.md
  - src/core/mesh.py               # Agent-A の成果物
  - src/core/material.py           # Agent-B の成果物
  - src/exporters/obj_exporter.py  # Agent-C の成果物
- Write:
  - tests/integration/test_full_pipeline.py

## Functional Contract
- Done条件:
  - type: hybrid
  - auto: tests/contracts/ 全パス + tests/integration/ 全パス
  - manual: Maya上でエクスポート結果を目視確認

## Merge Gate Checklist
- [ ] 全 Contract Test がパス
- [ ] モジュール間のデータ受け渡しで型変換が発生していない
- [ ] 座標系・単位系の不整合がない
- [ ] 同一概念に対して異なる用語が使われていない
```

**Merge Gate は人間が担当します。** エージェント駆動であっても、統合ポイントだけは人間のゲートキーパーを維持します。これがドリフト検知の最終防衛線です。

---

## LoDDへの組み込みまとめ

ここまでの対策を、LoDDの既存構造への影響度で整理します。

| 対策 | 影響を受ける要素 | 変更の性質 |
|------|----------------|-----------|
| interfaces/ の解像度向上 | `interfaces/` | 既存の拡張（記述量が増える） |
| Contract Test | `tests/contracts/` 新設 | 新規追加 |
| Shared Constants | `constants/` 新設 or `src/` 内に配置 | 新規追加 |
| Merge Gate | `tasks/` に新しいタスク種別 | 既存の拡張 |

### 5つの防壁への追加

| 防壁 | 対処する問題 | 実現手段 |
|------|------------|---------|
| 仕様の迷子防止 | — | `architecture.md` + `AGENTS.md` |
| 契約違反の阻止 | — | `interfaces/` + Context Boundary |
| コンテキスト汚染の防止 | — | `iterations/` の隔離とパージ |
| 過剰修正の防止 | — | Iteration Control の差分指示 |
| 境界突破の検知 | — | Lock-down Rules + Boundary Breach Request |
| **🆕 セマンティックドリフトの検知** | 並列Agentの成果物が意味的に乖離する | Contract Test + Merge Gate |

---

## 現実的な判断として

ただし、率直に言えば、想定スケール（1〜3人、500行×十数モジュール、数日〜10日）において、**エージェント並列化が本当に必要かどうか**は慎重に見極めるべきです。

```
並列化の利得:  タスク実行時間の短縮（分〜時間単位）
並列化のコスト: interfaces/ の厳密化 + Contract Test整備 + Merge Gate運用
```

このスケールでは、直列実行でも1タスクあたり数十分〜数時間で完了します。並列化で節約できる時間と、ドリフト対策の整備・運用コストを天秤にかけると、**現時点では直列のままLoDDを回すほうが「手戻りのなさ」という設計思想に合致する**可能性が高いです。

並列化が正当化されるのは、以下の条件が揃った場合でしょう。

- モジュール間の依存が本当に疎である（interfaces/ で完全に分断できる）
- 各タスクの実行時間が長く、直列では待ち時間が支配的になる
- Contract Testを整備するコストを吸収できる程度にプロジェクトが反復される

LoDDの「重機の設計思想」に立てば、**並列化は速度のための最適化ではなく、依存関係が本当にゼロの場合にのみ許される構造的な判断**として扱うのが一貫しています。