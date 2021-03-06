import React, { Component } from "react";
import { inject } from "react-ioc";
import { MenuItem, Icon, Intent } from "@blueprintjs/core";
import { ValidationSummay } from "ProductEditor/Components/ValidationSummary/ValidationSummary";
import { DataValidator } from "ProductEditor/Services/DataValidator";
import { OverlayPresenter } from "ProductEditor/Services/OverlayPresenter";
import { ActionController } from "ProductEditor/Services/ActionController";
import { ContentSchema } from "ProductEditor/Models/EditorSchemaModels";
import { Product } from "../TypeScriptSchema";

interface PublishButtonsProps {
  model: Product;
  contentSchema: ContentSchema;
}

export class PublishButtons extends Component<PublishButtonsProps> {
  @inject private _dataValidator: DataValidator;
  @inject private _overlayPresenter: OverlayPresenter;
  @inject private _actionController: ActionController;

  private publishProduct = async (e: any) => {
    e.stopPropagation();
    const { model, contentSchema } = this.props;
    const errors = this._dataValidator.collectErrors(model, contentSchema, false);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
      return;
    }
    await this._actionController.executeCustomAction("publish_product", model, contentSchema);
  };

  private stageProduct = async (e: any) => {
    e.stopPropagation();
    const { model, contentSchema } = this.props;
    const errors = this._dataValidator.collectErrors(model, contentSchema, false);
    if (errors.length > 0) {
      await this._overlayPresenter.alert(<ValidationSummay errors={errors} />, "OK");
      return;
    }
    await this._actionController.executeCustomAction("publish_to_stage", model, contentSchema);
  };

  render() {
    return (
      <>
        <MenuItem
          labelElement={<Icon icon="tick-circle" intent={Intent.SUCCESS} />}
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
