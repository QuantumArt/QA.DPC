import { Task } from "Tasks/Api-services/DataContracts/Task";

export class GridResponse {
  tasks: Task[];
  totalTasks: number;
  myLastTask?: Task;
}
