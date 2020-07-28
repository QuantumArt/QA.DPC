import { GridResponse } from "Tasks/Api-services/DataContracts/GridResponse";
import { IGridResponse } from "Tasks/Api-services/Api-interfaces/Grid-response";
import { mapGridResponse } from "Tasks/Api-services/Mappers/Map-grid-response";
import qs from "qs";
import { FilterOptions, PaginationOptions } from "Tasks/Api-services/DataContracts";

const mapResponse = async <TIn, TOut>(
  response: Response,
  mapper: (resp: TIn) => TOut
): Promise<TOut> => {
  const data: TIn = await tryGetResponse(response);
  return mapper(data);
};

const tryGetResponse = async <TOut>(response: Response): Promise<TOut> => {
  const { status } = response;
  if (status !== 200) {
    throw response;
  }
  return await response.json();
};

//copied from common.ts ClientApp
const urlFromHead = document.head.getAttribute("root-url") || "";
const rootUrl = urlFromHead.endsWith("/") ? urlFromHead.slice(0, -1) : urlFromHead;

class ApiService {
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
    const response = await fetch(`${rootUrl}/Task/TasksData?${queryStr}`);

    return mapResponse<IGridResponse, GridResponse>(response, mapGridResponse);
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
    const requestUrl = `${rootUrl}/Task/Rerun?${queryStr}`;
    await fetch(requestUrl, { method: "POST" });
  };
}

export const apiService = new ApiService();
