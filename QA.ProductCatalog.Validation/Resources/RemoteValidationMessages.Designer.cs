﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QA.ProductCatalog.Validation.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class RemoteValidationMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RemoteValidationMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("QA.ProductCatalog.Validation.Resources.RemoteValidationMessages", typeof(RemoteValidationMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Неверно указан customercode. Ожидался {0}, получен {1}.
        /// </summary>
        internal static string CustomerCodeInvalid {
            get {
                return ResourceManager.GetString("CustomerCodeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Встречается дублирование связей с продуктами: {0} .
        /// </summary>
        internal static string DuplicateRelationsProducts {
            get {
                return ResourceManager.GetString("DuplicateRelationsProducts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Тарифное направление встречается больше одного раза в параметрах: {0}.
        /// </summary>
        internal static string DuplicateTariffsAreas {
            get {
                return ResourceManager.GetString("DuplicateTariffsAreas", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Нет строки подключения.
        /// </summary>
        internal static string EmptyConnectionString {
            get {
                return ResourceManager.GetString("EmptyConnectionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Не найдено поле {0}.
        /// </summary>
        internal static string MissingParam {
            get {
                return ResourceManager.GetString("MissingParam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Некоторые продукты не имеют общих регионов с другими продуктами: {0}.
        /// </summary>
        internal static string Products_Different_Regions {
            get {
                return ResourceManager.GetString("Products_Different_Regions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Продукт содержит регионы из продуктов: {0}.
        /// </summary>
        internal static string ProductsRepeatingRegions {
            get {
                return ResourceManager.GetString("ProductsRepeatingRegions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Не задана настройка {0}.
        /// </summary>
        internal static string Settings_Missing {
            get {
                return ResourceManager.GetString("Settings_Missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SiteId не совпадает в валидации и QP.
        /// </summary>
        internal static string SiteIdInvalid {
            get {
                return ResourceManager.GetString("SiteIdInvalid", resourceCulture);
            }
        }
    }
}
