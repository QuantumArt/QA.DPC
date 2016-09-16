using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.UI.Binding
{
    public class ModelObjectCollection : IEnumerable<IModelObject>, IGetArticles
    {
        private readonly IEnumerable<IModelObject> _inner;

        public ModelObjectCollection(IEnumerable<IModelObject> inner)
        {
            _inner = inner;
        }

        #region IEnumerable<IModelObject> Members

        public IEnumerator<IModelObject> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_inner).GetEnumerator();
        }

        #endregion

        #region IGetArticles Members

        public IEnumerable<Article> GetArticles(IArticleFilter filter)
        {
            return this.Cast<Article>();
        }

        #endregion
    }
}
