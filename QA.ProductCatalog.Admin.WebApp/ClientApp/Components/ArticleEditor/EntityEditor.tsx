import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { EntityController } from "Services/EntityController";
import { isString } from "Utils/TypeChecks";
import { FieldSelector } from "Components/FieldEditors/AbstractFieldEditor";
import { EntityMenu, EntityActionNodes, bindEntityActions } from "./EntityMenu";
import { EntityLink } from "./EntityLink";
import { AbstractEditor, ArticleEditorProps } from "./ArticleEditor";
import "./ArticleEditor.scss";
import { action } from "mobx";

interface EntityEditorProps extends ArticleEditorProps {
  model: EntityObject;
  className?: string;
  withHeader?: ReactNode | boolean;
  titleField?: string | FieldSelector;
  // allowed actions
  canSaveEntity?: boolean;
  canRefreshEntity?: boolean;
  canReloadEntity?: boolean;
  canDetachEntity?: boolean;
  canRemoveEntity?: boolean;
  canPublishEntity?: boolean;
  canCloneEntity?: boolean;
  onSaveEntity?(entity: EntityObject, saveEntity: () => Promise<void>): void;
  onRefreshEntity?(entity: EntityObject, refreshEntity: () => Promise<void>): void;
  onReloadEntity?(entity: EntityObject, reloadEntity: () => Promise<void>): void;
  onPublishEntity?(entity: EntityObject, publishEntity: () => Promise<void>): void;
  onRemoveEntity?(entity: EntityObject): void;
  onDetachEntity?(entity: EntityObject): void;
  onCloneEntity?(entity: EntityObject): void;
  onShowEntity?(entity: EntityObject): void;
  onHideEntity?(entity: EntityObject): void;
  entityActions?: EntityActionNodes;
}

const defaultEntityHandler = (_entity, action) => action();

@consumer
@observer
export class EntityEditor extends AbstractEditor<EntityEditorProps> {
  static defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    onSaveEntity: defaultEntityHandler,
    onRefreshEntity: defaultEntityHandler,
    onReloadEntity: defaultEntityHandler,
    onPublishEntity: defaultEntityHandler
  };

  @inject private _entityController: EntityController;
  private _titleField: (model: EntityObject) => string;

  constructor(props: EntityEditorProps, context?: any) {
    super(props, context);
    const { contentSchema, titleField = contentSchema.DisplayFieldName || (() => "") } = this.props;
    this._titleField = isString(titleField) ? entity => entity[titleField] : titleField;
  }

  private saveEntity = () => {
    const { model, contentSchema, onSaveEntity } = this.props;
    onSaveEntity(
      model,
      action("saveEntity", async () => {
        await this._entityController.saveEntitySubgraph(model, contentSchema);
      })
    );
  };

  private refreshEntity = () => {
    const { model, contentSchema, onRefreshEntity } = this.props;
    onRefreshEntity(
      model,
      action("refreshEntity", async () => {
        await this._entityController.refreshEntity(model, contentSchema);
      })
    );
  };

  private reloadEntity = async () => {
    const { model, contentSchema, onReloadEntity } = this.props;
    onReloadEntity(
      model,
      action("reloadEntity", async () => {
        await this._entityController.reloadEntity(model, contentSchema);
      })
    );
  };

  private publishEntity = () => {
    const { model, contentSchema, onPublishEntity } = this.props;
    onPublishEntity(
      model,
      action("publishEntity", async () => {
        await this._entityController.publishEntity(model, contentSchema);
      })
    );
  };

  private detachEntity = () => {
    const { model, onDetachEntity } = this.props;
    if (onDetachEntity) {
      onDetachEntity(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onDetach` is not defined");
    }
  };

  private removeEntity = () => {
    const { model, onRemoveEntity } = this.props;
    if (onRemoveEntity) {
      onRemoveEntity(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onRemove` is not defined");
    }
  };

  private cloneEntity = () => {
    const { model, onCloneEntity } = this.props;
    if (onCloneEntity) {
      onCloneEntity(model);
    } else if (DEBUG) {
      console.warn("EntityEditor `onClone` is not defined");
    }
  };

  componentDidMount() {
    const { model, onShowEntity } = this.props;
    if (onShowEntity) {
      onShowEntity(model);
    }
  }

  componentWillUnmount() {
    const { model, onHideEntity } = this.props;
    if (onHideEntity) {
      onHideEntity(model);
    }
  }

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
      entityActions,
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
          onSave={canSaveEntity && this.saveEntity}
          onDetach={canDetachEntity && this.detachEntity}
          onRemove={canRemoveEntity && hasServerId && this.removeEntity}
          onRefresh={canRefreshEntity && hasServerId && this.refreshEntity}
          onReload={canReloadEntity && hasServerId && this.reloadEntity}
          onPublish={canPublishEntity && hasServerId && this.publishEntity}
          onClone={canCloneEntity && hasServerId && this.cloneEntity}
        >
          {bindEntityActions(entityActions, model)}
        </EntityMenu>
      </div>
    );
  }
}
