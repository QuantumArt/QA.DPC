import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon } from "@blueprintjs/core";
import { FederalTab } from "./FederalTab";
import { RegionalTab } from "./RegionalTab";
import { DevicesTab } from "./DevicesTab";
import { ActionsTab } from "./ActionsTab";
var EditorTabs = /** @class */ (function(_super) {
  tslib_1.__extends(EditorTabs, _super);
  function EditorTabs() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.activatedTabIds = ["federal"];
    _this.handleTabChange = function(newTabId) {
      if (!_this.activatedTabIds.includes(newTabId)) {
        _this.activatedTabIds.push(newTabId);
      }
    };
    return _this;
  }
  EditorTabs.prototype.render = function() {
    return React.createElement(
      Tabs,
      { id: "layout", large: true, onChange: this.handleTabChange },
      React.createElement(
        Tab,
        {
          id: "federal",
          panel:
            this.activatedTabIds.includes("federal") &&
            React.createElement(FederalTab, tslib_1.__assign({}, this.props))
        },
        React.createElement(Icon, { icon: "globe", iconSize: Icon.SIZE_LARGE }),
        React.createElement(
          "span",
          null,
          "\u041E\u0431\u0449\u0435\u0444\u0435\u0434\u0435\u0440\u0430\u043B\u044C\u043D\u044B\u0435 \u0445\u0430\u0440\u0430\u043A\u0442\u0435\u0440\u0438\u0441\u0442\u0438\u043A\u0438"
        )
      ),
      React.createElement(
        Tab,
        {
          id: "regional",
          panel:
            this.activatedTabIds.includes("regional") &&
            React.createElement(RegionalTab, tslib_1.__assign({}, this.props))
        },
        React.createElement(Icon, {
          icon: "locate",
          iconSize: Icon.SIZE_LARGE
        }),
        React.createElement(
          "span",
          null,
          "\u0420\u0435\u0433\u0438\u043E\u043D\u0430\u043B\u044C\u043D\u044B\u0435 \u0445\u0430\u0440\u0430\u043A\u0442\u0435\u0440\u0438\u0441\u0442\u0438\u043A\u0438"
        )
      ),
      React.createElement(
        Tab,
        {
          id: "actions",
          panel:
            this.activatedTabIds.includes("actions") &&
            React.createElement(ActionsTab, tslib_1.__assign({}, this.props))
        },
        React.createElement(Icon, { icon: "flag", iconSize: Icon.SIZE_LARGE }),
        React.createElement(
          "span",
          null,
          "\u0414\u0435\u0439\u0441\u0442\u0432\u0443\u044E\u0449\u0438\u0435 \u0430\u043A\u0446\u0438\u0438"
        )
      ),
      React.createElement(
        Tab,
        {
          id: "devices",
          panel:
            this.activatedTabIds.includes("devices") &&
            React.createElement(DevicesTab, tslib_1.__assign({}, this.props))
        },
        React.createElement(Icon, {
          icon: "projects",
          iconSize: Icon.SIZE_LARGE
        }),
        React.createElement(
          "span",
          null,
          "\u041E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u0435"
        )
      )
    );
  };
  tslib_1.__decorate(
    [observable, tslib_1.__metadata("design:type", Array)],
    EditorTabs.prototype,
    "activatedTabIds",
    void 0
  );
  tslib_1.__decorate(
    [action, tslib_1.__metadata("design:type", Object)],
    EditorTabs.prototype,
    "handleTabChange",
    void 0
  );
  EditorTabs = tslib_1.__decorate([observer], EditorTabs);
  return EditorTabs;
})(Component);
export { EditorTabs };
//# sourceMappingURL=EditorTabs.js.map
