import React from "react";
import cn from "classnames";
import { inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Button } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { DataContext } from "Services/DataContext";
import { EntityController } from "Services/EntityController";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import {
  AbstractRelationFieldEditor,
  ExpandableFieldEditorProps
} from "Components/FieldEditors/AbstractFieldEditor";
import "./RelationFieldForm.scss";

interface RelationFieldFormProps extends ExpandableFieldEditorProps {
  collapsed?: boolean;
  className?: string;
  borderless?: boolean;
}

const defaultRelationHandler = action => action();
const defaultEntityHandler = (_entity, action) => action();

@observer
export class RelationFieldForm extends AbstractRelationFieldEditor<RelationFieldFormProps> {
  static defaultProps = {
    canSaveEntity: true,
    canRefreshEntity: true,
    canReloadEntity: true,
    canReloadRelation: true,
    onClonePrototype: defaultRelationHandler,
    onCreateEntity: defaultRelationHandler,
    onCloneEntity: defaultEntityHandler,
    onRemoveEntity: defaultEntityHandler,
    onDetachEntity: defaultEntityHandler,
    onSelectRelation: defaultRelationHandler,
    onReloadRelation: defaultRelationHandler,
    onClearRelation: defaultRelationHandler
  };

  @inject private _dataContext: DataContext;
  @inject private _entityController: EntityController;

  readonly state = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed
  };

  private clonePrototype = () => {
    const { model, fieldSchema, onClonePrototype } = this.props as PrivateProps;
    onClonePrototype(
      action("clonePrototype", async () => {
        const clone = await this._relationController.cloneProductPrototype(model, fieldSchema);
        this.setState({
          isOpen: true,
          isTouched: true
        });
        return clone;
      })
    );
  };

  private createEntity = () => {
    const { model, fieldSchema, onCreateEntity } = this.props as PrivateProps;
    const contentName = fieldSchema.RelatedContent.ContentName;
    onCreateEntity(
      action("createEntity", () => {
        const entity = this._dataContext.createEntity(contentName);

        model[fieldSchema.FieldName] = entity;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: true,
          isTouched: true
        });
        return entity;
      })
    );
  };

  private detachEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onDetachEntity } = this.props as PrivateProps;
    onDetachEntity(
      entity,
      action("detachEntity", () => {
        model[fieldSchema.FieldName] = null;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private removeEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onRemoveEntity } = this.props as PrivateProps;
    onRemoveEntity(
      entity,
      action("removeEntity", async () => {
        const isRemoved = await this._entityController.removeRelatedEntity(
          model,
          fieldSchema,
          entity
        );
        if (isRemoved) {
          this.setState({
            isOpen: false,
            isTouched: false
          });
        }
        return isRemoved;
      })
    );
  };

  private cloneEntity = (entity: EntityObject) => {
    const { model, fieldSchema, onCloneEntity } = this.props as PrivateProps;
    onCloneEntity(
      entity,
      action("cloneEntity", async () => {
        return await this._entityController.cloneRelatedEntity(model, fieldSchema, entity);
      })
    );
  };

  private clearRelation = () => {
    const { model, fieldSchema, onClearRelation } = this.props;
    onClearRelation(
      action("clearRelation", () => {
        model[fieldSchema.FieldName] = null;
        model.setTouched(fieldSchema.FieldName, true);
        this.setState({
          isOpen: false,
          isTouched: false
        });
      })
    );
  };

  private selectRelation = () => {
    const { model, fieldSchema, onSelectRelation } = this.props as PrivateProps;
    onSelectRelation(
      action("selectRelation", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.selectRelation(model, fieldSchema);
      })
    );
  };

  private reloadRelation = () => {
    const { model, fieldSchema, onReloadRelation } = this.props as PrivateProps;
    onReloadRelation(
      action("reloadRelation", async () => {
        this.setState({
          isOpen: true,
          isTouched: true
        });
        await this._relationController.reloadRelation(model, fieldSchema);
      })
    );
  };

  private toggleEditor = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  render() {
    const { model, fieldSchema } = this.props as PrivateProps;
    return (
      <Col
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label field-editor__label--small">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          <Col md>
            {this.renderControls(model, fieldSchema)}
            {this.renderValidation(model, fieldSchema)}
          </Col>
        </Row>
        <Row>
          <Col md>{this.renderField(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }

  renderControls(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const {
      relationActions,
      canCreateEntity,
      canSelectRelation,
      canClearRelation,
      canReloadRelation,
      canClonePrototype
    } = this.props;
    const { isOpen } = this.state;
    const entity: EntityObject = model[fieldSchema.FieldName];
    return (
      <div className="relation-field-form__controls">
        <RelationFieldMenu
          onCreate={canCreateEntity && !this._readonly && !entity && this.createEntity}
          onSelect={canSelectRelation && !this._readonly && this.selectRelation}
          onClear={canClearRelation && !this._readonly && !!entity && this.clearRelation}
          onReload={canReloadRelation && model._ServerId > 0 && this.reloadRelation}
          onClonePrototype={
            canClonePrototype && model._ServerId > 0 && !entity && this.clonePrototype
          }
        >
          {relationActions && relationActions()}
        </RelationFieldMenu>
        <Button
          small
          disabled={!entity}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleEditor}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </div>
    );
  }

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const {
      skipOtherFields,
      fieldOrders,
      fieldEditors,
      borderless,
      className,
      entityActions,
      onMountEntity,
      onUnmountEntity,
      onSaveEntity,
      onRefreshEntity,
      onReloadEntity,
      onPublishEntity,
      canSaveEntity,
      canRefreshEntity,
      canReloadEntity,
      canDetachEntity,
      canRemoveEntity,
      canPublishEntity,
      canCloneEntity,
      children
    } = this.props;
    const { isOpen, isTouched } = this.state;
    const entity: EntityObject = model[fieldSchema.FieldName];
    return isTouched && entity ? (
      <div
        className={cn("relation-field-form", className, {
          "relation-field-form--hidden": !isOpen,
          "relation-field-form--borderless": borderless
        })}
      >
        <EntityEditor
          withHeader
          model={entity}
          contentSchema={fieldSchema.RelatedContent}
          skipOtherFields={skipOtherFields}
          fieldOrders={fieldOrders}
          fieldEditors={fieldEditors}
          onMountEntity={onMountEntity}
          onUnmountEntity={onUnmountEntity}
          onSaveEntity={onSaveEntity}
          onRefreshEntity={onRefreshEntity}
          onReloadEntity={onReloadEntity}
          onPublishEntity={onPublishEntity}
          onDetachEntity={this.detachEntity}
          onCloneEntity={this.cloneEntity}
          onRemoveEntity={this.removeEntity}
          canSaveEntity={canSaveEntity}
          canRefreshEntity={canRefreshEntity}
          canReloadEntity={canReloadEntity}
          canDetachEntity={!this._readonly && canDetachEntity}
          canRemoveEntity={canRemoveEntity}
          canPublishEntity={canPublishEntity}
          canCloneEntity={canCloneEntity}
          customActions={entityActions}
        >
          {children}
        </EntityEditor>
      </div>
    ) : null;
  }
}

interface PrivateProps extends RelationFieldFormProps {
  fieldSchema: SingleRelationFieldSchema;
}
