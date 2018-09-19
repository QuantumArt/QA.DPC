import React, { Component } from "react";
import { computed, observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon, Checkbox, TabId } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { ExtensionEditor } from "Components/ArticleEditor/ArticleEditor";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldTabs,
  MultiRelationFieldAccordion,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { Product, FixConnectAction, DevicesForFixConnectAction } from "../ProductEditorSchema";
import { FederalTab } from "./FederalTab";

const productRegionsField = (device: Product) =>
  device.Regions.map(region => region.Title).join(", ");

const actionTitleField = (action: FixConnectAction) =>
  action.Parent && action.Parent.MarketingProduct && action.Parent.MarketingProduct.Title;

const actionRegionsField = (action: FixConnectAction) =>
  action.Parent && action.Parent.Regions.map(region => region.Title).join(", ");

interface LayoutProps {
  model: Product;
  contentSchema: ContentSchema;
}

@observer
export class Layout extends Component<LayoutProps> {
  @observable private filterByRegions = false;
  @observable private activatedTabIds: TabId[] = ["federal"];

  @action
  private handleTabChange = (newTabId: TabId) => {
    if (!this.activatedTabIds.includes(newTabId)) {
      this.activatedTabIds.push(newTabId);
    }
  };

  @action
  private setFilterByRegions = (e: any) => {
    this.filterByRegions = !!e.target.checked;
  };

  @computed
  private get fixTariffRegionIds(): { [serverId: number]: true } {
    const fixTraiff = this.props.model as Product;
    return fixTraiff.Regions.reduce((obj, region) => {
      obj[region._ServerId] = true;
      return obj;
    }, {});
  }

  private filterProductsByRegion = (product: Product) => {
    if (!this.filterByRegions) {
      return true;
    }
    const fixTariffRegionIds = this.fixTariffRegionIds;
    return product.getBaseValue("Regions").some(city => fixTariffRegionIds[city._ServerId]);
  };

  private filterActionsByRegion = (action: FixConnectAction) => {
    if (!this.filterByRegions) {
      return true;
    }
    const fixTariffRegionIds = this.fixTariffRegionIds;
    return (
      action.Parent &&
      action.Parent.getBaseValue("Regions").some(city => fixTariffRegionIds[city._ServerId])
    );
  };

  private getMarketingFixConnectTariffProps() {
    const { model, contentSchema } = this.props;

    const extension =
      model.MarketingProduct && model.MarketingProduct.Type_Contents.MarketingFixConnectTariff;

    const extensionSchema = ((contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent.Fields.Type as ExtensionFieldSchema).ExtensionContents
      .MarketingFixConnectTariff;

    return { extension, extensionSchema };
  }

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

  private renderRegional() {
    if (!this.activatedTabIds.includes("regional")) {
      return null;
    }
    const { model, contentSchema } = this.props;
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();
    const marketingProductSchema = (contentSchema.Fields.MarketingProduct as RelationFieldSchema)
      .RelatedContent;

    return (
      <>
        <EntityEditor
          model={model}
          contentSchema={contentSchema}
          titleField={(p: Product) => p.MarketingProduct && p.MarketingProduct.Title}
          fieldEditors={{
            MarketingProduct: IGNORE
          }}
          header
          buttons
        />
        {this.renderFilter()}
        {extension ? (
          <ExtensionEditor
            model={extension}
            contentSchema={extensionSchema}
            skipOtherFields
            fieldEditors={{
              MarketingInternetTariff: props => (
                <SingleRelationFieldTabs
                  {...props}
                  displayField={"Title"}
                  skipOtherFields
                  fieldEditors={{
                    Products: props => (
                      <MultiRelationFieldAccordion
                        {...props}
                        displayFields={[productRegionsField]}
                        filterItems={this.filterProductsByRegion}
                      />
                    )
                  }}
                />
              ),
              MarketingPhoneTariff: props => (
                <SingleRelationFieldTabs
                  {...props}
                  displayField={"Title"}
                  skipOtherFields
                  fieldEditors={{
                    Products: props => (
                      <MultiRelationFieldAccordion
                        {...props}
                        displayFields={[productRegionsField]}
                        filterItems={this.filterProductsByRegion}
                      />
                    )
                  }}
                />
              )
            }}
          />
        ) : null}
        {model.MarketingProduct ? (
          <EntityEditor
            model={model.MarketingProduct}
            contentSchema={marketingProductSchema}
            skipOtherFields
            fieldEditors={{
              FixConnectActions: props => (
                <MultiRelationFieldAccordion
                  {...props}
                  displayFields={[actionTitleField, actionRegionsField]}
                  filterItems={this.filterActionsByRegion}
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
        ) : null}
      </>
    );
  }

  private renderDevices() {
    if (!this.activatedTabIds.includes("devices")) {
      return null;
    }
    const { extension, extensionSchema } = this.getMarketingFixConnectTariffProps();

    return extension ? (
      <>
        {this.renderFilter()}
        <ExtensionEditor
          model={extension}
          contentSchema={extensionSchema}
          skipOtherFields
          fieldEditors={{
            MarketingDevices: props => (
              <MultiRelationFieldTabs
                {...props}
                vertical
                className="container-xl"
                displayField={"Title"}
                skipOtherFields
                fieldEditors={{
                  Products: props => (
                    <MultiRelationFieldAccordion
                      {...props}
                      displayFields={[productRegionsField]}
                      filterItems={this.filterProductsByRegion}
                      fieldOrders={["Type", "Regions", "Parameters"]}
                    />
                  )
                }}
              />
            )
          }}
        />
      </>
    ) : null;
  }

  private renderFilter() {
    return (
      <Col md className="pt-form-group">
        <Row>
          <Col xl={2} md={3} className="field-editor__label">
            <label htmlFor="filterByRegions">Фильтровать по списку регионов: </label>
          </Col>
          <Col>
            <Checkbox
              id="filterByRegions"
              checked={this.filterByRegions}
              onChange={this.setFilterByRegions}
            />
          </Col>
        </Row>
      </Col>
    );
  }
}
