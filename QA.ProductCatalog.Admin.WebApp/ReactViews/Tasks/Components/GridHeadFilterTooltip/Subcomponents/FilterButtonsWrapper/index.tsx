import { Button, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { Filter } from "../../../../TaskStore";
import { observer } from "mobx-react-lite";
import "./style.scss";

interface Props {
  filter?: Filter;
  children: React.ReactElement<any>;
  acceptLabel?: string;
  revokeLabel?: string;
}

export const FilterButtonsWrapper = observer(
  ({ filter, children, acceptLabel, revokeLabel }: Props) => {
    const [value, setValue] = useState(filter.value);
    return (
      <div className="filter-options-wrap">
        {React.Children.map(children, child => {
          return React.cloneElement(child, { setValue, value });
        })}

        <div className="filter-options-wrap__buttons-wrap">
          <Button
            intent={Intent.PRIMARY}
            outlined
            className="filter-options-wrap__button"
            onClick={() => {
              filter.setValue(value);
              filter.toggleActive(true);
            }}
          >
            {acceptLabel || "Применить"}
          </Button>

          <Button
            outlined
            disabled={!filter.isActive}
            className="filter-options-wrap__button"
            onClick={() => {
              filter.setValue(null);
              filter.toggleActive(false);
            }}
          >
            {revokeLabel || "Отключить"}
          </Button>
        </div>
      </div>
    );
  }
);
