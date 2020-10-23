import { createContext } from "react";
import { action, computed, observable } from "mobx";
import { apiService } from "Notification/ApiServices";
import {
  checkPermissions,
  CycleDataFetch,
  getChannelStatusDescription,
  setBrowserNotifications
} from "Shared/Utils";
import {
  IChannel,
  IChannelsResponse,
  IGeneralSettings,
  ISystemSettings
} from "Notification/ApiServices/ApiInterfaces";
import { differenceWith, isEqual } from "lodash";

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
    checkPermissions();
  }
  private IsPriorityRequestAlreadyPending: boolean;
  private cycleFetch;
  @observable.ref private systemSettings: ISystemSettings;
  @observable.ref private channels: IChannel[] = [];
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
    const isSameData = differenceWith(data, this.channels, isEqual).length === 0;
    if (!isSameData) {
      if (this.channels.length) setBrowserNotifications(() => this.notificationsSender(data));
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
    const channelsWillBeNotify = data.filter(channel => {
      const oldChannel = this.channels.find(x => x.LastId === channel.LastId);
      return oldChannel && oldChannel?.State !== channel?.State;
    });

    if (channelsWillBeNotify.length) {
      channelsWillBeNotify.forEach(channel => {
        let body = `New state: ${getChannelStatusDescription(channel.State)}`;
        body += "\r\n" + `Id product: ${channel.LastId}`;
        new Notification(`state of product id ${channel.LastId} was changed`, {
          body: body
        });
      });
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
    return this.channels;
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
