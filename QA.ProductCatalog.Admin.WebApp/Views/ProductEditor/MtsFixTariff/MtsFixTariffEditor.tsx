import React, { Component } from "react";
import { observable, observe, action, Lambda, untracked, IArrayChange, IArraySplice } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon } from "@blueprintjs/core";
import { ArticleEditor, IGNORE } from "Components/ArticleEditor/ArticleEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldTags,
  MultiRelationFieldTabs,
  MultiRelationFieldTable,
  SingleRelationFieldTags
} from "Components/FieldEditors/FieldEditors";
import { ContentSchema, RelationFieldSchema } from "Models/EditorSchemaModels";
import { EntityObject } from "Models/EditorDataModels";
import { Product, DeviceOnTariffs, Region } from "./ProductEditorSchema";

interface MtsFixTariffEditorProps {
  model: EntityObject;
  contentSchema: ContentSchema;
}

@observer
export class MtsFixTariffEditor extends Component<MtsFixTariffEditorProps> {
  private _selectedRegionIds = observable.map<string, true>();
  private _regionsArrayObserver: Lambda;

  componentDidMount() {
    const { model } = this.props;
    this._regionsArrayObserver = observe(
      untracked(() => model.Regions),
      action((change: IArraySplice | IArrayChange) => {
        if (change.type === "splice") {
          if (change.removedCount > 0) {
            change.removed.forEach(oldValue => {
              // oldValue.snapshot хранит Region._ClientId
              this._selectedRegionIds.delete(String(oldValue.snapshot));
            });
          }
        } else {
          // oldValue.snapshot хранит Region._ClientId
          this._selectedRegionIds.delete(String(change.oldValue.snapshot));
        }
      })
    );
  }

  componentWillUnmount() {
    if (this._regionsArrayObserver) {
      this._regionsArrayObserver();
    }
  }

  @action
  private toggleRegion = (_e: any, region: Region) => {
    const regionId = String(region._ClientId);
    const isSelected = this._selectedRegionIds.has(regionId);
    if (isSelected) {
      this._selectedRegionIds.delete(regionId);
    } else {
      this._selectedRegionIds.set(regionId, true);
    }
  };

  private filterDevicesByRegions = (device: DeviceOnTariffs) => {
    if (this._selectedRegionIds.size === 0) {
      return true;
    }
    return device.Cities.some(city => this._selectedRegionIds.has(String(city._ClientId)));
  };

  render() {
    return (
      <Tabs id="layout" large>
        <Tab id="federal" panel={this.renderFederal()}>
          <Icon icon="globe" iconSize={Icon.SIZE_LARGE} />
          <span>Общефедеральные характеристики</span>
        </Tab>
        <Tab id="regional" panel={this.renderRegional()}>
          <Icon icon="flag" iconSize={Icon.SIZE_LARGE} />
          <span>Региональные характеристики и действующие акции</span>
        </Tab>
        <Tab id="devices" panel={this.renderDevices()}>
          <Icon icon="projects" iconSize={Icon.SIZE_LARGE} />
          <span>Оборудование</span>
        </Tab>
      </Tabs>
    );
  }

  private renderFederal() {
    const { model, contentSchema } = this.props;
    return (
      <ArticleEditor
        model={model}
        contentSchema={contentSchema}
        skipOtherFields
        fieldEditors={{
          MarketingProduct: props => (
            <SingleRelationFieldTabs
              borderless
              fieldEditors={{
                DevicesOnMarketingTariff: IGNORE,
                Type_Contents: {
                  MarketingFixConnectTariff: {
                    Category: SingleRelationFieldTags,
                    MarketingDevices: MultiRelationFieldTable
                  }
                }
              }}
              {...props}
            />
          )
        }}
      />
    );
  }

  private renderRegional() {
    const { model, contentSchema } = this.props;
    return (
      <ArticleEditor
        model={model}
        contentSchema={contentSchema}
        titleField={(p: Product) => p.MarketingProduct && p.MarketingProduct.Title}
        fieldEditors={{
          MarketingProduct: IGNORE,
          Regions: props => (
            <MultiRelationFieldTags
              selectMultiple
              orderByField="Title"
              onClick={this.toggleRegion}
              {...props}
            />
          )
        }}
        header
        buttons
      />
    );
  }

  private renderDevices() {
    let { model, contentSchema } = this.props;
    model = model.MarketingProduct;
    contentSchema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema).Content;
    return model ? (
      <ArticleEditor
        model={model}
        contentSchema={contentSchema}
        skipOtherFields
        fieldEditors={{
          DevicesOnMarketingTariff: props => (
            <MultiRelationFieldTabs
              {...props}
              displayField={(device: DeviceOnTariffs) => device.Parent && device.Parent.Title}
              filterItems={this.filterDevicesByRegions}
              vertical
            />
          )
        }}
      />
    ) : null;
  }
}
