using System;
using NUnit.Framework;
using QA.Core.DPC.UI;
using QA.Core.DPC.UI.Controls;
using QA.Core.Models.Entities;
using QA.Core.Models.Tests.Helpers;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests
{
    [TestFixture]
    public class ProductControlsTests
    {
        [Test]  
        public void Test_Entity_Editor()
        {
            Run(() =>
            {
                var model = GetModel();
                var control = ValidationHelper.GetXaml<EntityEditor>("QA.Core.Models.Tests.Xaml.EntityEditor.xaml");
                control.DataContext = model;
            });
        }

        [Test]
        public void Test_Entity_Editor_items()
        {
            Run(() =>
            {
                var model = GetModel();

                var control = ValidationHelper.GetXaml<EntityEditor>("QA.Core.Models.Tests.Xaml.EntityEditor_Collection.xaml");

                control.DataContext = model;

                foreach (var item in ((EntityCollection)control.Content).GetChildren())
                {
                    Assert.IsInstanceOf<Label>(item);
                    Assert.IsNotNull(((Label)item).Title);
                }
            });
        }

        [Test]
        public void Test_Entity_Editor_gridview()
        {
            Run(() =>
            {
                var model = GetModel();
                var control = ValidationHelper.GetXaml<GroupGridView>("QA.Core.Models.Tests.Xaml.EntityEditor_GridView.xaml");
                control.DataContext = model;
                control.GetChildren();
            });
        }

        private static IModelObject GetModel()
        {
            QPModelBindingValueProvider.ThrowOnErrors = true;
            var service = ObjectFactoryBase.Resolve<IProductService>();
            IModelObject model = service.GetProductById(2360);
            return model;
        }

        private static void Run(Action action)
        {
            lock (typeof(DefaultBindingValueProviderFactory))
            {
                try
                {
                    BindingValueProviderFactory.Current = new DefaultBindingValueProviderFactory(new QPModelBindingValueProvider());
                    action();
                }
                finally
                {
                    BindingValueProviderFactory.Current = null;
                }
            }
        }
    }
}
