import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { observer } from "mobx-react";
import { inject } from "react-ioc";
import { Divider, Button, Intent } from "@blueprintjs/core";
import { PublicationContext } from "Services/PublicationContext";
import { ArticleEditor, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  MultiRelationFieldTable,
  RelationFieldTabs,
  SingleRelationFieldTags,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import { by, asc, desc } from "Utils/Array";
import { hasUniqueRegions, isUniqueRegion } from "../Utils/ProductValidators";
import {
  hasUniqueMarketingDevice,
  onlyOnePerRegionHasDevices,
  onlyOneItemPerRegionHasDevices
} from "../Utils/ActionDeviceValidators";
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { action, computed } from "mobx";
import { PublishButtons } from "./PublishButtons";
var ActionsTab = /** @class */ (function(_super) {
  tslib_1.__extends(ActionsTab, _super);
  function ActionsTab() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.filterModel = new FilterModel(_this.props.model);
    _this.renderActions = function(props) {
      var fieldSchema = props.fieldSchema;
      var parentFieldSchema = fieldSchema.RelatedContent.Fields.Parent;
      var otherActions = _this.props.model.MarketingProduct.FixConnectActions;
      return React.createElement(
        RelationFieldAccordion,
        tslib_1.__assign({}, props, {
          filterItems: _this.filterModel.filterActions,
          highlightItems: _this.filterModel.highlightAction,
          sortItems: by(
            asc(function(action) {
              return (
                action.Parent.MarketingProduct &&
                action.Parent.MarketingProduct._ServerId
              );
            }),
            asc(function(action) {
              return action.Parent._ServerId;
            })
          ),
          columnProportions: [4, 10, 1],
          displayFields: [
            actionTitleDisplayField,
            actionRegionsDisplayField,
            function(action) {
              return React.createElement(PublicationStatusIcons, {
                model: action,
                product: action.Parent,
                contentSchema: fieldSchema.RelatedContent,
                publicationContext: _this.publicationContext
              });
            }
          ],
          fieldOrders: [
            "Parent",
            "PromoPeriod",
            "AfterPromo",
            "MarketingOffers"
          ],
          fieldEditors: {
            Parent: _this.renderActionParent,
            MarketingOffers: _this.renderMarketingOffers
          },
          validateItems: onlyOneItemPerRegionHasDevices(otherActions),
          canClonePrototype: true,
          canCloneEntity: true,
          canRemoveEntity: true,
          canSelectRelation: true,
          onMountEntity: function(action) {
            return action.Parent.setTouched("Regions").setTouched(
              "ActionMarketingDevices"
            );
          },
          onClonePrototype: function(clonePrototype) {
            return tslib_1.__awaiter(_this, void 0, void 0, function() {
              return tslib_1.__generator(this, function(_a) {
                switch (_a.label) {
                  case 0:
                    return [4 /*yield*/, clonePrototype()];
                  case 1:
                    _a.sent();
                    props.model.setChanged(props.fieldSchema.FieldName, false);
                    return [2 /*return*/];
                }
              });
            });
          },
          onSelectRelation: function(selectRelation) {
            return tslib_1.__awaiter(_this, void 0, void 0, function() {
              return tslib_1.__generator(this, function(_a) {
                switch (_a.label) {
                  case 0:
                    return [4 /*yield*/, selectRelation()];
                  case 1:
                    _a.sent();
                    props.model.setChanged(props.fieldSchema.FieldName, false);
                    return [2 /*return*/];
                }
              });
            });
          },
          entityActions: function(action) {
            return React.createElement(PublishButtons, {
              model: action.Parent,
              contentSchema: parentFieldSchema.RelatedContent
            });
          }
        })
      );
    };
    _this.renderMarketingOffers = function(props) {
      var fixAction = props.model;
      var marketingTariff = _this.props.model.MarketingProduct;
      return React.createElement(
        MultiRelationFieldTable,
        tslib_1.__assign({}, props, {
          highlightItems: function(marketingOffer) {
            return marketingOffer === marketingTariff
              ? 1 /* Highlight */
              : 0 /* None */;
          },
          sortItems: by(
            desc(function(marketingOffer) {
              return marketingOffer === marketingTariff;
            }),
            asc(function(marketingOffer) {
              return marketingOffer.Title;
            })
          ),
          relationActions: function() {
            return React.createElement(
              React.Fragment,
              null,
              !fixAction.MarketingOffers.includes(marketingTariff) &&
                React.createElement(
                  Button,
                  {
                    minimal: true,
                    small: true,
                    rightIcon: "pin",
                    intent: Intent.PRIMARY,
                    onClick: function() {
                      return _this.pinActionToMarketingTariff(fixAction);
                    },
                    title:
                      "\u041F\u0440\u0438\u0432\u044F\u0437\u0430\u0442\u044C \u043A \u0442\u0435\u043A\u0443\u0449\u0435\u043C\u0443 \u043C\u0430\u0440\u043A\u0435\u0442\u0438\u043D\u0433\u043E\u0432\u043E\u043C\u0443 \u0442\u0430\u0440\u0438\u0444\u0443 \u0444\u0438\u043A\u0441\u0438\u0440\u043E\u0432\u0430\u043D\u043D\u043E\u0439 \u0441\u0432\u044F\u0437\u0438"
                  },
                  "\u041F\u0440\u0438\u0432\u044F\u0437\u0430\u0442\u044C \u043A \u0442\u0435\u043A\u0443\u0449\u0435\u043C\u0443 \u0442\u0430\u0440\u0438\u0444\u0443"
                )
            );
          }
        })
      );
    };
    _this.renderActionParent = function(_a) {
      var model = _a.model,
        fieldSchema = _a.fieldSchema;
      var contentSchema = fieldSchema.RelatedContent;
      var product = model[fieldSchema.FieldName];
      return (
        product &&
        React.createElement(ArticleEditor, {
          model: product,
          contentSchema: contentSchema,
          fieldOrders: [
            "MarketingProduct",
            "Description",
            "Regions",
            "Parameters",
            "Advantages",
            "ActionMarketingDevices",
            "Modifiers",
            "PDF",
            "StartDate",
            "Priority",
            "EndDate",
            "SortOrder"
          ],
          fieldEditors: {
            MarketingProduct: SingleRelationFieldTags,
            Regions: _this.renderActionRegions,
            Parameters: _this.renderActionParameters,
            ActionMarketingDevices: _this.renderDevices
          }
        })
      );
    };
    _this.renderActionRegions = function(props) {
      var product = props.model;
      var otherProducts =
        _this.actionsParentsByMarketingAction.get(product.MarketingProduct) ||
        [];
      return React.createElement(
        MultiRelationFieldTags,
        tslib_1.__assign({}, props, {
          sortItemsBy: "Title",
          validate: hasUniqueRegions(product, otherProducts),
          validateItems: isUniqueRegion(product, otherProducts)
        })
      );
    };
    _this.renderActionParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            {
              Title: "Скидка на цену предложения, %",
              BaseParam: "SubscriptionFee",
              Modifiers: ["PercentDiscount"]
            },
            {
              Title: "Скидка на цену предложения, руб.",
              Unit: "rub",
              BaseParam: "SubscriptionFee",
              Modifiers: ["Discount"]
            }
          ]
        })
      );
    };
    _this.renderDevices = function(props) {
      var actionParent = props.model;
      var otherActions = _this.props.model.MarketingProduct.FixConnectActions;
      return React.createElement(
        RelationFieldTabs,
        tslib_1.__assign({}, props, {
          vertical: true,
          renderAllTabs: true,
          titleField: deviceTitleField,
          displayField: deviceDisplayField,
          fieldOrders: ["MarketingDevice", "Parent"],
          fieldEditors: {
            MarketingDevice: _this.renderMarketingDevice,
            Parent: _this.renderDeviceParent,
            FixConnectAction: IGNORE
          },
          validate: onlyOnePerRegionHasDevices(actionParent, otherActions),
          canClonePrototype: true,
          canRemoveEntity: true,
          onMountEntity: function(device) {
            return device.setTouched("MarketingDevice");
          }
        })
      );
    };
    _this.renderMarketingDevice = function(props) {
      var device = props.model;
      return React.createElement(
        SingleRelationFieldTags,
        tslib_1.__assign({}, props, {
          validate: hasUniqueMarketingDevice(device)
        })
      );
    };
    _this.renderDeviceParent = function(_a) {
      var model = _a.model,
        fieldSchema = _a.fieldSchema;
      var contentSchema = fieldSchema.RelatedContent;
      var productRelation = model.Parent;
      return (
        productRelation &&
        React.createElement(ArticleEditor, {
          model: productRelation,
          contentSchema: contentSchema,
          fieldOrders: ["Title", "Parameters"],
          fieldEditors: {
            Parameters: _this.renderDeviceParameters
          }
        })
      );
    };
    _this.renderDeviceParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" }
          ]
        })
      );
    };
    return _this;
  }
  Object.defineProperty(
    ActionsTab.prototype,
    "actionsParentsByMarketingAction",
    {
      get: function() {
        var result = new Map();
        this.props.model.MarketingProduct.FixConnectActions.forEach(function(
          action
        ) {
          var product = action.Parent;
          var marketingProduct = product.getBaseValue("MarketingProduct");
          var products = result.get(marketingProduct);
          if (products) {
            products.push(product);
          } else {
            result.set(marketingProduct, [product]);
          }
        });
        return result;
      },
      enumerable: true,
      configurable: true
    }
  );
  ActionsTab.prototype.pinActionToMarketingTariff = function(fixConnectAction) {
    var marketingTariff = this.props.model.MarketingProduct;
    if (!fixConnectAction.MarketingOffers.includes(marketingTariff)) {
      fixConnectAction.MarketingOffers.push(marketingTariff);
      fixConnectAction.setTouched("MarketingOffers");
    }
  };
  ActionsTab.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema;
    var marketingProductSchema =
      contentSchema.Fields.MarketingProduct.RelatedContent;
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(FilterBlock, { filterModel: this.filterModel }),
      React.createElement(Divider, null),
      React.createElement(ArticleEditor, {
        model: model.MarketingProduct,
        contentSchema: marketingProductSchema,
        skipOtherFields: true,
        fieldEditors: {
          FixConnectActions: this.renderActions
        }
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationContext)],
    ActionsTab.prototype,
    "publicationContext",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    ActionsTab.prototype,
    "actionsParentsByMarketingAction",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    ActionsTab.prototype,
    "pinActionToMarketingTariff",
    null
  );
  ActionsTab = tslib_1.__decorate([observer], ActionsTab);
  return ActionsTab;
})(Component);
export { ActionsTab };
var actionTitleDisplayField = function(action) {
  return (
    action.Parent &&
    action.Parent.MarketingProduct &&
    action.Parent.MarketingProduct.Title
  );
};
var actionRegionsDisplayField = function(action) {
  return (
    action.Parent &&
    React.createElement(
      "div",
      { className: "products-accordion__regions" },
      action.Parent.Regions.map(function(region) {
        return region.Title;
      }).join(", ")
    )
  );
};
var deviceDisplayField = function(device) {
  return device.MarketingDevice && device.MarketingDevice.Title;
};
var deviceTitleField = function(device) {
  return device.Parent && device.Parent.Title;
};
//# sourceMappingURL=ActionsTab.js.map
