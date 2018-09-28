import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { EditorController } from "Services/EditorController";
import { ArticleController } from "Services/ArticleController";
import { isString } from "Utils/TypeChecks";
import { ArticleMenu } from "./ArticleMenu";
import { ArticleLink } from "./ArticleLink";
import { AbstractEditor, ArticleEditorProps } from "./ArticleEditor";
import "./ArticleEditor.scss";

interface EntityEditorProps extends ArticleEditorProps {
  model: EntityObject;
  header?: ReactNode | boolean;
  buttons?: ReactNode | boolean;
  className?: string;
  titleField?: string | ((article: EntityObject) => string);
  onRemove?: (article: EntityObject) => void;
}

@consumer
@observer
export class EntityEditor extends AbstractEditor<EntityEditorProps> {
  @inject private _articleController: ArticleController;
  @inject private _editorController: EditorController;
  private _titleField: (model: EntityObject) => string;

  constructor(props: EntityEditorProps, context?: any) {
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
    const { className } = this.props;
    return (
      <>
        {this.renderHeader()}
        <Col key={2} md className={className}>
          <Row>{super.render()}</Row>
        </Col>
      </>
    );
  }

  private renderHeader() {
    const { model, contentSchema, header } = this.props;

    return header === true ? (
      <Col key={1} md className="article-editor__header">
        <div
          className="article-editor__title"
          title={
            contentSchema.ContentDescription ||
            contentSchema.ContentTitle ||
            contentSchema.ContentName
          }
        >
          <ArticleLink model={model} contentSchema={contentSchema} />
          {this._titleField(model)}
        </div>
        {this.renderButtons()}
      </Col>
    ) : (
      header || null
    );
  }

  private renderButtons() {
    const { model, buttons, onRemove } = this.props;
    const hasServerId = model._ServerId > 0;

    return buttons === true ? (
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
    );
  }
}
