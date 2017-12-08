using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signia.OmakaseCategoryFeeder.Model
{
    public static class CategoryExtensions
    {
        class Node
        {
            public List<Node> Children = new List<Node>();
            public Node Parent { get; set; }
            public Category AssociatedObject { get; set; }

            public string GetFullPathFromParenthood()
            {
                var path = AssociatedObject.Name;
                var node = this;
                while (node.Parent != null)
                {
                    path = $"{node.Parent.AssociatedObject.Name}{Category.CFullPathSeparator}{path}";
                    node = node.Parent;
                }
                return path;
            }
        }

        public static void BuildTree(this IList<Category> categories)
        {
            //in fact it brings to assignment of determine FullPath property rely on ParentID dependency
            var nodesTree = BuildTreeAndGetRoots(categories).ToList();
            var nodesTreeFlatten = nodesTree.SelectMany(GetNodeAndDescendants).ToList();
            foreach (var category in categories)
            {
                var node = nodesTreeFlatten.FirstOrDefault(n => n.AssociatedObject == category);
                if (node == null)
                    continue;

                category.ID = node.AssociatedObject.ID;
                category.ParentID = node.AssociatedObject.ParentID;
                category.FullPath = node.GetFullPathFromParenthood();
            }
        }

        static IEnumerable<Node> GetNodeAndDescendants(Node node)
        {
            return new[] { node }
                .Concat(node.Children.SelectMany(GetNodeAndDescendants));
        }

        static IEnumerable<Node> BuildTreeAndGetRoots(IList<Category> actualObjects)
        {
            var lookup = new Dictionary<string, Node>();
            var actualObjectList = actualObjects as List<Category> ?? actualObjects.ToList();
            actualObjectList.ForEach(x => lookup.Add(x.ID, new Node { AssociatedObject = x }));
            foreach (var item in lookup.Values)
            {
                if (lookup.TryGetValue(item.AssociatedObject.ParentID, out var proposedParent))
                {
                    item.Parent = proposedParent;
                    proposedParent.Children.Add(item);
                }
            }
            return lookup.Values.Where(x => x.Parent == null);
        }
    }
}
