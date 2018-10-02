import React, { Component } from "react";
import { consumer, inject } from "react-ioc";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { observer } from "mobx-react";
import { ArticleController } from "Services/ArticleController";
import { Icon, Button, Intent } from "@blueprintjs/core";
import { action } from "mobx";
import "./ArticleEditor.scss";

interface ArticleLinkProps {
  model: EntityObject;
  contentSchema: ContentSchema;
}

@consumer
@observer
export class ArticleLink extends Component<ArticleLinkProps> {
  @inject private _articleController: ArticleController;

  @action
  private handleMouseDown = e => {
    if (e.nativeEvent.which === 2) {
      e.preventDefault();
      const { model, contentSchema } = this.props;
      this._articleController.editEntity(model, contentSchema, false);
    }
  };

  @action
  private handleClick = e => {
    e.preventDefault();
    const { model, contentSchema } = this.props;
    this._articleController.editEntity(model, contentSchema, true);
  };

  render() {
    const { model } = this.props;
    return (
      model._ServerId > 0 && (
        <Button
          minimal
          small
          className="article-link"
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
