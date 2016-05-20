// Owners: Karlov Nikolay, Abretov Alexey

namespace QA.Razor.Core
{

    public interface IViewProvider
    {
        TemplateContent GetView(string viewName);
        string[] ViewPath { get; }
    }
}
