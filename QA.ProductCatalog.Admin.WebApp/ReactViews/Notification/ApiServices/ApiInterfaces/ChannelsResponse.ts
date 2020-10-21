import { ChannelStatuses } from "Shared/Enums";

export interface IChannelsResponse {
  Channels: IChannel[];
  CurrentSettings: IGeneralSettings;
  IsActual: true;
  NotificationProvider: string;
  Started: string; //date
}

export interface IChannel {
  Count: number;
  LastId: number;
  LastPublished: string; //date
  LastQueued: string; //date
  LastStatus: string;
  Name: string;
  State: ChannelStatuses;
}

export interface IGeneralSettings {
  Autopublish: boolean;
  CheckInterval: number;
  ErrorCountBeforeWait: number;
  PackageSize: number;
  TimeOut: number;
  WaitIntervalAfterErrors: number;
}

export type ISystemSettings = Pick<IChannelsResponse, "NotificationProvider" | "Started">;
