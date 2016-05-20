using System;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class Helpers
    {
        public static string GetClassNameWithoutVersion(Type type)
        {
            string assemblyQualifiedName = type.AssemblyQualifiedName;

            return assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(", Version="));
        } 
    }
}