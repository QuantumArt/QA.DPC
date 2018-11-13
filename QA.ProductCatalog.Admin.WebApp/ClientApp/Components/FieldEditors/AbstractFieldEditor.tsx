import React, { Component, ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { Validator, Validate } from "mst-validation-mixin";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import {
  FieldSchema,
  RelationFieldSchema,
  UpdatingMode,
  FieldExactTypes,
  MultiRelationFieldSchema
} from "Models/EditorSchemaModels";
import { FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { RelationController } from "Services/RelationController";
import { isArray, isString } from "Utils/TypeChecks";
import { required } from "Utils/Validators";
import { newUid } from "Utils/Common";
import "./FieldEditors.scss";
import { asc } from "Utils/Array";

/** Props для базового редактора произвольного поля */
export interface FieldEditorProps {
  /** Статья, содержащая поле */
  model: ArticleObject;
  /** Схема поля */
  fieldSchema: FieldSchema;
  /** Валидаторы поля */
  validate?: Validator | Validator[];
  /** Поле доступно только для чтения */
  readonly?: boolean;
}

/** Абстрактный компонент для редактирования произвольного поля */
export abstract class AbstractFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends Component<P> {
  protected _id = `_${newUid()}`;
  protected _readonly = this.props.readonly || this.props.fieldSchema.IsReadOnly;

  /** Отображение поля ввода */
  protected abstract renderField(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  /** Отображение блока валидации */
  protected renderValidation(model: ArticleObject, fieldSchema: FieldSchema): ReactNode {
    const { validate } = this.props;
    const rules = [];
    if (validate) {
      if (isArray(validate)) {
        rules.push(...validate);
      } else {
        rules.push(validate);
      }
    }
    if (fieldSchema.IsRequired) {
      rules.push(required);
    }
    return (
      <Validate
        model={model}
        name={fieldSchema.FieldName}
        errorClassName="bp3-form-helper-text"
        rules={rules}
      />
    );
  }

  /** Отображение названия поля */
  protected renderLabel(model: ArticleObject, fieldSchema: FieldSchema) {
    return (
      <label htmlFor={this._id} title={fieldSchema.FieldName}>
        {fieldSchema.IsRequired && <span className="field-editor__label-required">*&nbsp;</span>}
        <span
          className={cn("field-editor__label-text", {
            "field-editor__label-text--edited": model.isEdited(fieldSchema.FieldName),
            "field-editor__label-text--invalid": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        >
          {fieldSchema.FieldTitle || fieldSchema.FieldName}:
        </span>
        {fieldSchema.FieldDescription && (
          <>
            &nbsp;<Icon icon="help" title={fieldSchema.FieldDescription} />
          </>
        )}
      </label>
    );
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={6}
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={4} md={3} className="field-editor__label">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={4} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}

/** Функция выбора поля статьи */
export type FieldSelector<T = ReactNode> = (model: ArticleObject) => T;
/** Функция сравнения статей */
export type EntityComparer = (first: EntityObject, second: EntityObject) => number;

/** Абстрактный компонент для редактирования поля-связи */
export abstract class AbstractRelationFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends AbstractFieldEditor<P> {
  @inject protected _relationController: RelationController;

  constructor(props: P, context?: any) {
    super(props, context);
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    // фикс для M2ORelation
    this._readonly =
      this._readonly ||
      (fieldSchema.FieldType === FieldExactTypes.M2ORelation &&
        fieldSchema.UpdatingMode === UpdatingMode.Ignore);
  }

  /** Собрать функцию выбора поля для отображения в заголовке */
  protected makeDisplayFieldSelector<T = ReactNode>(
    displayField: string | FieldSelector<T>,
    fieldSchema: RelationFieldSchema
  ): FieldSelector<T> {
    const expression = displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    return isString(expression) ? entity => entity[expression] : expression;
  }

  /** Собрать набор функций выбора полей для отображения в заголовке */
  protected makeDisplayFieldsSelectors<T = ReactNode>(
    displayFields: (string | FieldSelector<T>)[],
    fieldSchema: RelationFieldSchema
  ): FieldSelector<T>[] {
    const expressions = displayFields || fieldSchema.DisplayFieldNames || [];
    return expressions.map(field => (isString(field) ? entity => entity[field] : field));
  }

  /** Собрать функцию сравнения статей по выбранным полям */
  protected makeEntityComparer(
    sortItems: string | FieldSelector | EntityComparer,
    fieldSchema: MultiRelationFieldSchema
  ): EntityComparer {
    const expression = sortItems || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
    if (isString(expression)) {
      return asc(entity => entity[expression]);
    }
    if (expression.length === 1) {
      return asc(expression as FieldSelector);
    }
    if (expression.length === 2) {
      return expression as EntityComparer;
    }
    throw new Error("Invalid `sortItems` parameter");
  }
}

/** Режим подсветки строк в таблице и аккордеоне */
export const enum HighlightMode {
  None,
  Highlight,
  Shade
}

/**
 * Props для редактора поля-связи который отображает внутри себя
 * форму статьи с помощью @see ArticleEditor . Используются в
 * @see RealtionFieldAccordion @see RealtionFieldForm @see RealtionFieldTabs etc.
 */
export interface ExpandableFieldEditorProps extends FieldEditorProps {
  /** Props для передачи в  @see EntityEditor */

  /** Порядок отображения полей в форме. @example ["Title", "Description", "Products"] */
  fieldOrders?: string[];
  /**
   * Настройки редакторов полей по имени поля. Если настройка для поля отсутствует,
   * то редактор определяется по типу схемы поля @see FieldSchema
   */
  fieldEditors?: FieldsConfig;
  /** Не отображать поля, не описанные в @see fieldEditors */
  skipOtherFields?: boolean;

  /** Props, отвечающие за набор кнопок для редактирования */

  /** Разрешено ли создание статьи по образцу */
  canClonePrototype?: boolean;
  /** Разрешено ли создание новой статьи */
  canCreateEntity?: boolean;
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
  /** Разрешен ли выбор новых статей из модального окна QP */
  canSelectRelation?: boolean;
  /** Разрешено ли открепление всех статей (очистка связи) */
  canClearRelation?: boolean;
  /** Разрешена ли перезагрузка связи и всех вложенных статей */
  canReloadRelation?: boolean;

  /** Обработчики действий по изменению поля-связи или вложенных статей связей */

  /** Обработчик, выполняющийся при добавлении редактора вложенной статьи в DOM */
  onMountEntity?(entity: EntityObject): void;
  /** Обработчик, выполняющийся при удалении редактора вложенной статьи из DOM */
  onUnmountEntity?(entity: EntityObject): void;
  /**
   * Обработчик, выполняющийся при создании новой вложенной статьи
   * @param createEntity Метод, выполняющий реальное создание статьи
   */
  onCreateEntity?(createEntity: () => EntityObject): void;
  /**
   * Обработчик, выполняющийся при создании вложенной статьи по образцу
   * @param clonePrototype Метод, выполняющий реальное создание статьи по образцу
   */
  onClonePrototype?(clonePrototype: () => Promise<EntityObject>): void;
  /**
   * Обработчик, выполняющийся при клонировании вложенной статьи
   * @param cloneEntity Метод, выполняющий реальное клонировании вложенной статьи
   */
  onCloneEntity?(entity: EntityObject, cloneEntity: () => Promise<EntityObject>): void;
  /**
   * Обработчик сохранения вложенной статьи
   * @param saveEntity Метод, выполняющий реальное сохранение
   */
  onSaveEntity?(entity: EntityObject, saveEntity: () => Promise<void>): void;
  /**
   * Обработчик обновления вложенной статьи
   * @param refreshEntity Метод, выполняющий реальное обновление
   */
  onRefreshEntity?(entity: EntityObject, refreshEntity: () => Promise<void>): void;
  /**
   * Обработчик перезагрузки вложенной статьи
   * @param reloadEntity Метод, выполняющий реальную перезагрузку
   */
  onReloadEntity?(entity: EntityObject, reloadEntity: () => Promise<void>): void;
  /**
   * Обработчик публикации вложенной статьи
   * @param publishEntity Метод, выполняющий реальную публикацию
   */
  onPublishEntity?(entity: EntityObject, publishEntity: () => Promise<void>): void;
  /**
   * Обработчик удаления вложенной статьи
   * @param removeEntity Метод, выполняющий реальное удаление
   */
  onRemoveEntity?(entity: EntityObject, removeEntity: () => Promise<boolean>): void;
  /**
   * Обработчик открепления вложенной статьи
   * @param detachEntity Метод, выполняющий реальное открепление
   */
  onDetachEntity?(entity: EntityObject, detachEntity: () => void): void;
  /**
   * Обработчик, выполняющийся при выборе новых статей из модального окна QP
   * @param selectRelation Метод, выполняющий реальный выбор новых статей из модального окна QP
   */
  onSelectRelation?(selectRelation: () => Promise<void>): void;
  /**
   * Обработчик, выполняющийся при перезагрузке связи и всех вложенных статей
   * @param relaoadRelation Метод, выполняющий реальную перезагрузку связи и всех вложенных статей
   */
  onReloadRelation?(relaoadRelation: () => Promise<void>): void;
  /**
   * Обработчик, выполняющийся при очистке связи
   * @param clearRelation Метод, выполняющий реальную очистку связи
   */
  onClearRelation?(clearRelation: () => void): void;

  /** Render Callback для добавления дополнительных кнопок-действий в редактор поля-связи */
  relationActions?: () => ReactNode;
  /** Render Callback для добавления дополнительных кнопок-действий в меню вложенных статьи */
  entityActions?: (entity: EntityObject) => ReactNode;
}
