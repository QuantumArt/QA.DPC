using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.DPC.UI;
using QA.Core.DPC.UI.Controls;
using QA.Core.Models.Entities;
using QA.Core.Models.Tests.Controls;
using QA.Core.Models.Tests.Helpers;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests
{
    [TestClass]
    public class ProductControlsTests
    {

        [TestMethod]
        public void Test_Entity_Editor()
        {
            Run(() =>
            {
                IModelObject model;
                model = GetModel();

                var control = ValidationHelper.GetXaml<EntityEditor>("QA.Core.Models.Tests.Xaml.EntityEditor.xaml");

                control.DataContext = model;

                var t1 = control.Title;
                var n1 = control.Name;

            });
        }

        [TestMethod]
        public void Test_Entity_Editor_items()
        {
            Run(() =>
            {
                IModelObject model;
                model = GetModel();

                var control = ValidationHelper.GetXaml<EntityEditor>("QA.Core.Models.Tests.Xaml.EntityEditor_Collection.xaml");

                control.DataContext = model;

                foreach(var item in ((EntityCollection)control.Content).GetChildren())
                {
                    Assert.IsInstanceOfType(item, typeof(Label));
                    Assert.IsNotNull(((Label)item).Title);
                }
            });
        }

        [TestMethod]
        public void Test_Entity_Editor_gridview()
        {
            Run(() =>
            {
                IModelObject model;
                model = GetModel();

                var control = ValidationHelper.GetXaml<GroupGridView>("QA.Core.Models.Tests.Xaml.EntityEditor_GridView.xaml");

                control.DataContext = model;

                foreach (var item in control.GetChildren())
                {
                    //Assert.IsInstanceOfType(item, typeof(Label));
                    //Assert.IsNotNull(((Label)item).Title);
                }
            });
        }

        private static IModelObject GetModel()
        {
            IModelObject model;
            QPModelBindingValueProvider.ThrowOnErrors = true;

            var service = ObjectFactoryBase.Resolve<IProductService>();
            model = service.GetProductById(2360);
            return model;
        }


        static void Run(Action action)
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
