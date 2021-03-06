import React from "react";
import cn from "classnames";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleObject, EntityObject } from "ProductEditor/Models/EditorDataModels";
import { SingleRelationFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { RelationFieldMenu } from "ProductEditor/Components/FieldEditors/RelationFieldMenu";
import { AbstractRelationFieldTable } from "./AbstractRelationFieldTable";
import { EntityLink } from "ProductEditor/Components/ArticleEditor/EntityLink";

/** Отображение поля-связи в виде таблицы */
@observer
export class SingleRelationFieldTable extends AbstractRelationFieldTable {
  @action
  private detachEntity = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private selectRelation = async () => {
    const { model, fieldSchema } = this.props;
    await this._relationController.selectRelation(model, fieldSchema as SingleRelationFieldSchema);
  };

  renderField(model: ArticleObject, fieldSchema: SingleRelationFieldSchema) {
    const { relationActions, validateItems } = this.props;
    const entity: EntityObject = model[fieldSchema.FieldName];
    const itemError =
      entity &&
      validateItems &&
      this._validationCache.getOrAdd(entity, () => validateItems(entity));
    return (
      <Col md>
        <RelationFieldMenu onSelect={!this._readonly && this.selectRelation}>
          {relationActions && relationActions()}
        </RelationFieldMenu>
        {this.renderValidation(model, fieldSchema)}
        {entity && (
          <div className="relation-field-table">
            <div className="relation-field-table__table">
              <div
                className={cn("relation-field-table__row", {
                  "relation-field-table__row--invalid": !!itemError
                })}
                title={itemError}
              >
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
                      onClick={this.detachEntity}
                    >
                      Отвязать
                    </Button>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </Col>
    );
  }
}
