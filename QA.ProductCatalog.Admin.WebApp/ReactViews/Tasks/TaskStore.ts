import { action, computed, observable, runInAction } from "mobx";
import { createContext } from "react";
import { PaginationActions, ScheduleFilterValues, TaskGridFilterType } from "Shared/Enums";
import { apiService } from "Tasks/Api-services";
import { FilterOptions, PaginationOptions, Task } from "Tasks/Api-services/DataContracts";

export class Pagination {
  constructor(onChangePage: (operation: PaginationActions) => void) {
    this.setPagination(this.initPaginationOptions);
    this.changePage = onChangePage;
  }
  @observable private skip: number = 0;
  private readonly take: number = 10;
  @observable private showOnlyMine: boolean = true;
  readonly changePage: (operation: PaginationActions) => void;
  readonly initPaginationOptions: PaginationOptions = {
    skip: 0,
    take: 10,
    showOnlyMine: true
  };

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
          ? (paginationOptions.skip += this.initPaginationOptions.take)
          : (paginationOptions.skip -= this.initPaginationOptions.take)
    });
  };
}
declare type filterValueType = string;

export class Filter {
  constructor(onChangeFilter: () => Promise<void>, field: string, getMappedValue?: () => boolean) {
    this.onChangeFilter = onChangeFilter;
    this.field = field;
    this.getMappedValue = getMappedValue;
  }
  @observable isActive: boolean = false;
  private field: string;
  private operator: string = "eq";
  value: filterValueType;

  private getMappedValue: () => boolean;
  onChangeFilter: () => Promise<void>;
  setValue = (value: filterValueType) => {
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
  readonly FETCH_TIMEOUT = 5000;
  @observable private gridData: Task[] = [];
  @observable private myLastTask: Task;
  @observable private total: number = 0;
  @observable isLoading: boolean = true;
  @observable.ref pagination: Pagination = new Pagination((operation: PaginationActions) => {
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
    setTimeout(this.cyclicFetchGrid, this.FETCH_TIMEOUT);
  };

  cyclicFetchGrid = async (): Promise<void> => {
    try {
      const paginationOptions = this.pagination.calcPaginationOptionsOnOperation(
        PaginationActions.None
      );
      const filtersOptions = this.getFiltersOptions();

      const response = await apiService.getTasksGrid(paginationOptions, filtersOptions);
      this.setGridData(response.tasks);
      this.setTotal(response.totalTasks);
    } catch (e) {
      console.error(e, "err on cyclic fetch");
    } finally {
      setTimeout(this.cyclicFetchGrid, this.FETCH_TIMEOUT);
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

  fetchGridData = async (operation: PaginationActions = PaginationActions.None): Promise<void> => {
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
  };

  fetchRerunTask = async (taskId: number): Promise<void> => {
    try {
      await apiService.fetchRerunTask(taskId);
    } catch (e) {
      console.error(e);
    }
  };

  setSchedule = async (
    taskId: number,
    isEnabled: boolean,
    cronExpression: string,
    repeatType = "on"
  ): Promise<void> => {
    try {
      await apiService.fetchSchedule(taskId, isEnabled, cronExpression, repeatType);
    } catch (e) {
      console.error(e);
    }
  };
}
export const TaskStoreContext = createContext(new TaskStore());
