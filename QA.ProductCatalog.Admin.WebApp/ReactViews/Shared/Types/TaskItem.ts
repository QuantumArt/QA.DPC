import { TaskState } from "../Enums";
import { TaskMessage } from './TaskMessage';

export default interface TaskItem {
  ChannelLanguage: string;
  ChannelState: string;
  ChannelDate: Date;
  IsDefault: boolean;
  TaskId: number;
  TaskState: TaskState;
  TaskProgress: number;
  TaskStart: string; // Date string
  TaskEnd: Date;
  TaskMessage: string | TaskMessage; // Backend returns object as a string, we parse is every time for in-app use
}
