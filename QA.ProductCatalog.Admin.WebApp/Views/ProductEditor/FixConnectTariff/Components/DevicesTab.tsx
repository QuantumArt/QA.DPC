import React, { Component } from "react";
import { consumer, inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
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
import { Product, DeviceOnTariffs, ProductRelation } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { ProductValidator } from "../Services/ProductValidator";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@consumer
export class DevicesTab extends Component<DevicesTabProps> {
  @inject private productValidator: ProductValidator;
  private filterModel = new FilterModel(this.props.model);

  render() {
    const { model, contentSchema } = this.props;
    const extension = model.MarketingProduct.Type_Extension.MarketingFixConnectTariff;
    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;

    return (
      <>
        <FilterBlock byMarketingTariff model={this.filterModel} />
        <Divider />
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
      canRemoveEntity
      canPublishEntity
      canClonePrototype
      columnProportions={[3, 1, 1]}
      displayFields={[this.renderRegions, this.renderRentPrice, this.renderSalePrice]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        MarketingProduct: IGNORE,
        Parameters: this.renderParameters
      }}
      onSaveEntity={this.saveDevice}
      onCloneEntity={this.cloneDevice}
      onClonePrototype={this.createDevice}
    />
  );

  private saveDevice = async (device: Product, saveEntity: () => Promise<void>) => {
    this.productValidator.validateProduct(device);
    await saveEntity();
  };

  private cloneDevice = async (_internetTariff: Product, cloneEntity: () => Promise<Product>) => {
    const clonedDevice = await cloneEntity();
    this.productValidator.validateProduct(clonedDevice);
  };

  private createDevice = async (clonePrototype: () => Promise<Product>) => {
    const clonedDevice = await clonePrototype();
    this.productValidator.validateProduct(clonedDevice);
  };

  private renderDevicesOnTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      canCloneEntity
      canRemoveEntity
      canPublishEntity
      canClonePrototype
      columnProportions={[3, 1, 1]}
      displayFields={[
        this.renderMatrixRegions,
        this.renderMatrixRentPrice,
        this.renderMatrixSalePrice
      ]}
      filterItems={this.filterModel.filterDevicesOnTariffs}
      fieldOrders={["Cities", "Parent", "MarketingTariffs"]}
      fieldEditors={{
        MarketingDevice: IGNORE,
        Parent: this.renderMatrixProductRelation,
        MarketingTariffs: MultiRelationFieldTable
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
