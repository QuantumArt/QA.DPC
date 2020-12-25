import { IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { assign } from "lodash";

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

export const mapEditFormModel = (x: IEditFormModel): EditFormModel => {
  const formModel = {} as EditFormModel;
  assign(formModel, x);
  return formModel;
};
