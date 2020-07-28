import { IGridTask } from "Tasks/Api-services/Api-interfaces/Grid-response";
import { Task } from "Tasks/Api-services/DataContracts/Task";

export const mapTask = (x: IGridTask): Task => x;
