import React, { Component } from "react";
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
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

export class DevicesTab extends Component<DevicesTabProps> {
  private filterModel = new FilterModel(this.props.model);

  private getMarketingFixConnectTariffProps() {
    const { model, contentSchema } = this.props;
    const extension = model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;
    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;
    return { extension, extensionSchema };
  }

  render() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();

    return (
      <>
        <FilterBlock byMarketingTariff model={this.filterModel} />
        <hr />
        <ExtensionEditor
          model={extension}
          contentSchema={extensionSchema}
          skipOtherFields
          fieldEditors={{
            MarketingDevices: props => (
              <MultiRelationFieldTabs
                {...props}
                vertical
                readonly
                renderOnlyActiveTab
                displayField={"Title"}
                fieldOrders={["Modifiers", "Products", "DevicesOnTariffs"]}
                fieldEditors={{
                  Title: IGNORE,
                  Products: props => (
                    <MultiRelationFieldAccordion
                      {...props}
                      readonly
                      renderOnlyActiveSection
                      columnProportions={[3, 1, 1]}
                      displayFields={[
                        this.renderRegions,
                        this.renderRentPrice,
                        this.renderSalePrice
                      ]}
                      filterItems={this.filterModel.filterProducts}
                      fieldOrders={["Type", "Regions", "Parameters"]}
                    />
                  ),
                  DevicesOnTariffs: props => (
                    <MultiRelationFieldAccordion
                      {...props}
                      readonly
                      renderOnlyActiveSection
                      columnProportions={[3, 1, 1]}
                      displayFields={[
                        this.renderMatrixRegions,
                        this.renderMatrixRentPrice,
                        this.renderMatrixSalePrice
                      ]}
                      filterItems={this.filterModel.filterDevicesOnTariffs}
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
            {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
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
            {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
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
            {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
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
            {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
          </div>
        </>
      )
    );
  };
}
