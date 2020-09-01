export interface IEditFormModel {
  FieldType: number;
  RelatedContentName: string;
  RelatedContentId: string;
  FieldId: number;
  CloningMode: number;
  UpdatingMode: number;
  DeletingMode: number;
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
  //new added in c#
  RelateTo: string;
  IsClassifier: string;
  RelationConditionDescription: string;
  ClonePrototypeConditionDescription: string;
}
