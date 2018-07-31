import React from "react";
import { consumer } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Icon, Intent } from "@blueprintjs/core";
import { SaveButton } from "Components/FormControls/FormControls";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { AbstractRelationFieldAccordion } from "./AbstractRelationFieldAccordion";

@consumer
@observer
export class SingleRelationFieldAccordion extends AbstractRelationFieldAccordion {
  readonly state = {
    isOpen: false,
    isTouched: false
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
  private removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private toggleRelation = (e: any) => {
    // нажали на элемент находящийся внутри <button>
    if (e.target.closest("button")) return;

    const { isOpen } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: true
    });
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  renderControls(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const article: ArticleObject = model[fieldSchema.FieldName];
    return (
      <ButtonGroup>
        <Button
          minimal
          small
          rightIcon="add"
          intent={Intent.SUCCESS}
          disabled={fieldSchema.IsReadOnly || !!article}
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
          onClick={this.selectRelation}
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
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { fieldEditors, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return article ? (
      <table className="relation-field-accordion" cellSpacing="0" cellPadding="0">
        <tbody>
          <tr
            className={cn("relation-field-accordion__header", {
              "relation-field-accordion__header--open": isOpen
            })}
            onClick={this.toggleRelation}
          >
            <td
              key={-1}
              className="relation-field-accordion__expander"
              title={isOpen ? "Свернуть" : "Развернуть"}
            >
              <Icon icon={isOpen ? "caret-down" : "caret-right"} title={false} />
            </td>
            <td key={-2} className="relation-field-accordion__cell">
              {article._ServerId > 0 && `(${article._ServerId})`}
            </td>
            {this._displayFields.map((displayField, i) => (
              <td key={i} className="relation-field-accordion__cell">
                {displayField(article)}
              </td>
            ))}
            <td key={-3} className="relation-field-accordion__controls">
              {!fieldSchema.IsReadOnly && (
                <ButtonGroup>
                  {this.showSaveButtion(article) && <SaveButton small />}
                  <Button
                    minimal
                    small
                    rightIcon="remove"
                    intent={Intent.DANGER}
                    onClick={this.removeRelation}
                  >
                    Удалить
                  </Button>
                </ButtonGroup>
              )}
            </td>
          </tr>
          <tr className="relation-field-accordion__main">
            <td
              className={cn("relation-field-accordion__body", {
                "relation-field-accordion__body--open": isOpen
              })}
              colSpan={this._displayFields.length + 3}
            >
              {isTouched && (
                <ArticleEditor
                  model={article}
                  contentSchema={fieldSchema.Content}
                  fieldEditors={fieldEditors}
                >
                  {children}
                </ArticleEditor>
              )}
            </td>
          </tr>
        </tbody>
      </table>
    ) : null;
  }
}
