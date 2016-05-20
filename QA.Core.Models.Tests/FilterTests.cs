using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Entities;
using QA.Core.Models.Filters;
using QA.Core.Models.Filters.Base;

namespace QA.Core.Models.Tests
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        [TestCategory("Filters")]
        public void Test_VisibleFilter_Filters()
        {
            var visible = new Article { Visible = true, Id = 1 };
            var invisible = new Article { Visible = false, Id = 2 };

            var filter = new VisibleFilter();

            Assert.IsTrue(filter.Matches(visible));
            Assert.IsFalse(filter.Matches(invisible));

            var filtered = filter.FilterArticles(visible, invisible);

            Assert.AreEqual(1, filtered.Length);
            Assert.AreEqual(1, filtered[0].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_PublishedFilter_Filters()
        {
            var pub = new Article { IsPublished = true, Id = 1 };
            var notPub = new Article { IsPublished = false, Id = 2 };

            var filter = new PublishedFilter();

            Assert.IsTrue(filter.Matches(pub));
            Assert.IsFalse(filter.Matches(notPub));

            var filtered = filter.FilterArticles(pub, notPub);

            Assert.AreEqual(1, filtered.Length);
            Assert.AreEqual(1, filtered[0].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_ArchivedFilter_Filters()
        {
            var archived = new Article { Archived = true, Id = 1 };
            var notArchived = new Article { Archived = false, Id = 2 };

            var filter = new ArchivedFilter();

            Assert.IsTrue(filter.Matches(archived));
            Assert.IsFalse(filter.Matches(notArchived));

            var filtered = filter.FilterArticles(archived, notArchived);

            Assert.AreEqual(1, filtered.Length);
            Assert.AreEqual(1, filtered[0].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_NotArchivedFilter_Filters()
        {
            var archived = new Article { Archived = true, Id = 1 };
            var notArchived = new Article { Archived = false, Id = 2 };

            var filter = new NotArchivedFilter();

            Assert.IsTrue(filter.Matches(notArchived));
            Assert.IsFalse(filter.Matches(archived));

            var filtered = filter.FilterArticles(archived, notArchived);

            Assert.AreEqual(1, filtered.Length);
            Assert.AreEqual(2, filtered[0].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_DefaultFilterFilter_Filters()
        {
            var archived = new Article { Archived = true, Visible = true, Id = 1 };
            var archivedInvisible = new Article { Archived = true, Visible = false, Id = 2 };
            var notArchivedVisible = new Article { Archived = false, Visible = true, Id = 3 };
            var notArchivedVisiblePublished = new Article { Archived = false, Visible = true, IsPublished = true, Id = 4 };
            var notArchivedInvisiblePublished = new Article { Archived = false, Visible = false, IsPublished = true, Id = 5 };
            var notArchivedInvisibleNotPublished = new Article { Archived = false, Visible = false, IsPublished = true, Id = 6 };

            var filter = ArticleFilter.DefaultFilter;

            Assert.IsTrue(filter.Matches(notArchivedVisible), "not archived visible");
            Assert.IsTrue(filter.Matches(notArchivedVisiblePublished), "not archived visible published");

            var filtered = filter.FilterArticles(archived, archivedInvisible, notArchivedVisible, notArchivedVisiblePublished, notArchivedInvisiblePublished, notArchivedInvisibleNotPublished);

            Assert.AreEqual(2, filtered.Length);
            Assert.AreEqual(3, filtered[0].Id);
            Assert.AreEqual(4, filtered[1].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_LiveFilterFilter_Filters()
        {
            var archived = new Article { Archived = true, Visible = true, Id = 1 };
            var archivedInvisible = new Article { Archived = true, Visible = false, Id = 2 };
            var notArchivedVisible = new Article { Archived = false, Visible = true, Id = 3 };
            var notArchivedVisiblePublished = new Article { Archived = false, Visible = true, IsPublished = true, Id = 4 };
            var notArchivedInvisiblePublished = new Article { Archived = false, Visible = false, IsPublished = true, Id = 5 };
            var notArchivedInvisibleNotPublished = new Article { Archived = false, Visible = false, IsPublished = true, Id = 6 };

            var filter = ArticleFilter.LiveFilter;

            Assert.IsTrue(filter.Matches(notArchivedVisiblePublished), "not archived visible published");

            var filtered = filter.FilterArticles(archived, archivedInvisible, notArchivedVisible, notArchivedVisiblePublished, notArchivedInvisiblePublished, notArchivedInvisibleNotPublished);

            Assert.AreEqual(1, filtered.Length);
            Assert.AreEqual(4, filtered[0].Id);
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_AllFilterFilter_Returns_False_If_Any_returns_false()
        {
            var filter = new AllFilter(new DelegateFilter(x => true), new DelegateFilter(x => true), new DelegateFilter(x => false));
            var article = new Article();


            Assert.IsFalse(filter.Matches(article));
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_AllFilterFilter_Returns_true()
        {
            var filter = new AllFilter(new DelegateFilter(x => true), new DelegateFilter(x => true), new DelegateFilter(x => true));
            var article = new Article();


            Assert.IsTrue(filter.Matches(article));
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_AnyFilterFilter_Returns_False_If_Any_returns_false()
        {
            var filter = new AnyFilter(new DelegateFilter(x => false), new DelegateFilter(x => false), new DelegateFilter(x => false));
            var article = new Article();


            Assert.IsFalse(filter.Matches(article));
        }


        [TestMethod]
        [TestCategory("Filters")]
        public void Test_AnyFilterFilter_Returns_true1()
        {
            var filter = new AnyFilter(new DelegateFilter(x => true), new DelegateFilter(x => false), new DelegateFilter(x => true));
            var article = new Article();


            Assert.IsTrue(filter.Matches(article));
        }

        [TestMethod]
        [TestCategory("Filters")]
        public void Test_AnyFilterFilter_Returns_true2()
        {
            var filter = new AnyFilter(new DelegateFilter(x => true), new DelegateFilter(x => true), new DelegateFilter(x => true));
            var article = new Article();


            Assert.IsTrue(filter.Matches(article));
        }

    }


    internal static class AC
    {
        public static Article[] FilterArticles(this IArticleFilter filter, params Article[] articles)
        {
            return filter
                .Filter(articles)
                .ToArray();
        }
    }
}
