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
