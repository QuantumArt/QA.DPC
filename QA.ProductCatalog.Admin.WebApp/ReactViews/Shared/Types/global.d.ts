declare interface Window {
  definitionEditor: DefinitionEditorSettings;
  pmrpc: any;
  highloadFront?: {
    legend: string;
    customerCode: string;
    processingIndex: string;
    culture: string;
    columnHeaders: {
      default: string;
      language: string;
      type: string;
      date: string;
      processing: string;
      updating: string;
      progress: string;
    };
  };
}
