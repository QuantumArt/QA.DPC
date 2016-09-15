using System;
using System.Linq;
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
    public class BindingTests
    {
        [AssemblyInitialize]
        public static void StartUp(TestContext ctx)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new StackPanel { Name = "" };
            ctx.WriteLine("Started!");
            UnityConfig.Configure();
        }

        [TestInitialize]
        public void Init()
        {
        }


        [TestMethod]
        public void TestMethod1()
        {
            lock (typeof(DefaultBindingValueProviderFactory))
            {
                var control = ValidationHelper.GetXaml<QPControlTest>("QA.Core.Models.Tests.Xaml.Test001.xaml");

                //Assert.IsNull(control.Name);
                //Assert.IsNotNull(control.Title);
                //Assert.AreEqual("not binded", control.Title);

                control.DataContext = new { Test = "123", Test1 = "binded!", AnotherCoolProperty = "test123", Inner = new { Prop = "1" } };

                Assert.IsNotNull(control);
                Assert.IsNotNull(control.Name);
                Assert.AreEqual("123", control.Name);

                Assert.AreEqual("binded!", ((QPControlTest)control.Content).Title);

                Assert.AreEqual(2, ((StackPanel)((QPControlTest)control.Content).Content).Items.Count);

                Assert.AreEqual("test123", ((QPControlTest)((StackPanel)((QPControlTest)control.Content).Content).Items[0]).Name);
                Assert.AreEqual("binded!", ((QPControlTest)((StackPanel)((QPControlTest)control.Content).Content).Items[0]).Title);
                Assert.AreEqual("1", ((QPControlTest)((StackPanel)((QPControlTest)control.Content).Content).Items[1]).Title);
            }
        }

        [Ignore]
        [TestMethod]
        public void TestMethodTestHierarchical1()
        {
            lock (typeof(DefaultBindingValueProviderFactory))
            {
                var control = ValidationHelper.GetXaml<StackPanel>("QA.Core.Models.Tests.Xaml.TestHierarchical.xaml");
                control.DataContext = new { Test = "123", Test1 = "binded!", AnotherCoolProperty = "test123", Inner = new { Prop = "1" } };

                Assert.IsNotNull(control);
                Assert.AreEqual("test", ((QPControlTest)((QPControlTest)control.Items[1]).Content).HierarchicalMember);
                Assert.AreEqual("test1", ((QPControlTest)((QPControlTest)control.Items[1]).Content).HierarchicalMember);
                Assert.IsNull(((QPControlTest)((QPControlTest)control.Items[1]).Content).HierarchicalMember);
            }
        }

        [Ignore]
        [TestMethod]
        public void Test_Binding_To_Model()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<QPControlTest>("QA.Core.Models.Tests.Xaml.Test002.xaml");
                var service = ObjectFactoryBase.Resolve<IProductService>();
                var model = service.GetProductById(2360);

                control.DataContext = model;
                var t = control.Title;
                var n = control.Name;
                var c = control.Caption;
                var intProp = control.ContentId;

                var t1 = ((QPControlTest)control.Content).Title;
                var n1 = ((QPControlTest)control.Content).Name;
                var c1 = ((QPControlTest)control.Content).Caption;

                Assert.IsTrue(intProp != 0);

                Assert.IsNotNull(t);
                Assert.IsNotNull(n);
                Assert.IsNotNull(c);

                Assert.IsNotNull(t1);
                Assert.IsNotNull(n1);
                Assert.IsNotNull(c1);

            });
        }



        [TestMethod]
        public void TestStatic1()
        {
            Console.WriteLine(@"1");
            var instance = new Class1();
            Console.WriteLine(@"1");
            var state1 = Tester.state;
            Console.WriteLine(@"1");
            Assert.IsNotNull(instance.Prop);
            var state2 = Tester.state;
            Console.WriteLine(@"1");

            Console.WriteLine($@"{state1} {state2}");

            Assert.IsTrue(state2);
            Assert.IsTrue(state1);
        }


        [TestMethod]
        public void TestStatic2()
        {
            Console.WriteLine(@"1");
            var instance = new Class2();
            Console.WriteLine(@"1");
            var state1 = Tester.state;
            Console.WriteLine(@"1");
            Assert.IsNotNull(instance.Prop);
            var state2 = Tester.state;
            Console.WriteLine(@"1");

            Console.WriteLine($@"{state1} {state2}");

            Assert.IsTrue(state2);
            Assert.IsTrue(state1);
        }

        [Ignore]
        [TestMethod]
        public void Test_Binding_To_Model_With_Converter()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<GroupGridView>("QA.Core.Models.Tests.Xaml.EntityEditor_GridView_Converter.xaml");
                var service = ObjectFactoryBase.Resolve<IProductService>();
                var model = service.GetProductById(2360);

                control.DataContext = model;

                Assert.AreEqual("true", control.Title);

                foreach (var child in control.GetChildren())
                {
                    var item = child;
                    if (item != null)
                    {
                        var bindedValue = ((Label)item.Columns[0].CellTemplate).Title;
                        Assert.IsNotNull(bindedValue);
                        Assert.IsTrue(bindedValue == "published" || bindedValue == "notpublished");
                    }
                }
            });
        }

        [Ignore]
        [TestMethod]
        public void Test_Binding_To_Model_With__bindable_Converter()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<Group>("QA.Core.Models.Tests.Xaml.EntityEditor_BindableConverter.xaml");

                var model = new Article { ContentId = 1, Id = 123 };
                model.Fields.Add("Test", new PlainArticleField { FieldName = "Test", Value = "testvalue" });
                model.Fields.Add("Test1", new PlainArticleField { FieldName = "Test1", Value = "testvalue1" });

                control.DataContext = model;

                var titl1 = ((Label)control.Items[0]).Title;
                var titl2 = ((Label)control.Items[1]).Title;

                Assert.AreEqual("testvalue", titl1);
                Assert.AreEqual("testvalue1-testvalue", titl2);
            });
        }


        [TestMethod]
        public void Test_Binding_To_Model_With__bindable_Converter_roots()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<Group>("QA.Core.Models.Tests.Xaml.EntityEditor_BindableConverter.xaml");

                var model = new Article { ContentId = 1, Id = 123 };
                model.Fields.Add("Test", new PlainArticleField { FieldName = "Test", Value = "testvalue" });

                var mu = new MultiArticleField { FieldName = "TestItems" };
                mu.Items.Add(223, new Article { Id = 223, ContentId = 2, ContentDisplayName = "test1" });
                mu.Items.Add(224, new Article { Id = 224, ContentId = 2, ContentDisplayName = "test1" });

                model.Fields.Add("TestItems", mu);

                control.DataContext = model;

                var ec = (EntityCollection)control.Items[2];

                var label = (Label)ec.GetChildren().First();

                Assert.AreEqual("testvalue-test1", label.Title);

            });
        }


        [TestMethod]
        public void Test_AttachedProperty()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<QPControlTest>("QA.Core.Models.Tests.Xaml.AttachedProperty.xaml");

                Assert.IsNotNull(AnotherControl.GetColor(control));

                Assert.AreEqual("red", AnotherControl.GetColor(control));
                Assert.IsNull(AnotherControl.GetColor((UIElement)control.Content));
                Assert.AreEqual("green", AnotherControl.GetColor((QPControlTest)((QPControlTest)control.Content).Content));

            });
        }

        [TestMethod]
        public void Test_AttachedProperty_with_inheritance()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<QPControlTest>("QA.Core.Models.Tests.Xaml.AttachedProperty.xaml");

                control.DataContext = new { Data = new { Name = "test1" } };

                control = (QPControlTest)control.Content;

                Assert.IsNotNull(QPControlTest.GetTestProperty(control));

                control = (QPControlTest)control.Content;

                Assert.IsNotNull(QPControlTest.GetTestProperty(control));

            });
        }


        [TestMethod]
        public void Test_Binding_AttachedProperty()
        {
            Run(() =>
            {
                QPModelBindingValueProvider.ThrowOnErrors = true;
                var control = ValidationHelper.GetXaml<QPControlTest>("QA.Core.Models.Tests.Xaml.AttachedProperty.xaml");

                var data = new { Data = new { Name = "test1" } };
                control.DataContext = data;
            });
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
