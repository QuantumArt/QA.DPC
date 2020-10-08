import { observable, action } from "mobx";
import moment from "moment";
import { TaskItem, TaskMessage } from "Shared/Types";
import { TaskState } from "Shared/Enums";
import { isString } from "lodash";
import { setBrowserNotifications, checkPermissions } from "Shared/Utils";

export default class HighloadFrontStore {

  constructor() {
    checkPermissions();
    this.clearLs();
  }

  @observable private timerId: number;
  @observable tasks: TaskItem[] = [];

  @action setTasks = (value: TaskItem[]): void => {
    this.tasks = value;
  };

  @action setTimerId = (value: number): void => {
    this.timerId = value;
  };

  clearLs = () => {
    for (const item in localStorage) {
      if (localStorage.hasOwnProperty(item) && isString(item)) {
        const record = moment(item);
        const yesterday = moment().subtract(1, 'days');
        if (record.isValid() && record.isBefore(yesterday)) {
          localStorage.removeItem(item);
        }
      }
    }
  }

  sendNoditifaction = (t: TaskItem, m: string) => {
    if (!localStorage.getItem(t.TaskStart)) {
      setBrowserNotifications(() => {
        new Notification(m, {
          body: `Channel: ${t.ChannelState}`
        });
      });
      localStorage.setItem(t.TaskStart, m);
    }
  }

  fetchTasks = async (): Promise<void> => {
    const { highloadFront: { CustomerCode } } = window;
    try {
      const response = await fetch(`/HighloadFront/GetSettings?customerCode=${CustomerCode}&url=api/sync/settings`);
      if (response.ok) {
        const res: TaskItem[] = await response.json();
        const tasks = res.map(t => {
          try {
            const taskMessage = (t.TaskMessage as unknown) as string;
            t.TaskMessage = JSON.parse(taskMessage);
            if (t.TaskState === 3) {
              const messages = t?.TaskMessage as TaskMessage;
              messages.Messages.forEach(m => {
                this.sendNoditifaction(t, m.Message);
              });
            } else if (t.TaskState === 4) {
              if (isString(t.TaskMessage)) {
                this.sendNoditifaction(t, `ERROR: ${t.TaskMessage}`);
              } else {
                t.TaskMessage.Messages.forEach(m => {
                  this.sendNoditifaction(t, `ERROR: ${m.Message}`);
                });
              }
            }
          } catch (e) {}
          return t;
        });
        this.setTasks(tasks);
      }
    } catch (error) {
      console.log(error);
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
    try {
      await fetch(
        `/HighloadFront/IndexChanel?customerCode=${CustomerCode}&url=api/sync/${task.ChannelLanguage}/${task.ChannelState}/reset`,
        { method: "POST" }
      );
      await this.fetchTasks();
    } catch (error) {
      console.log(error);
    }
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
