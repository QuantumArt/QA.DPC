import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { asc } from "Utils/Array/Sort";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { FieldSelector } from "../AbstractFieldEditor";
import { AbstractRelationFieldTable, RelationFieldTableProps } from "./AbstractRelationFieldTable";
import { ArticleLink } from "Components/ArticleEditor/ArticleLink";

@consumer
@observer
export class MultiRelationFieldTable extends AbstractRelationFieldTable {
  private _orderByField: FieldSelector;

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      orderByField = (fieldSchema as MultiRelationFieldSchema).OrderByFieldName ||
        ArticleObject._ServerId
    } = props;
    this._orderByField = isString(orderByField) ? article => article[orderByField] : orderByField;
  }

  @action
  private clearRelation = () => {
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = [];
    model.setTouched(fieldSchema.FieldName, true);
  };

  @action
  private removeRelation(e: any, article: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(article);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Col md>
        <RelationFieldMenu
          onSelect={this._canEditRelation && this.selectRelations}
          onClear={this._canEditRelation && !isEmpty && this.clearRelation}
        />
        {this.renderValidation(model, fieldSchema)}
        {list && (
          <div className="relation-field-table">
            {list
              .slice()
              .sort(asc(this._orderByField))
              .map(article => {
                return (
                  <div key={article._ClientId} className="relation-field-table__row">
                    <div key={-1} className="relation-field-table__cell">
                      <ArticleLink model={article} contentSchema={fieldSchema.RelatedContent} />
                    </div>
                    {this._displayFields.map((displayField, i) => (
                      <div key={i} className="relation-field-table__cell">
                        {displayField(article)}
                      </div>
                    ))}
                    <div key={-2} className="relation-field-table__controls">
                      {this._canEditRelation && (
                        <Button
                          minimal
                          small
                          rightIcon="remove"
                          intent={Intent.DANGER}
                          title="Удалить связь"
                          onClick={e => this.removeRelation(e, article)}
                        >
                          Удалить
                        </Button>
                      )}
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </Col>
    );
  }
}
