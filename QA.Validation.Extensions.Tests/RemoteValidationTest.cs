using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Validation.Extensions.Tests.Stubs;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.ListTypes;

namespace QA.Validation.Extensions.Tests
{
    [TestClass]
    public class RemoteValidationTest
    {
        /// <summary>
        /// Для запуска этого теста требуется запущенное приложение QA.Validation.Xaml.Extentions.WebApp 
        /// по адресу http://localhost:60857/testhandler.ashx
        /// Проверяется вся цепочка удаленной валидации с участием веб-приложения с валидационным хендлером.
        /// </summary>
        [TestMethod]
        [TestCategory("Remote validation: integration tests")]
        public void Test_RemoteValidation_Check_Whole_Workflow_With_Real_HttpHandler()
        {
            // подготовим и настроим все объекты
            var validator = PrepareDefinitions(new ProcessRemoteValidationIf(), "http://localhost:60857/testhandler.ashx");

            var values = new Dictionary<string, string> { 
                { "prop1", "" }, 
                { "prop2", "12" },
                { "prop3", "" },
                { "prop4", "99.99" },
                { "prop5", "" } 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsFalse(ctx.IsValid);
            Assert.AreEqual(1, ctx.Messages.Count);
            Assert.AreEqual(5, ctx.Result.Errors.Count);

            Assert.AreEqual(ctx.Messages[0], "Есть ошибки валидации");

            Assert.AreEqual(ctx.Result.Errors[0].Definition.PropertyName, "prop1");
            Assert.AreEqual(ctx.Result.Errors[0].Message, "Поле должно быть заполнено");

            Assert.AreEqual(ctx.Result.Errors[1].Definition.PropertyName, "prop2");
            Assert.AreEqual(ctx.Result.Errors[1].Message, "Возраст должен быть не менее 18, но не более 100");

            Assert.AreEqual(ctx.Result.Errors[2].Definition.PropertyName, "prop3");
            Assert.AreEqual(ctx.Result.Errors[2].Message, "Укажите дату");

            Assert.AreEqual(ctx.Result.Errors[3].Definition.PropertyName, "prop4");
            Assert.AreEqual(ctx.Result.Errors[3].Message, "Должно быть более 100");

            Assert.AreEqual(ctx.Result.Errors[4].Definition.PropertyName, "prop5");
            Assert.AreEqual(ctx.Result.Errors[4].Message, "Не может быть null");
            //Assert.AreEqual(ctx.Result.Errors[4].Message, "Должно быть выбрано");

        }

        /// <summary>
        /// Для запуска этого теста требуется запущенное приложение QA.Validation.Xaml.Extensions.MvcWebApp
        /// по адресу http://localhost:51380/remotevalidation
        /// Проверяется вся цепочка удаленной валидации с участием веб-приложения с валидационным хендлером.
        /// </summary>
        [TestMethod]
        [TestCategory("Remote validation: integration tests")]
        public void Test_RemoteValidation_Check_Whole_Workflow_With_Real_Mvc4_Application_EmptyDate()
        {
            // подготовим и настроим все объекты
            var validator = PrepareDefinitions(new ProcessRemoteValidationIf(), "http://localhost:51380/remotevalidation");

            var values = new Dictionary<string, string> { 
                { "prop1", "" }, 
                { "prop2", "12" },
                { "prop3", "" },
                { "prop4", "99.99" },
                { "prop5", "" } 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsFalse(ctx.IsValid);
            Assert.AreEqual(1, ctx.Messages.Count);
            Assert.AreEqual(1, ctx.Result.Errors.Count);

            Assert.AreEqual(ctx.Messages[0], "Тестовое сообщение");

            Assert.AreEqual(ctx.Result.Errors[0].Definition.PropertyName, "prop3");
            Assert.AreEqual(ctx.Result.Errors[0].Message, "Укажите дату");
        }

        /// <summary>
        /// Для запуска этого теста требуется запущенное приложение QA.Validation.Xaml.Extentions.WebApp 
        /// по адресу http://localhost:51380/remotevalidation
        /// Проверяется вся цепочка удаленной валидации с участием веб-приложения с валидационным хендлером.
        /// </summary>
        [TestMethod]
        [TestCategory("Remote validation: integration tests")]
        public void Test_RemoteValidation_Check_Whole_Workflow_With_Real_Mvc4_Application_CorrectDate()
        {
            // подготовим и настроим все объекты
            var validator = PrepareDefinitions(new ProcessRemoteValidationIf(), "http://localhost:51380/remotevalidation");

            var values = new Dictionary<string, string> { 
                { "prop1", "" }, 
                { "prop2", "12" },
                { "prop3", "2012.11.07" },
                { "prop4", "99.99" },
                { "prop5", "" } 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsFalse(ctx.IsValid);
            Assert.AreEqual(1, ctx.Messages.Count);
            Assert.AreEqual(1, ctx.Result.Errors.Count);

            Assert.AreEqual(ctx.Messages[0], "Тестовое сообщение");

            Assert.AreEqual(ctx.Result.Errors[0].Definition.PropertyName, "prop3");
            Assert.AreEqual(ctx.Result.Errors[0].Message, "Введенная дата меньше 2013.01.01 18:00:00");
        }


        /// <summary>
        /// Для запуска этого теста требуется запущенное приложение QA.Validation.Xaml.Extentions.WebApp 
        /// по адресу http://localhost:51380/remotevalidation
        /// Проверяется вся цепочка удаленной валидации с участием веб-приложения с валидационным хендлером.
        /// </summary>
        [TestMethod]
        [TestCategory("Remote validation: integration tests")]
        public void Test_RemoteValidation_Check_Whole_Workflow_With_Real_Mvc4_Application_List_of_int()
        {
            // подготовим и настроим все объекты
            var validator = PrepareDefinitionsWithArray(new ProcessRemoteValidationIf(), "http://localhost:51380/remotevalidation/listofinttest");

            var values = new Dictionary<string, string> { 
                { "prop6", "1,2,3,4,5,6,7,8,9" } 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsTrue(ctx.IsValid);
        }
        /// <summary>
        /// Для запуска этого теста требуется запущенное приложение QA.Validation.Xaml.Extentions.WebApp 
        /// по адресу http://localhost:51380/remotevalidation
        /// Проверяется вся цепочка удаленной валидации с участием веб-приложения с валидационным хендлером.
        /// </summary>
        [TestMethod]
        [TestCategory("Remote validation: integration tests")]
        [ExpectedException(typeof(XamlValidatorException))]
        public void Test_RemoteValidation_Check_Whole_Workflow_With_Real_Mvc4_Application_Incorrect_Date()
        {
            // подготовим и настроим все объекты
            var validator = PrepareDefinitions(new ProcessRemoteValidationIf(), "http://localhost:51380/remotevalidation");

            var values = new Dictionary<string, string> { 
                { "prop1", "" }, 
                { "prop2", "12" },
                { "prop3", "2012.11.07asd" }, // некорректное значение даты
                { "prop4", "99.99" },
                { "prop5", "" } 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsFalse(ctx.IsValid);
            Assert.AreEqual(1, ctx.Messages.Count);
            Assert.AreEqual(1, ctx.Result.Errors.Count);

            Assert.AreEqual(ctx.Messages[0], "Тестовое сообщение");

            Assert.AreEqual(ctx.Result.Errors[0].Definition.PropertyName, "prop3");
            Assert.AreEqual(ctx.Result.Errors[0].Message, "Введенная дата меньше 2013.01.01 18:00:00");
        }


        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_TestValidationHandler_Checks_That_Get_Is_Not_Allowed()
        {
            StringBuilder sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var fixture = HandlerExecutionFixture.Create()
                    .WithHttpMethod("get")
                    .WithWriteMethod(writer);

                var handler = new StubValidationHandler((m, c) => { });

                handler.OnProcessRequest(fixture.Use());
            }

            Assert.AreEqual("Use post to pass the model", sb.ToString());

        }

        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_TestValidationHandler_Check_Arguments()
        {
            string modelToSent = @"{'CustomerCode':'QP123','SiteId':'35','Values':{'prop1':'','prop2':12,'prop3':null,'prop4':99.99,'prop5':null},'Definitions':[{'PropertyType':'System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089','PropertyName':'prop1','Alias':'prop1','Description':null},{'PropertyType':'System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089','PropertyName':'prop2','Alias':'prop2','Description':null},{'PropertyType':'System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089','PropertyName':'prop3','Alias':'prop3','Description':null},{'PropertyType':'System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089','PropertyName':'prop4','Alias':'prop4','Description':null},{'PropertyType':'System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089','PropertyName':'prop5','Alias':'prop5','Description':null}]}";

            StringBuilder sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var fixture = HandlerExecutionFixture.Create()
                    .WithHttpMethod("post")
                    .WithInputStream(modelToSent)
                    .AllowSetResponseContentType()
                    .WithWriteMethod(writer);

                var handler = new StubValidationHandler((m, c) =>
                {
                    Assert.IsNotNull(m);
                    Assert.IsNotNull(c);
                    Assert.AreEqual(35, m.SiteId);
                    Assert.AreEqual("QP123", m.CustomerCode);
                });

                handler.OnProcessRequest(fixture.Use());
            }
        }

        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_RemoteValidation_Check_Interaction_With_Stubs()
        {
            var manager = new StubWebInteractionManager(() =>
                new StubValidationHandler(
                    (c, m) =>
                    {
                        // здесь настраиваем логику фальшивого хендлера
                        m.Messages.Add("Есть ошибки валидации");
                        m.Result.AddError("prop1", "Поле должно быть заполнено");

                        // для уверенности десериализуем все поля формы
                        Assert.AreEqual("test text", c.ProvideValueExact<string>("prop1"));
                        Assert.AreEqual(12, c.ProvideValueExact<int?>("prop2"));
                        Assert.AreEqual(DateTime.Parse("2012.01.01"), c.ProvideValueExact<DateTime?>("prop3"));
                        Assert.AreEqual(99.99, c.ProvideValueExact<double>("prop4"));
                        Assert.AreEqual(false, c.ProvideValueExact<bool?>("prop5"));
                    }));

            var validator = PrepareDefinitions(new ProcessRemoteValidationIf(manager), "http://app.com");

            // устанавливаенм значения формы
            var values = new Dictionary<string, string> { 
                { "prop1", "test text" }, 
                { "prop2", "12" },
                { "prop3", "2012.01.01" },
                { "prop4", "99.99" },
                { "prop5", "false" }, 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsFalse(ctx.IsValid);
            Assert.AreEqual(1, ctx.Messages.Count);
            Assert.AreEqual(1, ctx.Result.Errors.Count);

            Assert.AreEqual(ctx.Messages[0], "Есть ошибки валидации");

            Assert.AreEqual(ctx.Result.Errors[0].Definition.PropertyName, "prop1");
            Assert.AreEqual(ctx.Result.Errors[0].Message, "Поле должно быть заполнено");
        }

        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_RemoteValidation_Check_Interaction_With_Stubs_Using_ListOfInt()
        {
            var manager = new StubWebInteractionManager(() =>
                new StubValidationHandler(
                    (c, m) =>
                    {
                        var value = c.ProvideValueExact<ListOfInt>("prop1");
                        if (value == null || value.Count != 4 || value[2] != 3)
                            m.AddModelError("prop1", "");
                    }));

            var validator = new XamlValidator();
            validator.Definitions.Add("prop1", new PropertyDefinition("prop1", "prop1", typeof(ListOfInt)));

            var condition = new ProcessRemoteValidationIf(manager);
            condition.HttpMethod = "POST";
            condition.SiteId = 35;
            condition.CustomerCode = "QP123";
            condition.Timeout = 5000;
            condition.Url = new Uri("http://app.com");
            condition.DefinitionsToSend.AddRange(validator.Definitions.Values);

            validator.ValidationRules.Add(condition);
            // устанавливаенм значения формы
            var values = new Dictionary<string, string> { 
                { "prop1", "1, 2, 3, 4" }, 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsTrue(ctx.IsValid);
        }

        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_RemoteValidation_Check_Interaction_With_Stubs_Using_ListOfString()
        {
            var manager = new StubWebInteractionManager(() =>
                new StubValidationHandler(
                    (c, m) =>
                    {
                        var value = c.ProvideValueExact<ListOfString>("prop1");
                        if (value == null || value.Count != 4 || value[2] != "name2")
                            m.AddModelError("prop1", "");
                    }));

            var validator = new XamlValidator();
            validator.Definitions.Add("prop1", new PropertyDefinition("prop1", "prop1", typeof(ListOfString)));

            var condition = new ProcessRemoteValidationIf(manager);
            condition.HttpMethod = "POST";
            condition.SiteId = 35;
            condition.CustomerCode = "QP123";
            condition.Timeout = 5000;
            condition.Url = new Uri("http://app.com");
            condition.DefinitionsToSend.AddRange(validator.Definitions.Values);

            validator.ValidationRules.Add(condition);
            // устанавливаенм значения формы
            var values = new Dictionary<string, string> { 
                { "prop1", "name, name1, name2, name3" }, 
            };

            var ctx = new ValidationContext();

            validator.Validate(values, ctx);

            Assert.IsTrue(ctx.IsValid);
        }


        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_RemoteValidation_Check_Serialization_and_Deserialization()
        {
            var ctx = new RemoteValidationContext();

            ctx.Values["prop"] = new ListOfInt(new int[] { 1, 2, 3, 4 });

            var text = RemoteValidationContext.GetJson(ctx);

            var serializer = new JavaScriptSerializer();
            var obj = serializer.Deserialize<RemoteValidationContext>(text);

            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Values.Keys.Count);
            Assert.IsTrue(obj.Values.ContainsKey("prop"), "should contain prop");
            Assert.IsInstanceOfType(obj.Values["prop"], typeof(ArrayList));
        }

        [TestMethod]
        [TestCategory("Remote validation: fixture")]
        public void Test_RemoteValidation_Check_Serialization_and_Deserialization_MVC_STYLE()
        {
            var ctx = new RemoteValidationContext();

            ctx.Values["prop"] = new ListOfInt(new int[] { 1, 2, 3, 4 });

            var text = RemoteValidationContext.GetJson(ctx);

            var serializer = new JavaScriptSerializer();
            var obj = serializer.DeserializeObject(text);

            Assert.IsNotNull(obj);         
        }
      
        private static XamlValidator PrepareDefinitions(ProcessRemoteValidationIf condition, string url)
        {
            var validator = new XamlValidator();
            validator.Definitions.Add("prop1", new PropertyDefinition("prop1", "prop1", typeof(string)));
            validator.Definitions.Add("prop2", new PropertyDefinition("prop2", "prop2", typeof(int)));
            validator.Definitions.Add("prop3", new PropertyDefinition("prop3", "prop3", typeof(DateTime)));
            validator.Definitions.Add("prop4", new PropertyDefinition("prop4", "prop4", typeof(double)));
            validator.Definitions.Add("prop5", new PropertyDefinition("prop5", "prop5", typeof(bool)));

            condition.HttpMethod = "POST";
            condition.SiteId = 35;
            condition.CustomerCode = "QP123";
            condition.Timeout = 5000;
            condition.Url = new Uri(url);
            condition.DefinitionsToSend.AddRange(validator.Definitions.Values);

            validator.ValidationRules.Add(condition);
            return validator;
        }

        private static XamlValidator PrepareDefinitionsWithArray(ProcessRemoteValidationIf condition, string url)
        {
            var validator = new XamlValidator();
            validator.Definitions.Add("prop6", new PropertyDefinition("prop6", "prop6", typeof(ListOfInt)));

            condition.HttpMethod = "POST";
            condition.SiteId = 35;
            condition.CustomerCode = "QP123";
            condition.Timeout = 5000;
            condition.Url = new Uri(url);
            condition.DefinitionsToSend.AddRange(validator.Definitions.Values);

            validator.ValidationRules.Add(condition);
            return validator;
        }

    }
}
