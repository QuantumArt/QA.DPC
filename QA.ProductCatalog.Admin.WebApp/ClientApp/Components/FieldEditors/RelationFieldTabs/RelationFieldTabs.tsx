import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { RelationFieldTabsProps } from "./AbstractRelationFieldTabs";
import { SingleRelationFieldTabs } from "./SingleRelationFieldTabs";
import { MultiRelationFieldTabs } from "./MultiRelationFieldTabs";
export { SingleRelationFieldTabs, MultiRelationFieldTabs };

export class RelationFieldTabs extends Component<RelationFieldTabsProps> {
  FieldTabs = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldTabs
    : MultiRelationFieldTabs;

  render() {
    return <this.FieldTabs {...this.props} />;
  }
}
