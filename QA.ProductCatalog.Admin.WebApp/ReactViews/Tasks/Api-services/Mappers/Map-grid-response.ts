import { IGridResponse } from "Tasks/Api-services/Api-interfaces/Grid-response";
import { GridResponse } from "Tasks/Api-services/DataContracts";
import { mapTask } from "./Map-task";

export const mapGridResponse = (x: IGridResponse): GridResponse => {
  const gridResponse: GridResponse = new GridResponse();
  gridResponse.totalTasks = x.data.totalTasks;
  gridResponse.myLastTask = mapTask(x.data.myLastTask);
  gridResponse.tasks = x.data.tasks.map(mapTask);
  return gridResponse;
};
