using System.IO;

namespace QA.Core.DPC.Loader.Services
{
	public interface IProductPdfTemplateService
	{
		Stream GetTemplate(int productId);
	}
}