declare interface DefinitionEditorSettings {
  xml: string;
  saveToDb: string;
  saveDefinitionUrl: string;
  getSingleNodeUrl: string;
  getDefinitionLevelUrl: string;
  editBetaUrl: string;
  editUrl: string;
  strings: {
    DictionaryCachingSettings: string;
    MissingInQP: string;
    NotInDefinition: string;
    Apply: string;
    Refresh: string;
    SaveAndExit: string;
    Exit: string;
    WrapLines: string;
    QueryOnClick: string;
    Close: string;
    ExitAnyway: string;
    BackToEditing: string;
    HideLog: string;
    ShowLog: string;
    SameDefinition: string;
    JsonFieldName: string;
    RelationConditionDescription: string;
    Path: string;
    RemovePath: string;
    Converter: string;
    DontWrapInCData: string;
    LoadAsImage: string;
    FieldNameForCard: string;
    LabelText: string;
  };
  backendEnums: {
    update: string;
    publish: string;
    preload: string;
    delete: string;
    clone: string;
  };
}
