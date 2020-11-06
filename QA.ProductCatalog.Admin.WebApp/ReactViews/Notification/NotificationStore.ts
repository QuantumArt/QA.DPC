import { createContext } from "react";
import { action, computed, observable } from "mobx";
import { apiService } from "Notification/ApiServices";
import {
  CycleDataFetch,
  getChannelStatusDescription
} from "Shared/Utils";
import {
  IChannel,
  IChannelsResponse,
  IGeneralSettings,
  ISystemSettings
} from "Notification/ApiServices/ApiInterfaces";
import { differenceWith, isEqual, uniq } from "lodash";
import { ChannelNotificationType } from "Shared/Enums";
import { SendNotificationOptions, sendNotification } from "@quantumart/qp8backendapi-interaction";
import { l } from "./Localization";
import { firstLetterUp, firstLetterLow } from "Shared/Utils";

interface IChannelNotify extends IChannel {
  oldCount?: number;
}

export class NotificationStore {
  constructor() {
    this.cycleFetch = new CycleDataFetch<IChannelsResponse>(
      null,
      async (): Promise<IChannelsResponse> => {
        const data = await apiService.getModel();
        if (!this.IsPriorityRequestAlreadyPending) this.setData(data);
        return data;
      },
      5000,
      15000
    );
  }
  private IsPriorityRequestAlreadyPending: boolean;
  private cycleFetch;
  @observable.ref private systemSettings: ISystemSettings;
  @observable.ref private channels: IChannel[];
  @observable.ref private generalSettings: IGeneralSettings;
  @observable private _isActual: boolean = true;
  @observable isLoading: boolean = false;

  initRequests = async (): Promise<void> => {
    try {
      await this.cycleFetch.initCyclingFetch();
    } catch (e) {
      console.error(e);
    }
  };

  breakRequests = (): void => {
    this.cycleFetch.breakCycling();
  };

  priorityRequestInSameTime = async (cb: () => Promise<void>): Promise<void> => {
    try {
      this.IsPriorityRequestAlreadyPending = true;
      await cb();
    } catch (e) {
      console.error(e);
      throw e;
    } finally {
      this.IsPriorityRequestAlreadyPending = false;
    }
  };

  @action
  setData = (data: IChannelsResponse) => {
    this._isActual = data.IsActual;
    this.setSystemSettings({
      NotificationProvider: data.NotificationProvider,
      Started: data.Started
    });
    this.setChannels(data.Channels);
    this.setGeneralSettings(data.CurrentSettings);
  };

  @action
  setChannels = (data: IChannel[]) => {
    const isSameData = differenceWith(this.channels || [], data, isEqual).length === 0;
    if (!this.channels || !isSameData || this.channels.length !== data.length) {
      if (this.channels) {
        this.notificationsSender(data);
      }
      this.channels = data;
    }
  };

  @action
  updateConfiguration = async () => {
    try {
      this.toggleLoading();
      const data = await apiService.updateConfiguration();
      this.setData(data);
      this.toggleLoading();
    } catch (e) {
      console.error(e);
    }
  };

  notificationsSender = (data: IChannel[]): void => {
    if (!data) return;
    const batchedNotification: {
      body?: string;
      getHeader?: () => string;
      channelNames?: string[];
    } = {
      body: "",
      getHeader: function() {
        return `${l("channelsChanged")} ${uniq(this.channelNames).join(", ")}:`;
      },
      channelNames: []
    };
    const channelsToNotify = new Map<ChannelNotificationType, IChannelNotify[]>([
      [ChannelNotificationType.Add, []],
      [ChannelNotificationType.ChangeCount, []],
      [ChannelNotificationType.ChangeStatus, []],
      [ChannelNotificationType.ChangeState, []],
      [ChannelNotificationType.Remove, []]
    ]);

    /**
     * собираем изменившиеся каналы
     */
    if (data.length > this.channels.length && this.channels.length) {
      channelsToNotify
        .get(ChannelNotificationType.Add)
        .push(...data.filter(x => !this.channels.find(y => y.Name == x.Name)));
    }

    if (data.length < this.channels.length) {
      channelsToNotify
        .get(ChannelNotificationType.Remove)
        .push(...this.channels.filter(x => !data.find(y => y.Name === x.Name)));
    }

    data.forEach(channel => {
      const oldChannel = this.channels.find(x => x.Name === channel.Name);
      if (oldChannel && oldChannel?.State !== channel?.State) {
        channelsToNotify.get(ChannelNotificationType.ChangeState).push(channel);
      }
      if (oldChannel && oldChannel?.Count !== channel?.Count) {
        channelsToNotify
          .get(ChannelNotificationType.ChangeCount)
          .push({ ...channel, oldCount: oldChannel.Count });
      }
      if (oldChannel && oldChannel?.LastStatus !== channel?.LastStatus) {
        channelsToNotify.get(ChannelNotificationType.ChangeStatus).push(channel);
      }
    });

    /**
     * Отправляем шаблонные уведомления в зависимости от измененных данных канала
     */
    for (let notificationType of channelsToNotify.keys()) {
      const channels = channelsToNotify.get(notificationType);
      const nOptions: SendNotificationOptions = {
        title: "",
        body: ""
      };

      switch (notificationType) {
        case ChannelNotificationType.Add:
          channels.forEach(channel => {
            nOptions.body = `${l("stateOfChannel")}: ${getChannelStatusDescription(
              channel.State
            )}\n`;
            nOptions.body += `${l("statusOfChannel")}: ${channel.LastStatus}`;
            nOptions.title = `${firstLetterUp(l("new"))} ${firstLetterLow(l("channel"))} ${
              channel.Name
            } ${l("added")}`;
            sendNotification(nOptions, window.name, window.top);
          });
          break;
        case ChannelNotificationType.ChangeStatus:
        case ChannelNotificationType.ChangeCount:
          channels.forEach(channel => {
            batchedNotification.channelNames.push(channel.Name);
            batchedNotification.body +=
              notificationType === ChannelNotificationType.ChangeStatus
                ? `${l("statusOfChannel")} ${channel.Name} ${l("changed")} ${l("to")} ${
                    channel.LastStatus
                  }\n`
                : `${l("queue")} ${channel.Name} ${l("changed")} ${l("to")} ${channel.Count} ${l(
                    "from"
                  )} ${channel.oldCount}\n`;
          });
          break;
        case ChannelNotificationType.ChangeState:
          channels.forEach(channel => {
            nOptions.body = `${firstLetterUp(l("new"))} ${firstLetterLow(
              l("channelState")
            )} ${getChannelStatusDescription(channel.State)}`;
            nOptions.title = `${l("stateOfChannel")} ${channel.Name} ${l("changed")}`;
            sendNotification(nOptions, window.name, window.top);
          });
          break;
        case ChannelNotificationType.Remove:
          channels.forEach(channel => {
            nOptions.body = ``;
            nOptions.title = `${l("channel")} ${channel.Name} ${l("removed")}`;
            sendNotification(nOptions, window.name, window.top);
          });
          break;
      }
    }

    if (batchedNotification.channelNames.length) {
      const options: SendNotificationOptions = {
        title: batchedNotification.getHeader(),
        body: batchedNotification.body
      };
      sendNotification(options, window.name, window.top);
    }
  };

  @action
  toggleLoading = (val?: boolean): void => {
    this.isLoading = val || !this.isLoading;
  };

  @action
  setGeneralSettings = (data: IGeneralSettings) => {
    if (!isEqual(data, this.generalSettings)) {
      this.generalSettings = data;
    }
  };

  @action
  setSystemSettings = (data: ISystemSettings) => {
    if (!isEqual(data, this.systemSettings)) {
      this.systemSettings = data;
    }
  };

  @computed
  get getChannels(): IChannel[] {
    return this.channels || [];
  }
  @computed
  get isActual(): boolean {
    return this._isActual;
  }
  @computed
  get getGeneralSettings(): IGeneralSettings {
    return this.generalSettings;
  }
  @computed
  get getSystemSettings(): ISystemSettings {
    return this.systemSettings;
  }
}
export const NotificationStoreContext = createContext(new NotificationStore());
