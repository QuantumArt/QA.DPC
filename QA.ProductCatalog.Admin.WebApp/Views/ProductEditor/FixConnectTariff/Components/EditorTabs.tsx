import React, { Component } from "react";
import { observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon, TabId } from "@blueprintjs/core";
import { ContentSchema } from "ProductEditor/Models/EditorSchemaModels";
import { Product } from "../TypeScriptSchema";
import { FederalTab } from "./FederalTab";
import { RegionalTab } from "./RegionalTab";
import { DevicesTab } from "./DevicesTab";
import { ActionsTab } from "./ActionsTab";

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
          <Icon icon="locate" iconSize={Icon.SIZE_LARGE} />
          <span>Региональные характеристики</span>
        </Tab>
        <Tab
          id="actions"
          panel={this.activatedTabIds.includes("actions") && <ActionsTab {...this.props} />}
        >
          <Icon icon="flag" iconSize={Icon.SIZE_LARGE} />
          <span>Действующие акции</span>
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
}
