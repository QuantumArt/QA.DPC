import React, { Component } from "react";
import { consumer, inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { ExtensionEditor } from "Components/ArticleEditor/ExtensionEditor";
import { makePublicatoinStatusIcons } from "Components/PublicationStatusIcon/PublicationStatusIcon";
import {
  RelationFieldTabs,
  RelationFieldAccordion,
  MultiRelationFieldTable,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { PublicationContext } from "Services/PublicationContext";
import { Product, DeviceOnTariffs, ProductRelation } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { hasUniqueRegions } from "../Utils/Validators";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@consumer
export class DevicesTab extends Component<DevicesTabProps> {
  @inject private publicationContext: PublicationContext;

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
    <RelationFieldTabs
      {...props}
      vertical
      canSaveEntity={false}
      canRefreshEntity={false}
      displayField="Title"
      fieldOrders={["Modifiers", "Products", "DevicesOnTariffs"]}
      fieldEditors={{
        Title: IGNORE,
        Products: this.renderDevices,
        DevicesOnTariffs: this.renderDevicesOnTariffs
      }}
    />
  );

  private renderDevices = (props: FieldEditorProps) => (
    <RelationFieldAccordion
      {...props}
      canCloneEntity
      canRemoveEntity
      canPublishEntity
      canClonePrototype
      columnProportions={[9, 3, 3, 1]}
      displayFields={[
        regionsDisplayField,
        rentPriceDisplayField,
        salePriceDisplayField,
        makePublicatoinStatusIcons(this.publicationContext, props.fieldSchema)
      ]}
      filterItems={this.filterModel.filterProducts}
      highlightItems={this.filterModel.highlightProduct}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        MarketingProduct: IGNORE,
        Parameters: this.renderParameters,
        Regions: this.renderRegions
      }}
      onShowEntity={product => product.setTouched("Regions")}
    />
  );

  private renderDevicesOnTariffs = (props: FieldEditorProps) => (
    <RelationFieldAccordion
      {...props}
      canCloneEntity
      canRemoveEntity
      canClonePrototype
      columnProportions={[3, 1, 1]}
      displayFields={[
        matrixRegionsDisplayField,
        matrixRentPriceDisplayField,
        matrixSalePriceDisplayField
      ]}
      filterItems={this.filterModel.filterDevicesOnTariffs}
      highlightItems={this.filterModel.highlightDeviceOnTariffs}
      fieldOrders={["Cities", "Parent", "MarketingTariffs"]}
      fieldEditors={{
        MarketingDevice: IGNORE,
        Parent: this.renderDeviceOnTariffParent,
        MarketingTariffs: MultiRelationFieldTable
      }}
    />
  );

  private renderRegions = (props: FieldEditorProps) => {
    const product = props.model as Product;
    return (
      <MultiRelationFieldTags {...props} sortItemsBy="Title" validate={hasUniqueRegions(product)} />
    );
  };

  private renderParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" },
        { Title: "Цена продажи", Unit: "rub", BaseParam: "SalePrice" }
      ]}
    />
  );

  private renderDeviceOnTariffParent = ({ model, fieldSchema }: FieldEditorProps) => {
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

const regionsDisplayField = (device: Product) => (
  <div className="products-accordion__regions">
    {device.Regions.map(region => region.Title).join(", ")}
  </div>
);

const rentPriceDisplayField = (device: Product) => {
  const parameter = device.Parameters.find(parameter => {
    const baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "RentPrice";
  });
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

const salePriceDisplayField = (device: Product) => {
  const parameter = device.Parameters.find(parameter => {
    const baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "SalePrice";
  });
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

const matrixRegionsDisplayField = (device: DeviceOnTariffs) => (
  <div className="products-accordion__regions">
    {device.Cities.map(region => region.Title).join(", ")}
  </div>
);

const matrixRentPriceDisplayField = (device: DeviceOnTariffs) => {
  const parameter = device.Parent.Parameters.find(parameter => {
    const baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "RentPrice";
  });
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

const matrixSalePriceDisplayField = (device: DeviceOnTariffs) => {
  const parameter = device.Parent.Parameters.find(parameter => {
    const baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "SalePrice";
  });
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
