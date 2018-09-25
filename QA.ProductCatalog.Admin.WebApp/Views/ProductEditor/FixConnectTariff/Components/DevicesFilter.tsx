import React, { Component } from "react";
import { observer } from "mobx-react";
import { Col, Row } from "react-flexbox-grid";
import { Switch, Alignment, TagInput, Button } from "@blueprintjs/core";
import { DevicesFilterModel } from "../Models/DevicesFilterModel";

interface DevicesFilterProps {
  model: DevicesFilterModel;
}

@observer
export class DevicesFilter extends Component<DevicesFilterProps> {
  render() {
    const { model } = this.props;
    return (
      <Col md className="devices-tab__filter">
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
          <Col md={6}>
            <TagInput
              fill
              large
              addOnBlur
              inputValue=""
              leftIcon="filter-list"
              values={model.regionsFilter}
              onChange={model.setRegionsFilter}
              tagProps={{ minimal: true }}
              inputProps={{ placeholder: "Фильтровать по регионам" }}
              rightElement={
                model.regionsFilter.length > 0 && (
                  <Button minimal icon="cross" onClick={model.clearRegionsFilter} />
                )
              }
            />
          </Col>
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
        </Row>
      </Col>
    );
  }
}
