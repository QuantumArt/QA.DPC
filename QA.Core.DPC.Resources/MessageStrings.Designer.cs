﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QA.Core.DPC.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class MessageStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MessageStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("QA.Core.DPC.Resources.MessageStrings", typeof(MessageStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This action is not available because {0}.
        /// </summary>
        public static string ConsolidationErrorMessage {
            get {
                return ResourceManager.GetString("ConsolidationErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to it is active.
        /// </summary>
        public static string CustomerStateActive {
            get {
                return ResourceManager.GetString("CustomerStateActive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to customer code  is currently registering.
        /// </summary>
        public static string CustomerStateCreating {
            get {
                return ResourceManager.GetString("CustomerStateCreating", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to customer code  is currently unregistering.
        /// </summary>
        public static string CustomerStateDeleting {
            get {
                return ResourceManager.GetString("CustomerStateDeleting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to it is not defined.
        /// </summary>
        public static string CustomerStateNotDefined {
            get {
                return ResourceManager.GetString("CustomerStateNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to customer code is not exists.
        /// </summary>
        public static string CustomerStateNotFound {
            get {
                return ResourceManager.GetString("CustomerStateNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to customer code is not in consolidation mode.
        /// </summary>
        public static string CustomerStateNotRegistered {
            get {
                return ResourceManager.GetString("CustomerStateNotRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to customer code  is currently reregistering.
        /// </summary>
        public static string CustomerStateUpdating {
            get {
                return ResourceManager.GetString("CustomerStateUpdating", resourceCulture);
            }
        }
    }
}
