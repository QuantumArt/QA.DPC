using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Validation
{
    public class ProductsOptionsCommonValidationHelper
    {
        private readonly int _maxExpandDepth;

        public ProductsOptionsCommonValidationHelper(
            IHttpContextAccessor httpContextAccessor,
            ApiRestrictionOptions apiRestrictionOptions)
        {
            var isPost = HttpMethods.IsPost(httpContextAccessor.HttpContext.Request.Method);
            _maxExpandDepth = isPost
                ? apiRestrictionOptions.MaxExpandDepth ?? HighloadConstants.MaxExpandDepthDefault
                : HighloadConstants.MaxExpandDepthForGetRequests;
        }

        public int MaxExpandDepth => _maxExpandDepth;

        public bool IsAllowedExpandDepth(ProductsOptionsRoot model)
        {
            var initialDepth = 0;
            return IsAllowedExpandDepthInternal(model, ref initialDepth);
        }

        public bool IsNonEmptyExpandNameEverywhere(ProductsOptionsBase model)
        {
            if (model.Expand == null)
            {
                return true;
            }

            foreach (var expandModel in model.Expand)
            {
                if (expandModel.Name != null && expandModel.Name.Trim().Length == 0)
                {
                    return false;
                }

                if (!IsNonEmptyExpandNameEverywhere(expandModel))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsUniqueNamesInSameExpandArray(ProductsOptionsBase model)
        {
            if (model.Expand == null)
            {
                return true;
            }

            foreach (var expandModel in model.Expand)
            {
                var expandNames = model.Expand
                    .Where(x => x.Name != null)
                    .Select(x => x.Name)
                    .ToArray();

                if (expandNames.Distinct().Count() != expandNames.Length)
                {
                    return false;
                }

                if (!IsUniqueNamesInSameExpandArray(expandModel))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsExpandPathSpecifiedEverywhere(ProductsOptionsBase model)
        {
            if (model.Expand == null)
            {
                return true;
            }

            foreach (var expandModel in model.Expand)
            {
                if (string.IsNullOrWhiteSpace(expandModel.Path))
                {
                    return false;
                }

                if (!IsExpandPathSpecifiedEverywhere(expandModel))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsAllowedExpandDepthInternal(ProductsOptionsBase model, ref int currentDepth)
        {
            if (model.Expand != null)
            {
                currentDepth++;

                if (currentDepth > _maxExpandDepth)
                {
                    return false;
                }

                foreach (var expandModel in model.Expand)
                {
                    var currentDepthCopy = currentDepth;
                    if (!IsAllowedExpandDepthInternal(expandModel, ref currentDepthCopy))
                    {
                        return false;
                    }
                }
            }

            return currentDepth <= _maxExpandDepth;
        }
    }
}
