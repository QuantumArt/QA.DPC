import { observable, action } from "mobx";
import moment from "moment";

import { Task } from "../Shared/Types";
import { TaskState } from "../Shared/Enums";

export default class HighloadFrontStore {
  @observable private timerId: NodeJS.Timeout;
  @observable tasks: Task[] = [];

  @action setTasks = (value: Task[]): void => {
    this.tasks = value;
  };

  @action setTimerId = (value: NodeJS.Timeout): void => {
    this.timerId = value;
  };

  fetchTasks = async (): Promise<void> => {
    const { customerCode } = window.highloadFront;
    const response = await fetch(
      `/HighloadFront/GetSettings?customerCode=${customerCode}&url=api/sync/settings`
    );

    if (response.ok) {
      const json = await response.json();
      const tasks = json as Task[];
      this.setTasks(tasks);
    }
  };

  cyclicFetch = (): void => {
    this.fetchTasks();
    let timerId = setTimeout(this.cyclicFetch, 5000);
    this.setTimerId(timerId);
  };

  isIndexingAvailable = (taskState: TaskState): boolean => {
    return taskState != TaskState.New && taskState != TaskState.Running;
  };

  onIndexChannel = async (task: Task): Promise<void> => {
    const { customerCode } = window.highloadFront;
    await fetch(
      `/HighloadFront/IndexChanel?customerCode=${customerCode}&url=api/sync/${
        task.ChannelLanguage
      }/${task.ChannelState}/reset`,
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
    var timePassed = start || end;
    if (timePassed) {
      return moment(timePassed).fromNow();
    }

    return null;
  };
}
