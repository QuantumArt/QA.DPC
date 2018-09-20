import React, { Component } from "react";
import { computed, observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon, Checkbox, TabId } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { ExtensionEditor } from "Components/ArticleEditor/ArticleEditor";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldTabs,
  MultiRelationFieldAccordion,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { Product, FixConnectAction, DevicesForFixConnectAction } from "../ProductEditorSchema";

interface DevicesTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

const productRegionsField = (device: Product) =>
  device.Regions.map(region => region.Title).join(", ");

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
                    displayFields={[productRegionsField]}
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
}
