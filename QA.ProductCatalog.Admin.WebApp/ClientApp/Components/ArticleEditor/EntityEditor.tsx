import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import { observer } from "mobx-react";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EntityController } from "Services/EntityController";
import { isString, isFunction } from "Utils/TypeChecks";
import { FieldSelector } from "Components/FieldEditors/AbstractFieldEditor";
import { EntityMenu } from "./EntityMenu";
import { EntityLink } from "./EntityLink";
import { AbstractEditor, ArticleEditorProps } from "./ArticleEditor";
import "./ArticleEditor.scss";
import { action } from "mobx";

interface EntityEditorProps extends ArticleEditorProps {
  /** Статья для редакторования */
  model: EntityObject;
  /** Класс для тела редактора статьи */
  className?: string;
  /** Заголовок редактора статьи */
  withHeader?: ReactNode | boolean;
  /** Автопозиционирование меню в заголовке */
  autoPositionMenu?: boolean;
  /** Поле статьи для отображения в заголовке */
  titleField?: string | FieldSelector;
  /** Разрешено ли сохранение статьи @default true */
  canSaveEntity?: boolean;
  /** Разрешено ли обновление статьи @default true */
  canRefreshEntity?: boolean;
  /** Разрешена ли перезагрузка статьи @default true */
  canReloadEntity?: boolean;
  /** Разрешено ли открепление статьи-связи */
  canDetachEntity?: boolean;
  /** Разрешено ли удаление статьи */
  canRemoveEntity?: boolean;
  /** Разрешена ли публикация статьи */
  canPublishEntity?: boolean;
  /** Разрешено ли клонирование статьи */
  canCloneEntity?: boolean;
  /**
   * Обработчик сохранения статьи
   * @param saveEntity Метод, выполняющий реальное сохранение
   */
  onSaveEntity?(entity: EntityObject, saveEntity: () => Promise<void>): void;
  /**
   * Обработчик обновления статьи
   * @param refreshEntity Метод, выполняющий реальное обновление
   */
  onRefreshEntity?(entity: EntityObject, refreshEntity: () => Promise<void>): void;
  /**
   * Обработчик перезагрузки статьи
   * @param reloadEntity Метод, выполняющий реальную перезагрузку
   */
  onReloadEntity?(entity: EntityObject, reloadEntity: () => Promise<void>): void;
  /**
   * Обработчик публикации статьи
   * @param publishEntity Метод, выполняющий реальную публикацию
   */
  onPublishEntity?(entity: EntityObject, publishEntity: () => Promise<void>): void;
  /** Обработчик удаления статьи */
  onRemoveEntity?(entity: EntityObject): void;
  /** Обработчик открепления статьи-связи */
  onDetachEntity?(entity: EntityObject): void;
  /** Обработчик клонирования статьи */
  onCloneEntity?(entity: EntityObject): void;
  /** Обработчик, выполняющийся при добавлении редактора в DOM */
  onMountEntity?(entity: EntityObject): void;
  /** Обработчик, выполняющийся при удалении редактора из DOM */
  onUnmountEntity?(entity: EntityObject): void;
  /** Render Callback для добавления дополнительных кнопок-действий в меню статьи */
  customActions?(entity: EntityObject): ReactNode;
  /** Render Callback для добавления дополнительной разметки в @see EntityEditor */
  children?: (entity: EntityObject, contentSchema: ContentSchema) => ReactNode;
}

const defaultEntityHandler = (_entity, action) => action();

/** Компонент для отображения и редактирования статьи-сущности */
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
        await this._entityController.saveEntity(model, contentSchema);
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
    const { model, onMountEntity } = this.props;
    if (onMountEntity) {
      onMountEntity(model);
    }
  }

  componentWillUnmount() {
    const { model, onUnmountEntity } = this.props;
    if (onUnmountEntity) {
      onUnmountEntity(model);
    }
  }

  render() {
    const { model, contentSchema, className, children } = this.props;
    return (
      <>
        {this.renderHeader()}
        <Col key={2} md className={className}>
          <Row>{super.render()}</Row>
          {isFunction(children) && <Row>{children(model, contentSchema)}</Row>}
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
      autoPositionMenu,
      customActions,
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
          autoPosition={autoPositionMenu}
          onSave={canSaveEntity && this.saveEntity}
          onDetach={canDetachEntity && this.detachEntity}
          onRemove={canRemoveEntity && hasServerId && this.removeEntity}
          onRefresh={canRefreshEntity && hasServerId && this.refreshEntity}
          onReload={canReloadEntity && hasServerId && this.reloadEntity}
          onPublish={canPublishEntity && hasServerId && this.publishEntity}
          onClone={canCloneEntity && hasServerId && this.cloneEntity}
        >
          {customActions && customActions(model)}
        </EntityMenu>
      </div>
    );
  }
}
