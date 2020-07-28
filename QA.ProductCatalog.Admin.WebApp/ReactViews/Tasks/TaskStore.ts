import { action, computed, observable, runInAction } from "mobx";
import { createContext } from "react";
import { PaginationActions, TaskGridFilterType } from "Shared/Enums";
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

export class Filter {
  constructor(onChangeFilter: () => Promise<void>, field: string) {
    this.onChangeFilter = onChangeFilter;
    this.field = field;
  }
  @observable isActive: boolean = false;
  field: string;
  operator: string = "eq";
  value: string | boolean;
  onChangeFilter: () => Promise<void>;
  setValue = (value: string | boolean) => {
    this.value = value;
  };

  get filterOptions(): FilterOptions {
    return {
      field: this.field,
      operator: this.operator,
      value: this.value
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
  @observable total: number = 0;
  @observable isLoading: boolean = true;
  @observable.ref pagination: Pagination = new Pagination((operation: PaginationActions) => {
    this.withLoader(() => this.fetchGridData(operation));
  });

  @observable filters: Map<TaskGridFilterType, Filter> = new Map([
    [
      TaskGridFilterType.StatusFilter,
      new Filter(() => this.withLoader(() => this.fetchGridData()), "StateId")
    ],
    [
      TaskGridFilterType.ScheduleFilter,
      new Filter(() => this.withLoader(() => this.fetchGridData()), "HasSchedule")
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
    this.filters.forEach((value, key) => {
      if (value.isActive) {
        const options = this.filters.get(key).filterOptions;
        const isBooleanString = (val: string | boolean): boolean =>
          val === "true" || val === "false";

        if (isBooleanString(options.value)) {
          options.value = Boolean(value);
        }
        filtersOptions.push(options);
      }
    });
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
      this.pagination.setPagination(paginationOptions);
    } catch (e) {
      console.error(e);
    }
  };

  fetchRerunTask = async (taskId: number) => {
    try {
      await apiService.fetchRerunTask(taskId);
    } catch (e) {
      console.error(e);
    }
  };
}
export const TaskStoreContext = createContext(new TaskStore());
