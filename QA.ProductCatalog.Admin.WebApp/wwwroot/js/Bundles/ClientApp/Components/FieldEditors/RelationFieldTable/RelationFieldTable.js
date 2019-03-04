import * as tslib_1 from "tslib";
import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { SingleRelationFieldTable } from "./SingleRelationFieldTable";
import { MultiRelationFieldTable } from "./MultiRelationFieldTable";
export { SingleRelationFieldTable, MultiRelationFieldTable };
/** Отображение поля-связи в виде таблицы */
var RelationFieldTable = /** @class */ (function(_super) {
  tslib_1.__extends(RelationFieldTable, _super);
  function RelationFieldTable() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.FieldTable = isSingleRelationField(_this.props.fieldSchema)
      ? SingleRelationFieldTable
      : MultiRelationFieldTable;
    return _this;
  }
  RelationFieldTable.prototype.render = function() {
    return React.createElement(
      this.FieldTable,
      tslib_1.__assign({}, this.props)
    );
  };
  return RelationFieldTable;
})(Component);
export { RelationFieldTable };
//# sourceMappingURL=RelationFieldTable.js.map
