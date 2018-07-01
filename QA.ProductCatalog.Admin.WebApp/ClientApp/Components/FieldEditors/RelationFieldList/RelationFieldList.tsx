import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { RelationFieldListProps } from "./AbstractRelationFieldList";
import { SingleRelationFieldList } from "./SingleRelationFieldList";
import { MultiRelationFieldList } from "./MultiRelationFieldList";
export { SingleRelationFieldList, MultiRelationFieldList };

export class RelationFieldList extends Component<RelationFieldListProps> {
  FieldList = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldList
    : MultiRelationFieldList;

  render() {
    return <this.FieldList {...this.props} />;
  }
}
