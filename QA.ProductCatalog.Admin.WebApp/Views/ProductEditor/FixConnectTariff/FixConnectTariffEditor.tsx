import React, { Component } from "react";
import { observable, observe, action, Lambda, untracked, IArrayChange, IArraySplice } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon } from "@blueprintjs/core";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldTags,
  MultiRelationFieldTabs,
  MultiRelationFieldTable,
  MultiRelationFieldAccordion
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { EntityObject } from "Models/EditorDataModels";
import { Product, Region } from "./ProductEditorSchema";
import { ExtensionEditor } from "Components/ArticleEditor/ArticleEditor";

interface FixConnectTariffEditorProps {
  model: EntityObject;
  contentSchema: ContentSchema;
}

@observer
export class FixConnectTariffEditor extends Component<FixConnectTariffEditorProps> {
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

  private filterProductsByRegion = (product: Product) => {
    if (this._selectedRegionIds.size === 0) {
      return true;
    }
    return product.Regions.some(city => this._selectedRegionIds.has(String(city._ClientId)));
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
      <EntityEditor
        model={model}
        contentSchema={contentSchema}
        skipOtherFields
        fieldEditors={{
          MarketingProduct: props => (
            <SingleRelationFieldTabs
              borderless
              fieldEditors={{
                Type_Contents: {
                  MarketingFixConnectTariff: {
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
      <EntityEditor
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
    const extension = model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;
    contentSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema).RelatedContent
      .Fields.Type as ExtensionFieldSchema).ExtensionContents.MarketingFixConnectTariff;
    return extension ? (
      <ExtensionEditor
        model={extension}
        contentSchema={contentSchema}
        skipOtherFields
        fieldEditors={{
          MarketingDevices: props => (
            <MultiRelationFieldAccordion
              {...props}
              displayFields={["Title"]}
              fieldEditors={{
                Products: props => (
                  <MultiRelationFieldTabs
                    {...props}
                    displayField={(device: Product) => device._ServerId}
                    filterItems={this.filterProductsByRegion}
                    vertical
                  />
                )
              }}
            />
          )
        }}
      />
    ) : null;
  }
}
