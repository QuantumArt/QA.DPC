import React, { Component } from "react";
import { isSingleRelationField } from "Models/EditorSchemaModels";
import { RelationFieldTagsProps } from "./AbstractRelationFieldTags";
import { SingleRelationFieldTags } from "./SingleRelationFieldTags";
import { MultiRelationFieldTags } from "./MultiRelationFieldTags";
export { SingleRelationFieldTags, MultiRelationFieldTags };

/** Отображение поля-связи в виде списка тегов */
export class RelationFieldTags extends Component<RelationFieldTagsProps> {
  FieldList = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldTags
    : MultiRelationFieldTags;

  render() {
    return <this.FieldList {...this.props} />;
  }
}
