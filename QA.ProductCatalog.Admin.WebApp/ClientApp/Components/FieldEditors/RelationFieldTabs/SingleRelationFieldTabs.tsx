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
      <ButtonGroup>
        <Button
          minimal
          small
          icon="add"
          intent={Intent.SUCCESS}
          disabled={fieldSchema.IsReadOnly}
          onClick={this.createRelation}
        >
          Создать
        </Button>
        <Button
          minimal
          small
          icon="th-derived"
          intent={Intent.PRIMARY}
          disabled={fieldSchema.IsReadOnly}
        >
          Выбрать
        </Button>
        <Button
          minimal
          small
          icon="eraser"
          intent={Intent.DANGER}
          disabled={fieldSchema.IsReadOnly}
          onClick={this.removeRelation}
        >
          Очистить
        </Button>
        <Button
          minimal
          small
          disabled={!article}
          icon={isOpen ? "collapse-all" : "expand-all"}
          onClick={this.toggleRelation}
        >
          {isOpen ? "Свернуть" : "Развернуть"}
        </Button>
      </ButtonGroup>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { saveRelations, fieldEditors, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return isTouched && article ? (
      <div
        className={cn("single-relation-field-tabs", {
          "single-relation-field-tabs--hidden": !isOpen
        })}
      >
        <ArticleEditor
          model={article}
          contentSchema={fieldSchema.Content}
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
