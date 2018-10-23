import React, { Component } from "react";
import { consumer, inject } from "react-ioc";
import { MenuItem, Icon, Intent } from "@blueprintjs/core";
import { ValidationSummay } from "Components/ValidationSummary/ValidationSummary";
import { DataValidator } from "Services/DataValidator";
import { OverlayPresenter } from "Services/OverlayPresenter";
import { ContentSchema } from "Models/EditorSchemaModels";
import { Product } from "../TypeScriptSchema";

interface PublishButtonsProps {
  model: Product;
  contentSchema: ContentSchema;
}

@consumer
export class PublishButtons extends Component<PublishButtonsProps> {
  @inject private _dataValidator: DataValidator;
  @inject private _overlayPresenter: OverlayPresenter;

  private publishProduct = async (e: any) => {
    e.stopPropagation();
    const { model, contentSchema } = this.props;
    const errors = this._dataValidator.collectErrors(model, contentSchema, false);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
      return;
    }
    window.alert(`TODO: Публиковать ${model._ServerId}`);
  };

  private stageProduct = async (e: any) => {
    e.stopPropagation();
    const { model, contentSchema } = this.props;
    const errors = this._dataValidator.collectErrors(model, contentSchema, false);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
      return;
    }
    window.alert(`TODO: Отправить на stage ${model._ServerId}`);
  };

  render() {
    return (
      <>
        <MenuItem
          labelElement={<Icon icon="tick-circle" intent={Intent.SUCCESS} />}
          intent={Intent.SUCCESS}
          onClick={this.publishProduct}
          text="Публиковать"
          title="Публиковать"
        />
        <MenuItem
          labelElement={<Icon icon="send-to" intent={Intent.SUCCESS} />}
          onClick={this.stageProduct}
          text="Отправить на stage"
          title="Отправить на stage"
        />
      </>
    );
  }
}
