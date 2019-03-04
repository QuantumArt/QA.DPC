import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { inject } from "react-ioc";
import { Divider } from "@blueprintjs/core";
import { IGNORE } from "Components/ArticleEditor/ArticleEditor";
import { ExtensionEditor } from "Components/ArticleEditor/ExtensionEditor";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import {
  RelationFieldTabs,
  RelationFieldAccordion,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { PublicationContext } from "Services/PublicationContext";
import { FilterModel } from "../Models/FilterModel";
import { hasUniqueRegions, isUniqueRegion } from "../Utils/ProductValidators";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { PublishButtons } from "./PublishButtons";
import { DevicesOnTariffsBlock } from "./DevicesOnTariffsBlock";
import { ActionsByDeviceBlock } from "./ActionsByDeviceBlock";
import { DevicesWarningBlock } from "./DevicesWarningBlock";
var DevicesTab = /** @class */ (function(_super) {
  tslib_1.__extends(DevicesTab, _super);
  function DevicesTab() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.filterModel = new FilterModel(_this.props.model);
    _this.renderMarketingDevices = function(props) {
      return React.createElement(
        RelationFieldTabs,
        tslib_1.__assign({}, props, {
          vertical: true,
          canSaveEntity: false,
          canRefreshEntity: false,
          displayField: "Title",
          fieldOrders: ["Products", "DevicesOnTariffs"],
          fieldEditors: {
            Title: IGNORE,
            Modifiers: IGNORE,
            Products: _this.renderDevices,
            DevicesOnTariffs: _this.renderDevicesOnTariffs
          }
        }),
        _this.renderActionsByDevice
      );
    };
    _this.renderDevices = function(props) {
      var fixConnectTariff = _this.props.model;
      var marketingDevice = props.model;
      var fieldSchema = props.fieldSchema;
      return React.createElement(
        React.Fragment,
        null,
        React.createElement(DevicesWarningBlock, {
          fixConnectTariff: fixConnectTariff,
          marketingDevice: marketingDevice
        }),
        React.createElement(RelationFieldAccordion, {
          model: marketingDevice,
          fieldSchema: fieldSchema,
          canCloneEntity: true,
          canRemoveEntity: true,
          canClonePrototype: true,
          columnProportions: [9, 3, 3, 1],
          displayFields: [
            regionsDisplayField,
            rentPriceDisplayField,
            salePriceDisplayField,
            function(device) {
              return React.createElement(PublicationStatusIcons, {
                model: device,
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
            Parameters: _this.renderParameters,
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
    _this.renderParameters = function(props) {
      return React.createElement(
        ParameterFields,
        tslib_1.__assign({}, props, {
          fields: [
            { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" },
            { Title: "Цена продажи", Unit: "rub", BaseParam: "SalePrice" }
          ]
        })
      );
    };
    _this.renderDevicesOnTariffs = function(props) {
      return React.createElement(
        DevicesOnTariffsBlock,
        tslib_1.__assign({}, props, {
          filterModel: _this.filterModel,
          marketingTariff: _this.props.model.MarketingProduct
        })
      );
    };
    _this.renderActionsByDevice = function(marketingDevice) {
      var marketingTariff = _this.props.model.MarketingProduct;
      var actionsFieldSchema =
        _this.props.contentSchema.Fields.MarketingProduct.RelatedContent.Fields
          .FixConnectActions;
      return React.createElement(ActionsByDeviceBlock, {
        marketingDevice: marketingDevice,
        marketingTariff: marketingTariff,
        actionsFieldSchema: actionsFieldSchema,
        filterModel: _this.filterModel
      });
    };
    return _this;
  }
  DevicesTab.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      contentSchema = _a.contentSchema;
    var extension =
      model.MarketingProduct.Type_Extension.MarketingFixConnectTariff;
    var extensionSchema =
      contentSchema.Fields.MarketingProduct.RelatedContent.Fields.Type
        .ExtensionContents.MarketingFixConnectTariff;
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(FilterBlock, {
        byMarketingTariff: true,
        filterModel: this.filterModel
      }),
      React.createElement(Divider, null),
      React.createElement(ExtensionEditor, {
        model: extension,
        contentSchema: extensionSchema,
        skipOtherFields: true,
        fieldEditors: {
          MarketingDevices: this.renderMarketingDevices
        }
      })
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationContext)],
    DevicesTab.prototype,
    "publicationContext",
    void 0
  );
  return DevicesTab;
})(Component);
export { DevicesTab };
var regionsDisplayField = function(device) {
  return React.createElement(
    "div",
    { className: "products-accordion__regions" },
    device.Regions.map(function(region) {
      return region.Title;
    }).join(", ")
  );
};
var rentPriceDisplayField = function(device) {
  var parameter = device.Parameters.find(function(parameter) {
    var baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "RentPrice";
  });
  return (
    parameter &&
    parameter.NumValue !== null &&
    React.createElement(
      React.Fragment,
      null,
      React.createElement(
        "div",
        null,
        "\u0426\u0435\u043D\u0430 \u0430\u0440\u0435\u043D\u0434\u044B:"
      ),
      React.createElement(
        "div",
        null,
        parameter.NumValue,
        " ",
        parameter.Unit && parameter.Unit.Title
      )
    )
  );
};
var salePriceDisplayField = function(device) {
  var parameter = device.Parameters.find(function(parameter) {
    var baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "SalePrice";
  });
  return (
    parameter &&
    parameter.NumValue !== null &&
    React.createElement(
      React.Fragment,
      null,
      React.createElement(
        "div",
        null,
        "\u0426\u0435\u043D\u0430 \u043F\u0440\u043E\u0434\u0430\u0436\u0438:"
      ),
      React.createElement(
        "div",
        null,
        parameter.NumValue,
        " ",
        parameter.Unit && parameter.Unit.Title
      )
    )
  );
};
//# sourceMappingURL=DevicesTab.js.map
