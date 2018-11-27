import React, { Component } from "react";
import { computed } from "mobx";
import { observer } from "mobx-react";
import { MarketingProduct, Product, Region } from "../TypeScriptSchema";
import { Col } from "react-flexbox-grid";
import { Callout, Intent } from "@blueprintjs/core";

interface DevicesWarningBlockProps {
  fixConnectTariff: Product;
  marketingDevice: MarketingProduct;
}

@observer
export class DevicesWarningBlock extends Component<DevicesWarningBlockProps> {
  @computed
  get saleUnavailable() {
    return this.props.marketingDevice.Modifiers.some(
      modifier => modifier.Alias === "SaleUnavailable"
    );
  }

  @computed
  get rentUnavailable() {
    return this.props.marketingDevice.Modifiers.some(
      modifier => modifier.Alias === "RentUnavailable"
    );
  }

  @computed
  get notFoundRegions() {
    const { fixConnectTariff, marketingDevice } = this.props;
    const devices = marketingDevice.Products;

    const notFoundRegions: Region[] = [];

    fixConnectTariff.Regions.forEach(region => {
      if (
        !notFoundRegions.includes(region) &&
        !devices.some(device => this.deviceHasRegionAndParams(device, region))
      ) {
        notFoundRegions.push(region);
      }
    });

    return notFoundRegions;
  }

  /**
   * a.	Проверить на наличие Продуктов (тип “Оборудование”) в каждом городе текущего тарифа.
   * b.	Если у Маркетингового Продукта нет модификатора Продукта “Продажа недоступна”,
   *    проверить на наличие у Продуктов параметра с БП “Цена продажи”.
   * c.	Если у Маркетингового Продукта нет модификатора Продукта “Аренда недоступна”,
   *    проверить на наличие у Продуктов параметра с БП “Цена аренды”.
   */
  deviceHasRegionAndParams(device: Product, region: Region) {
    return (
      device.getBaseValue("Regions").includes(region) &&
      (this.saleUnavailable || this.deviceHasParameter(device, "SalePrice")) &&
      (this.rentUnavailable || this.deviceHasParameter(device, "RentPrice"))
    );
  }

  deviceHasParameter(device: Product, alias: string) {
    return device.getBaseValue("Parameters").some(parameter => {
      const baseParameter = parameter.BaseParameter;
      return (
        baseParameter &&
        baseParameter.Alias === alias &&
        parameter.getBaseValue("NumValue") !== null
      );
    });
  }

  render() {
    const { marketingDevice } = this.props;
    const regionTitles = this.notFoundRegions.map(region => region.Title);
    return regionTitles.length > 0 ? (
      <Col md={12}>
        <Callout intent={Intent.DANGER}>
          Внимание! Для городов {regionTitles.join(", ")} для оборудования {marketingDevice.Title}{" "}
          не заданы параметры: Цена продажи, Цена аренды!
        </Callout>
      </Col>
    ) : null;
  }
}
