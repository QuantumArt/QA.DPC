import React from "react";
import { Col } from "react-flexbox-grid";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema, MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { RelationSelection } from "Models/RelationSelection";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи
// TODO: Валидация required, maxCount
// TODO: передача RelationFieldEditorProps в ArticleEditor или ArticleEditor как render prop ?

interface RelationFieldEditorProps extends FieldEditorProps {
  fields?: FieldsConfig;
  save?: boolean;
  saveRelations?: RelationSelection;
}

@observer
export class SingleRelationEditor extends AbstractFieldEditor<RelationFieldEditorProps> {
  renderField(_model: ArticleObject | ExtensionObject, _fieldSchema: SingleRelationFieldSchema) {
    return (
      <Col xl={8} md={6}>
        TODO: таблица-аккордеон
      </Col>
    );
  }
}

@observer
export class MultiRelationEditor extends AbstractFieldEditor<RelationFieldEditorProps> {
  renderField(_model: ArticleObject | ExtensionObject, _fieldSchema: MultiRelationFieldSchema) {
    return (
      <Col xl={8} md={6}>
        TODO: таблица-аккордеон
      </Col>
    );
  }
}
