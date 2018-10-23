import React from "react";
import { Col } from "react-flexbox-grid";
import { consumer } from "react-ioc";
import { action, IObservableArray } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { MultiRelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationFieldMenu } from "Components/FieldEditors/RelationFieldMenu";
import { EntityComparer } from "../AbstractFieldEditor";
import { AbstractRelationFieldTable, RelationFieldTableProps } from "./AbstractRelationFieldTable";
import { EntityLink } from "Components/ArticleEditor/EntityLink";

@consumer
@observer
export class MultiRelationFieldTable extends AbstractRelationFieldTable {
  private _entityComparer: EntityComparer;

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const { sortItems, sortItemsBy } = props;
    const fieldSchema = props.fieldSchema as MultiRelationFieldSchema;
    this._entityComparer = this.makeEntityComparer(sortItems || sortItemsBy, fieldSchema);
  }

  @action
  private detachEntity(e: any, entity: EntityObject) {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    const array: IObservableArray<EntityObject> = model[fieldSchema.FieldName];
    if (array) {
      array.remove(entity);
      model.setTouched(fieldSchema.FieldName, true);
    }
  }

  @action
  private clearRelations = () => {
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName].replace([]);
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelations = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelations(model, fieldSchema as MultiRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: MultiRelationFieldSchema) {
    const { relationActions } = this.props;
    const list: EntityObject[] = model[fieldSchema.FieldName];
    const isEmpty = !list || list.length === 0;
    return (
      <Col md>
        <RelationFieldMenu
          onSelect={!this._readonly && this.selectRelations}
          onClear={!this._readonly && !isEmpty && this.clearRelations}
        >
          {relationActions}
        </RelationFieldMenu>
        {this.renderValidation(model, fieldSchema)}
        {list && (
          <div className="relation-field-table">
            <div className="relation-field-table__table">
              {list
                .slice()
                .sort(this._entityComparer)
                .map(entity => {
                  return (
                    <div key={entity._ClientId} className="relation-field-table__row">
                      <div key={-1} className="relation-field-table__cell">
                        <EntityLink model={entity} contentSchema={fieldSchema.RelatedContent} />
                      </div>
                      {this._displayFields.map((displayField, i) => (
                        <div key={i} className="relation-field-table__cell">
                          {displayField(entity)}
                        </div>
                      ))}
                      <div key={-2} className="relation-field-table__controls">
                        {!this._readonly && (
                          <Button
                            minimal
                            small
                            rightIcon="remove"
                            intent={Intent.DANGER}
                            title="Удалить связь с текущей статьей"
                            onClick={e => this.detachEntity(e, entity)}
                          >
                            Отвязать
                          </Button>
                        )}
                      </div>
                    </div>
                  );
                })}
            </div>
          </div>
        )}
      </Col>
    );
  }
}
