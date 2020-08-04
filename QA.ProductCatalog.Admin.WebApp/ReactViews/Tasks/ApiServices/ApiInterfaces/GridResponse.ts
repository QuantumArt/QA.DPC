export interface IGridResponse {
  data: {
    tasks: IGridTask[];
    totalTasks: number;
    myLastTask?: IGridTask;
  };
}

export interface IGridTask {
  CreatedTime: string;
  DisplayName: string;
  HasSchedule: boolean;
  IconName: string;
  Id: number;
  IsCancellationRequested: boolean;
  LastStatusChangeTime: string;
  Message: string;
  Name: string;
  Progress: number;
  ScheduleCronExpression: any;
  ScheduledFromTaskId: any;
  State: string;
  StateId: number;
  UserName: string;
}
