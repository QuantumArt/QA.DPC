using System;
using System.Linq;
using QA.Core.Models.Entities;
using QA.Core.Models.Extensions;

namespace QA.Core.Models.Processors
{
    public class HierarchySorter : ModelPostProcessorBase<HierarchySorterParameter>
    {
        public HierarchySorter(HierarchySorterParameter parameter)
        {
            Parameter = parameter;
        }

        public class AdditionalInfo
        {
            public int Order { get; set; }
            public int NewOrder { get; set; }
        }

        public class ModelNode : TreeNode<Article, int, AdditionalInfo>
        {
        }

        protected override Article ProcessModel(Article input, HierarchySorterParameter parameter)
        {
            var foundCollections = DPathProcessor.Process(parameter.PathToCollection, input);
            foreach (var collection in foundCollections)
            {
                var maxLevel = 0;
                var c = collection.ModelObject as MultiArticleField;
                if (c == null)
                {
                    continue;
                }

                var nodes = c.Items.Values.Select(x => new ModelNode
                {
                    Node = x,
                    Key = x.Id,
                    ParentId = x.GetField(parameter.ParentRelativePath)?.As<SingleArticleField>()?.Item?.Id,
                    Data = new AdditionalInfo
                    {
                        Order = (int)(x.GetField(parameter.PathToSortOrder)?.As<PlainArticleField>()?.NativeValue ?? 0)
                    }
                }).ToDictionary(k => k.Key, y => y);

                foreach (var node in nodes.Values)
                {
                    if (node.ParentId != null)
                    {
                        ModelNode p;
                        if (nodes.TryGetValue(node.ParentId.Value, out p))
                        {
                            node.SetParent(p);
                        }
                    }

                    var i = node.EnumerateParents().Count();
                    if (maxLevel < i)
                    {
                        maxLevel = i;
                    }
                }

                var j = 0;
                foreach (var item in nodes.Values.Where(x => x.Parent == null).OrderBy(x => x.Data.Order).ThenBy(x => x.Key))
                {
                    ProcessRecursive(item, maxLevel - 1, 0, parameter, ++j);
                }
            }

            return input;
        }

        private static void ProcessRecursive(TreeNode<Article, int, AdditionalInfo> item, int maxLevel, int level, HierarchySorterParameter parameter, int naturalOrder)
        {
            var order = naturalOrder * Math.Pow(parameter.Domain, maxLevel - level);
            if (item.Parent != null)
            {
                order += item.Parent.Data.NewOrder;
                if (parameter.ConstructHierarchy)
                {
                    var parentField = item.Node.GetField(parameter.ParentRelativePath) as SingleArticleField;
                    if (parentField != null)
                    {
                        parentField.Item = item.Parent.Node;
                    }
                }
            }

            item.Data.NewOrder = (int)order;
            item.Node.AddPlainField(parameter.PropertyToSet, item.Data.NewOrder);

            var j = 0;
            foreach (var child in item.Children.OrderBy(x => x.Data.Order).ThenBy(x => x.Key))
            {
                ProcessRecursive(child, maxLevel, level + 1, parameter, ++j);
            }
        }
    }

    public class HierarchySorterParameter
    {
        public string PathToCollection { get; set; }

        public string ParentRelativePath { get; set; }

        public string PathToSortOrder { get; set; }

        public string PropertyToSet { get; set; }

        public int Domain { get; set; }

        public bool ConstructHierarchy { get; set; }
    }
}
