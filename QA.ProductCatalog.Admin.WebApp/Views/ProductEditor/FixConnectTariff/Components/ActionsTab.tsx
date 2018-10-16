import React, { Component } from "react";
import { observer } from "mobx-react";
import { Divider } from "@blueprintjs/core";
import { ContentSchema, RelationFieldSchema } from "Models/EditorSchemaModels";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  MultiRelationFieldTable,
  RelationFieldTabs,
  SingleRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { by, asc } from "Utils/Array/Sort";
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

interface ActionsTabTabProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class ActionsTab extends Component<ActionsTabTabProps> {
  private filterModel = new FilterModel(this.props.model);

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
      sortItems={by(
        asc((action: FixConnectAction) => action.Parent.MarketingProduct._ServerId),
        asc((action: FixConnectAction) => action.Parent._ServerId)
      )}
      columnProportions={[2, 5]}
      displayFields={[actionTitleDisplayField, actionRegionsDisplayField]}
      fieldOrders={["Parent", "PromoPeriod", "AfterPromo", "MarketingOffers"]}
      fieldEditors={{
        Parent: this.renderActionParent,
        MarketingOffers: MultiRelationFieldTable
      }}
      canClonePrototype
      canCloneEntity
      canRemoveEntity
    />
  );

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
            "Modifiers",
            "StartDate",
            "PDF",
            "EndDate",
            "ActionMarketingDevices"
          ]}
          fieldEditors={{
            MarketingProduct: SingleRelationFieldTags,
            // TODO: Parameters: this.renderActionParameters
            ActionMarketingDevices: this.renderDevices
          }}
        />
      )
    );
  };

  private renderDevices = (props: FieldEditorProps) => (
    <RelationFieldTabs
      {...props}
      vertical
      titleField={deviceDisplayField}
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
  device.Parent && device.Parent.Title;
