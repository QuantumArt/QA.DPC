﻿using QA.ProductCatalog.Infrastructure;
using System.Collections.Concurrent;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
    public class ValidationServiceFake : IValidationService
    {
        public int[] GetProductIds()
        {
            return new int[0];
        }

        public void UpdateValidationInfo(int[] productIds, ConcurrentDictionary<int, string> errors)
        {
        }

        public ValidationReport ValidateAndUpdate(int chunkSize, int maxDegreeOfParallelism, ITaskExecutionContext context)
        {
            return new ValidationReport();
        }
    }
}