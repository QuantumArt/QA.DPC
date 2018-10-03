import React, { Component } from "react";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { ExtensionEditor } from "Components/ArticleEditor/ExtensionEditor";
import {
  MultiRelationFieldTabs,
  MultiRelationFieldAccordion,
  MultiRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { Product, DeviceOnTariffs, ProductRelation } from "../ProductEditorSchema";
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

export class DevicesTab extends Component<DevicesTabProps> {
  private filterModel = new FilterModel(this.props.model);

  render() {
    const { model, contentSchema } = this.props;
    const extension = model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;
    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;

    return (
      <>
        <FilterBlock byMarketingTariff model={this.filterModel} />
        <hr />
        <ExtensionEditor
          model={extension}
          contentSchema={extensionSchema}
          skipOtherFields
          fieldEditors={{
            MarketingDevices: this.renderMarketingDevices
          }}
        />
      </>
    );
  }

  private renderMarketingDevices = (props: FieldEditorProps) => (
    <MultiRelationFieldTabs
      {...props}
      vertical
      renderOnlyActiveTab
      canSaveEntity={false}
      canRefreshEntity={false}
      displayField={"Title"}
      fieldOrders={["Modifiers", "Products", "DevicesOnTariffs"]}
      fieldEditors={{
        Title: IGNORE,
        Products: this.renderDevices,
        DevicesOnTariffs: this.renderDevicesOnTariffs
      }}
    />
  );

  private renderDevices = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      canCloneEntity
      canPublishEntity
      columnProportions={[3, 1, 1]}
      displayFields={[this.renderRegions, this.renderRentPrice, this.renderSalePrice]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        Parameters: this.renderParameters
      }}
    />
  );

  private renderDevicesOnTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      canCloneEntity
      canPublishEntity
      columnProportions={[3, 1, 1]}
      displayFields={[
        this.renderMatrixRegions,
        this.renderMatrixRentPrice,
        this.renderMatrixSalePrice
      ]}
      filterItems={this.filterModel.filterDevicesOnTariffs}
      fieldOrders={["Cities", "Parent", "MarketingTariffs"]}
      fieldEditors={{
        Parent: this.renderMatrixProductRelation,
        MarketingTariffs: MultiRelationFieldTable,
        MarketingDevice: IGNORE
      }}
    />
  );

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

  private renderParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Цена аренды", Alias: "RentPrice", Unit: "rub_month" },
        { Title: "Цена продажи", Alias: "SalePrice", Unit: "rub" }
      ]}
    />
  );

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

  private renderMatrixProductRelation = ({ model, fieldSchema }: FieldEditorProps) => {
    const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
    const productRelation = model[fieldSchema.FieldName] as ProductRelation;
    return (
      productRelation && (
        <ArticleEditor
          model={productRelation}
          contentSchema={contentSchema}
          fieldOrders={["Title", "Parameters"]}
          fieldEditors={{
            Parameters: this.renderParameters
          }}
        />
      )
    );
  };
}
