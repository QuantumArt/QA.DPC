import { action, computed, observable, runInAction } from "mobx";
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
import { setBrowserNotifications } from "Shared/Utils";
import { l } from "Tasks/Localization";

export class Pagination {
  constructor(onChangePage: (operation: PaginationActions) => void) {
    this.setPagination(INIT_PAGINATION_OPTIONS);
    this.changePage = onChangePage;
  }
  @observable private skip: number = 0;
  private readonly take: number = 10;
  @observable private showOnlyMine: boolean = true;
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

export class Filter {
  constructor(onChangeFilter: () => Promise<void>, field: string, getMappedValue?: () => boolean) {
    this.onChangeFilter = onChangeFilter;
    this.field = field;
    this.getMappedValue = getMappedValue;
  }
  @observable isActive: boolean = false;
  private field: string;
  private operator: string = "eq";
  value: string;

  private getMappedValue: () => boolean;
  onChangeFilter: () => Promise<void>;
  setValue = (value: string) => {
    this.value = value;
  };

  get filterOptionsForRequest(): FilterOptions {
    return {
      field: this.field,
      operator: this.operator,
      value: this.getMappedValue ? this.getMappedValue() : this.value
    };
  }

  @action
  toggleActive = (val: boolean) => {
    this.isActive = val || !this.isActive;
    this.onChangeFilter();
  };
}

export class TaskStore {
  private isUpdateRateRequestsPending: boolean = false;
  private alreadyNotifiedTaskIds: { [x: number]: boolean } = {};
  @observable private gridData: Task[] = [];
  @observable private myLastTask: Task;
  @observable private total: number = 0;
  @observable isLoading: boolean = true;
  @observable pagination: Pagination = new Pagination((operation: PaginationActions) => {
    this.withLoader(() => this.fetchGridData(operation));
  });
  filterValueMapper = function() {
    return this.value === ScheduleFilterValues.YES;
  };
  @observable filters: Map<TaskGridFilterType, Filter> = new Map([
    [
      TaskGridFilterType.StatusFilter,
      new Filter(() => this.withLoader(() => this.fetchGridData()), "StateId")
    ],
    [
      TaskGridFilterType.ScheduleFilter,
      new Filter(
        () => this.withLoader(() => this.fetchGridData()),
        "HasSchedule",
        this.filterValueMapper
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
        this.isUpdateRateRequestsPending ||
        paginationOptions.skip !== this.pagination.getPaginationOptions.skip
      ) {
        throw "the request already have sent and new response from cyclic fetch will not accept in grid";
      }
      this.setGridData(response.tasks);
      this.setTotal(response.totalTasks);
      this.setMyLastTask(response.myLastTask);

      if (isNotifyActive) {
        setBrowserNotifications(() =>
          this.tasksNotificationsSender(response.tasks, runningStateId)
        );
      }

      setTimeout(this.cyclicFetchGrid, FETCH_TIMEOUT);
    } catch (e) {
      if (typeof e === "string" && e === SESSION_EXPIRED) {
        return;
      }
      setTimeout(this.cyclicFetchGrid, FETCH_ON_ERROR_TIMEOUT);
    }
  };

  tasksNotificationsSender = (tasks, runningStateId) => {
    const { formRenderedServerTime, img } = window.task.notify;

    const gridRenderServerTime = new Date(formRenderedServerTime);
    const tasksShouldNotify = tasks.filter(task => {
      const lastStatusChangeTime = task.LastStatusChangeTime
        ? new Date(task.LastStatusChangeTime)
        : null;
      return (
        lastStatusChangeTime &&
        task.StateId > runningStateId &&
        !this.alreadyNotifiedTaskIds[task.Id] &&
        lastStatusChangeTime.getTime() > gridRenderServerTime.getTime()
      );
    });

    if (tasksShouldNotify.length) {
      tasksShouldNotify.forEach(task => {
        this.alreadyNotifiedTaskIds[task.Id] = true;
        let body = `${l("state")}: ${task.State}`;
        if (task.Message) body += "\r\n" + task.Message;
        new Notification(`${l("task")}${task.DisplayName} ${l("proceed")}`, {
          body: body,
          icon: `${img}${task.IconName}48.png`
        });
      });
    }
  };

  @action
  setGridData = (data: Task[]) => {
    this.gridData = data;
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

  withIsPendingRequest = async (cb: () => Promise<void>): Promise<void> => {
    try {
      this.isUpdateRateRequestsPending = true;
      await cb();
    } catch (e) {
      console.error(e);
      throw e;
    } finally {
      this.isUpdateRateRequestsPending = false;
    }
  };

  @action
  fetchGridData = async (operation: PaginationActions = PaginationActions.None): Promise<void> =>
    this.withIsPendingRequest(async () => {
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
    this.withIsPendingRequest(async () => {
      try {
        await apiService.fetchSchedule(taskId, isEnabled, cronExpression, repeatType);
        await this.fetchGridData();
      } catch (e) {
        console.error(e);
      }
    });
}
export const TaskStoreContext = createContext(new TaskStore());
