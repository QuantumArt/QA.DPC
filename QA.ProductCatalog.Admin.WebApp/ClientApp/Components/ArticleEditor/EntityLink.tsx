import React, { Component } from "react";
import { inject } from "react-ioc";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { observer } from "mobx-react";
import { EntityController } from "Services/EntityController";
import { Icon, Button, Intent } from "@blueprintjs/core";
import { action } from "mobx";
import "./ArticleEditor.scss";

interface EntityLinkProps {
  model: EntityObject;
  contentSchema: ContentSchema;
}

@observer
export class EntityLink extends Component<EntityLinkProps> {
  @inject private _entityController: EntityController;

  @action
  private handleMouseDown = e => {
    if (e.nativeEvent.which === 2) {
      e.preventDefault();
      const { model, contentSchema } = this.props;
      this._entityController.editEntity(model, contentSchema, false);
    }
  };

  @action
  private handleClick = e => {
    e.preventDefault();
    const { model, contentSchema } = this.props;
    this._entityController.editEntity(model, contentSchema, true);
  };

  render() {
    const { model } = this.props;
    return (
      model._ServerId > 0 && (
        <Button
          minimal
          small
          className="entity-link"
          rightIcon={<Icon icon="manually-entered-data" title={null} />}
          intent={Intent.PRIMARY}
          title="Редактировать статью"
          onMouseDown={this.handleMouseDown}
          onClick={this.handleClick}
        >
          {model._ServerId}
        </Button>
      )
    );
  }
}
