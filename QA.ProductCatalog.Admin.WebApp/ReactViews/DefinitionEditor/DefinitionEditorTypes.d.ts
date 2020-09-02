﻿declare interface DefinitionEditorSettings {
  xml: string;
  saveToDb: string;
  saveDefinitionUrl: string;
  getSingleNodeUrl: string;
  getDefinitionLevelUrl: string;
  editBetaUrl: string;
  editUrl: string;
  saveText: string;
  editText: string;
  endEditText: string;
  backendEnums: {
    update: string;
    publish: string;
    preload: string;
    delete: string;
  };
}
