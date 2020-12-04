import { action, computed, observable, runInAction, onBecomeObserved, toJS } from "mobx";
import { createContext } from "react";
import { PaginationActions, ScheduleFilterValues, TaskGridFilterType } from "Shared/Enums";
import { apiService } from "Tasks/ApiServices";
import { FilterOptions, PaginationOptions, Task } from "Tasks/ApiServices/DataContracts";
import {
  FETCH_ON_ERROR_TIMEOUT,
  FETCH_TIMEOUT,
  INIT_PAGINATION_OPTIONS,
  SESSION_EXPIRED
} from "Tasks/Constants";
import { l } from "Tasks/Localization";
import { differenceWith, isEqual } from "lodash";
import { SendNotificationOptions, sendNotification } from "@quantumart/qp8backendapi-interaction";

export class Pagination {
  constructor(onChangePage: (operation: PaginationActions) => void) {
    this.setPagination(INIT_PAGINATION_OPTIONS);
    this.changePage = onChangePage;
  }
  @observable private skip: number = 0;
  private readonly take: number = 10;
  @observable private showOnlyMine: boolean;
  readonly changePage: (operation: PaginationActions) => void;

  @computed
  get getPaginationOptions(): PaginationOptions {
    return {
      skip: this.skip,
      take: this.take,
      showOnlyMine: this.showOnlyMine
    };
  }

  setPagination = (props: PaginationOptions) => {
    runInAction(() => {
      this.skip = props.skip;
      this.showOnlyMine = props.showOnlyMine;
    });
  };

  calcPaginationOptionsOnOperation = (operation: PaginationActions): PaginationOptions => {
    const paginationOptions = this.getPaginationOptions;
    if (operation === PaginationActions.None) return this.getPaginationOptions;
    return Object.assign(this.getPaginationOptions, {
      skip:
        operation === PaginationActions.IncrementPage
          ? (paginationOptions.skip += INIT_PAGINATION_OPTIONS.take)
          : (paginationOptions.skip -= INIT_PAGINATION_OPTIONS.take)
    });
  };
}

export interface ValidationResult {
  hasError: boolean;
  message: string;
}

export class Filter {
  constructor(
    initialValue: string,
    onChangeFilter: () => Promise<void>,
    field: string,
    getMappedValue?: () => boolean,
    validationFunc?: (val: string) => ValidationResult
  ) {
    this.value = initialValue;
    this.onChangeFilter = onChangeFilter;
    this.field = field;
    this.validationFunc = validationFunc;
    this.getMappedValue = getMappedValue;
  }

  @observable isActive: boolean = false;

  validationFunc?: (val: string) => ValidationResult;
  @observable validationResult: ValidationResult = { hasError: false, message: "" };

  @observable value: string;
  onChangeFilter: () => Promise<void>;

  get filterOptionsForRequest(): FilterOptions {
    return {
      field: this.field,
      operator: this.operator,
      value: this.getMappedValue ? this.getMappedValue() : this.value
    };
  }

  @action
  setValue = (value: string) => {
    if (this.validationFunc) {
      this.validationResult = this.validationFunc(value);
      if (!this.validationResult.hasError) {
        this.value = value;
      }
    } else {
      this.value = value;
    }
  };

  @action
  toggleActive = (val: boolean) => {
    if (!this.validationResult.hasError) {
      this.isActive = val;
      this.onChangeFilter();
    }
  };

  private readonly field: string;
  private operator: string = "eq";
  private readonly getMappedValue: () => boolean;
}

export class TaskStore {
  constructor() {
    onBecomeObserved(this, "gridData", this.init);
  }
  private IsPriorityRequestPending: boolean = false;
  private alreadyNotifiedTaskIds: { [x: number]: Task } = {};
  @observable.ref private gridData: Task[] = [];
  @observable private myLastTask: Task;
  @observable private total: number = 0;
  @observable isLoading: boolean = true;
  @observable pagination: Pagination = new Pagination((operation: PaginationActions) => {
    this.withLoader(() => this.fetchGridData(operation));
  });

  @observable filters: Map<TaskGridFilterType, Filter> = new Map([
    [
      TaskGridFilterType.StatusFilter,
      new Filter(
        String(window.task.statusValues[0].value),
        () => this.withLoader(() => this.fetchGridData()),
        "StateId"
      )
    ],
    [
      TaskGridFilterType.ScheduleFilter,
      new Filter(
        ScheduleFilterValues.YES,
        () => this.withLoader(() => this.fetchGridData()),
        "HasSchedule",
        function() {
          return this.value === ScheduleFilterValues.YES;
        }
      )
    ],
    [
      TaskGridFilterType.NameFilter,
      new Filter(
        "",
        () => this.withLoader(() => this.fetchGridData()),
        "DisplayName",
        null,
        val => {
          if (val === null) {
            return { message: "", hasError: false };
          } else if (val.length < 3) {
            return { message: "At least 3 characters", hasError: true };
          }
          return { message: "", hasError: false };
        }
      )
    ]
  ]);

  init = async () => {
    await this.withLoader(() => this.fetchGridData());
    setTimeout(this.cyclicFetchGrid, FETCH_TIMEOUT);
  };

  @action
  cyclicFetchGrid = async (): Promise<void> => {
    try {
      const { isNotifyActive, runningStateId } = window.task.notify;

      const paginationOptions = this.pagination.calcPaginationOptionsOnOperation(
        PaginationActions.None
      );
      const filtersOptions = this.getFiltersOptions();

      const response = await apiService.getTasksGrid(paginationOptions, filtersOptions);
      if (
        this.IsPriorityRequestPending ||
        paginationOptions.skip !== this.pagination.getPaginationOptions.skip
      ) {
        throw "the request already have sent and new response from cyclic fetch will not accept in grid";
      }
      this.setGridData(response.tasks);
      this.setTotal(response.totalTasks);
      this.setMyLastTask(response.myLastTask);

      if (isNotifyActive) {
        this.tasksNotificationsSender(response.tasks, runningStateId);
      }

      setTimeout(this.cyclicFetchGrid, FETCH_TIMEOUT);
    } catch (e) {
      if (typeof e === "string" && e === SESSION_EXPIRED) {
        return;
      }
      setTimeout(this.cyclicFetchGrid, FETCH_ON_ERROR_TIMEOUT);
    }
  };

  tasksNotificationsSender = (tasks: Task[], runningStateId: number) => {
    const { formRenderedServerTime, img } = window.task.notify;

    const gridRenderServerTime = new Date(formRenderedServerTime);
    const tasksShouldNotify = tasks.filter(task => {
      const lastStatusChangeTime = task.LastStatusChangeTime
        ? new Date(task.LastStatusChangeTime)
        : null;
      const alreadyNotifiedTask = this.alreadyNotifiedTaskIds[task.Id];
      const isTaskShodBeNotify = !!alreadyNotifiedTask
        ? new Date(task.LastStatusChangeTime).getTime() >
          new Date(alreadyNotifiedTask.LastStatusChangeTime).getTime()
        : true;
      return (
        lastStatusChangeTime &&
        isTaskShodBeNotify &&
        task.StateId > runningStateId &&
        lastStatusChangeTime.getTime() > gridRenderServerTime.getTime()
      );
    });

    if (tasksShouldNotify.length) {
      tasksShouldNotify.forEach(task => {
        this.alreadyNotifiedTaskIds[task.Id] = task;
        const nOptions: SendNotificationOptions = {
          title: `${l("task")}${task.DisplayName} ${l("proceed")}`,
          body: `${l("state")}: ${task.State}`,
          icon: `${window.location.origin}${img}Done48.png`
        };
        if (task.Message) nOptions.body += "\r\n" + task.Message;
        sendNotification(nOptions, window.name, window.top);
      });
    }
  };

  @action
  setGridData = (data: Task[]) => {
    const isSameData = differenceWith(this.gridData, data, isEqual).length === 0;
    if (!isSameData || this.gridData.length !== data.length) {
      this.gridData = data;
    }
  };

  @action
  toggleLoading = (val: boolean): void => {
    this.isLoading = val || !this.isLoading;
  };

  @action
  setTotal = (total: number): void => {
    this.total = total;
  };

  @action
  setMyLastTask = (task: Task): void => {
    this.myLastTask = task;
  };

  @computed
  get lastTask(): Task {
    return this.myLastTask;
  }

  @computed
  get getTotal(): number {
    return this.total;
  }

  @computed
  get getGridData(): Task[] {
    return this.gridData;
  }

  getFiltersOptions = (): FilterOptions[] => {
    const filtersOptions = [];
    this.filters.forEach(
      (value, key) =>
        value.isActive && filtersOptions.push(this.filters.get(key).filterOptionsForRequest)
    );
    return filtersOptions.length ? filtersOptions : null;
  };

  @action
  withLoader = async (cb: () => Promise<void>): Promise<void> => {
    try {
      this.toggleLoading(true);
      await cb();
    } catch (e) {
      throw e;
    } finally {
      this.toggleLoading(false);
    }
  };

  priorityRequestInSameTime = async (cb: () => Promise<void>): Promise<void> => {
    try {
      this.IsPriorityRequestPending = true;
      await cb();
    } catch (e) {
      console.error(e);
      throw e;
    } finally {
      this.IsPriorityRequestPending = false;
    }
  };

  @action
  fetchGridData = async (operation: PaginationActions = PaginationActions.None): Promise<void> =>
    this.priorityRequestInSameTime(async () => {
      try {
        const paginationOptions = this.pagination.calcPaginationOptionsOnOperation(operation);
        const filtersOptions = this.getFiltersOptions();
        const response = await apiService.getTasksGrid(paginationOptions, filtersOptions);
        this.setGridData(response.tasks);
        this.setTotal(response.totalTasks);
        this.setMyLastTask(response.myLastTask);
        this.pagination.setPagination(paginationOptions);
      } catch (e) {
        console.error(e);
      }
    });

  fetchRerunTask = async (taskId: number): Promise<void> => {
    try {
      const res = await apiService.fetchRerunTask(taskId);
      if (!res) {
        throw new Error();
      }
      await this.fetchGridData();
    } catch (e) {
      console.error(e);
    }
  };

  fetchCancelRerun = async (taskId: number): Promise<void> => {
    try {
      const res = await apiService.fetchCancelTask(taskId);
      if (!res) {
        throw new Error();
      }
      await this.fetchGridData();
    } catch (e) {
      console.error(e);
    }
  };

  setSchedule = async (
    taskId: number,
    isEnabled: boolean,
    cronExpression: string,
    repeatType = "on"
  ): Promise<void> =>
    this.priorityRequestInSameTime(async () => {
      try {
        await apiService.fetchSchedule(taskId, isEnabled, cronExpression, repeatType);
        await this.fetchGridData();
      } catch (e) {
        console.error(e);
      }
    });
}
export const TaskStoreContext = createContext(new TaskStore());
