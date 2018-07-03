import React from "react";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, ButtonGroup } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { required } from "Utils/Validators";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { AbstractRelationFieldTabs } from "./AbstractRelationFieldTabs";

@consumer
@observer
export class SingleRelationFieldTabs extends AbstractRelationFieldTabs {
  readonly state = {
    isOpen: this.props.open
  };

  @action
  private createRelation = () => {
    const { model, fieldSchema } = this.props;
    const contentName = (fieldSchema as SingleRelationFieldSchema).Content.ContentName;
    const article = this._dataContext.createArticle(contentName);
    model[fieldSchema.FieldName] = article;
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  renderControls(_model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    return (
      <ButtonGroup>
        <Button small icon="add" disabled={fieldSchema.IsReadOnly} onClick={this.createRelation}>
          Создать
        </Button>
        <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
          Выбрать
        </Button>
        <Button small icon="eraser" disabled={fieldSchema.IsReadOnly} onClick={this.removeRelation}>
          Очистить
        </Button>
      </ButtonGroup>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { saveRelations, fieldEditors, children } = this.props;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return (
      <>
        {article && (
          <div className="relation-field-tabs__panel">
            <ArticleEditor
              model={article}
              contentSchema={fieldSchema.Content}
              fieldEditors={fieldEditors}
              saveRelations={saveRelations}
              header
              buttons={!fieldSchema.IsReadOnly}
            >
              {children}
            </ArticleEditor>
          </div>
        )}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={fieldSchema.IsRequired && required}
        />
      </>
    );
  }
}
