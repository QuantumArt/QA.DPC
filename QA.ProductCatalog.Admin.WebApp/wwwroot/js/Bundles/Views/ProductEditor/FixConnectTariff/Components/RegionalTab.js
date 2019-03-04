import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { observer } from "mobx-react";
import { inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
import { PublicationContext } from "Services/PublicationContext";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { ArticleEditor, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  FileFieldEditor,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import { FilterModel } from "../Models/FilterModel";
import { hasUniqueRegions, isUniqueRegion } from "../Utils/ProductValidators";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { PublishButtons } from "./PublishButtons";
import { CloneButtons } from "./CloneButtons";
var RegionalTab = /** @class */ (function(_super) {
  tslib_1.__extends(RegionalTab, _super);
  function RegionalTab() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.filterModel = new FilterModel(_this.props.model);
    _this.renderFixTariffsFile = function(props) {
      return React.createElement(
        FileFieldEditor,
        tslib_1.__assign({}, props, { customSubFolder: "fix_tariffs" })
      );
    };
    _this.renderInternetTariffs = function(props) {
      var fieldSchema = props.fieldSchema;
      return React.createElement(
        RelationFieldAccordion,
        tslib_1.__assign({}, props, {
          canCloneEntity: true,
          canRemoveEntity: true,
          canClonePrototype: true,
          columnProportions: [20, 1],
          displayFields: [
            regionsDisplayField,
            function(tariff) {
              return React.createElement(PublicationStatusIcons, {
                model: tariff,
                contentSchema: fieldSchema.RelatedContent,
                publicationContext: _this.publicationContext
              });
            }
          ],
          filterItems: _this.filterModel.filterProducts,
          highlightItems: _this.filterModel.highlightProduct,
          fieldOrders: ["Modifiers", "Regions", "Parameters"],
          fieldEditors: {
            Type: IGNORE,
            MarketingProduct: IGNORE,
            Parameters: _this.renderInternetParameters,
            Regions: _this.renderRegions
          },
          onMountEntity: function(product) {
            return product.setTouched("Regions");
          },
          entityActions: function(product) {
            return React.createElement(PublishButtons, {
              model: product,
              contentSchema: fieldSchema.RelatedContent
            });
          }
        })
      );
    };
    _this.renderPhoneTariffs = function(props) {
      var fieldSchema = props.fieldSchema;
      return React.createElement(
        RelationFieldAccordion,
        tslib_1.__assign({}, props, {
          canCloneEntity: true,
          canRemoveEntity: true,
          canClonePrototype: true,
          columnProportions: [20, 1],
          displayFields: [
            regionsDisplayField,
            function(tariff) {
              return React.createElement(PublicationStatusIcons, {
                model: tariff,
                contentSchema: fieldSchema.RelatedContent,
                publicationContext: _this.publicationContext
              });
            }
          ],
          filterItems: _this.filterModel.filterProducts,
          highlightItems: _this.filterModel.highlightProduct,
          fieldOrders: ["Modifiers", "Regions", "Parameters"],
          fieldEditors: {
            Type: IGNORE,
            MarketingProduct: IGNORE,
            Parameters: _this.renderPhoneParameters,
            Regions: _this.renderRegions
          },
          onMountEntity: function(product) {
            return product.setTouched("Regions");
          },
          entityActions: function(product) {
            return React.createElement(PublishButtons, {
              model: product,
              contentSchema: fieldSchema.RelatedContent
            });
          }
        })
      );
    };
    _this.renderRegions = function(props) {
      var product = props.model;
      return React.createElement(
        MultiRelationFieldTags,
        tslib_1.__assign({}, props, {
          sortItemsBy: "Title",
          validate: hasUniqueRegions(product),
          validateItems: isUniqueRegion(product)
        })
      );
    };
    _this.renderFixConnectParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            { Title: "Цена", Unit: "rub_month", BaseParam: "SubscriptionFee" }
          ]
        })
      );
    };
    _this.renderInternetParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
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
          ]
        })
      );
    };
    _this.renderPhoneParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          showBaseParamModifiers: true,
          fields: [
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
          ]
        })
      );
    };
    return _this;
  }
  RegionalTab.prototype.getMarketingFixConnectTariffProps = function() {
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema;
    var extension =
      model.MarketingProduct.Type_Extension.MarketingFixConnectTariff;
    var extensionSchema =
      contentSchema.Fields.MarketingProduct.RelatedContent.Fields.Type
        .ExtensionContents.MarketingFixConnectTariff;
    return { extension: extension, extensionSchema: extensionSchema };
  };
  RegionalTab.prototype.render = function() {
    var _this = this;
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema;
    var productsFieldSchema =
      contentSchema.Fields.MarketingProduct.RelatedContent.Fields.Products;
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(EntityEditor, {
        withHeader: true,
        model: model,
        contentSchema: contentSchema,
        titleField: function(p) {
          return React.createElement(
            React.Fragment,
            null,
            p.MarketingProduct && p.MarketingProduct.Title,
            React.createElement(
              "div",
              { style: { float: "right" } },
              React.createElement(PublicationStatusIcons, {
                model: model,
                contentSchema: contentSchema,
                publicationContext: _this.publicationContext
              })
            )
          );
        },
        fieldOrders: [
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
        ],
        fieldEditors: {
          Type: IGNORE,
          MarketingProduct: IGNORE,
          PDF: this.renderFixTariffsFile,
          ListImage: this.renderFixTariffsFile,
          Parameters: this.renderFixConnectParameters,
          Regions: this.renderRegions
        },
        customActions: function() {
          return React.createElement(
            React.Fragment,
            null,
            React.createElement(CloneButtons, {
              product: model,
              marketingProduct: model.MarketingProduct,
              fieldSchema: productsFieldSchema
            }),
            React.createElement(PublishButtons, {
              model: model,
              contentSchema: contentSchema
            })
          );
        }
      }),
      React.createElement(Divider, null),
      React.createElement(FilterBlock, { filterModel: this.filterModel }),
      React.createElement(Divider, null),
      this.renderMarketingInternetTariff(),
      this.renderMarketingPhoneTariff()
    );
  };
  RegionalTab.prototype.renderMarketingInternetTariff = function() {
    var _a = this.getMarketingFixConnectTariffProps(),
      extension = _a.extension,
      extensionSchema = _a.extensionSchema;
    var internetTariff = extension.MarketingInternetTariff;
    var contentSchema =
      extensionSchema.Fields.MarketingInternetTariff.RelatedContent;
    return (
      internetTariff &&
      React.createElement(ArticleEditor, {
        model: internetTariff,
        contentSchema: contentSchema,
        skipOtherFields: true,
        fieldEditors: {
          Products: this.renderInternetTariffs
        }
      })
    );
  };
  RegionalTab.prototype.renderMarketingPhoneTariff = function() {
    var _a = this.getMarketingFixConnectTariffProps(),
      extension = _a.extension,
      extensionSchema = _a.extensionSchema;
    var phoneTariff = extension.MarketingPhoneTariff;
    var contentSchema =
      extensionSchema.Fields.MarketingPhoneTariff.RelatedContent;
    return (
      phoneTariff &&
      React.createElement(ArticleEditor, {
        model: phoneTariff,
        contentSchema: contentSchema,
        skipOtherFields: true,
        fieldEditors: {
          Products: this.renderPhoneTariffs
        }
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationContext)],
    RegionalTab.prototype,
    "publicationContext",
    void 0
  );
  RegionalTab = tslib_1.__decorate([observer], RegionalTab);
  return RegionalTab;
})(Component);
export { RegionalTab };
var regionsDisplayField = function(tariff) {
  return React.createElement(
    "div",
    { className: "products-accordion__regions" },
    tariff.Regions.map(function(region) {
      return region.Title;
    }).join(", ")
  );
};
//# sourceMappingURL=RegionalTab.js.map
