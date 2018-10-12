import React, { Component } from "react";
import { observer } from "mobx-react";
import { Divider } from "@blueprintjs/core";
import {
  ContentSchema,
  ExtensionFieldSchema,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { MultiRelationFieldAccordion, FileFieldEditor } from "Components/FieldEditors/FieldEditors";
import { Product } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { validateProduct } from "../Utils/Validators";
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
    const extension = model.MarketingProduct.Type_Extension.MarketingFixConnectTariff;
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
        <Divider />
        <FilterBlock model={this.filterModel} />
        <Divider />
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
      canRemoveEntity
      canPublishEntity
      canClonePrototype
      displayFields={[this.renderRegions]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        MarketingProduct: IGNORE,
        Parameters: this.renderInternetParameters
      }}
      onSaveEntity={this.saveInternetTariff}
      onCloneEntity={this.cloneInternetTariff}
      onClonePrototype={this.createInternetTariff}
    />
  );

  private saveInternetTariff = async (internetTariff: Product, saveEntity: () => Promise<void>) => {
    validateProduct(internetTariff);
    await saveEntity();
  };

  private cloneInternetTariff = async (
    _internetTariff: Product,
    cloneEntity: () => Promise<Product>
  ) => {
    const clonedTariff = await cloneEntity();
    validateProduct(clonedTariff);
  };

  private createInternetTariff = async (clonePrototype: () => Promise<Product>) => {
    const clonedTariff = await clonePrototype();
    validateProduct(clonedTariff);
  };

  private renderPhoneTariffs = (props: FieldEditorProps) => (
    <MultiRelationFieldAccordion
      {...props}
      // renderOnlyActiveSection
      canCloneEntity
      canRemoveEntity
      canPublishEntity
      canClonePrototype
      displayFields={[this.renderRegions]}
      filterItems={this.filterModel.filterProducts}
      fieldOrders={["Modifiers", "Regions", "Parameters"]}
      fieldEditors={{
        Type: IGNORE,
        MarketingProduct: IGNORE,
        Parameters: this.renderPhoneParameters
      }}
      onSaveEntity={this.savePhoneTariff}
      onCloneEntity={this.clonePhoneTaridd}
      onClonePrototype={this.createPhoneTariff}
    />
  );

  private savePhoneTariff = async (phoneTariff: Product, saveEntity: () => Promise<void>) => {
    validateProduct(phoneTariff);
    await saveEntity();
  };

  private clonePhoneTaridd = async (_phoneTariff: Product, cloneEntity: () => Promise<Product>) => {
    const clonedTariff = await cloneEntity();
    validateProduct(clonedTariff);
  };

  private createPhoneTariff = async (clonePrototype: () => Promise<Product>) => {
    const clonedTariff = await clonePrototype();
    validateProduct(clonedTariff);
  };

  private renderFixConnectParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[{ Title: "Цена", Unit: "rub_month", BaseParam: "SubscriptionFee" }]}
    />
  );

  private renderInternetParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Скорость доступа", Unit: "mbit", BaseParam: "MaxSpeed" },
        {
          Title: "Скорость доступа ночью",
          Unit: "mbit",
          BaseParam: "MaxSpeed",
          BaseParamModifiers: ["Night"]
        },
        {
          Title: "Включенный в тариф пакет трафика",
          Unit: "mb_month",
          BaseParam: "InternetPackage",
          BaseParamModifiers: ["IncludedInSubscriptionFee"]
        },
        {
          Title: "Стоимость трафика за 1 МБ при превышении лимита",
          Unit: "rub_mb",
          BaseParam: "1MbOfInternetTraffic"
        }
      ]}
    />
  );

  private renderPhoneParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Цена", Unit: "rub_month", BaseParam: "SubscriptionFee" },
        {
          Title: "Стоимость минуты местного вызова",
          Unit: "rub_minute",
          BaseParam: "OutgoingCalls",
          BaseParamModifiers: ["ToLocalCalls"]
        },
        {
          Title: "Стоимость минуты ВЗ вызова на МТС",
          Unit: "rub_minute",
          BaseParam: "OutgoingCalls",
          BaseParamModifiers: ["MTS"]
        },
        {
          Title: "Стоимость минуты ВЗ вызова на др. моб.",
          Unit: "rub_minute",
          BaseParam: "OutgoingCalls",
          BaseParamModifiers: ["ExceptMTS", "LowerBound"]
        },
        {
          Title: "Стоимость минуты ВЗ вызова на стационарные телефоны",
          Unit: "rub_minute",
          BaseParam: "OutgoingCalls",
          BaseParamModifiers: ["CityOnly"]
        },
        {
          Title: "Включенный в АП пакет местных вызовов",
          Unit: "minutes_call",
          BaseParam: "MinutesPackage",
          BaseParamModifiers: ["IncludedInSubscriptionFee"]
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
