# Screen Map Window — Design

Date: 2026-07-10
Package: `com.custom.screennavigator` (`Packages/UP-ScreenNavigator`)
Status: Approved for planning

## Summary

An editor window ("Screen Map") that scans every `ScreenDataSO` in the project and shows how the screens are configured and how they relate to one another (which screens open which, which close which), validates the configuration for common mistakes, and highlights which screens are open live during play mode. It folds the existing `OpenedScreenMonitor` live view in as one of its tabs.

## Goals

- Give a single place to see all screens, their flags, and their `opens`/`closes` relationships.
- Catch configuration mistakes that are otherwise invisible: duplicate/empty screen ids, dangling references, and cycles in nested screens.
- Show live open/closed state during play mode.
- Offer three interchangeable visualizations (tree, GraphView node graph, IMGUI-bezier node graph) selectable by tab, plus the folded-in live monitor tab.
- Do all of this without changing any runtime code or exposing `ScreenDataSO` internals just for tooling.

## Non-goals (v1)

- Editing screens from the window. It is view-only; clicking a node pings/selects the asset.
- Persisting node positions between sessions.
- Automatic graph layout beyond a simple initial placement.
- Any change to the runtime `ScreenNavigator`/`ScreenData` behavior.

## Architecture

One shared, UI-agnostic data model, consumed by several independent renderers. The model and validator contain no IMGUI/UIToolkit references so they remain unit-testable.

```
Editor/ScreenMap/
  Model/
    ScreenNode.cs              screenId, isLocked, closeAllOnOpen, nested[], toClose[], issues, isOpen
    ScreenEdge.cs              fromId, toId, ScreenEdgeKind
    ScreenEdgeKind.cs          enum { Opens, Closes }   (fixed 2-case set)
    ScreenGraph.cs             nodes (by id) + edges; query helpers
    ScreenGraphBuilder.cs      scans assets via AssetDatabase + SerializedObject -> ScreenGraph
    Validation/
      ValidationSeverity.cs    enum { Warning, Error }
      ValidationIssue.cs       severity, screenId, message
      ScreenGraphValidator.cs  pure logic: ScreenGraph -> issues (also annotates nodes)
  Runtime/
    EditorScreenNavigatorProvider.cs   resolves IScreenNavigator via ServiceLocator -> IDIContainer
    OpenScreensProvider.cs             live open screen ids in play mode
  Window/
    ScreenMapWindow.cs         EditorWindow; owns model, tab list, rebuild + repaint
    Tabs/
      IScreenMapTab.cs         Title { get; }; Draw(ScreenGraph)
      ScreenTreeTabView.cs
      ScreenGraphViewTabView.cs
      ScreenBezierTabView.cs
      ScreenMonitorTabView.cs
    ScreenWarningsPanel.cs     draws ValidationIssue list; click -> ping asset
```

### Reading screen data (key detail)

`ScreenDataSO` exposes only `ScreenId` publicly; `_nestedScreens`, `_toCloseScreens`, `_isLocked`, and `_hasToCloseAllScreensOnOpen` are private `[SerializeField]` fields. The builder reads them through `SerializedObject`/`SerializedProperty` in the editor. This means:

- No runtime code change and no new public getters exposing internals purely for tooling.
- `SerializedObject` reports an empty/`None` reference slot directly, which is what orphan detection needs. `ScreenDataSO.GetScreenData()` is unsuitable for the builder because it dereferences `_nestedScreens[i].ScreenId` and would `NullReferenceException` on a dangling reference — exactly the case the validator must report.

### Data flow

1. Window opens, or the user clicks Rebuild, or `EditorApplication.projectChanged` fires -> `ScreenGraphBuilder.Build()` runs `AssetDatabase.FindAssets("t:ScreenDataSO")`, loads each asset, reads fields via `SerializedObject`, and produces a `ScreenGraph`.
2. `ScreenGraphValidator.Validate(graph)` returns the issue list and annotates each offending `ScreenNode` so any renderer can mark it.
3. In play mode, `OpenScreensProvider` (using `EditorScreenNavigatorProvider`) supplies the set of currently-open ids; the window marks those nodes open.
4. The active tab draws the graph with overlays: issue nodes marked in the warning/error color, open nodes highlighted green. `ScreenWarningsPanel` lists issues; clicking an issue or node pings/selects the asset.

## Validation rules (v1)

`ScreenGraphValidator` operates on a `ScreenGraph` (no AssetDatabase dependency), so it is fully unit-testable with hand-built graphs.

- Duplicate `screenId` — two assets share an id (silent repository-key collision at runtime).
- Empty `screenId` — id is null/blank.
- Missing/orphan reference — a `nested` or `toClose` slot is empty, or references an id that has no matching asset.
- Nested cycle — a cycle exists in the `Opens` (nested) relationship, detected by DFS back-edge.

Each issue carries a severity; nodes involved are annotated for highlighting.

## Window, tabs, and live overlay

`ScreenMapWindow : EditorWindow` (menu item **Tools/Screen Map**) owns the model, a list of `IScreenMapTab`, and the active tab index. Tab selection uses the strategy list (an `IScreenMapTab` per tab), not a `switch`. It rebuilds on `EditorApplication.projectChanged` and via a manual Rebuild button.

Tabs:
1. Tree — IMGUI foldouts; each screen with flag badges and indented `opens ->` / `closes ->` children.
2. Graph (GraphView) — UIToolkit `GraphView` with a `Node` per screen and green `Opens` / red `Closes` edges.
3. Graph (IMGUI beziers) — hand-drawn, draggable, pannable nodes with bezier edges.
4. Monitor — the folded-in live open-screen list (replaces the standalone `OpenedScreenMonitor` menu).

Live overlay:
- `EditorScreenNavigatorProvider` centralizes the `ServiceLocatorInstance -> IDIContainer -> IScreenNavigator` resolution currently duplicated in `OpenedScreenMonitor` and `ScreenDataSOEditor`. Both existing call sites are refactored to use it (a targeted cleanup of code this feature already touches).
- In play mode the window subscribes to the navigator's `OnScreenOpened` / `OnScreenClosed` to `Repaint` efficiently, and `OpenScreensProvider` marks open nodes. Open nodes are highlighted green across every tab.

## GraphView / IMGUI integration (main risk)

The GraphView tab is UIToolkit; the tree, bezier, and monitor tabs are IMGUI. The window roots in UIToolkit with a toolbar of tab buttons and a content area that swaps between the GraphView element and an `IMGUIContainer` (hosting the active IMGUI tab), toggling visibility per tab. This is the fiddliest part of the implementation. Note also that the GraphView and IMGUI-bezier tabs render the same picture two ways; the bezier tab is intentionally redundant so the two approaches can be compared and the preferred one kept.

## Build order

Front-loads value, back-loads risk. Steps 1-3 produce a usable product; 4-5 add the graph visualizations.

1. Model + builder + validator, with EditMode tests for the validator (cycle, duplicate, empty, orphan).
2. Editor helpers (`EditorScreenNavigatorProvider`, `OpenScreensProvider`); refactor `OpenedScreenMonitor` and `ScreenDataSOEditor` onto the shared provider.
3. Window shell + Tree tab + Monitor tab + Warnings panel + live overlay. (Usable here.)
4. IMGUI-bezier graph tab.
5. GraphView graph tab; retire the standalone `OpenedScreenMonitor` menu item.

If needed, steps 1-3 can ship as v1 and steps 4-5 become a follow-up — a clean cut — but the plan targets all five.

## Testing

- `ScreenGraphValidator` and the `ScreenGraph` model are plain classes with no editor-API dependency, covered by EditMode tests following the repo convention (`MethodName_WhatConditions_DoesWhat`, AAA, one assert per test): duplicate-id detection, empty-id detection, orphan/missing-reference detection, nested-cycle detection, and a clean graph producing no issues.
- `ScreenGraphBuilder` (AssetDatabase/`SerializedObject`-coupled) stays a thin adapter and is exercised manually in the editor.

## Standards notes

- Follows the repo standards: one type per file, no `else`/ternaries, guard clauses, `ReferenceEquals(x, null)`, verb-first methods with role prefixes, no bool parameters, explicit types (no `var` outside `foreach`), `_camelCase` private fields, every type namespaced under `ScreenNavigators.Editors`.
- Enums (`ScreenEdgeKind`, `ValidationSeverity`) are small fixed sets; tab dispatch uses the `IScreenMapTab` strategy list rather than a `switch`.
- All new code is editor-only (under `Editor/`), so nothing ships in player builds.
