using System;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront
{
    public class SonicErrorDescriber
    {
        public virtual SonicError InvalidProductTitle(string title)
        {
            return new SonicError
            {
                Code = nameof(InvalidProductTitle),
                Description = $"Product title '{title}' is invalid."
            };
        }

        public virtual SonicError InvalideTypeName(string name)
        {
            return new SonicError
            {
                Code = nameof(InvalideTypeName),
                Description = $"Type name '{name}' is invalid."
            };
        }

        public virtual SonicError DuplicateProductTitle(string title)
        {
            return new SonicError
            {
                Code = nameof(DuplicateProductTitle),
                Description = $"Product title '{title}' is already taken."
            };
        }

        public virtual SonicError DuplicateTypeName(string name)
        {
            return new SonicError
            {
                Code = nameof(DuplicateTypeName),
                Description = $"Type name '{name}' is already taken."
            };
        }

        public static SonicError StoreFailure(string message, Exception ex = null)
        {
            return new SonicError
            {
                Code = nameof(StoreFailure),
                Description = $"Store failed with exception message '{message}'.",
                Exception = ex
            };
        }
    }
}