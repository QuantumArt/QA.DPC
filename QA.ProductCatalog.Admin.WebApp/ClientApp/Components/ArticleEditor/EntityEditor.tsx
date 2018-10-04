import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { ProductController } from "Services/ProductController";
import { EntityController } from "Services/EntityController";
import { isString } from "Utils/TypeChecks";
import { EntityMenu } from "./EntityMenu";
import { EntityLink } from "./EntityLink";
import { AbstractEditor, ArticleEditorProps } from "./ArticleEditor";
import "./ArticleEditor.scss";

interface EntityEditorProps extends ArticleEditorProps {
  model: EntityObject;
  className?: string;
  titleField?: string | ((entity: EntityObject) => string);
  withHeader?: ReactNode | boolean;
  onRemove?: (entity: EntityObject) => void;
  onDetach?: (entity: EntityObject) => void;
  onClone?: (entity: EntityObject) => void;
  // allowed actions
  canSaveEntity?: boolean;
  canRefreshEntity?: boolean;
  canReloadEntity?: boolean;
  canDetachEntity?: boolean;
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

  @inject private _entityController: EntityController;
  @inject private _productController: ProductController;
  private _titleField: (model: EntityObject) => string;

  constructor(props: EntityEditorProps, context?: any) {
    super(props, context);
    const { contentSchema, titleField = contentSchema.DisplayFieldName || (() => "") } = this.props;
    this._titleField = isString(titleField) ? entity => entity[titleField] : titleField;
  }

  private savePartialProduct = async () => {
    const { model, contentSchema } = this.props;
    await this._productController.savePartialProduct(model, contentSchema);
  };

  private refreshEntity = async () => {
    const { model, contentSchema } = this.props;
    await this._entityController.refreshEntity(model, contentSchema);
  };

  private reloadEntity = async () => {
    const { model, contentSchema } = this.props;
    await this._entityController.reloadEntity(model, contentSchema);
  };

  private detachEntity = () => {
    const { model, onDetach } = this.props;
    if (onDetach) {
      onDetach(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onDetach` is not defined");
    }
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
      <Col key={1} md className="entity-editor__header">
        <div
          className="entity-editor__title"
          title={
            contentSchema.ContentDescription ||
            contentSchema.ContentTitle ||
            contentSchema.ContentName
          }
        >
          <EntityLink model={model} contentSchema={contentSchema} />
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
      canDetachEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity
    } = this.props;
    const hasServerId = model._ServerId > 0;

    return (
      <div className="entity-editor__buttons">
        <EntityMenu
          onSave={canSaveEntity && this.savePartialProduct}
          onDetach={canDetachEntity && this.detachEntity}
          onRemove={canRemoveEntity && hasServerId && this.removeEntity}
          onRefresh={canRefreshEntity && hasServerId && this.refreshEntity}
          onReload={canReloadEntity && hasServerId && this.reloadEntity}
          onPublish={canPublishEntity && hasServerId && this.publishEntity}
          onClone={canCloneEntity && hasServerId && this.cloneEntity}
        />
      </div>
    );
  }
}
