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
  gridFiltersDefinitions: {
    isFalse: string;
    isTrue: string;
    filter: string;
    clear: string;
  };
  other: {
    statusValues: { label: string; value: number }[];
  };
  schedule: {
    every: string;
    year: string;
    hour: string;
    day: string;
    month: string;
    minute: string;
    on: string;
    off: string;
    dayOfMonth: string;
    at: string;
    everyDayOfMonth: string;
    everyDayOfWeek: string;
    minutesPastHour: string;
    taskRecurrenceSchedule: string;
    taskId: string;
    scheduleEnabled: string;
    recurrenceMode: string;
    modeRepeat: string;
    modeOneTime: string;
    recurrencePeriod: string;
    periodMinute: string;
    periodHour: string;
    periodDay: string;
    periodWeek: string;
    periodMonth: string;
    periodYear: string;
    monthDays: string;
    months: string;
    hours: string;
    minutes: string;
    weekDays: string;
    close: string;
    apply: string;
  };
  notify: {
    isNotifyActive: boolean;
    runningStateId: number;
    state: string;
    task: string;
    proceed: string;
    img: string;
    formRenderedServerTime: string;
  };
}
