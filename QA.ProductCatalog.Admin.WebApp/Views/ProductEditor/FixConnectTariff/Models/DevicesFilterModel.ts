import { observable, action, computed } from "mobx";
import { normailzeSerachString } from "Utils/Common";
import { Product, DeviceOnTariffs } from "../ProductEditorSchema";

export class DevicesFilterModel {
  @observable.ref public filterByTariffRegions = true;
  @observable.ref public filterByMarketingTariff = true;
  @observable.ref public regionsFilter: string[] = [];

  constructor(private _fixTariff: Product) {}

  @computed
  private get fixTariffRegionIds(): { [clientId: number]: true } {
    return this._fixTariff.Regions.reduce((obj, region) => {
      obj[region._ClientId] = true;
      return obj;
    }, {});
  }

  @computed
  private get normalizedRegionsFilter() {
    return this.regionsFilter.map(normailzeSerachString);
  }

  public filterProducts = (product: Product) => {
    const { filterByTariffRegions, normalizedRegionsFilter, fixTariffRegionIds } = this;
    if (
      filterByTariffRegions &&
      !product.getBaseValue("Regions").some(region => fixTariffRegionIds[region._ClientId])
    ) {
      return false;
    }
    if (
      normalizedRegionsFilter.length > 0 &&
      !product
        .getBaseValue("Regions")
        .some(region => normalizedRegionsFilter.includes(normailzeSerachString(region.Title)))
    ) {
      return false;
    }
    return true;
  };

  public filterDevicesOnTariffs = (device: DeviceOnTariffs) => {
    const {
      filterByTariffRegions,
      filterByMarketingTariff,
      normalizedRegionsFilter,
      fixTariffRegionIds
    } = this;
    if (
      filterByMarketingTariff &&
      !device.MarketingTariffs.includes(this._fixTariff.MarketingProduct)
    ) {
      return false;
    }
    if (
      filterByTariffRegions &&
      !device.getBaseValue("Cities").some(region => fixTariffRegionIds[region._ClientId])
    ) {
      return false;
    }
    if (
      normalizedRegionsFilter.length > 0 &&
      !device
        .getBaseValue("Cities")
        .some(region => normalizedRegionsFilter.includes(normailzeSerachString(region.Title)))
    ) {
      return false;
    }
    return true;
  };

  @action
  public toggleFilterByTariffRegions = () => {
    this.filterByTariffRegions = !this.filterByTariffRegions;
  };

  @action
  public toggleFilterByMarketingTariff = () => {
    this.filterByMarketingTariff = !this.filterByMarketingTariff;
  };

  @action
  public setRegionsFilter = (values: string[]) => {
    this.regionsFilter = values;
  };

  @action
  public clearRegionsFilter = () => {
    if (this.regionsFilter.length > 0) {
      this.regionsFilter = [];
    }
  };
}
