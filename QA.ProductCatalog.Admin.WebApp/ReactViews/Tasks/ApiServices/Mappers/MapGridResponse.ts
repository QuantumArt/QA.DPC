import { IGridResponse } from "../ApiInterfaces/GridResponse";
import { GridResponse } from "Tasks/ApiServices/DataContracts";
import { mapTask } from "./MapTask";

export const mapGridResponse = (x: IGridResponse): GridResponse => {
  const gridResponse: GridResponse = new GridResponse();
  gridResponse.totalTasks = x.data.totalTasks;
  gridResponse.myLastTask = mapTask(x.data.myLastTask);
  gridResponse.tasks = x.data.tasks.map(mapTask);
  gridResponse.hashCode = x.hashCode;
  return gridResponse;
};
