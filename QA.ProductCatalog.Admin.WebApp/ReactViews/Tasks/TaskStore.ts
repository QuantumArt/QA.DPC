import { action, computed, observable, onBecomeObserved, runInAction } from "mobx";
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
import { sendNotification, SendNotificationOptions } from "@quantumart/qp8backendapi-interaction";

export class Pagination {
  constructor(onChangePage: (operation: PaginationActions) => void) {
    this.setPagination(INIT_PAGINATION_OPTIONS);
    this.changePage = onChangePage;
  }
  @observable private skip: number;
  @observable private take: number;
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
      this.take = props.take;
      this.showOnlyMine = props.showOnlyMine;
    });
  };

  calcPaginationOptionsOnOperation = (
    operation: PaginationActions,
    total: number = null
  ): PaginationOptions => {
    const paginationOptions = this.getPaginationOptions;
    let skip = paginationOptions.skip;
    switch (operation) {
      case PaginationActions.FirstPage:
        skip = 0;
        break;
      case PaginationActions.IncrementPage:
        skip = paginationOptions.skip + INIT_PAGINATION_OPTIONS.take;
        break;
      case PaginationActions.DecrementPage:
        skip = paginationOptions.skip - INIT_PAGINATION_OPTIONS.take;
        break;
      case PaginationActions.LastPage: {
        if (total !== null) {
          skip = total - INIT_PAGINATION_OPTIONS.take;
        }
        break;
      }
      default:
      // use already defined skip, equivalent for PaginationActions.None
    }
    return { ...this.getPaginationOptions, skip };
  };
}

export interface ValidationResult {
  hasError: boolean;
  message: string;
}

type Operator = "eq" | "gt" | "gte" | "lt" | "lte";

export class Filter {
  constructor(
    initialValue: string,
    onChangeFilter: () => Promise<void>,
    field: string,
    getMappedValue?: () => boolean,
    validationFunc?: (val: string) => ValidationResult,
    operator: Operator = "eq"
  ) {
    this.value = initialValue;
    this.onChangeFilter = onChangeFilter;
    this.field = field;
    this.validationFunc = validationFunc;
    this.getFilteredValue = getMappedValue;
    this.operator = operator;
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
      value: this.getFilteredValue ? this.getFilteredValue() : this.value
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
  toggleActive = async (val: boolean) => {
    if (!this.validationResult.hasError) {
      this.isActive = val;
      if (this.onChangeFilter) {
        await this.onChangeFilter();
      }
    }
  };

  private readonly field: string;
  private readonly operator: Operator;
  private readonly getFilteredValue: () => boolean;
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
            return { message: l("nameFilterValidation"), hasError: true };
          }
          return { message: "", hasError: false };
        }
      )
    ],
    [TaskGridFilterType.DateFilterFrom, new Filter("", null, "CreatedTime", null, null, "gte")],
    [TaskGridFilterType.DateFilterTo, new Filter("", null, "CreatedTime", null, null, "lte")]
  ]);

  init = async () => {
    await this.withLoader(() => this.fetchGridData());
    setTimeout(this.cyclicFetchGrid, FETCH_TIMEOUT);
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
  toggleLoading = (val?: boolean): void => {
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
        const filtersOptions = this.getFiltersOptions();
        const paginationOptions = this.pagination.calcPaginationOptionsOnOperation(
          operation,
          this.total
        );
        // const paginationOptions =
        //   filtersOptions === null
        //     ? this.pagination.calcPaginationOptionsOnOperation(operation, this.total)
        //     : this.pagination.calcPaginationOptionsOnOperation(PaginationActions.FirstPage);
        const response = await apiService.getTasksGrid(paginationOptions, filtersOptions);
        this.setGridData(response.tasks);
        this.setTotal(response.totalTasks);
        this.setMyLastTask(response.myLastTask);
        this.pagination.setPagination(paginationOptions);
      } catch (e) {
        console.error(e);
      }
    });

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

  deleteSchedule = async () => {
    console.log("delete");
  };
}

export const TaskStoreContext = createContext(new TaskStore());
