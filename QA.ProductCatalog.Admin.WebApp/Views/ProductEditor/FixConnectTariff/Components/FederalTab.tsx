import React from "react";
import { Callout, Intent } from "@blueprintjs/core";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  MultiRelationFieldTable,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import { ContentSchema, RelationFieldSchema } from "Models/EditorSchemaModels";
import { Product } from "../ProductEditorSchema";

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
        model={model.MarketingProduct}
        contentSchema={schema}
        header
        buttons
        fieldOrders={[
          "Title",
          "Description",
          "Modifiers",
          "Advantages",
          "SortOrder",
          "ArchiveDate",
          "Type",
          "TitleForSite",
          "Segment",
          "Category",
          "MarketingInternetTariff",
          "MarketingPhoneTariff",
          "MarketingTvPackage",
          "BonusTVPackages",
          "MarketingDevices",
          "FixConnectActions"
        ]}
        fieldEditors={{
          FixConnectActions: IGNORE,
          Type: IGNORE,
          Type_Contents: {
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
