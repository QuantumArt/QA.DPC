using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using QA.Core.DPC.Formatters.Formatting;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using Unity;
using Unity.Injection;

namespace QA.Core.DPC.Formatters.Configuration
{
    public static class FormattersContainerExstensions
	{

		public static ModelMediaTypeFormatter<TModel> AddModelMediaTypeFormatter<TFormatter, TModel>(this MediaTypeFormatterCollection formatters, IUnityContainer container, string name, string mediaType, params Action<MediaTypeFormatter, string, string>[] f)
			where TModel : class
			where TFormatter : IFormatter<TModel>
		{
            var formatter = new ModelMediaTypeFormatter<TModel>(container.GetFactory<TFormatter, TModel>(), mediaType);

			foreach (var addMapping in f)
			{
				addMapping(formatter, name, mediaType);
			}

			formatters.Add(formatter);
			return formatter;
		}
		
		
		public static void RegisterModelMediaTypeFormatter<TFormatter, TModel>(this IUnityContainer container, string name, string mediaType)
			where TModel : class
			where TFormatter : IFormatter<TModel>
		{
			container.RegisterFactory<MediaTypeFormatter>(name,
				c => new ModelMediaTypeFormatter<TModel>(c.GetFactory<TFormatter, TModel>(), mediaType));
		}

		public static void RegisterModelMediaTypeFormatter<TFormatter, TModel>(this IUnityContainer container, string mediaType)
			where TModel : class
			where TFormatter : IFormatter<TModel>
		{
			string name = typeof(TModel).Name;
			container.RegisterModelMediaTypeFormatter<TFormatter, TModel>(name, mediaType);
		}

		public static void RegisterModelMediaTypeFormatter<TFormatter>(this IUnityContainer container, string name, string mediaType)
			where TFormatter : MediaTypeFormatter
		{
			container.RegisterType<MediaTypeFormatter, TFormatter>(name);
		}

		public static async Task WriteAsync(this IArticleFormatter formatter, Stream stream, Article product, IArticleFilter filter, bool includeRegionTags)		
		{
			string data = formatter.Serialize(product, filter, includeRegionTags);

            await using var writer = new StreamWriter(stream, leaveOpen: true);
			await writer.WriteAsync(data);
			await writer.FlushAsync();
		}

        public static Func<IFormatter<TModel>> GetFactory<TFormatter, TModel>(this IUnityContainer container)
          where TModel : class
          where TFormatter : IFormatter<TModel>
        {
            var f = container.Resolve<Func<TFormatter>>();
            return () => (IFormatter<TModel>)f();
        }
    }
}