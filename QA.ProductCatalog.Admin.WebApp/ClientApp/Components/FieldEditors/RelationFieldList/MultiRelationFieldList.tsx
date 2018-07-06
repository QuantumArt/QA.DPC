import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import { Validate } from "mst-validation-mixin";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { asc } from "Utils/Array/Sort";
import { isString } from "Utils/TypeChecks";
import { maxCount } from "Utils/Validators";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldList, RelationFieldListProps } from "./AbstractRelationFieldList";

interface MultiRelationFieldListState {
  selectedIds: {
    [articleId: number]: boolean;
  };
}

@observer
export class MultiRelationFieldList extends AbstractRelationFieldList {
  private _orderByField: FieldSelector;
  readonly state: MultiRelationFieldListState = {
    selectedIds: {}
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
  private clearRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({ selectedIds: {} });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { selectedIds } = this.state;
    delete selectedIds[article.Id];
    this.setState({ selectedIds });
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(article);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  private toggleRelation(e: any, article: ArticleObject) {
    const { selectMultiple, onClick } = this.props;
    let { selectedIds } = this.state;
    if (selectedIds[article.Id]) {
      delete selectedIds[article.Id];
    } else {
      if (selectMultiple) {
        selectedIds[article.Id] = true;
      } else {
        selectedIds = { [article.Id]: true };
      }
    }
    this.setState({ selectedIds });
    if (onClick) {
      onClick(e, article);
    }
  }

  renderValidation(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    return (
      <>
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          silent
          rules={fieldSchema.MaxDataListItemCount && maxCount(fieldSchema.MaxDataListItemCount)}
        />
        {super.renderValidation(model, fieldSchema)}
      </>
    );
  }

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { selectedIds } = this.state;
    const list: ArticleObject[] = model[fieldSchema.FieldName];
    return (
      <Col md className="relation-field-list__tags">
        <ButtonGroup>
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
            onClick={this.clearRelation}
          >
            Очистить
          </Button>
        </ButtonGroup>
        {list &&
          list
            .slice()
            .sort(asc(this._orderByField))
            .map(article => {
              let title = this._displayField(article);
              if (title == null) {
                title = "...";
              }
              return (
                <Fragment key={article.Id}>
                  {" "}
                  <span
                    onClick={e => this.toggleRelation(e, article)}
                    className={cn("pt-tag pt-minimal pt-interactive", {
                      "pt-tag-removable": !fieldSchema.IsReadOnly,
                      "pt-intent-primary": selectedIds[article.Id]
                    })}
                  >
                    {title}
                    {!fieldSchema.IsReadOnly && (
                      <button
                        className="pt-tag-remove"
                        title="Удалить"
                        onClick={e => this.removeRelation(e, article)}
                      />
                    )}
                  </span>
                </Fragment>
              );
            })}
      </Col>
    );
  }
}
