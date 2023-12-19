import { GridResponse } from "./DataContracts/GridResponse";
import { IGridResponse } from "./ApiInterfaces/GridResponse";
import { mapGridResponse } from "./Mappers/MapGridResponse";
import qs from "qs";
import { FilterOptions, PaginationOptions } from "Tasks/ApiServices/DataContracts";
import BaseApiService from "Shared/BaseApiService";
import { throwOnExpiredSession, throwOnSameHashCode } from "Shared/Utils";

class ApiService extends BaseApiService {
  private lastUpdateHashCode: number;
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
    throwOnExpiredSession(response.status);
    const mappedResponse = await this.mapResponse<IGridResponse, GridResponse>(
      response,
      mapGridResponse
    );
    throwOnSameHashCode(mappedResponse.hashCode, this.lastUpdateHashCode);
    return mappedResponse;
  }

  /**
   * POST /​Task/Rerun
   *
   * @param taskId id задачи
   */
  fetchRerunTask = async (taskId: number): Promise<boolean> => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${this.rootUrl}/Task/Rerun?${queryStr}`;
    const response = await fetch(requestUrl, { method: "POST" });
    throwOnExpiredSession(response.status);
    return await this.mapResponse<boolean, boolean>(response, x => x);
  };
  /**
   * POST /​Task/Rerun
   *
   * @param taskId id задачи
   */
  fetchCancelTask = async (taskId: number): Promise<boolean> => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${this.rootUrl}/Task/Cancel?${queryStr}`;
    const response = await fetch(requestUrl, { method: "POST" });
    throwOnExpiredSession(response.status);
    return await this.mapResponse<boolean, boolean>(response, x => x);
  };

  /**
   * POST /​Task/SaveSchedule
   *
   * @param taskId id задачи
   * @param cronExpression выражение для
   * @param repeatType 'on'
   * @param isEnabled boolean
   * @param allowConcurrentTasks boolean
   *
   */
  fetchSchedule = async (
    taskId: number,
    isEnabled: boolean,
    allowConcurrentTasks: boolean,
    cronExpression: string,
    repeatType = "on"
  ): Promise<void> => {
    const formData = new FormData();
    formData.append("Enabled", isEnabled ? "true" : "false");
    formData.append("AllowConcurrentTasks", allowConcurrentTasks ? "true" : "false");
    formData.append("CronExpression", cronExpression);
    formData.append("repeatType", repeatType);
    formData.append("TaskId", String(taskId));

    const requestUrl = `${this.rootUrl}/Task/SaveSchedule`;
    const response = await fetch(requestUrl, {
      method: "POST",
      body: formData
    });
    throwOnExpiredSession(response.status);
  };
}

export const apiService = new ApiService();
