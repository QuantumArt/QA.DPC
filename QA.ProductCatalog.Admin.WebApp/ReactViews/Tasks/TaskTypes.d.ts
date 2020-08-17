declare interface TaskWindowTypes {
  tableFields: {
    userName: string;
    status: number;
    schedule: string;
    progress: number;
    name: string;
    created: string;
    lastStatusChange: string;
    message: string;
  };
  utils: {
    cronExpression: string;
  };
  //отображение названий в  фильтрах грида
  gridFiltersDefinitions: {
    isFalse: string;
    isTrue: string;
    filter: string;
    clear: string;
  };
  other: {
    statusValues: { label: string; value: number }[];
  };
}
