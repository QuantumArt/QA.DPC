import React, { Fragment } from "react";
import { Col } from "react-flexbox-grid";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { consumer } from "react-ioc";
import cn from "classnames";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { asc } from "Utils/Array/Sort";
import { isString } from "Utils/TypeChecks";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldTags, RelationFieldTagsProps } from "./AbstractRelationFieldTags";

interface MultiRelationFieldTagsState {
  selectedIds: {
    [articleId: number]: boolean;
  };
}

@consumer
@observer
export class MultiRelationFieldTags extends AbstractRelationFieldTags {
  private _orderByField: FieldSelector;
  readonly state: MultiRelationFieldTagsState = {
    selectedIds: {}
  };

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    const orderByField =
      props.orderByField || fieldSchema.OrderByFieldName || ArticleObject._ServerId;
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
  private removeRelation(e: any, article: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const { selectedIds } = this.state;
    delete selectedIds[article._ClientId];
    this.setState({ selectedIds });
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(article);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  private toggleRelation(e: any, article: EntityObject) {
    const { selectMultiple, onClick } = this.props;
    if (onClick) {
      let { selectedIds } = this.state;
      if (selectedIds[article._ClientId]) {
        delete selectedIds[article._ClientId];
      } else {
        if (selectMultiple) {
          selectedIds[article._ClientId] = true;
        } else {
          selectedIds = { [article._ClientId]: true };
        }
      }
      this.setState({ selectedIds });
      onClick(e, article);
    }
  }

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const { onClick } = this.props;
    const { selectedIds } = this.state;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Col md className="relation-field-list__tags">
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelations}
          onClear={!this._readonly && !isEmpty && this.clearRelation}
        />
        {list &&
          list
            .slice()
            .sort(asc(this._orderByField))
            .map(article => (
              <Fragment key={article._ClientId}>
                {" "}
                <span
                  onClick={e => this.toggleRelation(e, article)}
                  className={cn("pt-tag pt-minimal", {
                    "pt-interactive": !!onClick,
                    "pt-tag-removable": !this._readonly,
                    "pt-intent-primary": selectedIds[article._ClientId]
                  })}
                >
                  {this.getTitle(article)}
                  {!this._readonly && (
                    <button
                      className="pt-tag-remove"
                      title="Удалить"
                      onClick={e => this.removeRelation(e, article)}
                    />
                  )}
                </span>
              </Fragment>
            ))}
      </Col>
    );
  }
}
