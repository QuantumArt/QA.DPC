import React, { Component } from "react";
import { action } from "mobx";
import { Button, Intent } from "@blueprintjs/core";
import { ArticleEditor, FieldEditorProps, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  RelationFieldAccordion,
  MultiRelationFieldTable,
  HighlightMode,
  MultiRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { by, desc, asc } from "Utils/Array";
import { DeviceOnTariffs, ProductRelation, MarketingProduct } from "../TypeScriptSchema";
import { FilterModel } from "../Models/FilterModel";
import {
  hasUniqueCities,
  isUniqueCity,
  isUniqueMarketingTariff,
  itemHasUniqueCities
} from "../Utils/DeviceOnTariffsValidators";
import { ParameterFields } from "./ParameterFields";

interface DevicesOnTariffsBlockProps extends FieldEditorProps {
  filterModel: FilterModel;
  marketingTariff: MarketingProduct;
}

export class DevicesOnTariffsBlock extends Component<DevicesOnTariffsBlockProps> {
  @action
  private pinDeviceToMarketingTariff(deviceOnTariffs: DeviceOnTariffs) {
    const { marketingTariff } = this.props;
    if (!deviceOnTariffs.MarketingTariffs.includes(marketingTariff)) {
      deviceOnTariffs.MarketingTariffs.push(marketingTariff);
      deviceOnTariffs.setTouched("MarketingTariffs");
    }
  }

  render() {
    const { model, fieldSchema, filterModel } = this.props;
    const marketingDevice = model as MarketingProduct;
    return (
      <RelationFieldAccordion
        model={model}
        fieldSchema={fieldSchema}
        canCloneEntity
        canRemoveEntity
        canClonePrototype
        columnProportions={[3, 1, 1]}
        displayFields={[regionsDisplayField, rentPriceDisplayField, salePriceDisplayField]}
        filterItems={filterModel.filterDevicesOnTariffs}
        highlightItems={filterModel.highlightDeviceOnTariffs}
        validateItems={itemHasUniqueCities(marketingDevice.DevicesOnTariffs)}
        fieldOrders={["Cities", "Parent", "MarketingTariffs"]}
        fieldEditors={{
          MarketingDevice: IGNORE,
          Cities: this.renderCities,
          Parent: this.renderParent,
          MarketingTariffs: this.renderMarketingTariffs
        }}
        onMountEntity={(device: DeviceOnTariffs) => device.setTouched("Cities")}
      />
    );
  }

  private renderCities = (props: FieldEditorProps) => {
    const device = props.model as DeviceOnTariffs;
    const marketingDevice = this.props.model as MarketingProduct;
    return (
      <MultiRelationFieldTags
        {...props}
        sortItemsBy="Title"
        validate={hasUniqueCities(device, marketingDevice.DevicesOnTariffs)}
        validateItems={isUniqueCity(device, marketingDevice.DevicesOnTariffs)}
      />
    );
  };

  private renderParent = ({ model, fieldSchema }: FieldEditorProps) => {
    const contentSchema = (fieldSchema as RelationFieldSchema).RelatedContent;
    const productRelation = model.Parent as ProductRelation;
    return (
      productRelation && (
        <ArticleEditor
          model={productRelation}
          contentSchema={contentSchema}
          fieldOrders={["Title", "Parameters"]}
          fieldEditors={{
            Parameters: this.renderParameters
          }}
        />
      )
    );
  };

  private renderMarketingTariffs = (props: FieldEditorProps) => {
    const device = props.model as DeviceOnTariffs;
    const marketingDevice = this.props.model as MarketingProduct;
    const { marketingTariff } = this.props;
    return (
      <MultiRelationFieldTable
        {...props}
        highlightItems={(deviceTariff: MarketingProduct) =>
          deviceTariff === marketingTariff ? HighlightMode.Highlight : HighlightMode.None
        }
        sortItems={by(
          desc((deviceTariff: MarketingProduct) => deviceTariff === marketingTariff),
          asc((deviceTariff: MarketingProduct) => deviceTariff.Title)
        )}
        validateItems={isUniqueMarketingTariff(device, marketingDevice.DevicesOnTariffs)}
        relationActions={() => (
          <>
            {!device.MarketingTariffs.includes(marketingTariff) && (
              <Button
                minimal
                small
                rightIcon="pin"
                intent={Intent.PRIMARY}
                onClick={() => this.pinDeviceToMarketingTariff(device)}
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

  private renderParameters = (props: FieldEditorProps) => (
    <ParameterFields
      {...props}
      fields={[
        { Title: "Цена аренды", Unit: "rub_month", BaseParam: "RentPrice" },
        { Title: "Цена продажи", Unit: "rub", BaseParam: "SalePrice" }
      ]}
    />
  );
}

const regionsDisplayField = (device: DeviceOnTariffs) => (
  <div className="products-accordion__regions">
    {device.Cities.map(region => region.Title).join(", ")}
  </div>
);

const rentPriceDisplayField = (device: DeviceOnTariffs) => {
  const parameter = device.Parent.Parameters.find(parameter => {
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

const salePriceDisplayField = (device: DeviceOnTariffs) => {
  const parameter = device.Parent.Parameters.find(parameter => {
    const baseParameter = parameter.BaseParameter;
    return baseParameter && baseParameter.Alias === "SalePrice";
  });
  return (
    parameter &&
    parameter.NumValue !== null && (
      <>
        <div>Цена продажи:</div>
        <div>
          {parameter.NumValue} {parameter.Unit && parameter.Unit.Title}
        </div>
      </>
    )
  );
};
