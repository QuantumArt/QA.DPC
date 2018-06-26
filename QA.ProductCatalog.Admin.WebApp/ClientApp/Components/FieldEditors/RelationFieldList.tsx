import React, { Component, Fragment, MouseEvent } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import {
  MultiRelationFieldSchema,
  SingleRelationFieldSchema,
  isSingleRelationField,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { Validate } from "Models/ValidatableMixin";
import { asc } from "Utils/Array/Sort";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
import { AbstractFieldEditor, FieldEditorProps } from "./AbstractFieldEditor";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

interface RelationFieldListProps extends FieldEditorProps {
  displayField?: string | ((article: ArticleObject) => string);
  orderByField?: string | ((article: ArticleObject) => string);
  selectMultiple?: boolean;
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
  _displayField: (model: ArticleObject) => string;

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayField = (fieldSchema as RelationFieldSchema).Content.DisplayFieldName || "Id"
    } = this.props;
    this._displayField = isString(displayField)
      ? displayField === "Id"
        ? () => ""
        : article => article[displayField]
      : displayField;
  }
}

@observer
class SingleRelationFieldList extends AbstractRelationFieldList {
  state = {
    isSelected: false
  };

  @action
  removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({ isSelected: false });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  toggleRelation(e: any, article: ArticleObject) {
    const { onClick } = this.props;
    const { isSelected } = this.state;
    this.setState({ isSelected: !isSelected });
    if (onClick) {
      onClick(e, article);
    }
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: SingleRelationFieldSchema) {
    const { isSelected } = this.state;
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
            className={cn("pt-tag pt-minimal pt-interactive", {
              "pt-tag-removable": !fieldSchema.IsReadOnly,
              "pt-intent-primary": isSelected
            })}
            onClick={e => this.toggleRelation(e, article)}
          >
            {this._displayField(article)}
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

interface MultiRelationFieldListState {
  selectedArticles: {
    [articleId: number]: boolean;
  };
}

@observer
class MultiRelationFieldList extends AbstractRelationFieldList {
  _orderByField: (article: ArticleObject) => string;
  state: MultiRelationFieldListState = {
    selectedArticles: {}
  };

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName || "Id"
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
  }

  @action
  clearRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({ selectedArticles: {} });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { selectedArticles } = this.state;
    delete selectedArticles[article.Id];
    this.setState({ selectedArticles });
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    array.remove(article);
    model.setTouched(fieldSchema.FieldName, true);
  }

  toggleRelation(e: any, article: ArticleObject) {
    const { selectMultiple, onClick } = this.props;
    let { selectedArticles } = this.state;
    if (selectedArticles[article.Id]) {
      delete selectedArticles[article.Id];
    } else {
      if (selectMultiple) {
        selectedArticles[article.Id] = true;
      } else {
        selectedArticles = { [article.Id]: true };
      }
    }
    this.setState({ selectedArticles });
    if (onClick) {
      onClick(e, article);
    }
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { selectedArticles } = this.state;
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
            .sort(asc(this._orderByField))
            .map(article => (
              <Fragment key={article.Id}>
                {" "}
                <span
                  onClick={e => this.toggleRelation(e, article)}
                  className={cn("pt-tag pt-minimal pt-interactive", {
                    "pt-tag-removable": !fieldSchema.IsReadOnly,
                    "pt-intent-primary": selectedArticles[article.Id]
                  })}
                >
                  {this._displayField(article)}
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
