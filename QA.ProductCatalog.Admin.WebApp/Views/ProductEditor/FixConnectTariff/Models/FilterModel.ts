import { observable, action, computed } from "mobx";
import { HighlightMode } from "ProductEditor/Components/FieldEditors/FieldEditors";
import { Product, DeviceOnTariffs, FixConnectAction, MarketingProduct } from "../TypeScriptSchema";

export class FilterModel {
  @observable.ref public filterByTariffRegions = true;
  @observable.ref public filterByMarketingTariff = true;
  @observable.ref public selectedRegionIds: number[] = [];

  constructor(private fixTariff: Product) {}

  @action
  public toggleFilterByTariffRegions = () => {
    this.filterByTariffRegions = !this.filterByTariffRegions;
  };

  @action
  public toggleFilterByMarketingTariff = () => {
    this.filterByMarketingTariff = !this.filterByMarketingTariff;
  };

  @action
  public setSelectedRegionIds = (values: number[]) => {
    this.selectedRegionIds = values;
  };

  @computed
  private get fixTariffHasRegionId(): { [clientId: number]: true } {
    return this.fixTariff.Regions.reduce((obj, region) => {
      obj[region._ClientId] = true;
      return obj;
    }, {});
  }

  @computed
  private get hasSelectedRegionId(): { [clientId: number]: true } {
    return this.selectedRegionIds.reduce((obj, clientId) => {
      obj[clientId] = true;
      return obj;
    }, {});
  }

  public filterProducts = (product: Product) => {
    const {
      filterByTariffRegions,
      fixTariffHasRegionId,
      selectedRegionIds,
      hasSelectedRegionId
    } = this;
    if (
      filterByTariffRegions &&
      !product.getBaseValue("Regions").some(region => fixTariffHasRegionId[region._ClientId])
    ) {
      return false;
    }
    if (
      selectedRegionIds.length > 0 &&
      !product.getBaseValue("Regions").some(region => hasSelectedRegionId[region._ClientId])
    ) {
      return false;
    }
    return true;
  };

  public filterActions = (action: FixConnectAction) => {
    const {
      filterByTariffRegions,
      fixTariffHasRegionId,
      selectedRegionIds,
      hasSelectedRegionId
    } = this;
    if (
      filterByTariffRegions &&
      !action.Parent.getBaseValue("Regions").some(region => fixTariffHasRegionId[region._ClientId])
    ) {
      return false;
    }
    if (
      selectedRegionIds.length > 0 &&
      !action.Parent.getBaseValue("Regions").some(region => hasSelectedRegionId[region._ClientId])
    ) {
      return false;
    }
    return true;
  };

  public filterDeviceActions = (markDevice: MarketingProduct) => (action: FixConnectAction) => {
    if (
      !action.Parent.getBaseValue("ActionMarketingDevices").some(
        actionDevice => actionDevice.getBaseValue("MarketingDevice") === markDevice
      )
    ) {
      return false;
    }
    return this.filterActions(action);
  };

  public filterDevicesOnTariffs = (device: DeviceOnTariffs) => {
    const {
      filterByMarketingTariff,
      filterByTariffRegions,
      fixTariffHasRegionId,
      selectedRegionIds,
      hasSelectedRegionId
    } = this;
    if (
      filterByMarketingTariff &&
      !device.getBaseValue("MarketingTariffs").includes(this.fixTariff.MarketingProduct)
    ) {
      return false;
    }
    if (
      filterByTariffRegions &&
      !device.getBaseValue("Cities").some(region => fixTariffHasRegionId[region._ClientId])
    ) {
      return false;
    }
    if (
      selectedRegionIds.length > 0 &&
      !device.getBaseValue("Cities").some(region => hasSelectedRegionId[region._ClientId])
    ) {
      return false;
    }
    return true;
  };

  public highlightProduct = (product: Product) => {
    const { filterByTariffRegions, fixTariffHasRegionId } = this;
    if (filterByTariffRegions) {
      return HighlightMode.None;
    }
    if (product.getBaseValue("Regions").some(region => fixTariffHasRegionId[region._ClientId])) {
      return HighlightMode.Highlight;
    }
    return HighlightMode.Shade;
  };

  public highlightAction = (action: FixConnectAction) => {
    const { filterByTariffRegions, fixTariffHasRegionId } = this;
    if (!action.getBaseValue("MarketingOffers").includes(this.fixTariff.MarketingProduct)) {
      return HighlightMode.Shade;
    }
    if (filterByTariffRegions) {
      return HighlightMode.None;
    }
    if (
      action.Parent.getBaseValue("Regions").some(region => fixTariffHasRegionId[region._ClientId])
    ) {
      return HighlightMode.Highlight;
    }
    return HighlightMode.Shade;
  };

  public highlightDeviceOnTariffs = (device: DeviceOnTariffs) => {
    const { filterByMarketingTariff, filterByTariffRegions, fixTariffHasRegionId } = this;
    if (filterByMarketingTariff && filterByTariffRegions) {
      return HighlightMode.None;
    }
    if (
      !filterByMarketingTariff &&
      !device.getBaseValue("MarketingTariffs").includes(this.fixTariff.MarketingProduct)
    ) {
      return HighlightMode.Shade;
    }
    if (
      !filterByTariffRegions &&
      !device.getBaseValue("Cities").some(region => fixTariffHasRegionId[region._ClientId])
    ) {
      return HighlightMode.Shade;
    }
    return HighlightMode.Highlight;
  };
}
