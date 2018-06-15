import React from "react";
import { Col } from "react-flexbox-grid";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { AbstractFieldEditor } from "./AbstractEditors";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { RelationSelection } from "Models/RelationSelection";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи
// TODO: Валидация required, maxCount
// TODO: передача MultiRelationEditorProps в ArticleEditor или ArticleEditor как render prop ?

interface MultiRelationEditorProps {
  fields?: FieldsConfig;
  save?: boolean;
  saveRelations: RelationSelection;
}

@observer
export class MultiRelationEditor extends AbstractFieldEditor<
  MultiRelationFieldSchema,
  MultiRelationEditorProps
> {
  renderField(_model: ArticleObject | ExtensionObject, _fieldSchema: MultiRelationFieldSchema) {
    return (
      <Col xl={8} md={6}>
        TODO: таблица-аккордеон
      </Col>
    );
  }
}

// TODO: MultiRelationTable - таблица нередактируемых статей
// TODO: MultiRelationList - список выбранных статей в одну строку вида: статья_a [x] статья_б [x]
