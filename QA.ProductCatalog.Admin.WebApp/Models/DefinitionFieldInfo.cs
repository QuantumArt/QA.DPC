using System;
using System.Collections.Generic;
using System.ComponentModel;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.DPC.Loader;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public enum FieldDefinitionType
    {
        PlainField,
        EntityField,
        BackwardRelationField,
        ExtensionField,
        Dictionaries,
        VirtualField,
        VirtualEntityField,
        VirtualMultiEntityField
    }
    
    public class DefinitionFieldInfo : DefinitionElement
	{

        public DefinitionFieldInfo()
        {
            
        }
        
        public DefinitionFieldInfo(Field field)
        {
            FieldName = field.FieldName;
            FieldTitle = field.FieldTitle;
            FieldId = field.FieldId;
            
            if (field is Dictionaries dict)
            {
                DefaultCachePeriod = dict.DefaultCachePeriod;
                FieldType = FieldDefinitionType.Dictionaries;
            }
            else if (field is PlainField pf)
            {
                InitPlainField(pf);
            }
            else if (field is Association assoc)
            {
                InitAssociation(assoc);
            } 
            else if (field is BaseVirtualField virt)
            {
                InitVirtualField(virt);
            }
        }

        private void InitVirtualField(BaseVirtualField virt)
        {
            if (virt is VirtualField virtCommon)
            {
                ObjectToRemovePath = virtCommon.ObjectToRemovePath;
                VirtualPath = virtCommon.Path;
                FieldType = FieldDefinitionType.VirtualField;
            }
            FieldType = virt is VirtualEntityField ? FieldDefinitionType.VirtualEntityField :  FieldDefinitionType.VirtualMultiEntityField;
        }

        public Field GetField()
        {
            Field result = GetEmptyField();
            result.FieldName = FieldName;
            result.FieldTitle = FieldTitle;
            if (result is Association association)
            {
                FillAssociationField(association);
            }
            else if (result is Dictionaries dict ) 
            {
                dict.DefaultCachePeriod = DefaultCachePeriod;          
            }
            else if (result is VirtualField virt)
            {
                FillVirtualField(virt);          
            }
            else if (result is PlainField plain)
            {
                FillPlainField(plain);          
            }

            return result;

        }

        private void FillPlainField(PlainField plain)
        {
            if (SkipCData)
            {
                plain.CustomProperties[XmlProductService.RenderTextFieldAsXmlName] = true;
            }

            if (LoadLikeImage)
            {
                plain.CustomProperties[XmlProductService.RenderFileFieldAsImage] = true;
            }
        }

        private void FillVirtualField(VirtualField virt)
        {
            virt.ObjectToRemovePath = ObjectToRemovePath;
            virt.Path = VirtualPath;

        }

        private void FillAssociationField(Association output)
        {
            output.CloningMode = CloningMode;
            output.DeletingMode = DeletingMode;
            output.UpdatingMode = UpdatingMode;

            if (output is EntityField er)
            {
                er.RelationCondition = RelationCondition;
                er.PreloadingMode = PreloadingMode;
                er.ClonePrototypeCondition = ClonePrototypeCondition;
            }
        }

        private Field GetEmptyField()
        {
            switch (FieldType)
            {
                case (FieldDefinitionType.VirtualField):
                    return new VirtualField();
                case (FieldDefinitionType.VirtualEntityField):
                    return new VirtualEntityField();
                case (FieldDefinitionType.VirtualMultiEntityField):
                    return new VirtualMultiEntityField();
                case (FieldDefinitionType.Dictionaries):
                    return new Dictionaries();;
                case (FieldDefinitionType.BackwardRelationField):
                    return new BackwardRelationField();
                case FieldDefinitionType.ExtensionField:
                    return new ExtensionField();
                case FieldDefinitionType.EntityField:
                    return new EntityField();
                default:
                    return new PlainField();
            }
        }

        private void InitPlainField(PlainField pf)
        {
            SkipCData = pf.CustomProperties.ContainsKey(XmlProductService.RenderTextFieldAsXmlName);
            LoadLikeImage = pf.CustomProperties.ContainsKey(XmlProductService.RenderFileFieldAsImage);
            FieldType = FieldDefinitionType.PlainField;
        }

        private void InitAssociation(Association assoc)
        {
            CloningMode = assoc.CloningMode;
            DeletingMode = assoc.DeletingMode;
            UpdatingMode = assoc.UpdatingMode;

            if (assoc is EntityField er)
            {
                RelatedContentId = er.Content.ContentId.ToString();
                RelatedContentName = er.Content.ContentName;

                RelationCondition = er.RelationCondition;
                PreloadingMode = er.PreloadingMode;
                ClonePrototypeCondition = er.ClonePrototypeCondition;

                FieldType = assoc is BackwardRelationField
                    ? FieldDefinitionType.BackwardRelationField
                    : FieldDefinitionType.EntityField;
            }
            else
            {
                FieldType = FieldDefinitionType.ExtensionField;
            }

        }

        public FieldDefinitionType FieldType { get; set; }
        
        public string RelatedContentName { get;  set; }

        public string RelatedContentId { get; set; }

        public int FieldId { get; set; }
        
        [DisplayName("При клонировании родительской сущности")]
        public CloningMode CloningMode { get; set; }

        [DisplayName("При создании\\обновлении")]
        public UpdatingMode UpdatingMode { get; set; }

        [DisplayName("При удалении родительской сущности или удалении связи")]
        public DeletingMode DeletingMode { get; set; }
        
        [DisplayName("Период кеширования по умолчанию")]
        public TimeSpan DefaultCachePeriod { get; set; }
        
        [DisplayName("Имя поля")]
        public string FieldName { get; set; }

        [DisplayName("Имя поля для карточки")]
        public string FieldTitle { get; set; }
        
        public PreloadingMode PreloadingMode { get; set; }

        public string RelationCondition { get; set; }

        public string ClonePrototypeCondition { get; set; }
        
        
        public string VirtualPath { get; set; }
        public string ObjectToRemovePath { get; set; }
        
        public bool PreserveSource { get; set; }

        public IFixedTypeValueConverter Converter { get; set; }
        
        public bool SkipCData { get; set; }
        
        public bool LoadLikeImage { get; set; }
        
        
	}
	
}