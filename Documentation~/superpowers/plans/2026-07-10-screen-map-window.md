# Screen Map Window Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build an editor window that visualizes every `ScreenDataSO` in the project (structure, relationships, validation issues, and live play-mode open state) across four tabs, without changing runtime code.

**Architecture:** One UI-agnostic data model (`ScreenGraph` of `ScreenNode`/`ScreenEdge`) built by scanning assets, a pure `ScreenGraphValidator`, and independent renderer tabs behind a `ScreenMapWindow`. Model and validator are unit-tested; the AssetDatabase-coupled builder is a thin adapter. Live open-state comes from a centralized `IScreenNavigator` resolver reused across the package.

**Tech Stack:** Unity 6000.2.13f1 editor scripting (IMGUI + UIToolkit GraphView), C#, NUnit EditMode tests, the package's custom DI/ServiceLocator (`DependencyInjector.Core`, `ServiceLocatorPattern`).

## Global Constraints

- All new code is editor-only, under `Packages/UP-ScreenNavigator/Editor/`, namespace `ScreenNavigators.Editors`. No runtime (`Runtime/`) code changes.
- Unity editor version 6000.2.13f1. GraphView types come from `UnityEditor.Experimental.GraphView`.
- Coding standards (enforced): one type per file; no `else`; no ternaries; guard clauses / early returns; `ReferenceEquals(x, null)` for null checks; verb-first PascalCase methods with role prefixes (`Get…`/`Is…`/`Has…`/`Set…`); no bool parameters (write two methods); no `var` outside `foreach`; `_camelCase` private fields; PascalCase `const`/`static readonly`; no public fields (expose via expression-body property); enums only for small fixed sets; tab dispatch via strategy list, never `switch`.
- **Unity execution note:** subagents cannot compile or run tests. Every "run test" / "verify compiles" step is performed by the human in the Unity Editor (Window ▸ General ▸ Test Runner for EditMode; Console for compile errors) at the review checkpoint after each task.
- Commits go into the package repo on its current branch (`1.4.1`): `git -C Packages/UP-ScreenNavigator …`, commit-message prefixes `[A]` add / `[U]` update / `[F]` fix.
- Tests: EditMode + NUnit, names `MethodName_WhatConditions_DoesWhat`, AAA, one assert per test.

---

## File Structure

```
Packages/UP-ScreenNavigator/
  Editor/ScreenMap/
    Model/
      ScreenEdgeKind.cs           enum { Opens, Closes }
      ScreenEdge.cs               fromId, toId, kind
      ScreenNode.cs               id, assetPath, flags, nested/toClose ids, emptyRefCount, issues, isOpen
      ScreenGraph.cs              nodes + edges + id lookups
      ScreenGraphBuilder.cs       AssetDatabase + SerializedObject -> ScreenGraph
      Validation/
        ValidationSeverity.cs     enum { Warning, Error }
        ValidationIssue.cs        severity, screenId, message
        ScreenGraphValidator.cs   ScreenGraph -> issues (+annotates nodes)
    Runtime/
      EditorScreenNavigatorProvider.cs
      OpenScreensProvider.cs
    Window/
      ScreenMapContext.cs         graph + open-id lookup + ping helper passed to tabs
      IScreenMapTab.cs
      ScreenMapWindow.cs
      Tabs/
        ScreenTreeTabView.cs
        ScreenMonitorTabView.cs
        ScreenBezierTabView.cs
        ScreenGraphViewTabView.cs
      ScreenWarningsPanel.cs
  Tests/EditMode/
    EditMode.Tests.ScreenNavigator.asmdef
    ScreenGraphValidatorTests.cs
```

---

## Task 1: Graph value types (edge kind, edge, severity, issue)

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenEdgeKind.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenEdge.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/Validation/ValidationSeverity.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/Validation/ValidationIssue.cs`

**Interfaces:**
- Produces: `enum ScreenEdgeKind { Opens, Closes }`; `ScreenEdge(string fromScreenId, string toScreenId, ScreenEdgeKind kind)` with `FromScreenId`/`ToScreenId`/`Kind` getters; `enum ValidationSeverity { Warning, Error }`; `ValidationIssue(ValidationSeverity severity, string screenId, string message)` with `Severity`/`ScreenId`/`Message` getters.

- [ ] **Step 1: Create `ScreenEdgeKind.cs`**

```csharp
namespace ScreenNavigators.Editors
{
    public enum ScreenEdgeKind
    {
        Opens,
        Closes
    }
}
```

- [ ] **Step 2: Create `ScreenEdge.cs`**

```csharp
namespace ScreenNavigators.Editors
{
    public class ScreenEdge
    {
        private readonly string _fromScreenId;
        private readonly string _toScreenId;
        private readonly ScreenEdgeKind _kind;

        public string FromScreenId => _fromScreenId;
        public string ToScreenId => _toScreenId;
        public ScreenEdgeKind Kind => _kind;

        public ScreenEdge(string fromScreenId, string toScreenId, ScreenEdgeKind kind)
        {
            _fromScreenId = fromScreenId;
            _toScreenId = toScreenId;
            _kind = kind;
        }
    }
}
```

- [ ] **Step 3: Create `ValidationSeverity.cs`**

```csharp
namespace ScreenNavigators.Editors
{
    public enum ValidationSeverity
    {
        Warning,
        Error
    }
}
```

- [ ] **Step 4: Create `ValidationIssue.cs`**

```csharp
namespace ScreenNavigators.Editors
{
    public class ValidationIssue
    {
        private readonly ValidationSeverity _severity;
        private readonly string _screenId;
        private readonly string _message;

        public ValidationSeverity Severity => _severity;
        public string ScreenId => _screenId;
        public string Message => _message;

        public ValidationIssue(ValidationSeverity severity, string screenId, string message)
        {
            _severity = severity;
            _screenId = screenId;
            _message = message;
        }
    }
}
```

- [ ] **Step 5: Verify compile (human, in Editor)**

Return to Unity. Console shows no compile errors for the four new files.

- [ ] **Step 6: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Model
git -C Packages/UP-ScreenNavigator commit -m "[A] Add screen graph value types (edge, kind, validation issue)"
```

---

## Task 2: `ScreenNode` and `ScreenGraph`

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenNode.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenGraph.cs`

**Interfaces:**
- Consumes: `ValidationIssue` (Task 1).
- Produces: `ScreenNode(string screenId, string assetPath, bool isLocked, bool closesAllScreensOnOpen, List<string> nestedScreenIds, List<string> toCloseScreenIds, int emptyReferenceCount)` with getters `ScreenId`, `AssetPath`, `IsLocked`, `ClosesAllScreensOnOpen`, `NestedScreenIds`, `ToCloseScreenIds`, `EmptyReferenceCount`, `Issues`, `IsOpen`; methods `AddIssue(ValidationIssue)`, `bool HasIssues()`, `MarkOpen()`, `MarkClosed()`. `ScreenGraph(List<ScreenNode> nodes, List<ScreenEdge> edges)` with `Nodes`/`Edges` getters and `bool HasNodeWithId(string)`, `int CountNodesWithId(string)`.

- [ ] **Step 1: Create `ScreenNode.cs`**

```csharp
using System.Collections.Generic;

namespace ScreenNavigators.Editors
{
    public class ScreenNode
    {
        private readonly string _screenId;
        private readonly string _assetPath;
        private readonly bool _isLocked;
        private readonly bool _closesAllScreensOnOpen;
        private readonly List<string> _nestedScreenIds;
        private readonly List<string> _toCloseScreenIds;
        private readonly int _emptyReferenceCount;
        private readonly List<ValidationIssue> _issues;

        private bool _isOpen;

        public string ScreenId => _screenId;
        public string AssetPath => _assetPath;
        public bool IsLocked => _isLocked;
        public bool ClosesAllScreensOnOpen => _closesAllScreensOnOpen;
        public IReadOnlyList<string> NestedScreenIds => _nestedScreenIds;
        public IReadOnlyList<string> ToCloseScreenIds => _toCloseScreenIds;
        public int EmptyReferenceCount => _emptyReferenceCount;
        public IReadOnlyList<ValidationIssue> Issues => _issues;
        public bool IsOpen => _isOpen;

        public ScreenNode(string screenId, string assetPath, bool isLocked, bool closesAllScreensOnOpen,
            List<string> nestedScreenIds, List<string> toCloseScreenIds, int emptyReferenceCount)
        {
            _screenId = screenId;
            _assetPath = assetPath;
            _isLocked = isLocked;
            _closesAllScreensOnOpen = closesAllScreensOnOpen;
            _nestedScreenIds = nestedScreenIds;
            _toCloseScreenIds = toCloseScreenIds;
            _emptyReferenceCount = emptyReferenceCount;
            _issues = new List<ValidationIssue>();
            _isOpen = false;
        }

        public void AddIssue(ValidationIssue issue)
        {
            _issues.Add(issue);
        }

        public bool HasIssues()
        {
            return _issues.Count > 0;
        }

        public void MarkOpen()
        {
            _isOpen = true;
        }

        public void MarkClosed()
        {
            _isOpen = false;
        }
    }
}
```

- [ ] **Step 2: Create `ScreenGraph.cs`**

```csharp
using System.Collections.Generic;

namespace ScreenNavigators.Editors
{
    public class ScreenGraph
    {
        private readonly List<ScreenNode> _nodes;
        private readonly List<ScreenEdge> _edges;

        public IReadOnlyList<ScreenNode> Nodes => _nodes;
        public IReadOnlyList<ScreenEdge> Edges => _edges;

        public ScreenGraph(List<ScreenNode> nodes, List<ScreenEdge> edges)
        {
            _nodes = nodes;
            _edges = edges;
        }

        public bool HasNodeWithId(string screenId)
        {
            foreach (ScreenNode node in _nodes)
            {
                if (node.ScreenId == screenId)
                    return true;
            }

            return false;
        }

        public int CountNodesWithId(string screenId)
        {
            int count = 0;
            foreach (ScreenNode node in _nodes)
            {
                if (node.ScreenId == screenId)
                    count++;
            }

            return count;
        }
    }
}
```

- [ ] **Step 3: Verify compile (human, in Editor)** — Console shows no errors.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Model/ScreenNode.cs Editor/ScreenMap/Model/ScreenGraph.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add ScreenNode and ScreenGraph model"
```

---

## Task 3: `ScreenGraphValidator` (TDD)

**Files:**
- Create: `Packages/UP-ScreenNavigator/Tests/EditMode/EditMode.Tests.ScreenNavigator.asmdef`
- Create: `Packages/UP-ScreenNavigator/Tests/EditMode/ScreenGraphValidatorTests.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/Validation/ScreenGraphValidator.cs`

**Interfaces:**
- Consumes: `ScreenGraph`, `ScreenNode`, `ScreenEdge`, `ScreenEdgeKind`, `ValidationIssue`, `ValidationSeverity`.
- Produces: `ScreenGraphValidator` with `List<ValidationIssue> Validate(ScreenGraph graph)` that also calls `node.AddIssue(...)` on offending nodes.

- [ ] **Step 1: Create the EditMode test asmdef**

`Packages/UP-ScreenNavigator/Tests/EditMode/EditMode.Tests.ScreenNavigator.asmdef`:

```json
{
    "name": "EditMode.Tests.ScreenNavigator",
    "rootNamespace": "",
    "references": [
        "Editor.ScreenNavigator"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Write the failing tests**

`Packages/UP-ScreenNavigator/Tests/EditMode/ScreenGraphValidatorTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using ScreenNavigators.Editors;

namespace ScreenNavigators.Editors.Tests
{
    public class ScreenGraphValidatorTests
    {
        private static ScreenNode CreateNode(string screenId, List<string> nested, List<string> toClose, int emptyReferenceCount)
        {
            return new ScreenNode(screenId, "path", false, false, nested, toClose, emptyReferenceCount);
        }

        [Test]
        public void Validate_CleanGraph_ReturnsNoIssues()
        {
            ScreenNode nodeA = CreateNode("A", new List<string> { "B" }, new List<string>(), 0);
            ScreenNode nodeB = CreateNode("B", new List<string>(), new List<string>(), 0);
            List<ScreenEdge> edges = new List<ScreenEdge> { new ScreenEdge("A", "B", ScreenEdgeKind.Opens) };
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { nodeA, nodeB }, edges);

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Validate_EmptyScreenId_ReturnsErrorIssue()
        {
            ScreenNode node = CreateNode("", new List<string>(), new List<string>(), 0);
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { node }, new List<ScreenEdge>());

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.AreEqual(ValidationSeverity.Error, issues[0].Severity);
        }

        [Test]
        public void Validate_DuplicateScreenId_ReturnsIssueForEachDuplicate()
        {
            ScreenNode first = CreateNode("Dup", new List<string>(), new List<string>(), 0);
            ScreenNode second = CreateNode("Dup", new List<string>(), new List<string>(), 0);
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { first, second }, new List<ScreenEdge>());

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.AreEqual(2, issues.Count);
        }

        [Test]
        public void Validate_EmptyReferenceSlot_ReturnsWarningIssue()
        {
            ScreenNode node = CreateNode("A", new List<string>(), new List<string>(), 1);
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { node }, new List<ScreenEdge>());

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.AreEqual(ValidationSeverity.Warning, issues[0].Severity);
        }

        [Test]
        public void Validate_ReferenceToUnknownScreen_ReturnsWarningIssue()
        {
            ScreenNode node = CreateNode("A", new List<string> { "Ghost" }, new List<string>(), 0);
            List<ScreenEdge> edges = new List<ScreenEdge> { new ScreenEdge("A", "Ghost", ScreenEdgeKind.Opens) };
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { node }, edges);

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.AreEqual(ValidationSeverity.Warning, issues[0].Severity);
        }

        [Test]
        public void Validate_NestedCycle_ReturnsErrorIssue()
        {
            ScreenNode nodeA = CreateNode("A", new List<string> { "B" }, new List<string>(), 0);
            ScreenNode nodeB = CreateNode("B", new List<string> { "A" }, new List<string>(), 0);
            List<ScreenEdge> edges = new List<ScreenEdge>
            {
                new ScreenEdge("A", "B", ScreenEdgeKind.Opens),
                new ScreenEdge("B", "A", ScreenEdgeKind.Opens)
            };
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { nodeA, nodeB }, edges);

            List<ValidationIssue> issues = new ScreenGraphValidator().Validate(graph);

            Assert.IsTrue(issues.Exists(issue => issue.Severity == ValidationSeverity.Error));
        }

        [Test]
        public void Validate_OffendingNode_IsAnnotatedWithIssue()
        {
            ScreenNode node = CreateNode("", new List<string>(), new List<string>(), 0);
            ScreenGraph graph = new ScreenGraph(new List<ScreenNode> { node }, new List<ScreenEdge>());

            new ScreenGraphValidator().Validate(graph);

            Assert.IsTrue(node.HasIssues());
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail (human, in Editor)**

Unity ▸ Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All. Expected: compile error / FAIL — `ScreenGraphValidator` does not exist yet.

- [ ] **Step 4: Implement `ScreenGraphValidator.cs`**

```csharp
using System.Collections.Generic;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphValidator
    {
        public List<ValidationIssue> Validate(ScreenGraph graph)
        {
            List<ValidationIssue> issues = new List<ValidationIssue>();

            AddEmptyIdIssues(graph, issues);
            AddDuplicateIdIssues(graph, issues);
            AddReferenceIssues(graph, issues);
            AddCycleIssues(graph, issues);

            return issues;
        }

        private void AddEmptyIdIssues(ScreenGraph graph, List<ValidationIssue> issues)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                if (!string.IsNullOrEmpty(node.ScreenId))
                    continue;

                AddIssue(issues, node, ValidationSeverity.Error, "Screen id is empty.");
            }
        }

        private void AddDuplicateIdIssues(ScreenGraph graph, List<ValidationIssue> issues)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                if (string.IsNullOrEmpty(node.ScreenId))
                    continue;

                if (graph.CountNodesWithId(node.ScreenId) < 2)
                    continue;

                AddIssue(issues, node, ValidationSeverity.Error, "Duplicate screen id '" + node.ScreenId + "'.");
            }
        }

        private void AddReferenceIssues(ScreenGraph graph, List<ValidationIssue> issues)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                if (node.EmptyReferenceCount > 0)
                    AddIssue(issues, node, ValidationSeverity.Warning,
                        "References " + node.EmptyReferenceCount + " missing or empty screen(s).");

                AddUnknownReferenceIssues(graph, node, node.NestedScreenIds, issues);
                AddUnknownReferenceIssues(graph, node, node.ToCloseScreenIds, issues);
            }
        }

        private void AddUnknownReferenceIssues(ScreenGraph graph, ScreenNode node,
            IReadOnlyList<string> referencedIds, List<ValidationIssue> issues)
        {
            foreach (string referencedId in referencedIds)
            {
                if (graph.HasNodeWithId(referencedId))
                    continue;

                AddIssue(issues, node, ValidationSeverity.Warning, "References unknown screen '" + referencedId + "'.");
            }
        }

        private void AddCycleIssues(ScreenGraph graph, List<ValidationIssue> issues)
        {
            Dictionary<string, List<string>> opensAdjacency = GetOpensAdjacency(graph);
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> nodesInCycle = new HashSet<string>();
            List<string> stack = new List<string>();

            foreach (ScreenNode node in graph.Nodes)
            {
                if (visited.Contains(node.ScreenId))
                    continue;

                FindCycles(node.ScreenId, opensAdjacency, visited, stack, nodesInCycle);
            }

            foreach (ScreenNode node in graph.Nodes)
            {
                if (!nodesInCycle.Contains(node.ScreenId))
                    continue;

                AddIssue(issues, node, ValidationSeverity.Error, "Nested screen cycle detected.");
            }
        }

        private void FindCycles(string screenId, Dictionary<string, List<string>> adjacency,
            HashSet<string> visited, List<string> stack, HashSet<string> nodesInCycle)
        {
            visited.Add(screenId);
            stack.Add(screenId);

            if (adjacency.ContainsKey(screenId))
            {
                foreach (string neighborId in adjacency[screenId])
                {
                    int stackIndex = stack.IndexOf(neighborId);
                    if (stackIndex >= 0)
                    {
                        MarkCycle(stack, stackIndex, nodesInCycle);
                        continue;
                    }

                    if (visited.Contains(neighborId))
                        continue;

                    FindCycles(neighborId, adjacency, visited, stack, nodesInCycle);
                }
            }

            stack.RemoveAt(stack.Count - 1);
        }

        private void MarkCycle(List<string> stack, int fromIndex, HashSet<string> nodesInCycle)
        {
            for (int i = fromIndex; i < stack.Count; i++)
            {
                nodesInCycle.Add(stack[i]);
            }
        }

        private Dictionary<string, List<string>> GetOpensAdjacency(ScreenGraph graph)
        {
            Dictionary<string, List<string>> adjacency = new Dictionary<string, List<string>>();
            foreach (ScreenEdge edge in graph.Edges)
            {
                if (edge.Kind != ScreenEdgeKind.Opens)
                    continue;

                if (!adjacency.ContainsKey(edge.FromScreenId))
                    adjacency.Add(edge.FromScreenId, new List<string>());

                adjacency[edge.FromScreenId].Add(edge.ToScreenId);
            }

            return adjacency;
        }

        private void AddIssue(List<ValidationIssue> issues, ScreenNode node, ValidationSeverity severity, string message)
        {
            ValidationIssue issue = new ValidationIssue(severity, node.ScreenId, message);
            issues.Add(issue);
            node.AddIssue(issue);
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass (human, in Editor)** — All 7 tests PASS.

- [ ] **Step 6: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Model/Validation/ScreenGraphValidator.cs Tests/EditMode
git -C Packages/UP-ScreenNavigator commit -m "[A] Add screen graph validator with EditMode tests"
```

---

## Task 4: `ScreenGraphBuilder` (asset scan)

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Model/ScreenGraphBuilder.cs`

**Interfaces:**
- Consumes: `ScreenDataSO` (`ScreenNavigators.Core`, has public `ScreenId`), `ScreenGraph`, `ScreenNode`, `ScreenEdge`, `ScreenEdgeKind`.
- Produces: `ScreenGraphBuilder` with `ScreenGraph Build()`. Reads owner private fields `_isLocked`, `_hasToCloseAllScreensOnOpen`, `_nestedScreens`, `_toCloseScreens` via `SerializedObject`; reads referenced targets' ids via the public `ScreenId` property; counts null reference slots into `emptyReferenceCount`.

- [ ] **Step 1: Create `ScreenGraphBuilder.cs`**

```csharp
using System.Collections.Generic;
using ScreenNavigators.Core;
using UnityEditor;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphBuilder
    {
        public ScreenGraph Build()
        {
            List<ScreenNode> nodes = new List<ScreenNode>();
            List<ScreenEdge> edges = new List<ScreenEdge>();

            string[] guids = AssetDatabase.FindAssets("t:ScreenDataSO");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScreenDataSO screenData = AssetDatabase.LoadAssetAtPath<ScreenDataSO>(assetPath);
                if (ReferenceEquals(screenData, null))
                    continue;

                ScreenNode node = BuildNode(screenData, assetPath);
                nodes.Add(node);
                AddEdges(node, edges);
            }

            return new ScreenGraph(nodes, edges);
        }

        private ScreenNode BuildNode(ScreenDataSO screenData, string assetPath)
        {
            SerializedObject serializedScreen = new SerializedObject(screenData);

            bool isLocked = serializedScreen.FindProperty("_isLocked").boolValue;
            bool closesAll = serializedScreen.FindProperty("_hasToCloseAllScreensOnOpen").boolValue;

            int emptyReferenceCount = 0;
            List<string> nestedIds = GetReferencedScreenIds(serializedScreen.FindProperty("_nestedScreens"), ref emptyReferenceCount);
            List<string> toCloseIds = GetReferencedScreenIds(serializedScreen.FindProperty("_toCloseScreens"), ref emptyReferenceCount);

            return new ScreenNode(screenData.ScreenId, assetPath, isLocked, closesAll, nestedIds, toCloseIds, emptyReferenceCount);
        }

        private List<string> GetReferencedScreenIds(SerializedProperty arrayProperty, ref int emptyReferenceCount)
        {
            List<string> screenIds = new List<string>();
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                ScreenDataSO referenced = element.objectReferenceValue as ScreenDataSO;
                if (ReferenceEquals(referenced, null))
                {
                    emptyReferenceCount++;
                    continue;
                }

                screenIds.Add(referenced.ScreenId);
            }

            return screenIds;
        }

        private void AddEdges(ScreenNode node, List<ScreenEdge> edges)
        {
            foreach (string nestedId in node.NestedScreenIds)
            {
                edges.Add(new ScreenEdge(node.ScreenId, nestedId, ScreenEdgeKind.Opens));
            }

            foreach (string toCloseId in node.ToCloseScreenIds)
            {
                edges.Add(new ScreenEdge(node.ScreenId, toCloseId, ScreenEdgeKind.Closes));
            }
        }
    }
}
```

- [ ] **Step 2: Verify compile (human, in Editor)** — Console shows no errors.

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Model/ScreenGraphBuilder.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add ScreenGraphBuilder scanning ScreenDataSO assets"
```

---

## Task 5: Shared navigator providers + refactor existing call sites

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Runtime/EditorScreenNavigatorProvider.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Runtime/OpenScreensProvider.cs`
- Modify: `Packages/UP-ScreenNavigator/Editor/ScreenDataSOEditor/ScreenDataSOEditor.cs`
- Modify: `Packages/UP-ScreenNavigator/Editor/OpenedScreenMonitor/OpenedScreenMonitor.cs`

**Interfaces:**
- Produces: `EditorScreenNavigatorProvider` with `IScreenNavigator GetScreenNavigator()` (returns null when unresolved). `OpenScreensProvider(EditorScreenNavigatorProvider navigatorProvider)` with `HashSet<string> GetOpenScreenIds()`.

- [ ] **Step 1: Create `EditorScreenNavigatorProvider.cs`**

```csharp
using DependencyInjector.Core;
using ScreenNavigators.Core;
using ServiceLocatorPattern;

namespace ScreenNavigators.Editors
{
    public class EditorScreenNavigatorProvider
    {
        public IScreenNavigator GetScreenNavigator()
        {
            if (!ServiceLocatorInstance.Instance.IsContained<IDIContainer>())
                return null;

            IDIContainer container = ServiceLocatorInstance.Instance.Get<IDIContainer>();

            if (!container.IsTypeContained(typeof(IScreenNavigator)))
                return null;

            return container.Get<IScreenNavigator>();
        }
    }
}
```

- [ ] **Step 2: Create `OpenScreensProvider.cs`**

```csharp
using System.Collections.Generic;
using ScreenNavigators.Core;

namespace ScreenNavigators.Editors
{
    public class OpenScreensProvider
    {
        private readonly EditorScreenNavigatorProvider _navigatorProvider;

        public OpenScreensProvider(EditorScreenNavigatorProvider navigatorProvider)
        {
            _navigatorProvider = navigatorProvider;
        }

        public HashSet<string> GetOpenScreenIds()
        {
            HashSet<string> openScreenIds = new HashSet<string>();

            IScreenNavigator navigator = _navigatorProvider.GetScreenNavigator();
            if (ReferenceEquals(navigator, null))
                return openScreenIds;

            foreach (string screenId in navigator.GetOpenScreenIds())
            {
                openScreenIds.Add(screenId);
            }

            return openScreenIds;
        }
    }
}
```

- [ ] **Step 3: Refactor `ScreenDataSOEditor.cs` to use the provider**

Replace its private `GetScreenNavigator()` method and its field with the shared provider. The class keeps a `private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();`, and `OnInspectorGUI` calls `_navigatorProvider.GetScreenNavigator()`. Remove the now-duplicated `using DependencyInjector.Core;` and `using ServiceLocatorPattern;` if no longer referenced. Full replacement of the resolution method:

Delete:
```csharp
        private IScreenNavigator GetScreenNavigator()
        {
            if (!ServiceLocatorInstance.Instance.IsContained<IDIContainer>())
                return null;

            IDIContainer container = ServiceLocatorInstance.Instance.Get<IDIContainer>();

            if (!container.IsTypeContained(typeof(IScreenNavigator)))
                return null;

            return container.Get<IScreenNavigator>();
        }
```

Add field near the top of the class:
```csharp
        private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();
```

Change the resolve call in `OnInspectorGUI` from `_screenNavigator = GetScreenNavigator();` to:
```csharp
            _screenNavigator = _navigatorProvider.GetScreenNavigator();
```

Then remove `using DependencyInjector.Core;` and `using ServiceLocatorPattern;` from the file (the provider now encapsulates them).

- [ ] **Step 4: Refactor `OpenedScreenMonitor.cs` to use the provider**

In `OnGUI`, replace the inline ServiceLocator resolution block with `_screenNavigator = _navigatorProvider.GetScreenNavigator();`. Add `private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();`. Remove the now-unused `using DependencyInjector.Core;` and `using ServiceLocatorPattern;`. The revised `OnGUI`:

```csharp
        private void OnGUI()
        {
            _screenNavigator = _navigatorProvider.GetScreenNavigator();

            if (ReferenceEquals(_screenNavigator, null))
            {
                GUILayout.Label("No ScreenNavigator instance found.");
                return;
            }

            string[] openScreenIds = _screenNavigator.GetOpenScreenIds();
            foreach (string screenId in openScreenIds)
            {
                GUILayout.Label(screenId);
            }
        }
```

- [ ] **Step 5: Verify compile + spot-check (human, in Editor)** — Console clean; enter Play Mode and confirm the `ScreenDataSO` inspector Open/Close buttons and the existing `Tools/OpenedScreenMonitor` window still work.

- [ ] **Step 6: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Runtime Editor/ScreenDataSOEditor/ScreenDataSOEditor.cs Editor/OpenedScreenMonitor/OpenedScreenMonitor.cs
git -C Packages/UP-ScreenNavigator commit -m "[U] Centralize editor IScreenNavigator resolution into shared provider"
```

---

## Task 6: Tab context + interface

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapContext.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/IScreenMapTab.cs`

**Interfaces:**
- Produces: `ScreenMapContext(ScreenGraph graph, HashSet<string> openScreenIds)` with `Graph` getter, `bool IsScreenOpen(string screenId)`, and `PingScreen(string assetPath)` (selects/pings the asset). `IScreenMapTab { string Title { get; } void Draw(ScreenMapContext context); }`.

- [ ] **Step 1: Create `ScreenMapContext.cs`**

```csharp
using System.Collections.Generic;
using UnityEditor;

namespace ScreenNavigators.Editors
{
    public class ScreenMapContext
    {
        private readonly ScreenGraph _graph;
        private readonly HashSet<string> _openScreenIds;

        public ScreenGraph Graph => _graph;

        public ScreenMapContext(ScreenGraph graph, HashSet<string> openScreenIds)
        {
            _graph = graph;
            _openScreenIds = openScreenIds;
        }

        public bool IsScreenOpen(string screenId)
        {
            return _openScreenIds.Contains(screenId);
        }

        public void PingScreen(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (ReferenceEquals(asset, null))
                return;

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
    }
}
```

Note: add `using Object = UnityEngine.Object;` at the top if the editor complains about the `Object` ambiguity.

- [ ] **Step 2: Create `IScreenMapTab.cs`**

```csharp
namespace ScreenNavigators.Editors
{
    public interface IScreenMapTab
    {
        string Title { get; }
        void Draw(ScreenMapContext context);
    }
}
```

- [ ] **Step 3: Verify compile (human, in Editor)** — no errors.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Window/ScreenMapContext.cs Editor/ScreenMap/Window/IScreenMapTab.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add screen map tab context and interface"
```

---

## Task 7: Warnings panel + Tree tab + Monitor tab

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenWarningsPanel.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/Tabs/ScreenTreeTabView.cs`
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/Tabs/ScreenMonitorTabView.cs`

**Interfaces:**
- Consumes: `ScreenMapContext`, `IScreenMapTab`, `ScreenGraph`, `ScreenNode`, `ValidationIssue`, `ValidationSeverity`.
- Produces: `ScreenWarningsPanel` with `Draw(ScreenGraph graph)`; `ScreenTreeTabView : IScreenMapTab`; `ScreenMonitorTabView : IScreenMapTab`.

- [ ] **Step 1: Create `ScreenWarningsPanel.cs`**

```csharp
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenWarningsPanel
    {
        public void Draw(ScreenGraph graph)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                foreach (ValidationIssue issue in node.Issues)
                {
                    DrawIssue(issue);
                }
            }
        }

        private void DrawIssue(ValidationIssue issue)
        {
            MessageType messageType = GetMessageType(issue.Severity);
            EditorGUILayout.HelpBox("[" + issue.ScreenId + "] " + issue.Message, messageType);
        }

        private MessageType GetMessageType(ValidationSeverity severity)
        {
            if (severity == ValidationSeverity.Error)
                return MessageType.Error;

            return MessageType.Warning;
        }
    }
}
```

- [ ] **Step 2: Create `ScreenTreeTabView.cs`**

```csharp
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenTreeTabView : IScreenMapTab
    {
        private static readonly Color OpenColor = new Color(0.45f, 0.8f, 0.5f);

        private Vector2 _scrollPosition;

        public string Title => "Tree";

        public void Draw(ScreenMapContext context)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (ScreenNode node in context.Graph.Nodes)
            {
                DrawNode(context, node);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawNode(ScreenMapContext context, ScreenNode node)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawNodeHeader(context, node);
            DrawReferences("opens", node.NestedScreenIds);
            DrawReferences("closes", node.ToCloseScreenIds);

            EditorGUILayout.EndVertical();
        }

        private void DrawNodeHeader(ScreenMapContext context, ScreenNode node)
        {
            EditorGUILayout.BeginHorizontal();

            Color previousColor = GUI.color;
            if (context.IsScreenOpen(node.ScreenId))
                GUI.color = OpenColor;

            EditorGUILayout.LabelField(GetNodeTitle(node), EditorStyles.boldLabel);
            GUI.color = previousColor;

            if (GUILayout.Button("Select", GUILayout.Width(60f)))
                context.PingScreen(node.AssetPath);

            EditorGUILayout.EndHorizontal();

            if (node.HasIssues())
                EditorGUILayout.LabelField("  ⚠ " + node.Issues.Count + " issue(s)");
        }

        private string GetNodeTitle(ScreenNode node)
        {
            string title = node.ScreenId;
            if (node.IsLocked)
                title = title + "  [locked]";

            if (node.ClosesAllScreensOnOpen)
                title = title + "  [closeAllOnOpen]";

            return title;
        }

        private void DrawReferences(string label, System.Collections.Generic.IReadOnlyList<string> screenIds)
        {
            if (screenIds.Count == 0)
                return;

            EditorGUILayout.LabelField("   " + label + " → " + string.Join(", ", screenIds));
        }
    }
}
```

- [ ] **Step 3: Create `ScreenMonitorTabView.cs`**

```csharp
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenMonitorTabView : IScreenMapTab
    {
        public string Title => "Monitor";

        public void Draw(ScreenMapContext context)
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see open screens.", MessageType.Info);
                return;
            }

            bool anyOpen = false;
            foreach (ScreenNode node in context.Graph.Nodes)
            {
                if (!context.IsScreenOpen(node.ScreenId))
                    continue;

                anyOpen = true;
                EditorGUILayout.LabelField(node.ScreenId);
            }

            if (!anyOpen)
                EditorGUILayout.LabelField("No screens open.");
        }
    }
}
```

- [ ] **Step 4: Verify compile (human, in Editor)** — no errors.

- [ ] **Step 5: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Window/ScreenWarningsPanel.cs Editor/ScreenMap/Window/Tabs/ScreenTreeTabView.cs Editor/ScreenMap/Window/Tabs/ScreenMonitorTabView.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add warnings panel, tree tab, and monitor tab"
```

---

## Task 8: `ScreenMapWindow` shell + live overlay (Tree + Monitor tabs)

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapWindow.cs`

**Interfaces:**
- Consumes: `ScreenGraphBuilder`, `ScreenGraphValidator`, `EditorScreenNavigatorProvider`, `OpenScreensProvider`, `ScreenMapContext`, `IScreenMapTab`, `ScreenTreeTabView`, `ScreenMonitorTabView`, `ScreenWarningsPanel`.
- Produces: `ScreenMapWindow : EditorWindow` with `[MenuItem("Tools/Screen Map")]`. This task wires only Tree + Monitor tabs; Tasks 9 and 10 append the two graph tabs to the `_tabs` list.

- [ ] **Step 1: Create `ScreenMapWindow.cs`**

```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenMapWindow : EditorWindow
    {
        private readonly ScreenGraphBuilder _graphBuilder = new ScreenGraphBuilder();
        private readonly ScreenGraphValidator _validator = new ScreenGraphValidator();
        private readonly EditorScreenNavigatorProvider _navigatorProvider = new EditorScreenNavigatorProvider();
        private readonly ScreenWarningsPanel _warningsPanel = new ScreenWarningsPanel();
        private readonly List<IScreenMapTab> _tabs = new List<IScreenMapTab>();

        private OpenScreensProvider _openScreensProvider;
        private ScreenGraph _graph;
        private int _activeTabIndex;

        [MenuItem("Tools/Screen Map")]
        public static void ShowWindow()
        {
            ScreenMapWindow window = GetWindow<ScreenMapWindow>();
            window.titleContent = new GUIContent("Screen Map");
            window.Show();
        }

        private void OnEnable()
        {
            _openScreensProvider = new OpenScreensProvider(_navigatorProvider);

            _tabs.Clear();
            _tabs.Add(new ScreenTreeTabView());
            _tabs.Add(new ScreenMonitorTabView());

            RebuildGraph();
            EditorApplication.projectChanged += RebuildGraph;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RebuildGraph;
        }

        private void OnInspectorUpdate()
        {
            if (!Application.isPlaying)
                return;

            Repaint();
        }

        private void RebuildGraph()
        {
            _graph = _graphBuilder.Build();
            _validator.Validate(_graph);
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (ReferenceEquals(_graph, null))
                return;

            ScreenMapContext context = CreateContext();

            _warningsPanel.Draw(_graph);
            _tabs[_activeTabIndex].Draw(context);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < _tabs.Count; i++)
            {
                DrawTabButton(i);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Rebuild", EditorStyles.toolbarButton))
                RebuildGraph();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabButton(int tabIndex)
        {
            bool isActive = _activeTabIndex == tabIndex;
            bool clicked = GUILayout.Toggle(isActive, _tabs[tabIndex].Title, EditorStyles.toolbarButton);
            if (clicked)
                _activeTabIndex = tabIndex;
        }

        private ScreenMapContext CreateContext()
        {
            HashSet<string> openScreenIds = _openScreensProvider.GetOpenScreenIds();
            return new ScreenMapContext(_graph, openScreenIds);
        }
    }
}
```

- [ ] **Step 2: Verify (human, in Editor)** — Open **Tools ▸ Screen Map**. The Tree tab lists every `ScreenDataSO` with flags/relationships; validation HelpBoxes appear for any misconfigured assets; switching to Monitor and entering Play Mode shows open screens updating live; Rebuild refreshes after asset edits.

- [ ] **Step 3: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Window/ScreenMapWindow.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add ScreenMapWindow with tree/monitor tabs and live overlay"
```

---

## Task 9: IMGUI-bezier graph tab

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/Tabs/ScreenBezierTabView.cs`
- Modify: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapWindow.cs` (add the tab to `_tabs`)

**Interfaces:**
- Consumes: `ScreenMapContext`, `IScreenMapTab`, `ScreenNode`, `ScreenEdge`, `ScreenEdgeKind`.
- Produces: `ScreenBezierTabView : IScreenMapTab` (Title `"Graph (IMGUI)"`). Draggable IMGUI nodes with bezier `Opens` (green) / `Closes` (red) edges, laid out on a grid initially, keyed by screen id in a position dictionary.

- [ ] **Step 1: Create `ScreenBezierTabView.cs`**

```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenBezierTabView : IScreenMapTab
    {
        private static readonly Color OpensColor = new Color(0.4f, 0.75f, 0.45f);
        private static readonly Color ClosesColor = new Color(0.85f, 0.45f, 0.45f);
        private static readonly Color OpenNodeColor = new Color(0.45f, 0.8f, 0.5f);
        private static readonly Vector2 NodeSize = new Vector2(140f, 48f);

        private readonly Dictionary<string, Rect> _nodeRects = new Dictionary<string, Rect>();

        private string _draggedScreenId;

        public string Title => "Graph (IMGUI)";

        public void Draw(ScreenMapContext context)
        {
            EnsureLayout(context.Graph);

            Rect canvas = GUILayoutUtility.GetRect(position: default, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUI.Box(canvas, GUIContent.none);

            DrawEdges(context.Graph);
            DrawNodes(context);
            HandleDrag(context.Graph);
        }

        private void EnsureLayout(ScreenGraph graph)
        {
            int index = 0;
            foreach (ScreenNode node in graph.Nodes)
            {
                if (_nodeRects.ContainsKey(node.ScreenId))
                {
                    index++;
                    continue;
                }

                int column = index % 4;
                int row = index / 4;
                Vector2 origin = new Vector2(40f + column * 200f, 60f + row * 110f);
                _nodeRects.Add(node.ScreenId, new Rect(origin, NodeSize));
                index++;
            }
        }

        private void DrawEdges(ScreenGraph graph)
        {
            foreach (ScreenEdge edge in graph.Edges)
            {
                if (!_nodeRects.ContainsKey(edge.FromScreenId))
                    continue;

                if (!_nodeRects.ContainsKey(edge.ToScreenId))
                    continue;

                DrawEdge(_nodeRects[edge.FromScreenId], _nodeRects[edge.ToScreenId], GetEdgeColor(edge.Kind));
            }
        }

        private void DrawEdge(Rect fromRect, Rect toRect, Color color)
        {
            Vector3 start = new Vector3(fromRect.center.x, fromRect.yMax, 0f);
            Vector3 end = new Vector3(toRect.center.x, toRect.yMin, 0f);
            Vector3 startTangent = start + Vector3.up * 40f;
            Vector3 endTangent = end + Vector3.down * 40f;
            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3f);
        }

        private Color GetEdgeColor(ScreenEdgeKind kind)
        {
            if (kind == ScreenEdgeKind.Closes)
                return ClosesColor;

            return OpensColor;
        }

        private void DrawNodes(ScreenMapContext context)
        {
            foreach (ScreenNode node in context.Graph.Nodes)
            {
                DrawNode(context, node);
            }
        }

        private void DrawNode(ScreenMapContext context, ScreenNode node)
        {
            Rect rect = _nodeRects[node.ScreenId];

            Color previousColor = GUI.backgroundColor;
            if (context.IsScreenOpen(node.ScreenId))
                GUI.backgroundColor = OpenNodeColor;

            GUI.Box(rect, GetNodeLabel(node), EditorStyles.helpBox);
            GUI.backgroundColor = previousColor;

            if (GUI.Button(new Rect(rect.x, rect.yMax - 18f, rect.width, 16f), "Select"))
                context.PingScreen(node.AssetPath);
        }

        private string GetNodeLabel(ScreenNode node)
        {
            if (node.HasIssues())
                return node.ScreenId + "  ⚠";

            return node.ScreenId;
        }

        private void HandleDrag(ScreenGraph graph)
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseDown)
                BeginDrag(graph, currentEvent.mousePosition);

            if (currentEvent.type == EventType.MouseUp)
                _draggedScreenId = null;

            if (currentEvent.type != EventType.MouseDrag)
                return;

            if (string.IsNullOrEmpty(_draggedScreenId))
                return;

            Rect rect = _nodeRects[_draggedScreenId];
            rect.position += currentEvent.delta;
            _nodeRects[_draggedScreenId] = rect;
            currentEvent.Use();
        }

        private void BeginDrag(ScreenGraph graph, Vector2 mousePosition)
        {
            foreach (ScreenNode node in graph.Nodes)
            {
                if (!_nodeRects[node.ScreenId].Contains(mousePosition))
                    continue;

                _draggedScreenId = node.ScreenId;
                return;
            }
        }
    }
}
```

- [ ] **Step 2: Register the tab in `ScreenMapWindow.OnEnable`**

Insert after the Tree tab add, before Monitor:

```csharp
            _tabs.Add(new ScreenBezierTabView());
```

- [ ] **Step 3: Verify (human, in Editor)** — The "Graph (IMGUI)" tab shows nodes connected by green/red beziers, nodes draggable, open nodes tinted in Play Mode, Select pings the asset. Adjust node label/handle sizing if overlapping.

- [ ] **Step 4: Commit**

```bash
git -C Packages/UP-ScreenNavigator add Editor/ScreenMap/Window/Tabs/ScreenBezierTabView.cs Editor/ScreenMap/Window/ScreenMapWindow.cs
git -C Packages/UP-ScreenNavigator commit -m "[A] Add IMGUI bezier graph tab"
```

---

## Task 10: GraphView tab + retire standalone monitor menu

**Files:**
- Create: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/Tabs/ScreenGraphViewTabView.cs`
- Modify: `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/ScreenMapWindow.cs`
- Modify: `Packages/UP-ScreenNavigator/Editor/OpenedScreenMonitor/OpenedScreenMonitor.cs` (remove `[MenuItem]`)

**Interfaces:**
- Consumes: `ScreenMapContext`, `IScreenMapTab`, `ScreenGraph`, `ScreenNode`, `ScreenEdge`, `ScreenEdgeKind`; `UnityEditor.Experimental.GraphView`.
- Produces: `ScreenGraphViewTabView : IScreenMapTab` (Title `"Graph (Node)"`). Because GraphView is a UIToolkit element and the other tabs are IMGUI, this tab renders its GraphView into the window through an `IMGUIContainer`-free path: the window hosts one persistent `GraphView` element toggled visible only when this tab is active. See integration note below.

**GraphView integration note for the implementer:** GraphView cannot be drawn from `OnGUI`. Implement this tab as a wrapper that owns a `GraphView` subclass (`ScreenGraphElement : GraphView`) added to `rootVisualElement` once, hidden by default. `ScreenMapWindow` shows/hides that element based on whether the active tab is the GraphView tab, and calls a `Rebuild(ScreenMapContext)` method when the graph changes. The other tabs continue to draw via `OnGUI`. This is the one place IMGUI and UIToolkit coexist; keep the GraphView element's `style.display` in sync with the active tab.

- [ ] **Step 1: Create `ScreenGraphViewTabView.cs`**

```csharp
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphViewTabView : IScreenMapTab
    {
        private static readonly Color OpensColor = new Color(0.4f, 0.75f, 0.45f);
        private static readonly Color ClosesColor = new Color(0.85f, 0.45f, 0.45f);

        private readonly ScreenGraphElement _graphElement;

        public string Title => "Graph (Node)";
        public VisualElement RootElement => _graphElement;

        public ScreenGraphViewTabView()
        {
            _graphElement = new ScreenGraphElement();
            _graphElement.style.flexGrow = 1f;
            _graphElement.style.display = DisplayStyle.None;
        }

        public void Draw(ScreenMapContext context)
        {
        }

        public void Rebuild(ScreenMapContext context)
        {
            _graphElement.Rebuild(context, OpensColor, ClosesColor);
        }

        public void SetVisible()
        {
            _graphElement.style.display = DisplayStyle.Flex;
        }

        public void SetHidden()
        {
            _graphElement.style.display = DisplayStyle.None;
        }
    }
}
```

- [ ] **Step 2: Create the GraphView element (same file region or a nested file `ScreenGraphElement.cs`)**

Create `Packages/UP-ScreenNavigator/Editor/ScreenMap/Window/Tabs/ScreenGraphElement.cs`:

```csharp
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ScreenNavigators.Editors
{
    public class ScreenGraphElement : GraphView
    {
        public ScreenGraphElement()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        }

        public void Rebuild(ScreenMapContext context, Color opensColor, Color closesColor)
        {
            DeleteElements(new List<GraphElement>(graphElements));

            Dictionary<string, Node> nodesById = new Dictionary<string, Node>();
            int index = 0;
            foreach (ScreenNode screenNode in context.Graph.Nodes)
            {
                Node node = CreateNode(screenNode, index);
                AddElement(node);
                if (!nodesById.ContainsKey(screenNode.ScreenId))
                    nodesById.Add(screenNode.ScreenId, node);

                index++;
            }

            foreach (ScreenEdge screenEdge in context.Graph.Edges)
            {
                AddEdge(nodesById, screenEdge, opensColor, closesColor);
            }
        }

        private Node CreateNode(ScreenNode screenNode, int index)
        {
            Node node = new Node();
            node.title = screenNode.ScreenId;
            node.SetPosition(new Rect(40f + (index % 5) * 200f, 60f + (index / 5) * 140f, 160f, 100f));

            Port inputPort = node.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "in";
            node.inputContainer.Add(inputPort);

            Port outputPort = node.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "out";
            node.outputContainer.Add(outputPort);

            node.RefreshPorts();
            node.RefreshExpandedState();
            return node;
        }

        private void AddEdge(Dictionary<string, Node> nodesById, ScreenEdge screenEdge, Color opensColor, Color closesColor)
        {
            if (!nodesById.ContainsKey(screenEdge.FromScreenId))
                return;

            if (!nodesById.ContainsKey(screenEdge.ToScreenId))
                return;

            Node fromNode = nodesById[screenEdge.FromScreenId];
            Node toNode = nodesById[screenEdge.ToScreenId];

            Port outputPort = fromNode.outputContainer[0] as Port;
            Port inputPort = toNode.inputContainer[0] as Port;

            Edge edge = outputPort.ConnectTo(inputPort);
            edge.edgeControl.inputColor = GetEdgeColor(screenEdge.Kind, opensColor, closesColor);
            edge.edgeControl.outputColor = GetEdgeColor(screenEdge.Kind, opensColor, closesColor);
            AddElement(edge);
        }

        private Color GetEdgeColor(ScreenEdgeKind kind, Color opensColor, Color closesColor)
        {
            if (kind == ScreenEdgeKind.Closes)
                return closesColor;

            return opensColor;
        }
    }
}
```

- [ ] **Step 3: Wire the GraphView tab into `ScreenMapWindow`**

Add a `private ScreenGraphViewTabView _graphViewTab;` field. In `OnEnable`, after creating the other tabs:

```csharp
            _graphViewTab = new ScreenGraphViewTabView();
            _tabs.Add(_graphViewTab);
            rootVisualElement.Add(_graphViewTab.RootElement);
```

Add a method that keeps the element's visibility and content in sync, called at the end of `OnGUI` and `RebuildGraph`:

```csharp
        private void SyncGraphViewTab(ScreenMapContext context)
        {
            if (ReferenceEquals(_graphViewTab, null))
                return;

            bool isGraphViewActive = _tabs[_activeTabIndex] == _graphViewTab;
            if (!isGraphViewActive)
            {
                _graphViewTab.SetHidden();
                return;
            }

            _graphViewTab.SetVisible();
            _graphViewTab.Rebuild(context);
        }
```

Call `SyncGraphViewTab(context)` right after `_tabs[_activeTabIndex].Draw(context);` in `OnGUI`. (The IMGUI `OnGUI` still draws the toolbar and warnings; the GraphView element floats above the IMGUI content area only when active — acceptable for v1; refine layout if needed.)

- [ ] **Step 4: Remove the standalone monitor menu**

In `OpenedScreenMonitor.cs`, delete the `[MenuItem("Tools/OpenedScreenMonitor")]` attribute and the `GetWindow()` menu method (the live view now lives in the Screen Map Monitor tab). Leave the class only if referenced elsewhere; otherwise delete the file and its `.meta`. Verify no other code calls `OpenedScreenMonitor.GetWindow()` (the earlier `ScreenDataSOEditor` monitor button did — update it to open Screen Map instead):

In `ScreenDataSOEditor.DrawMonitorButton`, change the call to:
```csharp
            if (GUILayout.Button("Open Screen Map"))
                ScreenMapWindow.ShowWindow();
```
and rename the button label accordingly.

- [ ] **Step 5: Verify (human, in Editor)** — "Graph (Node)" tab shows a draggable/zoomable node graph with colored edges; switching tabs hides/shows it correctly; the old `Tools/OpenedScreenMonitor` menu is gone; the inspector's button now opens Screen Map.

- [ ] **Step 6: Bump version + commit**

Update `Packages/UP-ScreenNavigator/package.json` `version` from `2.6.0` to `2.7.0` (new feature, SemVer minor).

```bash
git -C Packages/UP-ScreenNavigator add Editor package.json
git -C Packages/UP-ScreenNavigator commit -m "[A] Add GraphView tab and retire standalone OpenedScreenMonitor menu"
```

---

## Self-Review

- **Spec coverage:** shared model (Tasks 1–2), SerializedObject-based builder (Task 4), full validator with the four rules + node annotation and tests (Task 3), centralized navigator resolution + refactor of both call sites (Task 5), window shell + Tree/Monitor tabs + live overlay + warnings (Tasks 6–8), IMGUI-bezier tab (Task 9), GraphView tab + monitor fold-in/retire (Task 10), version bump (Task 10). All spec sections map to a task.
- **Placeholder scan:** no TBD/TODO; every code step contains full code. The GraphView tab carries an explicit integration note rather than a placeholder because it is the one IMGUI/UIToolkit boundary; its code is complete and Editor-verified at the checkpoint.
- **Type consistency:** `ScreenNode` constructor arity (7) matches Task 2, builder (Task 4), and tests (Task 3). `IScreenMapTab.Draw(ScreenMapContext)`, `ScreenMapContext.IsScreenOpen`/`PingScreen`/`Graph`, `EditorScreenNavigatorProvider.GetScreenNavigator`, `OpenScreensProvider.GetOpenScreenIds`, `ScreenGraph.HasNodeWithId`/`CountNodesWithId` are used consistently across tasks.
