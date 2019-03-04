import React from "react";
import { Callout, Intent } from "@blueprintjs/core";
import { IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import {
  MultiRelationFieldTable,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
export var FederalTab = function(_a) {
  var model = _a.model,
    contentSchema = _a.contentSchema;
  var schema = contentSchema.Fields.MarketingProduct.RelatedContent;
  return React.createElement(
    React.Fragment,
    null,
    React.createElement(
      Callout,
      { intent: Intent.WARNING },
      "\u0418\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F, \u0441\u0434\u0435\u043B\u0430\u043D\u043D\u044B\u0435 \u0432 \u043E\u0431\u0449\u0435\u0444\u0435\u0434\u0435\u0440\u0430\u043B\u044C\u043D\u044B\u0445 \u0445\u0430\u0440\u0430\u043A\u0442\u0435\u0440\u0438\u0441\u0442\u0438\u043A\u0430\u0445 \u043F\u043E\u0432\u043B\u0438\u044F\u044E\u0442 \u043D\u0430 \u0442\u0430\u0440\u0438\u0444\u044B \u0432\u043E \u0432\u0441\u0435\u0445 \u0440\u0435\u0433\u0438\u043E\u043D\u0430\u0445"
    ),
    React.createElement(EntityEditor, {
      withHeader: true,
      model: model.MarketingProduct,
      contentSchema: schema,
      fieldOrders: [
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
      ],
      fieldEditors: {
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
      }
    })
  );
};
//# sourceMappingURL=FederalTab.js.map
