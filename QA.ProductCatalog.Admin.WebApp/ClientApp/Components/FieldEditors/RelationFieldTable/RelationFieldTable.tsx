import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { RelationFieldTableProps } from "./AbstractRelationFieldTable";
import { SingleRelationFieldTable } from "./SingleRelationFieldTable";
import { MultiRelationFieldTable } from "./MultiRelationFieldTable";
export { SingleRelationFieldTable, MultiRelationFieldTable };

/** Отображение поля-связи в виде таблицы */
export class RelationFieldTable extends Component<RelationFieldTableProps> {
  FieldTable = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldTable
    : MultiRelationFieldTable;

  render() {
    return <this.FieldTable {...this.props} />;
  }
}
