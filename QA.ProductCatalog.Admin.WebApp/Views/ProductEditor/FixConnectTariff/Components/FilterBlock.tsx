import React, { Component } from "react";
import ReactSelect, { Options, Option } from "react-select";
import { inject } from "react-ioc";
import { computed } from "mobx";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Switch, Alignment } from "@blueprintjs/core";
import { DataContext } from "ProductEditor/Services/DataContext";
import { FilterModel } from "../Models/FilterModel";
import { Tables } from "../TypeScriptSchema";

interface FilterBlockProps {
  filterModel: FilterModel;
  byMarketingTariff?: boolean;
}

@observer
export class FilterBlock extends Component<FilterBlockProps> {
  @inject private _dataContext: DataContext<Tables>;

  @computed
  private get options() {
    const options: Options<number> = [];
    for (const region of this._dataContext.tables.Region.values()) {
      options.push({
        value: region._ClientId,
        label: region.Title
      });
    }
    return options;
  }

  private handleRegionsFilterChange = (selection: Option<number>[]) => {
    this.props.filterModel.setSelectedRegionIds(selection.map(option => option.value));
  };

  render() {
    const { filterModel, byMarketingTariff } = this.props;
    return (
      <Col md className="devices-filter">
        <Row>
          <Col md={3}>
            <Switch
              large
              alignIndicator={Alignment.RIGHT}
              checked={filterModel.filterByTariffRegions}
              label=""
              onChange={filterModel.toggleFilterByTariffRegions}
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
              value={filterModel.selectedRegionIds}
              onChange={this.handleRegionsFilterChange}
            />
          </Col>
          {byMarketingTariff && (
            <Col md={3}>
              <Switch
                large
                alignIndicator={Alignment.RIGHT}
                checked={filterModel.filterByMarketingTariff}
                onChange={filterModel.toggleFilterByMarketingTariff}
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
