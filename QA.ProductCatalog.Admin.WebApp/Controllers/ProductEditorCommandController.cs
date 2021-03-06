﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader.Editor;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Services.API.Models;
using QA.Core.DPC.API.Update;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Exceptions;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ProductEditorCommandController : ProductEditorController
    {
        private readonly IFieldService _fieldService;
        private readonly IProductUpdateService _productUpdateService;
        private readonly CloneBatchAction _cloneBatchAction;
        private readonly DeleteAction _deleteAction;
        private readonly PublishAction _publishAction;

        public ProductEditorCommandController(
            // base dependencies
            IContentDefinitionService contentDefinitionService,
            IProductService productService,
            IReadOnlyArticleService articleService,
            EditorSchemaService editorSchemaService,
            EditorDataService editorDataService,
            EditorPartialContentService editorPartialContentService,
            EditorLocaleService editorLocaleService,
            // self dependencies
            IFieldService fieldService,
            IProductUpdateService productUpdateService,
            CloneBatchAction cloneBatchAction,
            DeleteAction deleteAction,
            PublishAction publishAction)
            : base(contentDefinitionService,
                productService,
                articleService,
                editorSchemaService,
                editorDataService,
                editorPartialContentService,
                editorLocaleService)
        {
            _fieldService = fieldService;
            _productUpdateService = productUpdateService;
            _cloneBatchAction = cloneBatchAction;
            _deleteAction = deleteAction;
            _publishAction = publishAction;
        }

        [HttpPost]
        public ActionResult SavePartialProduct(
            [FromBody] SavePartialProductRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath);

            Article partialProduct = _editorDataService
                .DeserializeProduct(request.PartialProduct, partialContent);

            var partialDefinition = new ProductDefinition { StorageSchema = partialContent };

            try
            {
                // TODO: what about validation ?
                InsertData[] insertData = _productUpdateService.Update(partialProduct, partialDefinition, isLive);

                var idMappings = insertData.Select(data => new
                {
                    ClientId = data.OriginalArticleId,
                    ServerId = data.CreatedArticleId,
                });

                int partialProductId = partialProduct.Id;
                if (partialProductId <= 0)
                {
                    partialProductId = insertData
                        .Single(data => data.OriginalArticleId == partialProduct.Id)
                        .CreatedArticleId;
                }

                ArticleObject articleObject = LoadProductGraph(partialContent, partialProductId, isLive);

                string responseJson = JsonConvert.SerializeObject(new
                {
                    IdMappings = idMappings,
                    PartialProduct = articleObject,
                });

                return Content(responseJson, "application/json");
            }
            catch (ProductUpdateConcurrencyException)
            {
                ArticleObject articleObject = LoadProductGraph(partialContent, partialProduct.Id, isLive);

                string productJson = JsonConvert.SerializeObject(articleObject);

                return new ContentResult
                {
                    Content = productJson, 
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ContentType = "application/json"
                };
            }
        }

        [HttpPost]
        public ActionResult PublishProduct(int articleId)
        {
            try
            {
                _publishAction.PublishProduct(articleId, null);
            }
            catch (ProductException exception)
            {
                string errorJson = JsonConvert.SerializeObject(new
                {
                    exception.ProductId,
                    exception.Message,
                });

                return new ContentResult
                {
                    Content = errorJson, 
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ContentType = "application/json"
                };
            }

            return NoContent();
        }

        [HttpPost]
        public ActionResult ClonePartialProduct(
            [FromBody] ClonePartialProductRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath);

            Content cloneContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath, forClone: true);

            int clonedProdictId = _cloneBatchAction
                .CloneProduct(request.CloneArticleId, cloneContent.DeepCopy(), null)
                .Value;

            ArticleObject articleObject = LoadProductGraph(partialContent, clonedProdictId, isLive);

            string productJson = JsonConvert.SerializeObject(articleObject);

            return Content(productJson, "application/json");
        }

        [HttpPost]
        public ActionResult ClonePartialProductPrototype(
            [FromBody] ClonePartialProductPrototypeRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content relationContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath);

            EntityField relationField = (EntityField)relationContent.Fields
                .Single(f => f.FieldName == request.RelationFieldName);

            Content cloneContent = relationField.CloneDefinition ?? relationField.Content;

            var qpFiels = _fieldService.Read(relationField.FieldId);

            int backwardFieldId = relationField is BackwardRelationField
                ? relationField.FieldId
                : qpFiels.BackRelationId.Value;

            int clonePrototypeId = 0;
            if (!String.IsNullOrWhiteSpace(relationField.ClonePrototypeCondition))
            {
                clonePrototypeId = _articleService
                    .Ids(relationField.Content.ContentId, null, filter: relationField.ClonePrototypeCondition)
                    .FirstOrDefault();
            }
            if (clonePrototypeId == 0)
            {
                throw new InvalidOperationException(
                    $"Unable to receive prototype for product creating: contentId={relationField.Content.ContentId}");
            }

            int clonedProdictId = _cloneBatchAction
                .CloneProduct(clonePrototypeId, cloneContent.DeepCopy(), new Dictionary<string, string>
                {
                    ["FieldId"] = backwardFieldId.ToString(),
                    ["ArticleId"] = request.ParentArticleId.ToString(),
                })
                .Value;

            ArticleObject articleObject = LoadProductGraph(relationField.Content, clonedProdictId, isLive);

            string productJson = JsonConvert.SerializeObject(articleObject);

            return Content(productJson, "application/json");
        }

        [HttpPost]
        public ActionResult RemovePartialProduct(
            [FromBody] RemovePartialProductRequest request, bool isLive = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Content rootContent = _contentDefinitionService
                .GetDefinitionById(request.ProductDefinitionId, isLive);

            Content partialContent = _editorPartialContentService
                .FindContentByPath(rootContent, request.ContentPath);

            var productDefinition = new ProductDefinition { StorageSchema = partialContent };

            var qpArticle = _articleService.Read(request.RemoveArticleId);

            if (qpArticle != null)
            {
                _deleteAction.DeleteProduct(qpArticle, productDefinition);
            }

            return NoContent();
        }

        #region Requests
        
        public class SavePartialProductRequest : PartialProductRequest
        {
            [Required]
            public JObject PartialProduct { get; set; }
        }

        public class ClonePartialProductRequest : PartialProductRequest
        {
            /// <summary>
            /// Id корневой статьи для клонирования
            /// </summary>
            public int CloneArticleId { get; set; }
        }

        public class ClonePartialProductPrototypeRequest : PartialProductRequest
        {
            /// <summary>
            /// Имя поля связи, которое необходимо загрузить
            /// </summary>
            [Required]
            public string RelationFieldName { get; set; }

            /// <summary>
            /// Id родительской статьи
            /// </summary>
            public int ParentArticleId { get; set; }
        }

        public class RemovePartialProductRequest : PartialProductRequest
        {
            /// <summary>
            /// Id корневой статьи для удаления
            /// </summary>
            public int RemoveArticleId { get; set; }
        }

        #endregion
    }
}