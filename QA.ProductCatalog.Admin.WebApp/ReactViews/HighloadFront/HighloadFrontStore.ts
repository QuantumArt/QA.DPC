import { observable, action } from "mobx";
import moment from "moment";
import { TaskItem, TaskMessage } from "Shared/Types";
import { TaskState } from "Shared/Enums";
import { isString } from "lodash";
import { rootUrl } from "ProductEditor/Utils/Common";
import { SendNotificationOptions, sendNotification } from "@quantumart/qp8backendapi-interaction";

export default class HighloadFrontStore {
  constructor() {
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
        const yesterday = moment().subtract(1, "days");
        if (record.isValid() && record.isBefore(yesterday)) {
          localStorage.removeItem(item);
        }
      }
    }
  };

  sendNotification = (t: TaskItem, m: string, isSuccess: boolean = true) => {
    const { imgPath } = window.highloadFront.notify;
    if (!localStorage.getItem(t.TaskStart)) {
      const options: SendNotificationOptions = {
        title: `Channel: ${t.ChannelState}`,
        icon: `${window.location.origin}${imgPath}${isSuccess ? "Done48.png" : "Failed48.png"}`,
        body: m
      };
      sendNotification(options, window.name, window.top);
      localStorage.setItem(t.TaskStart, m);
    }
  };

  fetchTasks = async (): Promise<void> => {
    const {
      highloadFront: { CustomerCode }
    } = window;
    try {
      const response = await fetch(
        `${rootUrl}/HighloadFront/GetSettings?customerCode=${CustomerCode}&url=api/sync/settings`
      );
      if (response.ok) {
        const res: TaskItem[] = await response.json();
        const tasks = res.map(t => {
          try {
            const taskMessage = (t.TaskMessage as unknown) as string;
            t.TaskMessage = JSON.parse(taskMessage);
          } catch (e) {}
          if (t.TaskState === TaskState.Done) {
            const messages = t?.TaskMessage as TaskMessage;
            messages.Messages.forEach(m => {
              this.sendNotification(t, m.Message);
            });
          } else if (t.TaskState === TaskState.Failed) {
            if (isString(t.TaskMessage)) {
              this.sendNotification(t, `ERROR: ${t.TaskMessage}`, false);
            } else {
              t.TaskMessage.Messages.forEach(m => {
                this.sendNotification(t, `ERROR: ${m.Message}`, false);
              });
            }
          }
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
    const {
      highloadFront: { CustomerCode }
    } = window;
    try {
      await fetch(
        `${rootUrl}/HighloadFront/IndexChanel?customerCode=${CustomerCode}&url=api/sync/${task.ChannelLanguage}/${task.ChannelState}/reset`,
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
