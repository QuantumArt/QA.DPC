export default interface TaskModel {
  displayName: string;
  id: number;
  createdTime: Date;
  userName: string;
  stateId: number;
  state: string;
  progress: number;
  lastStatusChangeTime: Date;
  message: string;
}
