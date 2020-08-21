using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QA.Configuration;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
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

        public DefinitionEditorController(IContentDefinitionService contentDefinitionService, DefinitionEditorService definitionEditorService, IFieldService fieldService, IServiceFactory serviceFactory, ICacheItemWatcher cacheItemWatcher)
        {
            _contentDefinitionService = contentDefinitionService;

            _definitionEditorService = definitionEditorService;

            definitionEditorService.PathSeparator = DefinitionTreeNode.Separator;

            _fieldService = fieldService;

            _contentService = serviceFactory.GetContentService();

            _cacheItemWatcher = cacheItemWatcher;
        }

        [RequireCustomAction]
        public ActionResult Index([Bind("content_item_id")] int? contentItemId, int? contentId, bool beta = false)
        {
            _cacheItemWatcher.TrackChanges();

            var definitionXml = contentItemId.HasValue
                ? _contentDefinitionService.GetDefinitionXml(contentItemId.Value)
                : contentId.HasValue
                    ? XamlConfigurationParser.CreateFromObject(new Content { ContentId = contentId.Value })
                    : string.Empty;

            if (beta)
            {
                return View("DefinitionEditor", new DefinitionEditor { ContentItemId = contentItemId, Xml = definitionXml });
            }

            return View(new DefinitionEditor { ContentItemId = contentItemId, Xml = definitionXml });
        }

        [RequireCustomAction]
        public ActionResult SaveDefinition([Bind("content_item_id")] int contentItemId, string xml)
        {
            _contentDefinitionService.SaveDefinition(contentItemId, xml);

            return Json("Ok");
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

            var objFromDef = _definitionEditorService.GetObjectFromPath(content, defInfo.Path, out var notFoundInDef);

            if (objFromDef == null)
                return Json(new { MissingFieldToDeleteId = defInfo.Path });

            DefinitionTreeNode resultObj = null;

            switch (objFromDef)
            {
                case Content def:
                    {
                        var isFromDictionaries = _definitionEditorService.GetParentObjectFromPath(content, defInfo.Path) is Dictionaries;

                        resultObj = new DefinitionTreeNode(def, null, defInfo.Path, isFromDictionaries, notFoundInDef, _contentService);
                        break;
                    }
                case Field field:
                    {
                        var existsInQp = true;

                        if (!(field is BaseVirtualField))
                            existsInQp = _fieldService.Read(field.FieldId) != null;

                        resultObj = new DefinitionTreeNode(field, null, defInfo.Path, !existsInQp, notFoundInDef);
                        break;
                    }
            }

            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(resultObj) };
        }

        [RequireCustomAction]
        public ActionResult EditBeta(DefinitionPathInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var objectToEdit = _definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var notFoundInDef);

            if (objectToEdit is Field edit)
                return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(new DefinitionFieldInfo(edit)
                {
                    InDefinition = !notFoundInDef,
                    Path = defInfo.Path,
                    Xml = defInfo.Xml
                }
               )};

            var isFromDictionaries = false;

            if (!Equals(rootContent, objectToEdit))
                isFromDictionaries = _definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path) is Dictionaries;

            var contentToEdit = (Content)objectToEdit;
            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(new DefinitionContentInfo
            {
                ContentName = contentToEdit.ContentName,
                IsReadOnly = contentToEdit.IsReadOnly,
                PublishingMode = contentToEdit.PublishingMode,
                ContentId = contentToEdit.ContentId,
                LoadAllPlainFields = contentToEdit.LoadAllPlainFields,
                CacheEnabled = contentToEdit.CachePeriod.HasValue,
                CachePeriod = contentToEdit.CachePeriod ?? new TimeSpan(1, 45, 0),
                Path = defInfo.Path,
                Xml = defInfo.Xml,
                InDefinition = !notFoundInDef,
                IsFromDictionaries = isFromDictionaries,
            }) };  
        }

        [RequireCustomAction]
        public ActionResult Edit(DefinitionPathInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var objectToEdit = _definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var notFoundInDef);

            if (objectToEdit is Field edit)
                return
                    PartialView(new DefinitionFieldInfo(edit)
                    {
                        InDefinition = !notFoundInDef,
                        Path = defInfo.Path,
                        Xml = defInfo.Xml
                    });

            var isFromDictionaries = false;

            if (!Equals(rootContent, objectToEdit))
                isFromDictionaries = _definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path) is Dictionaries;

            var contentToEdit = (Content)objectToEdit;

            return PartialView("Edit",
                new DefinitionContentInfo
                {
                    ContentName = contentToEdit.ContentName,
                    IsReadOnly = contentToEdit.IsReadOnly,
                    PublishingMode = contentToEdit.PublishingMode,
                    ContentId = contentToEdit.ContentId,
                    LoadAllPlainFields = contentToEdit.LoadAllPlainFields,
                    CacheEnabled = contentToEdit.CachePeriod.HasValue,
                    CachePeriod = contentToEdit.CachePeriod ?? new TimeSpan(1, 45, 0),
                    Path = defInfo.Path,
                    Xml = defInfo.Xml,
                    InDefinition = !notFoundInDef,
                    IsFromDictionaries = isFromDictionaries,
                });
        }

        [RequireCustomAction]
        public ActionResult SaveField(DefinitionFieldInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var savedField = _definitionEditorService.UpdateOrDeleteField(rootContent, defInfo.GetField(), defInfo.Path, !defInfo.InDefinition);

            string resultXml = XamlConfigurationParser.CreateFromObject(rootContent);

            ModelState.Clear();

            Field fieldForEditView = savedField ?? (Field)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out _);

            return PartialView("Edit", new DefinitionFieldInfo(fieldForEditView)
            {
                InDefinition = defInfo.InDefinition,
                Path = defInfo.Path,
                Xml = resultXml
            });
        }

        [RequireCustomAction]
        public ActionResult SaveContent(DefinitionContentInfo defInfo)
        {
            var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

            var contentToSave = (Content)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out var notFoundInDef);

            contentToSave.ContentName = defInfo.ContentName;
            contentToSave.CachePeriod = defInfo.CacheEnabled ? defInfo.CachePeriod : (TimeSpan?)null;

            if (defInfo.IsFromDictionaries)
            {
                var dictionaries = (Dictionaries)_definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path);

                if (contentToSave.CachePeriod != null && notFoundInDef)
                    dictionaries.ContentDictionaries[contentToSave.ContentId] = contentToSave;
                else if (contentToSave.CachePeriod == null && !notFoundInDef)
                    dictionaries.ContentDictionaries.Remove(contentToSave.ContentId);
            }
            else
            {
                contentToSave.IsReadOnly = defInfo.IsReadOnly;
                contentToSave.LoadAllPlainFields = defInfo.LoadAllPlainFields;
                contentToSave.PublishingMode = defInfo.PublishingMode;



                if (notFoundInDef)
                {
                    var parentExtension = (ExtensionField)_definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path);

                    parentExtension.ContentMapping[contentToSave.ContentId] = contentToSave;
                }
            }

            string resultXml = XamlConfigurationParser.CreateFromObject(rootContent);


            ModelState.Clear();

            return Edit(new DefinitionPathInfo { Xml = resultXml, Path = defInfo.Path });
        }
    }
}