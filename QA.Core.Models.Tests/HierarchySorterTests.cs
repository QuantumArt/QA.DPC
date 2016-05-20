using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Entities;
using QA.Core.Models.Extensions;
using QA.Core.Models.UI;
using System.Collections.Generic;
using QA.Core.Models.Processors;

namespace QA.Core.Models.Tests
{
    [TestClass]
    public class HierarchySorterTests
    {
        [TestMethod]
        public void Test_HierarchySorter_valid_Fields()
        {
            var root = CreateValidTree();

            var tp = GetTp();

            root = tp.ProcessModel(root);

            var result1 = root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null)
                .Select(x => new
                {
                    x.Id,
                    Order = (int?)x.GetField("Order")?.As<PlainArticleField>()?.NativeValue,
                    NewOrder = (int?)x.GetField(tp.Parameter.PropertyToSet)?.As<PlainArticleField>()?.NativeValue
                })
                .OrderBy(x => x.NewOrder)
                .ToList();

            foreach (var item in root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null))
            {
                Assert.IsNotNull(item.GetField(tp.Parameter.PropertyToSet));
            }

            Assert.AreEqual(1, result1[0].Id);
            Assert.AreEqual(11, result1[1].Id);
            Assert.AreEqual(12, result1[2].Id);
            Assert.AreEqual(2, result1[3].Id);
            Assert.AreEqual(21, result1[4].Id);
            Assert.AreEqual(211, result1[5].Id);
        }

        [TestMethod]
        public void Test_HierarchySorter_valid_Fields_independent_orders()
        {
            var root = CreateTreeWithIndependentOrders();

            var tp = GetTp();

            root = tp.ProcessModel(root);

            var result1 = root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null)
                .Select(x => new
                {
                    x.Id,
                    Order = (int?)x.GetField("Order")?.As<PlainArticleField>()?.NativeValue,
                    NewOrder = (int?)x.GetField(tp.Parameter.PropertyToSet)?.As<PlainArticleField>()?.NativeValue
                })
                .OrderBy(x => x.NewOrder)
                .ToList();

            foreach (var item in root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null))
            {
                Assert.IsNotNull(item.GetField(tp.Parameter.PropertyToSet));
            }

            Assert.AreEqual(1, result1[0].Id);
            Assert.AreEqual(11, result1[1].Id);
            Assert.AreEqual(12, result1[2].Id);
            Assert.AreEqual(2, result1[3].Id);
            Assert.AreEqual(21, result1[4].Id);
            Assert.AreEqual(211, result1[5].Id);
        }

        [TestMethod]
        public void Test_HierarchySorter_empty()
        {
            Article root = new Article();

            GetTp().ProcessModel(root);
        }

        [TestMethod]
        public void Test_HierarchySorter_invalid_Fields2()
        {
            Article root = new Article();
            root.Fields.Add("Parameters", new MultiArticleField());

            GetTp().ProcessModel(root);
        }

        [TestMethod]
        public void Test_HierarchySorter_invalid_Fields()
        {
            Article root = new Article();
            root.Fields.Add("Parameters", new SingleArticleField());

            GetTp().ProcessModel(root);
        }

        [TestMethod]
        public void Test_HierarchySorter_missing_null_value()
        {
            Article root = new Article()
                          .AddField<MultiArticleField>("Parameters",
                                  f => f
                                      .AddArticle(new Article(2).AddPlainField("Order", null).AddPlainField("Title", "root 2"))
                                      .AddArticle(new Article(3).AddPlainField("Order", 5).AddPlainField("Title", "root 3"))
                                     );

            root = GetTp().ProcessModel(root);
        }

        [TestMethod]
        public void Test_HierarchySorter_null_parent()
        {
            Article root = new Article()
                          .AddField<MultiArticleField>("Parameters",
                                  f => f
                                      .AddArticle(new Article(1).AddPlainField("Title", "root 1"))
                                      .AddArticle(new Article(3).AddPlainField("Order", 5)
                                        .AddPlainField("Title", "root 3")
                                        .AddArticle("Parent", null))
                                     );

            root = GetTp().ProcessModel(root);
        }
        [TestMethod]
        public void Test_HierarchySorter_missing_fields()
        {
            Article root = new Article()
                          .AddField<MultiArticleField>("Parameters",
                                  f => f
                                      .AddArticle(new Article(1).AddPlainField("Title", "root 1"))
                                      .AddArticle(new Article(3).AddPlainField("Order", 5).AddPlainField("Title", "root 3"))
                                     );

            root = GetTp().ProcessModel(root);
        }

        private static HierarchySorter GetTp()
        {
            return new HierarchySorter(
                            new HierarchySorterParameter
                            {
                                PathToCollection = "/Parameters",
                                ParentRelativePath = "Parent",
                                PathToSortOrder = "Order",
                                PropertyToSet = "HierarchyOrder",
                                Domain = 100
                            }
                        );
        }

        [TestMethod]
        public void Test_HierarchySorter_ConstructHierarchy()
        {
            Article root = new Article()
               .AddField<MultiArticleField>("Parameters",
                       f => f
                           .AddArticle(new Article(2).AddPlainField("Order", 3).AddPlainField("Title", "root 2"))
                           .AddArticle(new Article(21)
                               .AddPlainField("Order", 1)
                               .AddPlainField("Title", "2.1")
                               .AddArticle("Parent", new Article(2)))
                           .AddArticle(new Article(211)
                               .AddPlainField("Order", 1)
                               .AddPlainField("Title", "2.1.1")
                               .AddArticle("Parent", new Article(21)))
                           .AddArticle(new Article(22)
                               .AddPlainField("Order", 2)
                               .AddPlainField("Title", "2.2")
                               .AddArticle("Parent", new Article(2)))
                               );

            var tp = GetTp();

            tp.Parameter.ConstructHierarchy = true;

            root = tp.ProcessModel(root);

            var result1 = root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null)
                .Select(x => new
                {
                    x.Id,
                    Node = x,
                    Order = (int?)x.GetField("Order")?.As<PlainArticleField>().NativeValue,
                    NewOrder = (int?)x.GetField(tp.Parameter.PropertyToSet)?.As<PlainArticleField>().NativeValue
                })
                .OrderBy(x => x.NewOrder)
                .ToList();

            foreach (var item in root
                .GetField("Parameters").As<MultiArticleField>()
                .GetArticles(null))
            {
                Assert.IsNotNull(item.GetField(tp.Parameter.PropertyToSet));
            }


            Assert.AreEqual(2, result1[0].Id);
            Assert.AreEqual(21, result1[1].Id);
            Assert.AreEqual(211, result1[2].Id);
            Assert.AreEqual(22, result1[3].Id);

            var grandParent = result1[2].Node
                .GetField("Parent").As<SingleArticleField>().GetItem(null)
                .GetField("Parent")?.As<SingleArticleField>()?.GetItem(null);

            Assert.IsNotNull(grandParent);
            Assert.AreEqual(2, grandParent.Id);
        }

        private static Article CreateValidTree()
        {
            Article root = new Article()
                .AddField<MultiArticleField>("Parameters",
                        f => f
                            .AddArticle(new Article(1).AddPlainField("Order", 2).AddPlainField("Title", "root 1"))
                            .AddArticle(new Article(2).AddPlainField("Order", 3).AddPlainField("Title", "root 2"))
                            .AddArticle(new Article(3).AddPlainField("Order", 5).AddPlainField("Title", "root 3"))
                            .AddArticle(new Article(11)
                                .AddPlainField("Order", 1)
                                .AddPlainField("Title", "1.1")
                                .AddArticle("Parent", new Article(1)))
                            .AddArticle(new Article(12)
                                .AddPlainField("Order", 2)
                                .AddPlainField("Title", "1.2")
                                .AddArticle("Parent", new Article(1)))
                            .AddArticle(new Article(21)
                                .AddPlainField("Order", 1)
                                .AddPlainField("Title", "2.1")
                                .AddArticle("Parent", new Article(2)))
                            .AddArticle(new Article(211)
                                .AddPlainField("Order", 1)
                                .AddPlainField("Title", "2.1.1")
                                .AddArticle("Parent", new Article(21)))
                            .AddArticle(new Article(22)
                                .AddPlainField("Order", 2)
                                .AddPlainField("Title", "2.2")
                                .AddArticle("Parent", new Article(2)))
                                );

            return root;
        }

        private static Article CreateTreeWithIndependentOrders()
        {
            Article root = new Article()
                .AddField<MultiArticleField>("Parameters",
                        f => f
                            .AddArticle(new Article(1).AddPlainField("Order", 2).AddPlainField("Title", "root 1"))
                            .AddArticle(new Article(2).AddPlainField("Order", 3).AddPlainField("Title", "root 2"))
                            .AddArticle(new Article(3).AddPlainField("Order", 5).AddPlainField("Title", "root 3"))
                            .AddArticle(new Article(11)
                                .AddPlainField("Order", 1111)
                                .AddPlainField("Title", "1.1")
                                .AddArticle("Parent", new Article(1)))
                            .AddArticle(new Article(12)
                                .AddPlainField("Order", 2222)
                                .AddPlainField("Title", "1.2")
                                .AddArticle("Parent", new Article(1)))
                            .AddArticle(new Article(21)
                                .AddPlainField("Order", 1)
                                .AddPlainField("Title", "2.1")
                                .AddArticle("Parent", new Article(2)))
                            .AddArticle(new Article(211)
                                .AddPlainField("Order", 1)
                                .AddPlainField("Title", "2.1.1")
                                .AddArticle("Parent", new Article(21)))
                            .AddArticle(new Article(22)
                                .AddPlainField("Order", 2)
                                .AddPlainField("Title", "2.2")
                                .AddArticle("Parent", new Article(2)))
                                );

            return root;
        }
    }


}
