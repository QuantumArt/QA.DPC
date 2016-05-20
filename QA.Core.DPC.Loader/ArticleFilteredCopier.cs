using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.Loader
{
    public class ArticleFilteredCopier
    {
        public static Article Copy(Article sourceArticle, IArticleFilter filter)
        {
	        return CopyIfNeeded(sourceArticle, filter, new Dictionary<Article, Article>());
        }

	    private static Article CopyIfNeeded(Article sourceArticle, IArticleFilter filter, Dictionary<Article, Article> copiedArticles)
	    {
		    if (copiedArticles.ContainsKey(sourceArticle))
			    return copiedArticles[sourceArticle];

			var copiedArticle = Mapper.Map<Article, Article>(sourceArticle);

		    copiedArticles[sourceArticle] = copiedArticle;

			copiedArticle.Fields = CopyArticleFields(sourceArticle.Fields, filter, copiedArticles);

			return copiedArticle; 
	    }


	    static ArticleFilteredCopier()
	    {
		    Mapper.CreateMap<Article, Article>();

			Mapper.CreateMap<SingleArticleField, SingleArticleField>();

			Mapper.CreateMap<MultiArticleField, MultiArticleField>();

			Mapper.CreateMap<BackwardArticleField, BackwardArticleField>();

			Mapper.CreateMap<ExtensionArticleField, ExtensionArticleField>();

			Mapper.CreateMap<PlainArticleField, PlainArticleField>();

			Mapper.CreateMap<VirtualArticleField, VirtualArticleField>();
			Mapper.CreateMap<VirtualMultiArticleField, VirtualMultiArticleField>();

		    Mapper.CreateMap<IReadOnlyDictionary<string, object>, IReadOnlyDictionary<string, object>>()
			    .ConvertUsing(
				    x => x == null ? null : (IReadOnlyDictionary<string, object>) x.ToDictionary(y => y.Key, y => y.Value));
	    }

		private static Dictionary<string, ArticleField> CopyArticleFields(Dictionary<string, ArticleField> sourceFields, IArticleFilter filter, Dictionary<Article, Article> copiedArticles)
        {
            var resultFields = new Dictionary<string, ArticleField>(sourceFields.Count);

	        foreach (var articleField in sourceFields.Values)
	        {
		        var copiedField = (ArticleField) Mapper.Map(articleField, articleField.GetType(), articleField.GetType());

		        if (articleField is SingleArticleField)
		        {
			        var singleArticleField = (SingleArticleField) articleField;

			        ((SingleArticleField) copiedField).Item = (singleArticleField.GetItem(filter) == null && !(articleField is ExtensionArticleField)) || singleArticleField.Item == null
				        ? null
				        : CopyIfNeeded(singleArticleField.Item, filter, copiedArticles);
		        }
				else if (articleField is MultiArticleField)
					((MultiArticleField) copiedField).Items = ((MultiArticleField) articleField).GetArticles(filter).ToDictionary(x => x.Id, x => CopyIfNeeded(x, filter, copiedArticles));
				else if (articleField is VirtualMultiArticleField)
				{
					((VirtualMultiArticleField) copiedField).VirtualArticles = CopyArticleFields(
						((VirtualMultiArticleField) articleField).VirtualArticles.ToDictionary(x => x.FieldName, x => (ArticleField)x),
						filter,
						copiedArticles)
						.Values
						.Cast<VirtualArticleField>()
						.ToArray();
				}
				else if (articleField is VirtualArticleField)
					((VirtualArticleField) copiedField).Fields = CopyArticleFields(((VirtualArticleField) articleField).Fields.ToDictionary(x => x.FieldName, x => x), filter, copiedArticles).Values.ToArray();

				resultFields.Add(copiedField.FieldName, copiedField);
	        }

            return resultFields;
        }
    }
}
