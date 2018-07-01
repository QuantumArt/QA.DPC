import React, { ReactNode } from "react";
import { Row } from "react-flexbox-grid";
import { Button } from "@blueprintjs/core";
import { inject, consumer } from "react-ioc";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import { RelationSelection, validateRelationSelection } from "Models/RelationSelection";
import { DataSerializer } from "Services/DataSerializer";
import { isString, isFunction } from "Utils/TypeChecks";
import { ObjectEditor, ObjectEditorProps } from "./ObjectEditor";
export { IGNORE, FieldsConfig } from "./ObjectEditor";
import "./ArticleEditor.scss";

export type RenderArticle = (headerNode: ReactNode, fieldsNode: ReactNode) => ReactNode;

interface ArticleEditorProps {
  model: ArticleObject;
  save?: boolean;
  saveRelations?: RelationSelection;
  titleField?: string | ((article: ArticleObject) => string);
  children?: RenderArticle | ReactNode;
}

@consumer
@observer
export class ArticleEditor extends ObjectEditor<ArticleEditorProps> {
  @inject private _dataSerializer: DataSerializer;
  private _titleField: (model: ArticleObject) => string;

  constructor(props: ObjectEditorProps & ArticleEditorProps, context?: any) {
    super(props, context);
    const {
      contentSchema,
      saveRelations,
      titleField = contentSchema.DisplayFieldName || "Id"
    } = this.props;
    if (saveRelations) {
      validateRelationSelection(contentSchema, saveRelations);
    }
    this._titleField = isString(titleField)
      ? titleField === "Id"
        ? () => ""
        : article => article[titleField]
      : titleField;
  }

  render() {
    const { model, contentSchema, save, children } = this.props;
    const serverId = this._dataSerializer.getServerId(model);
    const headerNode = save && (
      <Row key={1} className="article-editor__header">
        <div className="article-editor__title" title={contentSchema.ContentDescription}>
          {contentSchema.ContentTitle || contentSchema.ContentName}
          {serverId > 0 && `: (${serverId})`} {this._titleField(model)}
        </div>
        <Button icon="floppy-disk">Сохранить</Button>
      </Row>
    );
    const fieldsNode = <Row key={2}>{super.render()}</Row>;
    return isFunction(children) ? children(headerNode, fieldsNode) : [headerNode, fieldsNode];
  }
}
