import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { Validate } from "mst-validation-mixin";
import { asc } from "Utils/Array/Sort";
import { isString } from "Utils/TypeChecks";
import { required, maxCount } from "Utils/Validators";
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
  state: MultiRelationFieldListState = {
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
  clearRelation = () => {
    const { model, fieldSchema } = this.props;
    this.setState({ selectedIds: {} });
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  removeRelation(e: any, article: ArticleObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { selectedIds } = this.state;
    delete selectedIds[article.Id];
    this.setState({ selectedIds });
    const array: IObservableArray<ArticleObject> = model[fieldSchema.FieldName];
    array.remove(article);
    model.setTouched(fieldSchema.FieldName, true);
  }

  toggleRelation(e: any, article: ArticleObject) {
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

  renderField(model: ArticleObject | ExtensionObject, fieldSchema: MultiRelationFieldSchema) {
    const { selectedIds } = this.state;
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
                    "pt-intent-primary": selectedIds[article.Id]
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
