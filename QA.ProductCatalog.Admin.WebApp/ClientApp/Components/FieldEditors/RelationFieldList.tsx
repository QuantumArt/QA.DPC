import React, { Component, Fragment, MouseEvent } from "react";
import { Col } from "react-flexbox-grid";
import { action, untracked, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import {
  MultiRelationFieldSchema,
  SingleRelationFieldSchema,
  isSingleRelationField,
  RelationFieldSchema,
  isStringField
} from "Models/EditorSchemaModels";
import { Validate } from "Models/ValidatableMixin";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";
import { by, asc, desc } from "Utils/Array/Sort";
import { required, maxCount } from "Utils/Validators";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

interface RelationFieldListProps extends FieldEditorProps {
  displayField?: string;
  orderByField?: string;
  onClick?: (e: MouseEvent<HTMLElement>, article: ArticleObject) => void;
}

export class RelationFieldList extends Component<RelationFieldListProps> {
  FieldList = isSingleRelationField(this.props.fieldSchema)
    ? SingleRelationFieldList
    : MultiRelationFieldList;

  render() {
    return <this.FieldList {...this.props} />;
  }
}

abstract class AbstractRelationFieldList extends AbstractFieldEditor<RelationFieldListProps> {
  _displayField: string;

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    if (props.displayField) {
      this._displayField = props.displayField;
    } else {
      const fieldSchema = props.fieldSchema as RelationFieldSchema;

      const stringField = Object.values(fieldSchema.Content.Fields)
        .sort(by(desc(field => field.ViewInList), asc(field => field.FieldOrder)))
        .find(isStringField);

      this._displayField = stringField ? stringField.FieldName : "Id";
    }
  }
}

@observer
class SingleRelationFieldList extends AbstractRelationFieldList {
  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  handleClick = (e: any) => {
    const { model, fieldSchema, onClick } = this.props;
    if (onClick) {
      untracked(() => onClick(e, model[fieldSchema.FieldName]));
    }
  };

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { onClick } = this.props;
    const article: ArticleObject = model[fieldSchema.FieldName];
    return (
      <Col xl={8} md={6} className="relation-field-list__tags">
        <ButtonGroup>
          <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
            Выбрать
          </Button>
        </ButtonGroup>{" "}
        {article && (
          <span
            className={cn("pt-tag pt-minimal", {
              "pt-tag-removable": !fieldSchema.IsReadOnly,
              "pt-interactive": !!onClick
            })}
            onClick={this.handleClick}
          >
            {article[this._displayField]}
            {!fieldSchema.IsReadOnly && (
              <button className="pt-tag-remove" onClick={this.removeRelation} />
            )}
          </span>
        )}
        {this.renderErrors(model, fieldSchema)}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={fieldSchema.IsRequired && required}
        />
      </Col>
    );
  }
}

@observer
class MultiRelationFieldList extends AbstractRelationFieldList {
  _orderByField: string;

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    if (props.orderByField) {
      this._orderByField = props.orderByField;
    } else {
      this._orderByField = (props.fieldSchema as MultiRelationFieldSchema).OrderByFieldName || "Id";
    }
  }

  @action
  clearRelation = () => {
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    array.remove(article);
    model.setTouched(fieldSchema.FieldName, true);
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { onClick } = this.props;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    return (
      <Col xl={8} md={6} className="relation-field-list__tags">
        <ButtonGroup>
          <Button small icon="th-derived" disabled={fieldSchema.IsReadOnly}>
            Выбрать
          </Button>
          <Button
            small
            icon="eraser"
            disabled={fieldSchema.IsReadOnly}
            onClick={this.clearRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        {list &&
          list
            .slice()
            .sort(asc(article => article[this._orderByField]))
            .map(article => (
              <Fragment key={article.Id}>
                {" "}
                <span
                  onClick={e => onClick && untracked(() => onClick(e, article))}
                  className={cn("pt-tag pt-minimal", {
                    "pt-tag-removable": !fieldSchema.IsReadOnly,
                    "pt-interactive": !!onClick
                  })}
                >
                  {article[this._displayField]}
                  {!fieldSchema.IsReadOnly && (
                    <button
                      className="pt-tag-remove"
                      onClick={e => this.removeRelation(e, article)}
                    />
                  )}
                </span>
              </Fragment>
            ))}
        {this.renderErrors(model, fieldSchema)}
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          rules={[
            fieldSchema.IsRequired && required,
            fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)
          ]}
        />
      </Col>
    );
  }
}
