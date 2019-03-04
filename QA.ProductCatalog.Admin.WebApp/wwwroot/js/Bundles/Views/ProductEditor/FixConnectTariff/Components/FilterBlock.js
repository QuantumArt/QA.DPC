import * as tslib_1 from "tslib";
import React, { Component } from "react";
import ReactSelect from "react-select";
import { inject } from "react-ioc";
import { computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Switch, Alignment } from "@blueprintjs/core";
import { DataContext } from "Services/DataContext";
var FilterBlock = /** @class */ (function(_super) {
  tslib_1.__extends(FilterBlock, _super);
  function FilterBlock() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.handleRegionsFilterChange = function(selection) {
      _this.props.filterModel.setSelectedRegionIds(
        selection.map(function(option) {
          return option.value;
        })
      );
    };
    return _this;
  }
  Object.defineProperty(FilterBlock.prototype, "options", {
    get: function() {
      var e_1, _a;
      var options = [];
      try {
        for (
          var _b = tslib_1.__values(this._dataContext.tables.Region.values()),
            _c = _b.next();
          !_c.done;
          _c = _b.next()
        ) {
          var region = _c.value;
          options.push({
            value: region._ClientId,
            label: region.Title
          });
        }
      } catch (e_1_1) {
        e_1 = { error: e_1_1 };
      } finally {
        try {
          if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
        } finally {
          if (e_1) throw e_1.error;
        }
      }
      return options;
    },
    enumerable: true,
    configurable: true
  });
  FilterBlock.prototype.render = function() {
    var _a = this.props,
      filterModel = _a.filterModel,
      byMarketingTariff = _a.byMarketingTariff;
    return React.createElement(
      Col,
      { md: true, className: "devices-filter" },
      React.createElement(
        Row,
        null,
        React.createElement(
          Col,
          { md: 3 },
          React.createElement(
            Switch,
            {
              large: true,
              alignIndicator: Alignment.RIGHT,
              checked: filterModel.filterByTariffRegions,
              label: "",
              onChange: filterModel.toggleFilterByTariffRegions
            },
            "\u0424\u0438\u043B\u044C\u0442\u0440\u043E\u0432\u0430\u0442\u044C \u043F\u043E \u0440\u0435\u0433\u0438\u043E\u043D\u0430\u043C ",
            React.createElement("br", null),
            " \u0442\u0430\u0440\u0438\u0444\u0430 \u0444\u0438\u043A\u0441\u0438\u0440\u043E\u0432\u0430\u043D\u043D\u043E\u0439 \u0441\u0432\u044F\u0437\u0438"
          )
        ),
        React.createElement(
          Col,
          { md: 6, mdOffset: byMarketingTariff ? 0 : 3 },
          React.createElement(ReactSelect, {
            multi: true,
            clearable: true,
            className: "Select--large",
            placeholder:
              "\u0424\u0438\u043B\u044C\u0442\u0440\u043E\u0432\u0430\u0442\u044C \u043F\u043E \u0440\u0435\u0433\u0438\u043E\u043D\u0430\u043C",
            noResultsText:
              "\u041D\u0438\u0447\u0435\u0433\u043E \u043D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u043E",
            options: this.options,
            value: filterModel.selectedRegionIds,
            onChange: this.handleRegionsFilterChange
          })
        ),
        byMarketingTariff &&
          React.createElement(
            Col,
            { md: 3 },
            React.createElement(
              Switch,
              {
                large: true,
                alignIndicator: Alignment.RIGHT,
                checked: filterModel.filterByMarketingTariff,
                onChange: filterModel.toggleFilterByMarketingTariff
              },
              "\u0424\u0438\u043B\u044C\u0442\u0440\u043E\u0432\u0430\u0442\u044C \u043F\u043E \u043C\u0430\u0440\u043A\u0435\u0442\u0438\u043D\u0433\u043E\u0432\u043E\u043C\u0443 ",
              React.createElement("br", null),
              " \u0442\u0430\u0440\u0438\u0444\u0443 \u0444\u0438\u043A\u0441\u0438\u0440\u043E\u0432\u0430\u043D\u043D\u043E\u0439 \u0441\u0432\u044F\u0437\u0438"
            )
          )
      )
    );
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    FilterBlock.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    FilterBlock.prototype,
    "options",
    null
  );
  FilterBlock = tslib_1.__decorate([observer], FilterBlock);
  return FilterBlock;
})(Component);
export { FilterBlock };
//# sourceMappingURL=FilterBlock.js.map
