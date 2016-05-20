// Owners: Karlov Nikolay, Abretov Alexey

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using QA.Razor.Core;

namespace QA.Razor.Engine
{
    /// <summary>
    /// Process razor templates.
    /// </summary>
    public static class Razor
    {
        private static object _syncRoot = new object();
        private static RazorCompiler _сompiler;
        private static readonly IDictionary<string, ITemplate> _templates;

        /// <summary>
        /// Statically initialises the <see cref="Razor"/> type.
        /// </summary>
        static Razor()
        {
            _сompiler = new RazorCompiler(new CSharpRazorProvider());
            _templates = new Dictionary<string, ITemplate>();
        }

        /// <summary>
        /// Gets an <see cref="ITemplate"/> for the specified template.
        /// </summary>
        /// <param name="template">The template to parse.</param>
        /// <param name="modelType">The model to use in the template.</param>
        /// <param name="name">[Optional] The name of the template.</param>
        /// <returns></returns>
        private static ITemplate GetTemplate(string template, Type modelType, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentNullException("template");
            }

            var name = GetMD5Hash(template);

            lock (_syncRoot)
            {

                if (!string.IsNullOrEmpty(name))
                {
                    if (_templates.ContainsKey(name))
                        return _templates[name];
                }

                var instance = _сompiler.CreateTemplate(template, encoding, modelType);

                if (!string.IsNullOrEmpty(name))
                {
                    if (!_templates.ContainsKey(name))
                    {
                        _templates.Add(name, instance);
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Parses the specified template using the specified model.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model to use in the template.</param>
        /// <param name="name">[Optional] A name for the template used for caching.</param>
        /// <returns>The parsed template.</returns>
        public static string Parse<T>(TemplateContent template, T model, Encoding encoding = null)
        {
            var instance = GetTemplate(template.Content, typeof(T), encoding);
            if (instance is ITemplate<T>)
                ((ITemplate<T>)instance).Model = model;

            instance.Execute();
            return instance.Result;
        }

        /// <summary>
        /// Parses the specified template
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model to use in the template.</param>
        /// <param name="name">[Optional] A name for the template used for caching.</param>
        /// <returns>The parsed template.</returns>
        public static string Parse(TemplateContent template, Encoding encoding = null)
        {
            var instance = GetTemplate(template.Content, null, encoding);

            instance.Execute();

            return instance.Result;
        }
        
        /// <summary>
        /// Sets the razor provider used for compiling templates.
        /// </summary>
        /// <param name="provider">The razor provider.</param>
        public static void SetRazorProvider(IRazorProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _сompiler = new RazorCompiler(provider);
        }


        private static string GetMD5Hash(string input)
        {
            var x = new MD5CryptoServiceProvider();

            byte[] bytes = Encoding.UTF8.GetBytes(input);

            bytes = x.ComputeHash(bytes);

            var s = new StringBuilder();

            foreach (byte b in bytes)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            return s.ToString();
        }
    }
}