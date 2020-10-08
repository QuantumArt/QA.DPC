import { IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { isNull, isUndefined } from "lodash";

class EditFormModel implements IEditFormModel {
  ClonePrototypeCondition: string;
  ContentName: string;
  ClonePrototypeConditionDescription: string;
  CloningMode: number;
  Converter: {};
  DefaultCachePeriod: string;
  DeletingMode: number;
  PublishingMode: number;
  FieldId: number;
  ContentId: number;
  FieldName: string;
  FieldTitle: string;
  FieldType: number;
  InDefinition: boolean;
  IsClassifier: string;
  LoadLikeImage: boolean;
  ObjectToRemovePath: string;
  Path: string;
  PreloadingMode: number;
  PreserveSource: boolean;
  RelateTo: string;
  RelatedContentId: string;
  RelatedContentName: string;
  RelationCondition: string;
  RelationConditionDescription: string;
  SkipCData: boolean;
  UpdatingMode: number;
  VirtualPath: string;
  Xml: string;
  IsReadOnly: boolean;
  LoadAllPlainFields: boolean;
  IsFromDictionaries: boolean;
  CachePeriod: string;
  CacheEnabled: boolean;
  AlreadyCachedAsDictionary: boolean;
}

/**
 * маппер задает:
 * 1.порядок отрисовки полей в UI
 * 2.отсеивает nullable свойства и сохраняет, там где они ожидаются
 * */
export const mapEditFormModel = (x: IEditFormModel): EditFormModel => {
  const formModel = {} as EditFormModel;
  const isValue = (val: any, isValueCanBeNull: boolean = false) => {
    return isValueCanBeNull ? !isUndefined(val) : !isNull(val) && !isUndefined(val);
  };

  if (isValue(x?.ContentId)) formModel.ContentId = x.ContentId;
  if (isValue(x?.InDefinition) && isValue(x?.FieldType)) formModel.InDefinition = x.InDefinition;
  if (isValue(x?.CachePeriod)) formModel.CachePeriod = x.CachePeriod;
  if (isValue(x?.CacheEnabled)) formModel.CacheEnabled = x.CacheEnabled;
  if (isValue(x?.ContentName)) formModel.ContentName = x.ContentName;
  if (isValue(x?.DefaultCachePeriod)) formModel.DefaultCachePeriod = x.DefaultCachePeriod;
  if (isValue(x?.FieldId)) formModel.FieldId = x.FieldId;
  if (isValue(x?.FieldName)) formModel.FieldName = x.FieldName;
  if (isValue(x.FieldTitle, true)) formModel.FieldTitle = x.FieldTitle;
  if (isValue(x?.CloningMode)) formModel.CloningMode = x.CloningMode;
  if (isValue(x?.DeletingMode)) formModel.DeletingMode = x.DeletingMode;
  if (isValue(x?.UpdatingMode)) formModel.UpdatingMode = x.UpdatingMode;
  if (isValue(x?.IsClassifier)) formModel.IsClassifier = x.IsClassifier;
  if (isValue(x?.PreloadingMode)) formModel.PreloadingMode = x.PreloadingMode;
  if (isValue(x?.ClonePrototypeConditionDescription))
    formModel.ClonePrototypeConditionDescription = x.ClonePrototypeConditionDescription;
  if (isValue(x.RelationCondition, true)) formModel.RelationCondition = x.RelationCondition;
  if (isValue(x.ClonePrototypeCondition, true))
    formModel.ClonePrototypeCondition = x.ClonePrototypeCondition;
  if (isValue(x?.RelationConditionDescription))
    formModel.RelationConditionDescription = x.RelationConditionDescription;
  if (isValue(x?.RelateTo)) formModel.RelateTo = x.RelateTo;
  if (isValue(x?.RelatedContentId)) formModel.RelatedContentId = x.RelatedContentId;
  if (isValue(x?.RelatedContentName)) formModel.RelatedContentName = x.RelatedContentName;
  if (isValue(x?.ObjectToRemovePath)) formModel.ObjectToRemovePath = x.ObjectToRemovePath;
  if (isValue(x?.Path)) formModel.Path = x.Path;
  if (isValue(x?.VirtualPath)) formModel.VirtualPath = x.VirtualPath;
  if (isValue(x?.Xml)) formModel.Xml = x.Xml;
  if (isValue(x?.SkipCData) && x?.FieldType === 0) formModel.SkipCData = x.SkipCData;
  if (isValue(x?.LoadLikeImage) && x?.FieldType === 0) formModel.LoadLikeImage = x.LoadLikeImage;
  if (isValue(x?.FieldType)) formModel.FieldType = x.FieldType;
  if (isValue(x?.Converter)) formModel.Converter = x.Converter;
  if (isValue(x?.IsReadOnly)) formModel.IsReadOnly = x.IsReadOnly;
  if (isValue(x?.LoadAllPlainFields)) formModel.LoadAllPlainFields = x.LoadAllPlainFields;
  if (isValue(x?.IsFromDictionaries)) formModel.IsFromDictionaries = x.IsFromDictionaries;
  if (isValue(x?.AlreadyCachedAsDictionary))
    formModel.AlreadyCachedAsDictionary = x.AlreadyCachedAsDictionary;
  if (isValue(x?.PublishingMode) && x?.IsFromDictionaries === false)
    formModel.PublishingMode = x.PublishingMode;
  return formModel;
};
