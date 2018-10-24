import qs from "qs";
import { inject } from "react-ioc";
import { IReactionDisposer, computed, reaction } from "mobx";
import { rootUrl } from "Utils/Common";
import { EditorQueryParams } from "Models/EditorSettingsModels";
import { DataContext } from "Services/DataContext";
import { PublicationContext } from "Services/PublicationContext";
import { isIsoDateString } from "Utils/TypeChecks";

export class PublicationTracker {
  @inject private _queryParams: EditorQueryParams;
  @inject private _dataContext: DataContext;
  @inject private _publicationContext: PublicationContext;

  private _loadedProductIds = new Set<number>();
  private _reactionDisposer: IReactionDisposer;
  private _updateTimer: number;

  @computed
  private get allProductIds() {
    return [...this._dataContext.tables.Product.values()]
      .filter(product => product._ServerId)
      .map(product => Number(product._ServerId));
  }

  public dispose() {
    if (this._reactionDisposer) {
      this._reactionDisposer();
    }
    if (this._updateTimer) {
      window.clearInterval(this._updateTimer);
    }
  }

  public async initStatusTracking() {
    if ("Product" in this._dataContext.tables) {
      await Promise.all([this.loadMaxPublicationTime(), this.loadPublicationTimestamps()]);

      this._reactionDisposer = reaction(() => this.allProductIds, this.loadPublicationTimestamps);

      this._updateTimer = window.setInterval(this.updatePublicationTimestamps, 5000);
    }
  }

  private loadMaxPublicationTime = async () => {
    const response = await fetch(
      `${rootUrl}/ProductEditor/GetMaxPublicationTime?${qs.stringify(this._queryParams)}`,
      {
        credentials: "include"
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }

    const maxPublicationTime = new Date(await response.json());

    this._publicationContext.updateMaxPublicationTime(maxPublicationTime);
  };

  private loadPublicationTimestamps = async () => {
    const newProductIds = this.allProductIds.filter(id => !this._loadedProductIds.has(id));
    if (newProductIds.length === 0) {
      return;
    }

    newProductIds.forEach(id => this._loadedProductIds.add(id));

    const response = await fetch(
      `${rootUrl}/ProductEditor/GetPublicationTimestamps?${qs.stringify(this._queryParams)}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(newProductIds)
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }

    const timestampsById = JSON.parse(await response.text(), (_key, value) => {
      return isIsoDateString(value) ? new Date(value) : value;
    });

    this._publicationContext.updateTimestamps(timestampsById);
  };

  private updatePublicationTimestamps = async () => {
    if (document.hidden) {
      return;
    }
    const maxPublicationTime = this._publicationContext.maxPublicationTime;
    if (!maxPublicationTime) {
      return;
    }

    const response = await fetch(
      `${rootUrl}/ProductEditor/GetPublicationTimestamps?${qs.stringify({
        ...this._queryParams,
        updatedSince: maxPublicationTime
      })}`,
      {
        credentials: "include"
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }

    const timestampsById = JSON.parse(await response.text(), (_key, value) => {
      return isIsoDateString(value) ? new Date(value) : value;
    });

    this._publicationContext.updateTimestamps(timestampsById);
  };
}
