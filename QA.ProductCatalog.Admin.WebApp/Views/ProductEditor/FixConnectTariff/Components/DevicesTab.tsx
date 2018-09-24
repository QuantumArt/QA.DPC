import React, { Component } from "react";
import { observable, action, computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Switch, Alignment, TagInput, Button } from "@blueprintjs/core";
import { normailzeSerachString } from "Utils/Common";
import { ExtensionEditor } from "Components/ArticleEditor/ArticleEditor";
import { IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  MultiRelationFieldTabs,
  MultiRelationFieldAccordion
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { Product, DeviceOnTariffs } from "../ProductEditorSchema";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class DevicesTab extends Component<DevicesTabProps> {
  @observable.ref private filterByTariffRegions = true;
  @observable.ref private filterByMarketingTariff = true;
  @observable.ref private regionsFilter: string[] = [];

  @action
  private toggleFilterByTariffRegions = () => {
    this.filterByTariffRegions = !this.filterByTariffRegions;
  };

  @action
  private toggleFilterByMarketingTariff = () => {
    this.filterByMarketingTariff = !this.filterByMarketingTariff;
  };

  @action
  private setRegionsFilter = (values: string[]) => {
    this.regionsFilter = values;
  };

  @action
  private clearRegionsFilter = () => {
    if (this.regionsFilter.length > 0) {
      this.regionsFilter = [];
    }
  };

  @computed
  private get fixTariffRegionIds(): { [clientId: number]: true } {
    const fixTraiff = this.props.model as Product;
    return fixTraiff.Regions.reduce((obj, region) => {
      obj[region._ClientId] = true;
      return obj;
    }, {});
  }

  @computed
  private get normalizedRegionsFilter() {
    return this.regionsFilter.map(normailzeSerachString);
  }

  private filterProducts = (product: Product) => {
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

  private filterDevicesOnTariffs = (device: DeviceOnTariffs) => {
    const {
      filterByTariffRegions,
      filterByMarketingTariff,
      normalizedRegionsFilter,
      fixTariffRegionIds
    } = this;
    if (
      filterByMarketingTariff &&
      !device.MarketingTariffs.includes(this.props.model.MarketingProduct)
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

  private getMarketingFixConnectTariffProps() {
    const { model, contentSchema } = this.props;

    const extension =
      model.MarketingProduct && model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;

    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;

    return { extension, extensionSchema };
  }

  render() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();

    return (
      <>
        {this.renderFilter()}
        <ExtensionEditor
          model={extension}
          contentSchema={extensionSchema}
          skipOtherFields
          fieldEditors={{
            MarketingDevices: props => (
              <MultiRelationFieldTabs
                {...props}
                vertical
                renderOnlyActiveTab
                className="container-xl"
                displayField={"Title"}
                fieldOrders={["Modifiers", "Products", "DevicesOnTariffs"]}
                fieldEditors={{
                  Title: IGNORE,
                  Products: props => (
                    <MultiRelationFieldAccordion
                      {...props}
                      renderOnlyActiveSection
                      columnProportions={[3, 1, 1]}
                      displayFields={[
                        this.renderRegions,
                        this.renderRentPrice,
                        this.renderSalePrice
                      ]}
                      filterItems={this.filterProducts}
                      fieldOrders={["Type", "Regions", "Parameters"]}
                    />
                  ),
                  DevicesOnTariffs: props => (
                    <MultiRelationFieldAccordion
                      {...props}
                      renderOnlyActiveSection
                      columnProportions={[3, 1, 1]}
                      displayFields={[
                        this.renderMatrixRegions,
                        this.renderMatrixRentPrice,
                        this.renderMatrixSalePrice
                      ]}
                      filterItems={this.filterDevicesOnTariffs}
                      fieldOrders={["Type", "Regions", "Parameters"]}
                    />
                  )
                }}
              />
            )
          }}
        />
      </>
    );
  }

  private renderFilter() {
    return (
      <Col md className="devices-tab__filter">
        <Row>
          <Col md={3}>
            <Switch
              large
              alignIndicator={Alignment.RIGHT}
              checked={this.filterByTariffRegions}
              label=""
              onChange={this.toggleFilterByTariffRegions}
            >
              Фильтровать по регионам <br /> тарифа фиксированной связи
            </Switch>
          </Col>
          <Col md={6}>
            <TagInput
              fill
              large
              addOnBlur
              inputValue=""
              leftIcon="filter-list"
              values={this.regionsFilter}
              onChange={this.setRegionsFilter}
              tagProps={{ minimal: true }}
              inputProps={{ placeholder: "Фильтровать по регионам" }}
              rightElement={
                this.regionsFilter.length > 0 && (
                  <Button minimal icon="cross" onClick={this.clearRegionsFilter} />
                )
              }
            />
          </Col>
          <Col md={3}>
            <Switch
              large
              alignIndicator={Alignment.RIGHT}
              checked={this.filterByMarketingTariff}
              onChange={this.toggleFilterByMarketingTariff}
            >
              Фильтровать по маркетинговому <br /> тарифу фиксированной связи
            </Switch>
          </Col>
        </Row>
      </Col>
    );
  }

  private renderRegions = (device: Product) => (
    <div className="products-accordion__regions">
      {device.Regions.map(region => region.Title).join(", ")}
    </div>
  );

  private renderRentPrice = (device: Product) => {
    const parameter = device.Parameters.find(parameter => parameter.Title === "Цена аренды");
    return (
      parameter &&
      parameter.NumValue !== null && (
        <>
          <div>Цена аренды:</div>
          <div>
            {parameter.NumValue} {parameter.Unit.Title}
          </div>
        </>
      )
    );
  };

  private renderSalePrice = (device: Product) => {
    const parameter = device.Parameters.find(parameter => parameter.Title === "Цена продажи");
    return (
      parameter &&
      parameter.NumValue !== null && (
        <>
          <div>Цена продажи:</div>
          <div>
            {parameter.NumValue} {parameter.Unit.Title}
          </div>
        </>
      )
    );
  };

  private renderMatrixRegions = (device: DeviceOnTariffs) => (
    <div className="products-accordion__regions">
      {device.Cities.map(region => region.Title).join(", ")}
    </div>
  );

  private renderMatrixRentPrice = (device: DeviceOnTariffs) => {
    const parameter = device.Parent.Parameters.find(parameter => parameter.Title === "Цена аренды");
    return (
      parameter &&
      parameter.NumValue !== null && (
        <>
          <div>Цена аренды:</div>
          <div>
            {parameter.NumValue} {parameter.Unit.Title}
          </div>
        </>
      )
    );
  };

  private renderMatrixSalePrice = (device: DeviceOnTariffs) => {
    const parameter = device.Parent.Parameters.find(
      parameter => parameter.Title === "Цена продажи"
    );
    return (
      parameter &&
      parameter.NumValue !== null && (
        <>
          <div>Цена продажи:</div>
          <div>
            {parameter.NumValue} {parameter.Unit.Title}
          </div>
        </>
      )
    );
  };
}
