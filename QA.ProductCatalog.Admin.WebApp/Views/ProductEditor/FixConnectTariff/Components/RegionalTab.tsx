import React, { Component } from "react";
import { observer } from "mobx-react";
import {
  ContentSchema,
  ExtensionFieldSchema,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { MultiRelationFieldAccordion, FileFieldEditor } from "Components/FieldEditors/FieldEditors";
import { Product } from "../ProductEditorSchema";
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";
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
          withHeader
          canPublishEntity
          model={model}
          contentSchema={contentSchema}
          titleField={(p: Product) => p.MarketingProduct && p.MarketingProduct.Title}
          fieldOrders={[
            // поля статьи расширения
            "Type",
            "TitleForSite",
            // поля основной статьи
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
            PDF: this.renderFixTariffsFile,
            ListImage: this.renderFixTariffsFile,
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

  private renderFixTariffsFile = (props: FieldEditorProps) => (
    <FileFieldEditor {...props} customSubFolder="fix_tariffs" />
  );

  private renderInternetTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      canCloneEntity
      canPublishEntity
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
      // renderOnlyActiveSection
      canCloneEntity
      canPublishEntity
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
    <ParameterFields
      {...props}
      fields={[{ Title: "Цена", Alias: "SubscriptionFee", Unit: "rub_month" }]}
    />
  );

  private renderInternetParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Скорость доступа", Alias: "MaxSpeed", Unit: "mbit" },
        { Title: "Скорость доступа ночью", Alias: "MaxSpeed", Unit: "mbit" },
        { Title: "Включенный в тариф пакет трафика", Alias: "InternetPackage", Unit: "mb_month" },
        {
          Title: "Стоимость трафика за 1 МБ при превышении лимита",
          Alias: "1MbOfInternetTraffic",
          Unit: "rub_mb"
        }
      ]}
    />
  );

  private renderPhoneParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Цена", Alias: "SubscriptionFee", Unit: "rub_month" },
        { Title: "Стоимость минуты местного вызова", Alias: "OutgoingCalls", Unit: "rub_minute" },
        { Title: "Стоимость минуты ВЗ вызова на МТС", Alias: "OutgoingCalls", Unit: "rub_minute" },
        {
          Title: "Стоимость минуты ВЗ вызова на др. моб.",
          Alias: "OutgoingCalls",
          Unit: "rub_minute"
        },
        {
          Title: "Стоимость минуты ВЗ вызова на стационарные телефоны",
          Alias: "OutgoingCalls",
          Unit: "rub_minute"
        },
        {
          Title: "Включенный в АП пакет местных вызовов",
          Alias: "MinutesPackage",
          Unit: "minutes_call"
        }
      ]}
    />
  );

  private renderRegions = (device: Product) => (
    <div className="products-accordion__regions">
      {device.Regions.map(region => region.Title).join(", ")}
    </div>
  );
}
