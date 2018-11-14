import React, { Component } from "react";
import { inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
import { FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { ExtensionEditor } from "Components/ArticleEditor/ExtensionEditor";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import {
  RelationFieldTabs,
  RelationFieldAccordion,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { PublicationContext } from "Services/PublicationContext";
import { Product } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { hasUniqueRegions, isUniqueRegion } from "../Utils/ProductValidators";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { PublishButtons } from "./PublishButtons";
import { DevicesOnTariffsBlock } from "./DevicesOnTariffsBlock";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

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
        <FilterBlock byMarketingTariff filterModel={this.filterModel} />
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

  private renderDevicesOnTariffs = (props: FieldEditorProps) => (
    <DevicesOnTariffsBlock
      {...props}
      filterModel={this.filterModel}
      marketingTariff={this.props.model.MarketingProduct}
    />
  );

  private renderDevices = (props: FieldEditorProps) => {
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    return (
      <RelationFieldAccordion
        {...props}
        canCloneEntity
        canRemoveEntity
        canClonePrototype
        columnProportions={[9, 3, 3, 1]}
        displayFields={[
          regionsDisplayField,
          rentPriceDisplayField,
          salePriceDisplayField,
          (device: Product) => (
            <PublicationStatusIcons
              model={device}
              contentSchema={fieldSchema.RelatedContent}
              publicationContext={this.publicationContext}
            />
          )
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
        onMountEntity={(product: Product) => product.setTouched("Regions")}
        entityActions={(product: Product) => (
          <PublishButtons model={product} contentSchema={fieldSchema.RelatedContent} />
        )}
      />
    );
  };

  private renderRegions = (props: FieldEditorProps) => {
    const product = props.model as Product;
    return (
      <MultiRelationFieldTags
        {...props}
        sortItemsBy="Title"
        validate={hasUniqueRegions(product)}
        validateItems={isUniqueRegion(product)}
      />
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
