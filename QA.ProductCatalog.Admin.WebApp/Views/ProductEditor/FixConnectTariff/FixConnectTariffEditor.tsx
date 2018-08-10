import React, { Component } from "react";
import { computed, observable, action } from "mobx";
import { observer } from "mobx-react";
import { Tabs, Tab, Icon, Checkbox } from "@blueprintjs/core";
import { Col, Row } from "react-flexbox-grid";
import { ExtensionEditor } from "Components/ArticleEditor/ArticleEditor";
import { EntityEditor, IGNORE } from "Components/ArticleEditor/EntityEditor";
import {
  SingleRelationFieldTabs,
  MultiRelationFieldTabs,
  MultiRelationFieldTable,
  MultiRelationFieldAccordion,
  SingleRelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import {
  ContentSchema,
  RelationFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { EntityObject } from "Models/EditorDataModels";
import { Product } from "./ProductEditorSchema";

interface FixConnectTariffEditorProps {
  model: EntityObject;
  contentSchema: ContentSchema;
}

@observer
export class FixConnectTariffEditor extends Component<FixConnectTariffEditorProps> {
  @observable private filterByRegions = false;

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
    return product.Regions.some(city => fixTariffRegionIds[city._ServerId]);
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
                    FixConnectActions: IGNORE as any,
                    MarketingTvPackage: SingleRelationFieldTable,
                    MarketingInternetTariff: SingleRelationFieldTable,
                    BonusTVPackages: SingleRelationFieldTable,
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
          MarketingProduct: IGNORE
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
      <>
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
        <ExtensionEditor
          model={extension}
          contentSchema={contentSchema}
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
                      displayFields={[
                        (device: Product) => device.Regions.map(region => region.Title).join(", ")
                      ]}
                      filterItems={this.filterProductsByRegion}
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
}
