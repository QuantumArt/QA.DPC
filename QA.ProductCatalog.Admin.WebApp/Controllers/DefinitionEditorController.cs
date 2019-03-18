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

		public ActionResult Index(int? content_item_id, int? contentId)
		{
			_cacheItemWatcher.TrackChanges();

			string definitionXml = content_item_id.HasValue
				? _contentDefinitionService.GetDefinitionXml(content_item_id.Value)
				: contentId.HasValue
					? XamlConfigurationParser.CreateFromObject(new Content {ContentId = contentId.Value})
					: string.Empty;

			return View(new DefinitionEditor {ContentItemId = content_item_id, Xml = definitionXml});
		}

		[RequireCustomAction]
		public ActionResult SaveDefinition(int content_item_id, string xml)
		{
			_contentDefinitionService.SaveDefinition(content_item_id, xml);

			return Json("Ok");
		}

		public ActionResult GetDefinitionLevel(DefinitionPathInfo defInfo)
		{
			var content = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);
            var objects = DefinitionTreeNode.GetObjectsFromPath(content, defInfo.Path, _fieldService,
                _definitionEditorService, _contentService);
			return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(objects)};
		}


		public ActionResult GetSingleNode(DefinitionPathInfo defInfo)
		{
			var content = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

			bool notFoundInDef;

			var objFromDef = _definitionEditorService.GetObjectFromPath(content, defInfo.Path, out notFoundInDef);

			if (objFromDef == null)
				return Json(new { MissingFieldToDeleteId = defInfo.Path });

			DefinitionTreeNode resultObj = null;

			if (objFromDef is Content)
			{
				bool isFromDictionaries = _definitionEditorService.GetParentObjectFromPath(content, defInfo.Path) is Dictionaries;

				resultObj = new DefinitionTreeNode((Content)objFromDef, null, defInfo.Path, isFromDictionaries, notFoundInDef, _contentService);

			}
			else if (objFromDef is Field)
			{
				var field = (Field)objFromDef;

				bool existsInQp = true;

				if (!(field is BaseVirtualField))
					existsInQp = _fieldService.Read(field.FieldId) != null;

				resultObj = new DefinitionTreeNode((Field)objFromDef, null, defInfo.Path, !existsInQp, notFoundInDef);
			}

            return new ContentResult() { ContentType = "application/json", Content = JsonConvert.SerializeObject(resultObj)};
		}

		public ActionResult Edit(DefinitionPathInfo defInfo)
		{
			var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

			bool notFoundInDef;

			var objectToEdit = _definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out notFoundInDef);

			if (objectToEdit is Field)
				return
					PartialView(new DefinitionFieldInfo
					{
						Field = (Field)objectToEdit,
						InDefinition = !notFoundInDef,
						Path = defInfo.Path,
						Xml = defInfo.Xml
					});
			else
			{
				bool isFromDictionaries = false;

				if (rootContent != objectToEdit)
					isFromDictionaries = _definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path) is Dictionaries;

				bool alreadyCachedAsDictionary = false;

				Content contentToEdit = (Content)objectToEdit;

				Dictionaries dicSettings = (Dictionaries)rootContent.Fields.SingleOrDefault(x => x is Dictionaries);

				if (!isFromDictionaries)
					alreadyCachedAsDictionary = dicSettings != null && dicSettings.ContentDictionaries.Any(x => x.Key == contentToEdit.ContentId);
				else if (!notFoundInDef && XmlMappingBehavior.GetCachePeriod(contentToEdit) == null)
				{
					//если внутри словарей не задан явно период кеширования то берем дефолтный
					XmlMappingBehavior.SetCachePeriod(contentToEdit, dicSettings.DefaultCachePeriod);
				}

				return PartialView("Edit",
					new DefinitionContentInfo
					{
						Content = contentToEdit,
						Path = defInfo.Path,
						Xml = defInfo.Xml,
						InDefinition = !notFoundInDef,
						IsFromDictionaries = isFromDictionaries,
						AlreadyCachedAsDictionary = alreadyCachedAsDictionary
					});
			}
		}

		public ActionResult SaveDictionaries(DefinitionInfoForFieldsSave<Dictionaries> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			return SaveField(defInfo);
		}

		public ActionResult SaveVirtualField(DefinitionInfoForFieldsSave<VirtualField> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			return SaveField(defInfo);
		}

		public ActionResult SaveEntityField(DefinitionInfoForFieldsSave<EntityField> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			return SaveField(defInfo);
		}

		public ActionResult SaveBackwardRelationField(DefinitionInfoForFieldsSave<BackwardRelationField> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			return SaveField(defInfo);
		}

		public ActionResult SavePlainField(DefinitionInfoForFieldsSave<PlainField> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			bool skipCdata = Request.Form["skipcdata"] != "false";

			if (skipCdata)
				defInfo.Field.CustomProperties[QA.Core.DPC.Loader.XmlProductService.RenderTextFieldAsXmlName] = true;

            var loadLikeImage = ((string[])Request.Form["loadlikeimage"])?.Contains("true") ?? false;

		    if (loadLikeImage)
		        defInfo.Field.CustomProperties[QA.Core.DPC.Loader.XmlProductService.RenderFileFieldAsImage] = true;

            return SaveField(defInfo);
		}

		public ActionResult SaveExtensionField(DefinitionInfoForFieldsSave<ExtensionField> defInfo)
		{
			((DefinitionFieldInfo)defInfo).Field = defInfo.Field;

			return SaveField(defInfo);
		}

		private ActionResult SaveField(DefinitionFieldInfo defInfo)
		{
			var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

			var savedField = _definitionEditorService.UpdateOrDeleteField(rootContent, defInfo.Field, defInfo.Path, !defInfo.InDefinition);

			string resultXml = XamlConfigurationParser.CreateFromObject(rootContent);

			ModelState.Clear();

			bool junk;

			Field fieldForEditView = savedField ?? (Field)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out junk);

			return PartialView("Edit", new DefinitionFieldInfo
			{
				InDefinition = defInfo.InDefinition,
				Field = fieldForEditView,
				Path = defInfo.Path,
				Xml = resultXml
			});
		}
        
		public ActionResult SaveContent(DefinitionContentInfo defInfo)
		{
			var rootContent = (Content)XamlConfigurationParser.CreateFrom(defInfo.Xml);

			bool notFoundInDef;

			var contentToSave = (Content)_definitionEditorService.GetObjectFromPath(rootContent, defInfo.Path, out notFoundInDef);

			contentToSave.ContentName = defInfo.Content.ContentName;

			TimeSpan? cachePeriod = (TimeSpan?)XmlMappingBehavior.GetCachePeriod(defInfo.Content);

			XmlMappingBehavior.SetCachePeriod(contentToSave, cachePeriod);

			if (defInfo.IsFromDictionaries)
			{
				var dictionaries = (Dictionaries)_definitionEditorService.GetParentObjectFromPath(rootContent, defInfo.Path);

				if (cachePeriod.HasValue && notFoundInDef)
					dictionaries.ContentDictionaries[contentToSave.ContentId] = contentToSave;
				else if (!cachePeriod.HasValue && !notFoundInDef)
					dictionaries.ContentDictionaries.Remove(contentToSave.ContentId);
			}
			else
			{
                contentToSave.IsReadOnly = defInfo.Content.IsReadOnly;
				contentToSave.LoadAllPlainFields = defInfo.Content.LoadAllPlainFields;
				contentToSave.PublishingMode = defInfo.Content.PublishingMode;

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