import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer, inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Icon } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { DataSerializer } from "Services/DataSerializer";
import { required } from "Utils/Validators";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { AbstractRelationFieldAccordion } from "./AbstractRelationFieldAccordion";

@consumer
@observer
export class SingleRelationFieldAccordion extends AbstractRelationFieldAccordion {
  @inject private _dataSerializer: DataSerializer;
  readonly state = {
    isOpen: false,
    isTouched: false
  };

  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({
      isOpen: false,
      isTouched: false
    });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  toggleRelation = () => {
    const { isOpen, isTouched } = this.state;
    this.setState({
      isOpen: !isOpen,
      isTouched: isTouched || !isOpen
    });
  };

  renderControls(fieldSchema: SingleRelationFieldSchema) {
    return (
      <ButtonGroup>
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
    const { save, fields, children } = this.props;
    const { isOpen, isTouched } = this.state;
    const article: ArticleObject = model[fieldSchema.FieldName];
    const serverId = article && this._dataSerializer.getServerId(article);
    return (
      <>
        {article && (
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
                  {serverId > 0 && `(${serverId})`}
                </td>
                {this._displayFields.map((displayField, i) => (
                  <td key={i} className="relation-field-accordion__cell">
                    {displayField(article)}
                  </td>
                ))}
                <td key={-3} className="relation-field-accordion__controls">
                  {!fieldSchema.IsReadOnly && (
                    <ButtonGroup>
                      {save && (
                        <Button small icon="floppy-disk">
                          Сохранить
                        </Button>
                      )}
                      <Button
                        small
                        icon={<Icon icon="small-cross" title={false} />}
                        disabled={fieldSchema.IsReadOnly}
                        onClick={this.removeRelation}
                      />
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
                    <Col md>
                      <ArticleEditor
                        model={article}
                        contentSchema={fieldSchema.Content}
                        fields={fields}
                      >
                        {children}
                      </ArticleEditor>
                    </Col>
                  )}
                </td>
              </tr>
            </tbody>
          </table>
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
