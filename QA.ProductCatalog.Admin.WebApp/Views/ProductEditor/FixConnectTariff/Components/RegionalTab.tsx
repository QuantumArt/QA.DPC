import React, { Component } from "react";
import { observer } from "mobx-react";
import { inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
import {
  ContentSchema,
  ExtensionFieldSchema,
  RelationFieldSchema
} from "ProductEditor/Models/EditorSchemaModels";
import { PublicationContext } from "ProductEditor/Services/PublicationContext";
import { EntityEditor } from "ProductEditor/Components/ArticleEditor/EntityEditor";
import { ArticleEditor, FieldEditorProps, IGNORE } from "ProductEditor/Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  FileFieldEditor,
  MultiRelationFieldTags
} from "ProductEditor/Components/FieldEditors/FieldEditors";
import { PublicationStatusIcons } from "ProductEditor/Components/PublicationStatusIcons/PublicationStatusIcons";
import { Product } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { hasUniqueRegions, isUniqueRegion } from "../Utils/ProductValidators";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { PublishButtons } from "./PublishButtons";
import { CloneButtons } from "./CloneButtons";

interface RegionalTabTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class RegionalTab extends Component<RegionalTabTabProps> {
  @inject private publicationContext: PublicationContext;

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
    const productsFieldSchema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Products as RelationFieldSchema;
    return (
      <>
        <EntityEditor
          withHeader
          model={model}
          contentSchema={contentSchema}
          titleField={(p: Product) => (
            <>
              {p.MarketingProduct && p.MarketingProduct.Title}
              <div style={{ float: "right" }}>
                <PublicationStatusIcons
                  model={model}
                  contentSchema={contentSchema}
                  publicationContext={this.publicationContext}
                />
              </div>
            </>
          )}
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
            Parameters: this.renderFixConnectParameters,
            Regions: this.renderRegions
          }}
          customActions={() => (
            <>
              <CloneButtons
                product={model}
                marketingProduct={model.MarketingProduct}
                fieldSchema={productsFieldSchema}
              />
              <PublishButtons model={model} contentSchema={contentSchema} />
            </>
          )}
        />
        <Divider />
        <FilterBlock filterModel={this.filterModel} />
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

  private renderInternetTariffs = (props: FieldEditorProps) => {
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    return (
      <RelationFieldAccordion
        {...props}
        canCloneEntity
        canRemoveEntity
        canClonePrototype
        columnProportions={[20, 1]}
        displayFields={[
          regionsDisplayField,
          (tariff: Product) => (
            <PublicationStatusIcons
              model={tariff}
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
          Parameters: this.renderInternetParameters,
          Regions: this.renderRegions
        }}
        onMountEntity={(product: Product) => product.setTouched("Regions")}
        entityActions={(product: Product) => (
          <PublishButtons model={product} contentSchema={fieldSchema.RelatedContent} />
        )}
      />
    );
  };

  private renderPhoneTariffs = (props: FieldEditorProps) => {
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    return (
      <RelationFieldAccordion
        {...props}
        canCloneEntity
        canRemoveEntity
        canClonePrototype
        columnProportions={[20, 1]}
        displayFields={[
          regionsDisplayField,
          (tariff: Product) => (
            <PublicationStatusIcons
              model={tariff}
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
          Parameters: this.renderPhoneParameters,
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
      showBaseParamModifiers
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
          BaseParamModifiers: ["ExceptMTS"]
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
}

const regionsDisplayField = (tariff: Product) => (
  <div className="products-accordion__regions">
    {tariff.Regions.map(region => region.Title).join(", ")}
  </div>
);
