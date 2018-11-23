import React, { Component } from "react";
import { inject } from "react-ioc";
import { action } from "mobx";
import { Intent, Icon, MenuItem } from "@blueprintjs/core";
import { FieldEditorProps, ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { PublicationStatusIcons } from "Components/PublicationStatusIcons/PublicationStatusIcons";
import {
  RelationFieldAccordion,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { PublicationContext } from "Services/PublicationContext";
import { EntityController } from "Services/EntityController";
import { by, asc } from "Utils/Array";
import { Product, FixConnectAction, MarketingProduct } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import { onlyOneItemPerRegionHasDevices } from "../Utils/ActionDeviceValidators";
import { ParameterFields } from "./ParameterFields";

interface ActionsByDeviceBlockProps {
  marketingDevice: MarketingProduct;
  marketingTariff: MarketingProduct;
  actionsFieldSchema: RelationFieldSchema;
  filterModel: FilterModel;
}

export class ActionsByDeviceBlock extends Component<ActionsByDeviceBlockProps> {
  @inject private publicationContext: PublicationContext;
  @inject private entityController: EntityController;

  @action
  private async saveActionDevice(e: any, action: FixConnectAction) {
    e.stopPropagation();
    const { marketingDevice, actionsFieldSchema } = this.props;
    const actionDevice = action.Parent.ActionMarketingDevices.find(
      device => device.MarketingDevice === marketingDevice
    );

    const productRelation = actionDevice && actionDevice.Parent;

    const productRelationContentSchema = (((actionsFieldSchema.RelatedContent.Fields
      .Parent as RelationFieldSchema).RelatedContent.Fields
      .ActionMarketingDevices as RelationFieldSchema).RelatedContent.Fields
      .Parent as RelationFieldSchema).RelatedContent;

    if (productRelation) {
      await this.entityController.saveEntity(productRelation, productRelationContentSchema);
    }
  }

  render() {
    const { marketingDevice, marketingTariff, actionsFieldSchema, filterModel } = this.props;
    return (
      <RelationFieldAccordion
        model={marketingTariff}
        fieldSchema={actionsFieldSchema}
        filterItems={filterModel.filterDeviceActions(marketingDevice)}
        highlightItems={filterModel.highlightAction}
        sortItems={by(
          asc(
            (action: FixConnectAction) =>
              action.Parent.MarketingProduct && action.Parent.MarketingProduct._ServerId
          ),
          asc((action: FixConnectAction) => action.Parent._ServerId)
        )}
        columnProportions={[4, 8, 3, 1]}
        displayFields={[
          titleDisplayField,
          regionsDisplayField,
          rentPriceDisplayField(marketingDevice),
          (action: FixConnectAction) => (
            <PublicationStatusIcons
              model={action}
              product={action.Parent}
              contentSchema={actionsFieldSchema.RelatedContent}
              publicationContext={this.publicationContext}
            />
          )
        ]}
        skipOtherFields
        fieldEditors={{
          Parent: this.renderActionParent
        }}
        validateItems={onlyOneItemPerRegionHasDevices(marketingTariff.FixConnectActions)}
        canSaveEntity={false}
        entityActions={(action: FixConnectAction) => (
          <MenuItem
            labelElement={<Icon icon="floppy-disk" />}
            intent={Intent.PRIMARY}
            onClick={e => this.saveActionDevice(e, action)}
            text="Сохранить"
            title="Сохранить акционное оборудование"
          />
        )}
      />
    );
  }

  private renderActionParent = ({ model, fieldSchema }: FieldEditorProps) => {
    const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
    const product = model.Parent as Product;
    return (
      product && (
        <ArticleEditor
          model={product}
          contentSchema={contentSchema}
          skipOtherFields
          fieldOrders={["Regions", "ActionMarketingDevices"]}
          fieldEditors={{
            Regions: this.renderActionRegions,
            ActionMarketingDevices: this.renderActionDevice
          }}
        />
      )
    );
  };

  private renderActionRegions = (props: FieldEditorProps) => {
    return <MultiRelationFieldTags {...props} readonly sortItemsBy="Title" />;
  };

  private renderActionDevice = (props: FieldEditorProps) => {
    const { marketingDevice } = this.props;
    const actionParent = props.model as Product;
    const actionDevice = actionParent.ActionMarketingDevices.find(
      device => device.MarketingDevice === marketingDevice
    );

    const productRelationContentSchema = ((props.fieldSchema as RelationFieldSchema).RelatedContent
      .Fields.Parent as RelationFieldSchema).RelatedContent;

    return (
      actionDevice &&
      actionDevice.Parent && (
        <ArticleEditor
          model={actionDevice.Parent}
          contentSchema={productRelationContentSchema}
          fieldOrders={["Title", "Parameters"]}
          fieldEditors={{
            Parameters: this.renderActionDeviceParameters
          }}
        />
      )
    );
  };

  private renderActionDeviceParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[{ Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" }]}
    />
  );
}

const titleDisplayField = (action: FixConnectAction) =>
  action.Parent && action.Parent.MarketingProduct && action.Parent.MarketingProduct.Title;

const regionsDisplayField = (action: FixConnectAction) =>
  action.Parent && (
    <div className="products-accordion__regions products-accordion__regions--action">
      {action.Parent.Regions.map(region => region.Title).join(", ")}
    </div>
  );

const rentPriceDisplayField = (marketingDevice: MarketingProduct) => (action: FixConnectAction) => {
  const device =
    action.Parent &&
    action.Parent.ActionMarketingDevices.find(device => device.MarketingDevice === marketingDevice);
  const parameter =
    device &&
    device.Parent.Parameters.find(parameter => {
      const baseParameter = parameter.BaseParameter;
      return baseParameter && baseParameter.Alias === "RentPrice";
    });
  return (
    parameter &&
    parameter.NumValue !== null && (
      <>
        <div>Цена аренды:</div>
        <div>
          {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
        </div>
      </>
    )
  );
};
