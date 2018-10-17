import { observable, action } from "mobx";

export interface PublicationTimestamp {
  Live?: Date;
  Stage?: Date;
}

export class PublicationContext {
  private _timestampsById = observable.map<number, PublicationTimestamp>();

  public maxTimestamp: Date = null;

  public getLiveTime(serverId: number) {
    const timestamp = this._timestampsById.get(serverId);
    return timestamp && timestamp.Live;
  }

  public getStageTime(serverId: number) {
    const timestamp = this._timestampsById.get(serverId);
    return timestamp && timestamp.Stage;
  }

  @action
  public updateTimestamps(timestampsById: { [key: number]: PublicationTimestamp }) {
    Object.entries(timestampsById).forEach(([serverId, timestamp]) => {
      const oldTimestamp = this._timestampsById.get(Number(serverId));
      if (oldTimestamp) {
        if (oldTimestamp.Live < timestamp.Live) {
          oldTimestamp.Live = timestamp.Live;
        }
        if (oldTimestamp.Stage < timestamp.Live) {
          oldTimestamp.Stage = timestamp.Stage;
        }
      } else {
        this._timestampsById.set(Number(serverId), observable(timestamp));
      }
      if (this.maxTimestamp < timestamp.Live) {
        this.maxTimestamp = timestamp.Live;
      }
      if (this.maxTimestamp < timestamp.Live) {
        this.maxTimestamp = timestamp.Stage;
      }
    });
  }
}
