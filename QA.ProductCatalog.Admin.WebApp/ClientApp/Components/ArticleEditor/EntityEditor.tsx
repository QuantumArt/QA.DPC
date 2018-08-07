import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { SchemaContext } from "Services/SchemaContext";
import { EditorController } from "Services/EditorController";
import { isString, isFunction } from "Utils/TypeChecks";
import { ArticleMenu } from "./ArticleMenu";
import { ArticleEditor, ArticleEditorProps } from "./ArticleEditor";
export { IGNORE, FieldsConfig, RelationsConfig } from "./ArticleEditor";
import "./ArticleEditor.scss";

export type RenderEntity = (headerNode: ReactNode, fieldsNode: ReactNode) => ReactNode;

interface EntityEditorProps {
  model: EntityObject;
  header?: ReactNode | boolean;
  buttons?: ReactNode | boolean;
  onRemove?: (article: EntityObject) => void;
  titleField?: string | ((article: EntityObject) => string);
  children?: RenderEntity | ReactNode;
}

@consumer
@observer
export class EntityEditor extends ArticleEditor<EntityEditorProps> {
  @inject private _editorController: EditorController;
  @inject private _schemaContext: SchemaContext;
  private _titleField: (model: EntityObject) => string;

  constructor(props: ArticleEditorProps & EntityEditorProps, context?: any) {
    super(props, context);
    const { contentSchema, titleField = contentSchema.DisplayFieldName || (() => "") } = this.props;
    this._titleField = isString(titleField) ? article => article[titleField] : titleField;
  }

  hadleSaveAll = () => {
    const { model, contentSchema } = this.props;
    this._editorController.savePartialProduct(model, contentSchema);
  };

  render() {
    const { model, contentSchema, header, buttons, onRemove, children } = this.props;
    if (isFunction(children) && children.length === 0) {
      return children(null, null);
    }
    const showSaveButton = model._ServerId > 0 || this._schemaContext.rootSchema === contentSchema;

    const headerNode =
      header === true ? (
        <Col key={1} md className="article-editor__header">
          <div className="article-editor__title" title={contentSchema.ContentDescription}>
            {contentSchema.ContentTitle || contentSchema.ContentName}
            {model._ServerId > 0 && `: (${model._ServerId})`} {this._titleField(model)}
          </div>
          {buttons === true ? (
            <div className="article-editor__buttons">
              <ArticleMenu
                onSave={showSaveButton && (() => {})}
                onSaveAll={showSaveButton && this.hadleSaveAll}
                onRemove={onRemove && (() => onRemove(model))}
                onRefresh={() => {}} // TODO: refersh PartialProduct
                onClone={() => {}} // TODO: clone PartialProduct
                onPublish={() => {}} // TODO: publish PartialProduct
              />
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
