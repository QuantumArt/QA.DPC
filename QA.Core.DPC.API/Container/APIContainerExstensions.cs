using System;
using System.Linq.Expressions;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Unity.Injection;

namespace QA.Core.DPC.API.Container
{
    public static class APIContainerExstensions
	{
		public static IUnityContainer RegisterArticleMatchService<TCondition, TMapper>(this IUnityContainer container, string connectionString)
			where TCondition : class
			where TMapper : IConditionMapper<TCondition>
		{			
			return container.RegisterArticleMatchService<TCondition, TMapper>(c => connectionString);
		}

		public static IUnityContainer RegisterArticleMatchService<TCondition, TMapper>(this IUnityContainer container, Func<IUnityContainer, string> factoryFunc)
			where TCondition : class
			where TMapper : IConditionMapper<TCondition>
		{
            container.RegisterType<IArticleMatchService<TCondition>, ArticleMatchService<TCondition>>(new InjectionFactory(c => new ArticleMatchService<TCondition>(factoryFunc(c), c.Resolve<IConditionMapper<TCondition>>())));
            container.RegisterType<IConditionMapper<TCondition>, TMapper>();
            return container;
		}

		public static IUnityContainer RegisterExpressionArticleMatchService(this IUnityContainer container)
		{
			return container.RegisterArticleMatchService<Expression<Predicate<IArticle>>, ExpressionConditionMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
		}
	}
}
