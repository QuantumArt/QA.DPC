import { TaskState } from "../Enums";

export default interface TaskItem {
  ChannelLanguage: string;
  ChannelState: string;
  ChannelDate: Date;
  IsDefault: boolean;
  TaskId: number;
  TaskState: TaskState;
  TaskProgress: number;
  TaskStart: Date;
  TaskEnd: Date;
  TaskMessage: string;
}
