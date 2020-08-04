import { IGridTask } from "../ApiInterfaces/GridResponse";
import { Task } from "../DataContracts/Task";

export const mapTask = (x: IGridTask): Task => x;
