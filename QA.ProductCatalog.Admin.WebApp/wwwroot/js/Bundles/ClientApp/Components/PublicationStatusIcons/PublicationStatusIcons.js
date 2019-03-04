import * as tslib_1 from "tslib";
import React, { Component } from "react";
import cn from "classnames";
import { observer } from "mobx-react";
import "./PublicationStatusIcons.scss";
var PublicationStatusIcons = /** @class */ (function(_super) {
  tslib_1.__extends(PublicationStatusIcons, _super);
  function PublicationStatusIcons() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  PublicationStatusIcons.prototype.render = function() {
    var _a = this.props,
      model = _a.model,
      _b = _a.product,
      product = _b === void 0 ? model : _b,
      contentSchema = _a.contentSchema,
      publicationContext = _a.publicationContext;
    var lastModified = contentSchema.getLastModified(model);
    var liveTime = publicationContext.getLiveTime(product);
    var stageTime = publicationContext.getStageTime(product);
    var liveIsSync = liveTime && liveTime >= lastModified;
    var stageIsSync = stageTime && stageTime >= lastModified;
    return React.createElement(
      React.Fragment,
      null,
      stageTime &&
        React.createElement(
          "div",
          {
            className: cn("publication-status-icon", {
              "publication-status-icon--sync": stageIsSync
            }),
            title: stageIsSync
              ? "Опубликовано на STAGE"
              : "Требуется публикация на STAGE"
          },
          "S"
        ),
      liveTime &&
        React.createElement(
          "div",
          {
            className: cn("publication-status-icon", {
              "publication-status-icon--sync": liveIsSync
            }),
            title: liveIsSync
              ? "Опубликовано на LIVE"
              : "Требуется публикация на LIVE"
          },
          "L"
        )
    );
  };
  PublicationStatusIcons = tslib_1.__decorate(
    [observer],
    PublicationStatusIcons
  );
  return PublicationStatusIcons;
})(Component);
export { PublicationStatusIcons };
//# sourceMappingURL=PublicationStatusIcons.js.map
