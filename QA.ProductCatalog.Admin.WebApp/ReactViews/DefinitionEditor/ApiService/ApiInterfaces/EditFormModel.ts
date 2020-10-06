export interface IEditFormModel {
  FieldType: number;
  RelatedContentName: string;
  RelatedContentId: string;
  FieldId: number;
  CloningMode: number;
  UpdatingMode: number;
  DeletingMode: number;
  PublishingMode: number;
  DefaultCachePeriod: string;
  FieldName: string;
  FieldTitle: string;
  PreloadingMode: number;
  RelationCondition: string;
  ClonePrototypeCondition: string;
  VirtualPath: string;
  ObjectToRemovePath: string;
  PreserveSource: boolean;
  Converter: {};
  SkipCData: boolean;
  LoadLikeImage: boolean;
  InDefinition: boolean;
  Path: string;
  Xml: string;
  IsReadOnly: boolean;
  LoadAllPlainFields: boolean;
  IsFromDictionaries: boolean;
  CachePeriod: string;
  CacheEnabled: boolean;
  AlreadyCachedAsDictionary: boolean;
  ContentName: string;
  //new added in c#
  RelateTo: string;
  IsClassifier: string;
  RelationConditionDescription: string;
  ClonePrototypeConditionDescription: string;
}
