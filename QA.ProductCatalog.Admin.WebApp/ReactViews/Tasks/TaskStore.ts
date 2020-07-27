import { action, computed, observable, runInAction } from "mobx";
import { createContext } from "react";
import qs from "qs";
import { PaginationActions, TaskGridFilterType } from "Shared/Enums";

interface IGridResponse {
  data: {
    tasks: IGridTask[];
    totalTasks: number;
  };
}

interface IGridTask {
  CreatedTime: string;
  DisplayName: string;
  HasSchedule: boolean;
  IconName: string;
  Id: number;
  IsCancellationRequested: boolean;
  LastStatusChangeTime: string;
  Message: string;
  Name: string;
  Progress: number;
  ScheduleCronExpression: any;
  ScheduledFromTaskId: any;
  State: string;
  StateId: number;
  UserName: string;
}

interface IPaginationOptions {
  skip: number;
  take: number;
  showOnlyMine: boolean;
}

interface IFilterOptions {
  field: string;
  operator: string;
  value: string | boolean;
}

//copied from common.ts ClientApp
const urlFromHead = document.head.getAttribute("root-url") || "";
const rootUrl = urlFromHead.endsWith("/") ? urlFromHead.slice(0, -1) : urlFromHead;

export class Pagination {
  constructor(onChangePage: (operation: PaginationActions) => void) {
    this.setPagination(this.initPaginationOptions);
    this.changePage = onChangePage;
  }
  @observable private skip: number = 0;
  private readonly take: number = 10;
  @observable private showOnlyMine: boolean = true;
  changePage: (operation: PaginationActions) => void;
  readonly initPaginationOptions: IPaginationOptions = {
    skip: 0,
    take: 10,
    showOnlyMine: true
  };

  @computed
  get getPaginationOptions(): IPaginationOptions {
    return {
      skip: this.skip,
      take: this.take,
      showOnlyMine: this.showOnlyMine
    };
  }

  setPagination = (props: IPaginationOptions) => {
    runInAction(() => {
      this.skip = props.skip;
      this.showOnlyMine = props.showOnlyMine;
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

  get filterOptions(): IFilterOptions {
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
  @observable private gridData: IGridTask[] = [];
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
      const paginationOptions = this.getCalculatedPagination(PaginationActions.None);
      const filtersOptions = this.getFiltersOptions();
      const response = await fetch(
        this.createGridDataRequestUrl(paginationOptions, filtersOptions)
      );
      if (response.ok) {
        const { data }: IGridResponse = await response.json();
        this.setGridData(data.tasks);
        this.setTotal(data.totalTasks);
      }
    } catch (e) {
      console.error(e, "err on cyclic fetch");
    } finally {
      setTimeout(this.cyclicFetchGrid, this.FETCH_TIMEOUT);
    }
  };

  @action
  setGridData = (data: IGridTask[]) => {
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
  get getGridData(): IGridTask[] {
    return this.gridData;
  }

  //TODO переписать в мапперы
  getCalculatedPagination = (operation: PaginationActions): IPaginationOptions => {
    const paginationOptions = this.pagination.getPaginationOptions;
    if (operation === PaginationActions.None) return this.pagination.getPaginationOptions;
    return Object.assign(this.pagination, {
      skip:
        operation === PaginationActions.IncrementPage
          ? (paginationOptions.skip += this.pagination.initPaginationOptions.take)
          : (paginationOptions.skip -= this.pagination.initPaginationOptions.take)
    });
  };
  getFiltersOptions = (): IFilterOptions[] => {
    const filtersOptions = [];
    this.filters.forEach((value, key) => {
      if (value.isActive) {
        const options = this.filters.get(key).filterOptions;
        if (options.value === "true" || options.value === "false") {
          options.value = Boolean(value);
        }
        filtersOptions.push(options);
      }
    });
    return filtersOptions.length ? filtersOptions : null;
  };

  createGridDataRequestUrl = (
    paginationOpts: IPaginationOptions,
    filtersOpts: IFilterOptions[]
  ): string => {
    const filterString = filtersOpts
      ? "&filterJson=" + encodeURIComponent(JSON.stringify(filtersOpts))
      : "";
    const queryStr: string = qs.stringify(paginationOpts) + filterString;
    return `${rootUrl}/Task/TasksData?${queryStr}`;
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
      const paginationOptions = this.getCalculatedPagination(operation);
      const filtersOptions = this.getFiltersOptions();
      const response = await fetch(
        this.createGridDataRequestUrl(paginationOptions, filtersOptions)
      );

      if (response.ok) {
        const { data }: IGridResponse = await response.json();
        this.setGridData(data.tasks);
        this.setTotal(data.totalTasks);
        this.pagination.setPagination(paginationOptions);
      }
    } catch (e) {
      console.error(e);
    }
  };

  rerun = async (taskId: number) => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${rootUrl}/Task/Rerun?${queryStr}`;

    fetch(requestUrl, { method: "POST" })
      .then(response => {
        return response.json();
      })
      .then(response => {
        console.log(response);
      })
      .catch(e => {
        console.error(e.text());
      });
  };
}
export const TaskStoreContext = createContext(new TaskStore());
