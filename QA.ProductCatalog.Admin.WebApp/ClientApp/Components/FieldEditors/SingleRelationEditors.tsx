import React from "react";
import { Col } from "react-flexbox-grid";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { AbstractFieldEditor } from "./AbstractEditors";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { RelationSelection } from "Models/RelationSelection";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи
// TODO: Валидация required
// TODO: передача SingleRelationEditorProps в ArticleEditor или ArticleEditor как render prop ?

interface SingleRelationEditorProps {
  fields?: FieldsConfig;
  save?: boolean;
  saveRelations: RelationSelection;
}

@observer
export class SingleRelationEditor extends AbstractFieldEditor<
  SingleRelationFieldSchema,
  SingleRelationEditorProps
> {
  renderField(_model: ArticleObject | ExtensionObject, _fieldSchema: SingleRelationFieldSchema) {
    return (
      <Col xl={8} md={6}>
        TODO: таблица-аккордеон
      </Col>
    );
  }
}

// TODO: SingleRelationTable - таблица из одной нередактируемой статьи
// TODO: SingleRelationList - статья в одну строку вида: статья_a [x]
