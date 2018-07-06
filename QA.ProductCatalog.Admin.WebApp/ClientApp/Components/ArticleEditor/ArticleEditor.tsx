import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { Button, Intent } from "@blueprintjs/core";
import { inject, consumer } from "react-ioc";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import { RelationSelection, validateRelationSelection } from "Models/RelationSelection";
import { DataSerializer } from "Services/DataSerializer";
import { isString, isFunction } from "Utils/TypeChecks";
import { ObjectEditor, ObjectEditorProps } from "./ObjectEditor";
export { IGNORE, FieldsConfig, RelationsConfig } from "./ObjectEditor";
import "./ArticleEditor.scss";

export type RenderArticle = (headerNode: ReactNode, fieldsNode: ReactNode) => ReactNode;

interface ArticleEditorProps {
  model: ArticleObject;
  header?: ReactNode | boolean;
  buttons?: ReactNode | boolean;
  onRemove?: (article: ArticleObject) => void;
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
    if (DEBUG && saveRelations) {
      validateRelationSelection(contentSchema, saveRelations);
    }
    this._titleField = isString(titleField)
      ? titleField === "Id"
        ? () => ""
        : article => article[titleField]
      : titleField;
  }

  render() {
    const { model, contentSchema, header, buttons, onRemove, children } = this.props;
    if (isFunction(children) && children.length === 0) {
      return children(null, null);
    }
    const serverId = this._dataSerializer.getServerId(model);
    const headerNode =
      header === true ? (
        <Col key={1} md className="article-editor__header">
          <div className="article-editor__title" title={contentSchema.ContentDescription}>
            {contentSchema.ContentTitle || contentSchema.ContentName}
            {serverId > 0 && `: (${serverId})`} {this._titleField(model)}
          </div>
          {buttons === true ? (
            <div className="article-editor__buttons">
              <Button minimal rightIcon="floppy-disk" intent={Intent.PRIMARY}>
                Сохранить
              </Button>
              {onRemove && (
                <Button
                  minimal
                  rightIcon="remove"
                  intent={Intent.DANGER}
                  onClick={() => onRemove(model)}
                >
                  Удалить
                </Button>
              )}
            </div>
          ) : (
            buttons || null
          )}
        </Col>
      ) : (
        header || null
      );
    const fieldsNode = (
      <Col key={2} md>
        <Row>{super.render()}</Row>
      </Col>
    );
    return isFunction(children) ? children(headerNode, fieldsNode) : [headerNode, fieldsNode];
  }
}
