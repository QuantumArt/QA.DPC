import React, { Component } from "react";
import { observer } from "mobx-react";
import {
  ContentSchema,
  ExtensionFieldSchema,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { Product } from "../ProductEditorSchema";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { MultiRelationFieldAccordion } from "Components/FieldEditors/FieldEditors";
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";

interface RegionalTabTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class RegionalTab extends Component<RegionalTabTabProps> {
  private filterModel = new FilterModel(this.props.model);

  private getMarketingFixConnectTariffProps() {
    const { model, contentSchema } = this.props;
    const extension = model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;
    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;
    return { extension, extensionSchema };
  }

  private getMarketingInternetTariffProps() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();
    const internetTariff = extension.MarketingInternetTariff;
    const contentSchema = (extensionSchema.Fields.MarketingInternetTariff as RelationFieldSchema)
      .RelatedContent;
    return { internetTariff, contentSchema };
  }

  private getMarketingPhoneTariffProps() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();
    const phoneTariff = extension.MarketingPhoneTariff;
    const contentSchema = (extensionSchema.Fields.MarketingPhoneTariff as RelationFieldSchema)
      .RelatedContent;
    return { phoneTariff, contentSchema };
  }

  render() {
    const { model, contentSchema } = this.props;
    return (
      <>
        <EntityEditor
          header
          buttons
          model={model}
          contentSchema={contentSchema}
          titleField={(p: Product) => p.MarketingProduct && p.MarketingProduct.Title}
          fieldOrders={[
            "Type",
            "TitleForSite",
            "Description",
            "Modifiers",
            "Regions",
            "PDF",
            "Priority",
            "ListImage",
            "SortOrder",
            "Parameters",
            "Advantages"
          ]}
          fieldEditors={{
            Type: IGNORE,
            MarketingProduct: IGNORE
          }}
        />
        <FilterBlock model={this.filterModel} />
        {this.renderInternet()}
        {this.renderPhone()}
      </>
    );
  }

  private renderInternet() {
    const { internetTariff, contentSchema } = this.getMarketingInternetTariffProps();
    return (
      internetTariff && (
        <ArticleEditor
          model={internetTariff}
          contentSchema={contentSchema}
          skipOtherFields
          fieldEditors={{
            Products: props => (
              <MultiRelationFieldAccordion
                {...props}
                displayFields={[this.renderRegions]}
                filterItems={this.filterModel.filterProducts}
              />
            )
          }}
        />
      )
    );
  }

  private renderPhone() {
    const { phoneTariff, contentSchema } = this.getMarketingPhoneTariffProps();
    return (
      phoneTariff && (
        <ArticleEditor
          model={phoneTariff}
          contentSchema={contentSchema}
          skipOtherFields
          fieldEditors={{
            Products: props => (
              <MultiRelationFieldAccordion
                {...props}
                displayFields={[this.renderRegions]}
                filterItems={this.filterModel.filterProducts}
              />
            )
          }}
        />
      )
    );
  }

  private renderRegions = (device: Product) => (
    <div className="products-accordion__regions">
      {device.Regions.map(region => region.Title).join(", ")}
    </div>
  );
}
