import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { RelationFieldAccordionProps } from "./AbstractRelationFieldAccordion";
import { SingleRelationFieldAccordion } from "./SingleRelationFieldAccordion";
import { MultiRelationFieldAccordion } from "./MultiRelationFieldAccordion";
export { SingleRelationFieldAccordion, MultiRelationFieldAccordion };

export class RelationFieldAccordion extends Component<RelationFieldAccordionProps> {
  FieldAccordion = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldAccordion
    : MultiRelationFieldAccordion;

  render() {
    return <this.FieldAccordion {...this.props} />;
  }
}
