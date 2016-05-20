using System;
using System.Linq.Expressions;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using QA.Core.ProductCatalog;

namespace QA.Core.DPC.API.Container
{
	public static class APIContainerExstensions
	{
		public static IUnityContainer RegisterArticleMatchService<TCondition, TMapper>(this IUnityContainer container, string connectionString)
			where TCondition : class
			where TMapper : IConditionMapper<TCondition>
		{
			container.RegisterType<IArticleMatchService<TCondition>, ArticleMatchService<TCondition>>(new InjectionFactory(c => new ArticleMatchService<TCondition>(connectionString, c.Resolve<IConditionMapper<TCondition>>())));
			container.RegisterType<IConditionMapper<TCondition>, TMapper>();
			return container;
		}

		public static IUnityContainer RegisterArticleMatchService<TCondition, TMapper>(this IUnityContainer container, Func<IUnityContainer, string> factoryFunc)
			where TCondition : class
			where TMapper : IConditionMapper<TCondition>
		{
			return container.RegisterArticleMatchService<TCondition, TMapper>(factoryFunc(container));
		}

		public static IUnityContainer RegisterExpressionArticleMatchService(this IUnityContainer container)
		{
			return container.RegisterArticleMatchService<Expression<Predicate<IArticle>>, ExpressionConditionMapper>(c => c.GetConnectionString());
		}
	}
}
