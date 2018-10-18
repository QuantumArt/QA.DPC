import { observable, action } from "mobx";
import { EntityObject } from "Models/EditorDataModels";

export interface PublicationTimestamp {
  Live?: Date;
  Stage?: Date;
}

export class PublicationContext {
  private _timestampsById = observable.map<number, PublicationTimestamp>();
  private _maxPublicationTime: Date = null;

  public get maxPublicationTime() {
    return this._maxPublicationTime;
  }

  public updateMaxPublicationTime(publicationTime: Date) {
    if (this._maxPublicationTime < publicationTime) {
      this._maxPublicationTime = publicationTime;
    }
  }

  public getLiveTime(product: EntityObject) {
    const serverId = product._ServerId;
    const timestamp = serverId && this._timestampsById.get(serverId);
    return timestamp && timestamp.Live;
  }

  public getStageTime(product: EntityObject) {
    const serverId = product._ServerId;
    const timestamp = serverId && this._timestampsById.get(serverId);
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
      this.updateMaxPublicationTime(timestamp.Live);
      this.updateMaxPublicationTime(timestamp.Stage);
    });
  }
}
