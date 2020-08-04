import { GridResponse } from "./DataContracts/GridResponse";
import { IGridResponse } from "./ApiInterfaces/GridResponse";
import { mapGridResponse } from "./Mappers/MapGridResponse";
import qs from "qs";
import { FilterOptions, PaginationOptions } from "Tasks/ApiServices/DataContracts";
import BaseApiService from "Shared/BaseApiService";

class ApiService extends BaseApiService {
  /**
   * GET /​Task/TasksData
   *
   * @param paginationOpts пагинация
   * @param filtersOpts фильтры
   */
  public async getTasksGrid(
    paginationOpts: PaginationOptions,
    filtersOpts: FilterOptions[]
  ): Promise<GridResponse> {
    const filterString = filtersOpts
      ? "&filterJson=" + encodeURIComponent(JSON.stringify(filtersOpts))
      : "";
    const queryStr: string = qs.stringify(paginationOpts) + filterString;
    const response = await fetch(`${this.rootUrl}/Task/TasksData?${queryStr}`);

    return await this.mapResponse<IGridResponse, GridResponse>(response, mapGridResponse);
  }

  /**
   * POST /​Task/Rerun
   *
   * @param taskId id задачи
   */
  fetchRerunTask = async (taskId: number): Promise<void> => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${this.rootUrl}/Task/Rerun?${queryStr}`;
    await fetch(requestUrl, { method: "POST" });
  };

  /**
   * POST /​Task/SaveSchedule
   *
   * @param taskId id задачи
   * @param cronExpression
   * @param repeatType on/
   * @param isEnabled boolean | [boolean]
   *
   */
  fetchSchedule = async (
    taskId: number,
    isEnabled: boolean,
    cronExpression: string,
    repeatType = "on"
  ): Promise<void> => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${this.rootUrl}/Task/SaveSchedule?${queryStr}`;
    const response = await fetch(requestUrl, {
      method: "POST",
      body: JSON.stringify({
        Enabled: isEnabled === true ? [true, false] : false,
        TaskId: taskId,
        CronExpression: cronExpression,
        repeatType
      }),
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
      }
    });
    console.log(response);
  };
}

export const apiService = new ApiService();
