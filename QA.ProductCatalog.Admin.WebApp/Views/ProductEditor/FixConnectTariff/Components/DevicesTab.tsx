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

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

export class DevicesTab extends Component<DevicesTabProps> {
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
      <ExtensionEditor
        model={extension}
        contentSchema={extensionSchema}
        skipOtherFields
        fieldEditors={{
          MarketingDevices: props => (
            <MultiRelationFieldTabs
              {...props}
              vertical
              className="container-xl"
              displayField={"Title"}
              fieldOrders={["Modifiers", "Products", "DevicesOnTariffs"]}
              fieldEditors={{
                Title: IGNORE,
                Products: props => (
                  <MultiRelationFieldAccordion
                    {...props}
                    columnProportions={[3, 1, 1]}
                    displayFields={[this.renderRegions, this.renderRentPrice, this.renderSalePrice]}
                    // filterItems={this.filterProductsByRegion}
                    fieldOrders={["Type", "Regions", "Parameters"]}
                  />
                ),
                DevicesOnTariffs: props => (
                  <MultiRelationFieldAccordion
                    {...props}
                    columnProportions={[3, 1, 1]}
                    displayFields={[
                      this.renderMatrixRegions,
                      this.renderMatrixRentPrice,
                      this.renderMatrixSalePrice
                    ]}
                    // filterItems={this.filterProductsByRegion}
                    fieldOrders={["Type", "Regions", "Parameters"]}
                  />
                )
              }}
            />
          )
        }}
      />
    );
  }

  private renderRegions = (device: Product) => {
    const text = device.Regions.map(region => region.Title).join(", ");
    return (
      <div className="products-accordion__regions" title={text}>
        {text}
      </div>
    );
  };

  private renderRentPrice = (device: Product) => {
    const parameter = device.Parameters.find(
      parameter => parameter.BaseParameter.Alias === "RentPrice"
    );
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
    const parameter = device.Parameters.find(
      parameter => parameter.BaseParameter.Alias === "SalePrice"
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

  private renderMatrixRegions = (device: DeviceOnTariffs) => {
    const text = device.Cities.map(region => region.Title).join(", ");
    return (
      <div className="products-accordion__regions" title={text}>
        {text}
      </div>
    );
  };

  private renderMatrixRentPrice = (device: DeviceOnTariffs) => {
    const parameter = device.Parent.Parameters.find(
      parameter => parameter.BaseParameter.Alias === "RentPrice"
    );
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
      parameter => parameter.BaseParameter.Alias === "SalePrice"
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
