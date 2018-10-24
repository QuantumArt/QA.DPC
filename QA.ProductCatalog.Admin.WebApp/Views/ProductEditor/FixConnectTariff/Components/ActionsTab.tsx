import React, { Component } from "react";
import { observer } from "mobx-react";
import { consumer, inject } from "react-ioc";
import { Divider, Button, Intent } from "@blueprintjs/core";
import { ContentSchema, RelationFieldSchema } from "Models/EditorSchemaModels";
import { PublicationContext } from "Services/PublicationContext";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  MultiRelationFieldTable,
  RelationFieldTabs,
  SingleRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { makePublicatoinStatusIcons } from "Components/PublicationStatusIcon/PublicationStatusIcon";
import { by, asc } from "Utils/Array";
import {
  Product,
  FixConnectAction,
  DevicesForFixConnectAction,
  ProductRelation
} from "../TypeScriptSchema";
import { hasUniqueMarketingDevice } from "../Utils/Validators";
import { FilterModel } from "../Models/FilterModel";
import { FilterBlock } from "./FilterBlock";
import { ParameterFields } from "./ParameterFields";
import { action } from "mobx";

interface ActionsTabTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@consumer
@observer
export class ActionsTab extends Component<ActionsTabTabProps> {
  @inject private publicationContext: PublicationContext;

  private filterModel = new FilterModel(this.props.model);

  @action
  private pinActionToMarketingTariff(fixConnectAction: FixConnectAction) {
    const marketingTariff = this.props.model.MarketingProduct;
    if (!fixConnectAction.MarketingOffers.includes(marketingTariff)) {
      fixConnectAction.MarketingOffers.push(marketingTariff);
      fixConnectAction.setTouched("MarketingOffers");
    }
  }

  render() {
    const { model, contentSchema } = this.props;
    const marketingProductSchema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent;

    return (
      <>
        <FilterBlock model={this.filterModel} />
        <Divider />
        <ArticleEditor
          model={model.MarketingProduct}
          contentSchema={marketingProductSchema}
          skipOtherFields
          fieldEditors={{
            FixConnectActions: this.renderActions
          }}
        />
      </>
    );
  }

  private renderActions = (props: FieldEditorProps) => (
    <RelationFieldAccordion
      {...props}
      filterItems={this.filterModel.filterActions}
      highlightItems={this.filterModel.highlightAction}
      sortItems={by(
        asc((action: FixConnectAction) => action.Parent.MarketingProduct._ServerId),
        asc((action: FixConnectAction) => action.Parent._ServerId)
      )}
      columnProportions={[4, 10, 1]}
      displayFields={[
        actionTitleDisplayField,
        actionRegionsDisplayField,
        makePublicatoinStatusIcons(this.publicationContext, props.fieldSchema)
      ]}
      fieldOrders={["Parent", "PromoPeriod", "AfterPromo", "MarketingOffers"]}
      fieldEditors={{
        Parent: this.renderActionParent,
        MarketingOffers: this.renderMarketingOffers
      }}
      canClonePrototype
      canCloneEntity
      canRemoveEntity
      canSelectRelation
      onClonePrototype={async clonePrototype => {
        await clonePrototype();
        props.model.setChanged(props.fieldSchema.FieldName, false);
      }}
      onSelectRelation={async selectRelation => {
        await selectRelation();
        props.model.setChanged(props.fieldSchema.FieldName, false);
      }}
    />
  );

  private renderMarketingOffers = (props: FieldEditorProps) => {
    const fixAction = props.model as FixConnectAction;
    const marketingTariff = this.props.model.MarketingProduct;
    return (
      <MultiRelationFieldTable
        {...props}
        relationActions={() => (
          <>
            {!fixAction.MarketingOffers.includes(marketingTariff) && (
              <Button
                minimal
                small
                rightIcon="pin"
                intent={Intent.PRIMARY}
                onClick={() => this.pinActionToMarketingTariff(props.model as FixConnectAction)}
                title="Привязать к текущему маркетинговому тарифу фиксированной связи"
              >
                Привязать к текущему тарифу
              </Button>
            )}
          </>
        )}
      />
    );
  };

  private renderActionParent = ({ model, fieldSchema }: FieldEditorProps) => {
    const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
    const product = model[fieldSchema.FieldName] as Product;
    return (
      product && (
        <ArticleEditor
          model={product}
          contentSchema={contentSchema}
          fieldOrders={[
            "MarketingProduct",
            "Description",
            "Regions",
            "Parameters",
            "Advantages",
            "ActionMarketingDevices",
            "Modifiers",
            "PDF",
            "StartDate",
            "Priority",
            "EndDate",
            "SortOrder"
          ]}
          fieldEditors={{
            MarketingProduct: SingleRelationFieldTags,
            Parameters: this.renderActionParameters,
            ActionMarketingDevices: this.renderDevices
          }}
        />
      )
    );
  };

  private renderActionParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        {
          Title: "Скидка на цену предложения, %",
          BaseParam: "SubscriptionFee",
          Modifiers: ["PercentDiscount"]
        },
        {
          Title: "Скидка на цену предложения, руб.",
          Unit: "rub",
          BaseParam: "SubscriptionFee",
          Modifiers: ["Discount"]
        }
      ]}
    />
  );

  private renderDevices = (props: FieldEditorProps) => (
    <RelationFieldTabs
      {...props}
      vertical
      titleField={deviceTitleField}
      displayField={deviceDisplayField}
      fieldOrders={["MarketingDevice", "Parent"]}
      fieldEditors={{
        MarketingDevice: this.renderMarketingDevice,
        Parent: this.renderDeviceParent,
        FixConnectAction: IGNORE
      }}
      canClonePrototype
      canRemoveEntity
      onShowEntity={device => device.setTouched("MarketingDevice")}
    />
  );

  private renderMarketingDevice = (props: FieldEditorProps) => {
    const device = props.model as DevicesForFixConnectAction;
    return <SingleRelationFieldTags {...props} validate={hasUniqueMarketingDevice(device)} />;
  };

  private renderDeviceParent = ({ model, fieldSchema }: FieldEditorProps) => {
    const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
    const productRelation = model[fieldSchema.FieldName] as ProductRelation;
    return (
      productRelation && (
        <ArticleEditor
          model={productRelation}
          contentSchema={contentSchema}
          fieldOrders={["Title", "Parameters"]}
          fieldEditors={{
            Parameters: this.renderDeviceParameters
          }}
        />
      )
    );
  };

  private renderDeviceParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[{ Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" }]}
    />
  );
}

const actionTitleDisplayField = (action: FixConnectAction) =>
  action.Parent && action.Parent.MarketingProduct && action.Parent.MarketingProduct.Title;

const actionRegionsDisplayField = (action: FixConnectAction) =>
  action.Parent && (
    <div className="products-accordion__regions">
      {action.Parent.Regions.map(region => region.Title).join(", ")}
    </div>
  );

const deviceDisplayField = (device: DevicesForFixConnectAction) =>
  device.MarketingDevice && device.MarketingDevice.Title;

const deviceTitleField = (device: DevicesForFixConnectAction) =>
  device.Parent && device.Parent.Title;
