import React, { Component } from "react";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Switch, Alignment } from "@blueprintjs/core";
import ReactSelect, { Options, Option } from "react-select";
import { FilterModel } from "../Models/FilterModel";
import { consumer, inject } from "react-ioc";
import { DataContext } from "Services/DataContext";
import { computed } from "mobx";
import { Region } from "../ProductEditorSchema";

interface FilterBlockProps {
  model: FilterModel;
  byMarketingTariff?: boolean;
}

@consumer
@observer
export class FilterBlock extends Component<FilterBlockProps> {
  @inject private _dataContext: DataContext;

  @computed
  private get options() {
    const options: Options<number> = [];
    for (const entity of this._dataContext.store.Region.values()) {
      const region = entity as Region;
      options.push({
        value: region._ClientId,
        label: region.Title
      });
    }
    return options;
  }

  private handleRegionsFilterChange = (selection: Option<number>[]) => {
    this.props.model.setSelectedRegionIds(selection.map(option => option.value));
  };

  render() {
    const { model, byMarketingTariff } = this.props;
    return (
      <Col md className="devices-filter">
        <Row>
          <Col md={3}>
            <Switch
              large
              alignIndicator={Alignment.RIGHT}
              checked={model.filterByTariffRegions}
              label=""
              onChange={model.toggleFilterByTariffRegions}
            >
              Фильтровать по регионам <br /> тарифа фиксированной связи
            </Switch>
          </Col>
          <Col md={6} mdOffset={byMarketingTariff ? 0 : 3}>
            <ReactSelect
              multi
              clearable
              className="Select--large"
              placeholder="Фильтровать по регионам"
              noResultsText="Ничего не найдено"
              options={this.options}
              value={model.selectedRegionIds}
              onChange={this.handleRegionsFilterChange}
            />
          </Col>
          {byMarketingTariff && (
            <Col md={3}>
              <Switch
                large
                alignIndicator={Alignment.RIGHT}
                checked={model.filterByMarketingTariff}
                onChange={model.toggleFilterByMarketingTariff}
              >
                Фильтровать по маркетинговому <br /> тарифу фиксированной связи
              </Switch>
            </Col>
          )}
        </Row>
      </Col>
    );
  }
}
