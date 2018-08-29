import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { SchemaContext } from "Services/SchemaContext";
import { EditorController } from "Services/EditorController";
import { ArticleController } from "Services/ArticleController";
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
  @inject private _articleController: ArticleController;
  @inject private _editorController: EditorController;
  @inject private _schemaContext: SchemaContext;
  private _titleField: (model: EntityObject) => string;

  constructor(props: ArticleEditorProps & EntityEditorProps, context?: any) {
    super(props, context);
    const { contentSchema, titleField = contentSchema.DisplayFieldName || (() => "") } = this.props;
    this._titleField = isString(titleField) ? article => article[titleField] : titleField;
  }

  private savePartialProduct = async () => {
    const { model, contentSchema } = this.props;
    await this._editorController.savePartialProduct(model, contentSchema);
  };

  private refreshEntity = async () => {
    const { model, contentSchema } = this.props;
    await this._articleController.refreshEntity(model, contentSchema);
  };

  private reloadEntity = async () => {
    const { model, contentSchema } = this.props;
    await this._articleController.reloadEntity(model, contentSchema);
  };

  render() {
    const { model, contentSchema, header, buttons, onRemove, children } = this.props;
    if (isFunction(children) && children.length === 0) {
      return children(null, null);
    }
    const hasServerId = model._ServerId > 0;

    const headerNode =
      header === true ? (
        <Col key={1} md className="article-editor__header">
          <div className="article-editor__title" title={contentSchema.ContentDescription}>
            {contentSchema.ContentTitle || contentSchema.ContentName}
            {hasServerId && `: (${model._ServerId})`} {this._titleField(model)}
          </div>
          {buttons === true ? (
            <div className="article-editor__buttons">
              <ArticleMenu
                onSave={this.savePartialProduct}
                onRemove={onRemove && (() => onRemove(model))}
                onRefresh={hasServerId && this.refreshEntity}
                onReload={hasServerId && this.reloadEntity}
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
