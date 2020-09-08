declare interface DefinitionEditorSettings {
  xml: string;
  saveToDb: string;
  saveDefinitionUrl: string;
  getSingleNodeUrl: string;
  getDefinitionLevelUrl: string;
  editBetaUrl: string;
  editUrl: string;
  editorStrings: {
    dictionaryCahingSettings: string;
    saveText: string;
    editText: string;
    endEditText: string;
  };
  backendEnums: {
    update: string;
    publish: string;
    preload: string;
    delete: string;
    clone: string;
  };
  formControlStrings: {
    fieldNameForCard: string;
    labelText: string;
  };
}
