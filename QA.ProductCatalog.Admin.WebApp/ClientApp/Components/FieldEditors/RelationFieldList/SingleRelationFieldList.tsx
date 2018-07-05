import React from "react";
import { Col } from "react-flexbox-grid";
import { action } from "mobx";
import { observer } from "mobx-react";
import cn from "classnames";
import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { SingleRelationFieldSchema } from "Models/EditorSchemaModels";
import { AbstractRelationFieldList } from "./AbstractRelationFieldList";

@observer
export class SingleRelationFieldList extends AbstractRelationFieldList {
  readonly state = {
    isSelected: false
  };

  @action
  private removeRelation = (e: any) => {
    e.stopPropagation();
    const { model, fieldSchema } = this.props;
    this.setState({ isSelected: false });
    model[fieldSchema.FieldName] = null;
    model.setTouched(fieldSchema.FieldName, true);
  };

  private toggleRelation(e: any, article: ArticleObject) {
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
      <Col md className="relation-field-list__tags">
        <ButtonGroup>
          <Button
            minimal
            small
            icon="th-derived"
            intent={Intent.PRIMARY}
            disabled={fieldSchema.IsReadOnly}
          >
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
              <button className="pt-tag-remove" title="Удалить" onClick={this.removeRelation} />
            )}
          </span>
        )}
      </Col>
    );
  }
}
