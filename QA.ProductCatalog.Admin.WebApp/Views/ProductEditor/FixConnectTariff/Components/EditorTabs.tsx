import React, { Component } from "react";
import { observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon, TabId } from "@blueprintjs/core";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldAccordion,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import { ContentSchema, RelationFieldSchema } from "Models/EditorSchemaModels";
import { Product, FixConnectAction, DevicesForFixConnectAction } from "../ProductEditorSchema";
import { FederalTab } from "./FederalTab";
import { DevicesTab } from "./DevicesTab";
import { RegionalTab } from "./RegionalTab";

const actionTitleField = (action: FixConnectAction) =>
  action.Parent && action.Parent.MarketingProduct && action.Parent.MarketingProduct.Title;

const actionRegionsField = (action: FixConnectAction) =>
  action.Parent && action.Parent.Regions.map(region => region.Title).join(", ");

interface EditorTabsProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class EditorTabs extends Component<EditorTabsProps> {
  @observable private activatedTabIds: TabId[] = ["federal"];

  @action
  private handleTabChange = (newTabId: TabId) => {
    if (!this.activatedTabIds.includes(newTabId)) {
      this.activatedTabIds.push(newTabId);
    }
  };

  render() {
    return (
      <Tabs id="layout" large onChange={this.handleTabChange}>
        <Tab
          id="federal"
          panel={this.activatedTabIds.includes("federal") && <FederalTab {...this.props} />}
        >
          <Icon icon="globe" iconSize={Icon.SIZE_LARGE} />
          <span>Общефедеральные характеристики</span>
        </Tab>
        <Tab
          id="regional"
          panel={this.activatedTabIds.includes("regional") && <RegionalTab {...this.props} />}
        >
          <Icon icon="flag" iconSize={Icon.SIZE_LARGE} />
          <span>Региональные характеристики и действующие акции</span>
        </Tab>
        <Tab
          id="devices"
          panel={this.activatedTabIds.includes("devices") && <DevicesTab {...this.props} />}
        >
          <Icon icon="projects" iconSize={Icon.SIZE_LARGE} />
          <span>Оборудование</span>
        </Tab>
      </Tabs>
    );
  }

  // TODO: FixConnectActions
  // TODO: ActionMarketingDevices
  private renderActions() {
    if (!this.activatedTabIds.includes("regional")) {
      return null;
    }
    const { model, contentSchema } = this.props;
    const marketingProductSchema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent;

    return (
      <ArticleEditor
        model={model.MarketingProduct}
        contentSchema={marketingProductSchema}
        skipOtherFields
        fieldEditors={{
          FixConnectActions: props => (
            <MultiRelationFieldAccordion
              {...props}
              displayFields={[actionTitleField, actionRegionsField]}
              // filterItems={this.filterActionsByRegion}
              fieldEditors={{
                Parent: props => (
                  <SingleRelationFieldTabs
                    {...props}
                    fieldEditors={{
                      MarketingProduct: SingleRelationFieldTable,
                      ActionMarketingDevices: props => (
                        <MultiRelationFieldAccordion
                          {...props}
                          displayFields={[
                            (actionDevice: DevicesForFixConnectAction) =>
                              actionDevice.MarketingDevice && actionDevice.MarketingDevice.Title
                          ]}
                          fieldEditors={{ MarketingDevice: SingleRelationFieldTable }}
                        />
                      )
                    }}
                  />
                )
              }}
            />
          )
        }}
      />
    );
  }
}
