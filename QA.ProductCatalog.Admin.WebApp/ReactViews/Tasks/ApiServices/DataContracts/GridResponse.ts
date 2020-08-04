import { Task } from "Tasks/ApiServices/DataContracts/Task";

export class GridResponse {
  tasks: Task[];
  totalTasks: number;
  myLastTask?: Task;
}
