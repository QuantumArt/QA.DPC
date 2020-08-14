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
    // const queryStr: string = qs.stringify({
    //   taskId
    // });
    const formData = new FormData();
    formData.append("Enabled", isEnabled === true ? "[true, false]" : "false");
    formData.append("CronExpression", cronExpression);
    formData.append("repeatType", repeatType);
    formData.append("TaskId", String(taskId));

    const requestUrl = `${this.rootUrl}/Task/SaveSchedule`;
    const response = await fetch(requestUrl, {
      // const response = await fetch('https://qp8.dev.qsupport.ru/Dpc.Admin/Task/SaveSchedule', {
      method: "POST",
      body: formData
      // headers: {
      //   "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
      //   // "Content-Type": "text/plain; charset=utf-8"
      //   // "Content-Type": "multipart/form-data ; charset=utf-8"
      //   "Content-Disposition": "form-data"
      // }
    });
    console.log(response);
  };
}

export const apiService = new ApiService();
