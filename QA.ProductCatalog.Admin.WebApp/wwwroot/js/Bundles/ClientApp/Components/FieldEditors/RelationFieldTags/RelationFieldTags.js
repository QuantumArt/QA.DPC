import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { SingleRelationFieldTags } from "./SingleRelationFieldTags";
import { MultiRelationFieldTags } from "./MultiRelationFieldTags";
export { SingleRelationFieldTags, MultiRelationFieldTags };
/** Отображение поля-связи в виде списка тегов */
var RelationFieldTags = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldTags, _super);
  function RelationFieldTags() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.FieldList = isSingleRelationField(_this.props.fieldSchema)
      ? SingleRelationFieldTags
      : MultiRelationFieldTags;
    return _this;
  }
  RelationFieldTags.prototype.render = function() {
    return React.createElement(
      this.FieldList,
      tslib_1.__assign({}, this.props)
    );
  };
  return RelationFieldTags;
})(Component);
export { RelationFieldTags };
//# sourceMappingURL=RelationFieldTags.js.map
