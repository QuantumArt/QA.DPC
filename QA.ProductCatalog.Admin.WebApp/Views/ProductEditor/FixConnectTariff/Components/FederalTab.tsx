import React from "react";
import { Callout, Intent } from "@blueprintjs/core";
import { IGNORE } from "ProductEditor/Components/ArticleEditor/ArticleEditor";
import { EntityEditor } from "ProductEditor/Components/ArticleEditor/EntityEditor";
import {
  MultiRelationFieldTable,
  SingleRelationFieldTable
} from "ProductEditor/Components/FieldEditors/FieldEditors";
import { ContentSchema, RelationFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { Product } from "../TypeScriptSchema";

interface FederalTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

export const FederalTab = ({ model, contentSchema }: FederalTabProps) => {
  const schema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema).RelatedContent;
  return (
    <>
      <Callout intent={Intent.WARNING}>
        Изменения, сделанные в общефедеральных характеристиках повлияют на тарифы во всех регионах
      </Callout>
      <EntityEditor
        withHeader
        model={model.MarketingProduct}
        contentSchema={schema}
        fieldOrders={[
          // поля основной статьи
          "Title",
          "Description",
          "Modifiers",
          "Advantages",
          "SortOrder",
          "ArchiveDate",
          // поля статьи расширения
          "Type",
          "TitleForSite",
          "Segment",
          "Category",
          "MarketingInternetTariff",
          "MarketingPhoneTariff",
          "MarketingTvPackage",
          "BonusTVPackages",
          "MarketingDevices",
          // поля основной статьи
          "FixConnectActions"
        ]}
        fieldEditors={{
          Products: IGNORE,
          FixConnectActions: IGNORE,
          Type: IGNORE,
          Type_Extension: {
            MarketingFixConnectTariff: {
              MarketingTvPackage: SingleRelationFieldTable,
              MarketingInternetTariff: SingleRelationFieldTable,
              MarketingPhoneTariff: SingleRelationFieldTable,
              BonusTVPackages: MultiRelationFieldTable,
              MarketingDevices: MultiRelationFieldTable
            }
          }
        }}
      />
    </>
  );
};
