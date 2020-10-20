import { createContext } from "react";
import { action, computed, observable, runInAction } from "mobx";
import { apiService } from "Notification/ApiServices";
import { CycleDataFetch } from "Shared/Utils";
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
      this.setData,
      apiService.getModel.bind(apiService),
      5000,
      15000
    );
  }
  private cycleFetch;
  @observable.ref private systemSettings: ISystemSettings;
  @observable.ref private channels: IChannel[] = [];
  @observable.ref private generalSettings: IGeneralSettings;

  initRequests = async (): Promise<void> => {
    try {
      await this.cycleFetch.initCyclingFetch();
    } catch (e) {
      console.error(e, "abvab");
    }
  };

  breakRequests = (): void => {
    this.cycleFetch.breakCycling();
  };

  @action
  setData = (data: IChannelsResponse) => {
    console.log(data);
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
      this.channels = data;
    }
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
  get getGeneralSettings(): IGeneralSettings {
    return this.generalSettings;
  }
  @computed
  get getSystemSettings(): ISystemSettings {
    return this.systemSettings;
  }
}
export const NotificationStoreContext = createContext(new NotificationStore());
