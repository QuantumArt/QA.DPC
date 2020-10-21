/**
 * Global definitions
 * DO NOT IMPORT ANYTHING OR YOU WILL BREAK BUILD PROCESS!
 */

declare interface JQueryStatic {
  scrollTo(...args): void;
}

declare interface JQuery {
  scrollTo(...args): void;
}

declare interface Window {
  definitionEditor: DefinitionEditorSettings;
  task: TaskWindowTypes;
  notification: NotificationWindowTypes;
  notificationPermissionRequested: boolean;
  pmrpc: any;

  highloadFront?: {
    CustomerCode: string;
    Culture: string;
    strings: {
      Legend: string;
      ProceedIndexing: string;
      Default: string;
      Language: string;
      Type: string;
      Date: string;
      Processing: string;
      Updating: string;
      Progress: string;
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

  Quantumart: import("../../../wwwroot/js/qp/QP8BackendApi.Interaction").Quantumart;

  QA?: {
    Product?: {
      Index?: any;
      TabStrip?: any;
    };
    Utils?: any;
    Integration?: any;
    Enums?: any;
  };
}
