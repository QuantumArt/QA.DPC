import { observable, action } from "mobx";
import moment from "moment";
import { TaskItem } from "Shared/Types";
import { TaskState } from "Shared/Enums";

export default class HighloadFrontStore {
  @observable private timerId: number;
  @observable tasks: TaskItem[] = [];

  @action setTasks = (value: TaskItem[]): void => {
    this.tasks = value;
  };

  @action setTimerId = (value: number): void => {
    this.timerId = value;
  };

  fetchTasks = async (): Promise<void> => {
    const { highloadFront: { CustomerCode } } = window;
    const response = await fetch(
      `/HighloadFront/GetSettings?customerCode=${CustomerCode}&url=api/sync/settings`
    );

    if (response.ok) {
      const json = await response.json();
      const tasks = json as TaskItem[];
      this.setTasks(tasks);
    }
  };

  cyclicFetch = (): void => {
    this.fetchTasks();
    const timerId = window.setTimeout(this.cyclicFetch, 5000);
    this.setTimerId(timerId);
  };

  isIndexingAvailable = (taskState: TaskState): boolean => {
    return taskState != TaskState.New && taskState != TaskState.Running;
  };

  handleIndexChannel = async (task: TaskItem): Promise<void> => {
    const { highloadFront: { CustomerCode } } = window;
    await fetch(
      `/HighloadFront/IndexChanel?customerCode=${CustomerCode}&url=api/sync/${task.ChannelLanguage}/${task.ChannelState}/reset`,
      { method: "POST" }
    );
    await this.fetchTasks();
  };

  clearTimeout = (): void => {
    clearTimeout(this.timerId);
  };

  getFormattedChannelDate = (date: Date): string => {
    if (date) {
      return moment(date).calendar();
    }

    return null;
  };

  getTimePassed = (start, end): string => {
    const timePassed = start || end;
    if (timePassed) {
      return moment(timePassed).fromNow();
    }

    return null;
  };
}
