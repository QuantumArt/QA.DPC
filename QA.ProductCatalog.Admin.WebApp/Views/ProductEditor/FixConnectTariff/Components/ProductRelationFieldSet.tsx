import React from "react";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { FieldEditorProps } from "Components/FieldEditors/AbstractFieldEditor";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";

export const ProductRelationFieldSet = ({ model, fieldSchema }: FieldEditorProps) => {
  const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
  const productRelation = model[fieldSchema.FieldName];
  return (
    productRelation && (
      <ArticleEditor
        model={productRelation}
        contentSchema={contentSchema}
        fieldOrders={["Title", "Parameters"]}
      />
    )
  );
};
