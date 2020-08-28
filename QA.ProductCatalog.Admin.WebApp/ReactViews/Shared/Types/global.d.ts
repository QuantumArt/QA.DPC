/**
 * Global definitions for developement
 * DO NOT IMPORT ANYTHING OR YOU WILL BREAK BUILD PROCESS!
 */

declare interface Window {
  definitionEditor: DefinitionEditorSettings;
  task: TaskWindowTypes;
  notificationPermissionRequested: boolean;

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

  partialSend?: {
    getActiveTaskIdUrl: string;

    sendForm: {
      hidden: {
        ignoredStatuses: string[];
        localize: string;
      };
      legend: string;
      description: string;
      processSpecialStatusesCheckbox: string;
      sendOnStageOnlyCheckbox: string;
      sendUrl: string;
      sendButton: string;
    };

    result: {
      culture: string;
      legend: string;
      getTaskUrl: string;
      labels: {
        displayName: string;
        id: string;
        createdDate: string;
        userName: string;
        state: string;
        progress: string;
        lastStatusChangeTime: string;
        message: string;
      };
      sendNewPackageButton: string;
    };
  };
}
