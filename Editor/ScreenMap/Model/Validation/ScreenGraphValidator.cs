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
