import React, { Component } from "react";
import { inject } from "react-ioc";
import { MenuItem, Icon, Intent } from "@blueprintjs/core";
import { ActionController } from "Services/ActionController";
import { EntityController } from "Services/EntityController";
import { RelationController } from "Services/RelationController";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { Product, MarketingProduct } from "../TypeScriptSchema";

interface CloneButtonsProps {
  product: Product;
  marketingProduct: MarketingProduct;
  fieldSchema: RelationFieldSchema;
}

export class CloneButtons extends Component<CloneButtonsProps> {
  @inject private _actionController: ActionController;
  @inject private _entityController: EntityController;
  @inject private _relationController: RelationController;

  private cloneProduct = async (e: any) => {
    e.stopPropagation();
    const { product, marketingProduct, fieldSchema } = this.props;
    const clonedProduct = await this._entityController.cloneRelatedEntity(
      marketingProduct,
      fieldSchema,
      product
    );
    await this._actionController.executeCustomAction(
      "Редактор dev",
      clonedProduct,
      fieldSchema.RelatedContent
    );
  };

  private cloneProductPrototype = async (e: any) => {
    e.stopPropagation();
    const { marketingProduct, fieldSchema } = this.props;
    const clonedProduct = await this._relationController.cloneProductPrototype(
      marketingProduct,
      fieldSchema
    );
    await this._actionController.executeCustomAction(
      "Редактор dev",
      clonedProduct,
      fieldSchema.RelatedContent
    );
  };

  render() {
    return (
      <>
        <MenuItem
          labelElement={<Icon icon="duplicate" />}
          intent={Intent.SUCCESS}
          onClick={this.cloneProduct}
          text="Клонировать"
          title="Клонировать текущую статью"
        />
        <MenuItem
          labelElement={<Icon icon="add" />}
          intent={Intent.SUCCESS}
          onClick={this.cloneProductPrototype}
          text="Создать по образцу"
          title="Создать статью по образцу"
        />
      </>
    );
  }
}
