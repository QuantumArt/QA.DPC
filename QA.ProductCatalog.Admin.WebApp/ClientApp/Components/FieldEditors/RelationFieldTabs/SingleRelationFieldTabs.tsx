import React from "react";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { AbstractRelationFieldTabs } from "./AbstractRelationFieldTabs";

@consumer
@observer
export class SingleRelationFieldTabs extends AbstractRelationFieldTabs {
  readonly state = {
    isOpen: !this.props.collapsed,
    isTouched: !this.props.collapsed
  };

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).Content.ContentName;
    const article = this._dataContext.createArticle(contentName);
    this.setState({
      isOpen: true,
      isTouched: true
    });
    model[fieldSchema.FieldName] = article;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private toggleRelation = () => {
    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  renderControls(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { isOpen } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return (
      <div className="relation-field-tabs__controls">
        <ButtonGroup>
          <Button
            minimal
            small
            rightIcon="add"
            intent={Intent.SUCCESS}
            disabled={fieldSchema.IsReadOnly}
            onClick={this.createRelation}
          >
            Создать
          </Button>
          <Button
            minimal
            small
            rightIcon="th-derived"
            intent={Intent.PRIMARY}
            disabled={fieldSchema.IsReadOnly}
          >
            Выбрать
          </Button>
          <Button
            minimal
            small
            rightIcon="eraser"
            intent={Intent.DANGER}
            disabled={fieldSchema.IsReadOnly}
            onClick={this.removeRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        <Button
          small
          disabled={!article}
          rightIcon={isOpen ? "chevron-up" : "chevron-down"}
          onClick={this.toggleRelation}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </div>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { saveRelations, skipOtherFields, fieldEditors, borderless, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return isTouched && article ? (
      <div
        className={cn("single-relation-field-tabs", {
          "single-relation-field-tabs--hidden": !isOpen,
          "single-relation-field-tabs--borderless": borderless
        })}
      >
        <ArticleEditor
          model={article}
          contentSchema={fieldSchema.Content}
          skipOtherFields={skipOtherFields}
          fieldEditors={fieldEditors}
          saveRelations={saveRelations}
          header
          buttons={!fieldSchema.IsReadOnly}
          onRemove={this.removeRelation}
        >
          {children}
        </ArticleEditor>
      </div>
    ) : null;
  }
}
