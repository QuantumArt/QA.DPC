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
import { FieldEditorProps } from "Components/FieldEditors/AbstractFieldEditor";
import { ParameterFields } from "./ParameterFields";

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
            MarketingProduct: IGNORE,
            Parameters: this.renderFixConnectParameters
          }}
        />
        <hr />
        <FilterBlock model={this.filterModel} />
        <hr />
        {this.renderMarketingInternetTariff()}
        {this.renderMarketingPhoneTariff()}
      </>
    );
  }

  private renderMarketingInternetTariff() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();
    const internetTariff = extension.MarketingInternetTariff;
    const contentSchema = (extensionSchema.Fields.MarketingInternetTariff as RelationFieldSchema)
      .RelatedContent;

    return (
      internetTariff && (
        <ArticleEditor
          model={internetTariff}
          contentSchema={contentSchema}
          skipOtherFields
          fieldEditors={{
            Products: this.renderInternetTariffs
          }}
        />
      )
    );
  }

  private renderMarketingPhoneTariff() {
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();
    const phoneTariff = extension.MarketingPhoneTariff;
    const contentSchema = (extensionSchema.Fields.MarketingPhoneTariff as RelationFieldSchema)
      .RelatedContent;

    return (
      phoneTariff && (
        <ArticleEditor
          model={phoneTariff}
          contentSchema={contentSchema}
          skipOtherFields
          fieldEditors={{
            Products: this.renderPhoneTariffs
          }}
        />
      )
    );
  }

  private renderInternetTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      displayFields={[this.renderRegions]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        Parameters: this.renderInternetParameters
      }}
    />
  );

  private renderPhoneTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      readonly
      // renderOnlyActiveSection
      displayFields={[this.renderRegions]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        Parameters: this.renderPhoneParameters
      }}
    />
  );

  private renderFixConnectParameters = (props: FieldEditorProps) => (
    <ParameterFields {...props} fields={[{ Title: "Цена", Alias: "SubscriptionFee" }]} />
  );

  private renderInternetParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Включенный в тариф пакет трафика", Alias: "InternetPackage" },
        { Title: "Стоимость трафика за 1 МБ при превышении лимита", Alias: "1MbOfInternetTraffic" },
        { Title: "Скорость доступа", Alias: "MaxSpeed" },
        { Title: "Скорость доступа ночью", Alias: "MaxSpeed" }
      ]}
    />
  );

  private renderPhoneParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Включенный в АП пакет местных вызовов", Alias: "MinutesPackage" },
        { Title: "Стоимость минуты ВЗ вызова на др. моб.", Alias: "OutgoingCalls" },
        { Title: "Стоимость минуты ВЗ вызова на МТС", Alias: "OutgoingCalls" },
        { Title: "Стоимость минуты ВЗ вызова на стационарные телефоны", Alias: "OutgoingCalls" },
        { Title: "Стоимость минуты местного вызова", Alias: "OutgoingCalls" },
        { Title: "Цена", Alias: "SubscriptionFee" }
      ]}
    />
  );

  private renderRegions = (device: Product) => (
    <div className="products-accordion__regions">
      {device.Regions.map(region => region.Title).join(", ")}
    </div>
  );
}
