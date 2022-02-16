using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.Configuration;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.Resources;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class DefinitionEditorController : Controller
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly DefinitionEditorService _definitionEditorService;
        private readonly IFieldService _fieldService;
        private readonly ContentService _contentService;
        private readonly ICacheItemWatcher _cacheItemWatcher;
        private readonly Properties _props;

        public DefinitionEditorController(IContentDefinitionService contentDefinitionService, DefinitionEditorService definitionEditorService, IFieldService fieldService, IServiceFactory serviceFactory, ICacheItemWatcher cacheItemWatcher, IOptions<Properties> options)
        {
            _contentDefinitionService = contentDefinitionService;

            _definitionEditorService = definitionEditorService;

            definitionEditorService.PathSeparator = DefinitionTreeNode.Separator;

            _fieldService = fieldService;

            _contentService = serviceFactory.GetContentService();

            _cacheItemWatcher = cacheItemWatcher;

            _props = options.Value;
        }

        [RequireCustomAction]
        public ActionResult Index([Bind("content_item_id")] int? contentItemId, int? contentId)
        {
            _cacheItemWatcher.TrackChanges();

			var definitionXml = contentItemId.HasValue
				? _contentDefinitionService.GetDefinitionXml(contentItemId.Value)
				: contentId.HasValue
					? XamlConfigurationParser.Save(new Content {ContentId = contentId.Value})
					: string.Empty;

            return View(new DefinitionEditor
            {
                ContentItemId = contentItemId, 
                Xml = definitionXml,
                Version = _props.UseTimestampVersion ? Properties.TimeStamp : _props.BuildVersion 
            });
        }

        [RequireCustomAction]
        public ActionResult SaveDefinition([Bind("content_item_id")] int contentItemId, string xml)
        {
            _contentDefinitionService.SaveDefinition(contentItemId, xml);

            return Json("Ok");
        } 
        
        public ActionResult GetInitialNodeUrl(int? contentId)
        {
            var xml = XamlConfigurationParser.Save(new Content {ContentId = contentId.Value});

            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(xml) };
        }

        public ActionResult GetDefinitionLevel(DefinitionPathInfo defInfo)
        {
            var content = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);
            var objects = DefinitionTreeNode.GetObjectsFromPath(content, defInfo.Path, _fieldService,
                _definitionEditorService, _contentService);
            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(objects) };
        }


        public ActionResult GetSingleNode(DefinitionPathInfo defInfo)
        {
            var content = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var objFromDef = _definitionEditorService.GetObjectFromPath(content, defInfo.Path, out var searchResult);

            if (objFromDef == null)
                return Json(new { MissingFieldToDeleteId = defInfo.Path });

            DefinitionTreeNode resultObj = null;

            switch (objFromDef)
            {
                case Content def:
                    {
                        var parentObject = _definitionEditorService.GetParentObjectFromPath(content, defInfo.Path);
                        resultObj = new DefinitionTreeNode(
                            def, _contentService,
                            ownPath: defInfo.Path,
                            isFromDictionaries: parentObject is Dictionaries,
                            isExtensionField: parentObject is ExtensionField,
                            inDefinition: searchResult.Found
                        );
                        break;
                    }
                case Field field:
                    {
                        var existsInQp = true;

                        if (!(field is BaseVirtualField))
                            existsInQp = _fieldService.Read(field.FieldId) != null;

                        resultObj = new DefinitionTreeNode(
                            field, ownPath: defInfo.Path, missingInQp: !existsInQp, inDefinition: searchResult.Found
                        );
                        break;
                    }
            }

            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(resultObj) };
        }

        [RequireCustomAction]
        public ActionResult Edit(DefinitionPathInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);
            var objectToEdit = _definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var searchResult);
            
            if (objectToEdit is Field edit)
            {
                var resultFieldDefInfo = new DefinitionFieldInfo(edit, searchResult, defInfo);

                return new ContentResult()
                {
                    ContentType = "application/json",
                    Content = JsonConvert.SerializeObject(resultFieldDefInfo)
                };
            }

            var isFromDictionaries = false;
            var isExtensionContent = false;

            if (!Equals(rootContent, objectToEdit))
            {
                var parentObject = _definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path);
                isFromDictionaries = parentObject is Dictionaries;
                isExtensionContent = parentObject is ExtensionField;
            }

            var contentToEdit = (Content)objectToEdit;
            var resultContentDefInfo = new DefinitionContentInfo(contentToEdit, searchResult, defInfo)
            {
                IsFromDictionaries = isFromDictionaries,
                IsExtensionContent = isExtensionContent
            };
            
            return new ContentResult()
            {
                ContentType = "application/json", 
                Content = JsonConvert.SerializeObject(resultContentDefInfo)
            };  
        }

        [RequireCustomAction]
        public ActionResult SaveField(DefinitionFieldInfo defInfo)
        {
            if (!String.IsNullOrEmpty(defInfo.InDefinitionImplicitly))
            {
                defInfo.InDefinition = true;
            }
            
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);
            _definitionEditorService.UpdateOrDeleteField(rootContent, defInfo.GetField(), defInfo.Path, !defInfo.InDefinition);
            var resultXml = XamlConfigurationParser.Save(rootContent);

            ModelState.Clear();

            Field edit = (Field)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var searchResult);
            var resultDefInfo = new DefinitionFieldInfo(edit, searchResult, defInfo) { Xml = resultXml };
            return new ContentResult()
            {
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(resultDefInfo)
            };
        }
    
        [RequireCustomAction]
        public ActionResult SaveContent(DefinitionContentInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var contentToSave = (Content)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var searchResult);
            var isRoot = Equals(contentToSave, rootContent);

            contentToSave.ContentName = defInfo.ContentName;
            contentToSave.CachePeriod = defInfo.CacheEnabled ? defInfo.CachePeriod : (TimeSpan?)null;

            var parentObject = (isRoot) ? null : _definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path);
            if (defInfo.IsFromDictionaries)
            {
                var dictionaries = (Dictionaries)parentObject;

                if (contentToSave.CachePeriod != null && !searchResult.Found)
                    dictionaries.ContentDictionaries[contentToSave.ContentId] = contentToSave;
                else if (contentToSave.CachePeriod == null && searchResult.Found)
                    dictionaries.ContentDictionaries.Remove(contentToSave.ContentId);
            }
            else
            {
                contentToSave.IsReadOnly = defInfo.IsReadOnly;
                contentToSave.LoadAllPlainFields = defInfo.LoadAllPlainFields;
                contentToSave.PublishingMode = defInfo.PublishingMode;

                if (parentObject is ExtensionField parentExtension)
                {
                    if (!searchResult.Found && defInfo.InDefinition)
                    {
                        parentExtension.ContentMapping[contentToSave.ContentId] = contentToSave;
                    }
                    
                    if (searchResult.Found && !defInfo.InDefinition)
                    {
                        parentExtension.ContentMapping.Remove(contentToSave.ContentId);
                    }
                    
                }
            }

            string resultXml = XamlConfigurationParser.Save(rootContent);

            var objectToEdit = _definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out _);
            var contentToEdit = (Content)objectToEdit;
            
            ModelState.Clear();

            var resultDefInfo = new DefinitionContentInfo(contentToEdit, searchResult, defInfo)
            {
                Xml = resultXml,
                IsFromDictionaries = parentObject is Dictionaries,
                IsExtensionContent = parentObject is ExtensionField
            };
            
            return new ContentResult()
            {
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(resultDefInfo)
            };
        }
    }
}