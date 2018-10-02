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
  className?: string;
  titleField?: string | ((article: EntityObject) => string);
  withHeader?: ReactNode | boolean;
  onRemove?: (article: EntityObject) => void;
  onClone?: (article: EntityObject) => void;
  // allowed actions
  canSaveEntity?: boolean;
  canRefreshEntity?: boolean;
  canReloadEntity?: boolean;
  canRemoveEntity?: boolean;
  canPublishEntity?: boolean;
  canCloneEntity?: boolean;
}

@consumer
@observer
export class EntityEditor extends AbstractEditor<EntityEditorProps> {
  static defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true
  };

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

  private removeEntity = () => {
    const { model, onRemove } = this.props;
    if (onRemove) {
      onRemove(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onRemove` is not defined");
    }
  };

  private cloneEntity = () => {
    const { model, onClone } = this.props;
    if (onClone) {
      onClone(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onClone` is not defined");
    }
  };

  private publishEntity = () => {
    alert("TODO: публикация");
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
    const { model, contentSchema, withHeader } = this.props;

    return withHeader === true ? (
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
      withHeader || null
    );
  }

  private renderButtons() {
    const {
      model,
      canSaveEntity,
      canRefreshEntity,
      canReloadEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const hasServerId = model._ServerId > 0;

    return (
      <div className="article-editor__buttons">
        <ArticleMenu
          onSave={canSaveEntity && this.savePartialProduct}
          onRemove={canRemoveEntity && this.removeEntity}
          onRefresh={canRefreshEntity && hasServerId && this.refreshEntity}
          onReload={canReloadEntity && hasServerId && this.reloadEntity}
          onPublish={canPublishEntity && hasServerId && this.publishEntity}
          onClone={canCloneEntity && hasServerId && this.cloneEntity}
        />
      </div>
    );
  }
}
