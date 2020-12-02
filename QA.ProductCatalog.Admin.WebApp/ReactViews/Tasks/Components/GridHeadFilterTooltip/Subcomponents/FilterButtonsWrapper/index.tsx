import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { observer } from "mobx-react-lite";
import { IconNames } from "@blueprintjs/icons";
import { Filter } from "Tasks/TaskStore";
import "./Style.scss";

interface Props {
  filter?: Filter;
  children: React.ReactElement;
  acceptLabel?: string;
  revokeLabel?: string;
}

export const FilterButtonsWrapper = observer(
  ({ filter, children, acceptLabel, revokeLabel }: Props) => {
    const [value, setValue] = useState<string>(filter.value);
    return (
      <div className="filter-options-wrap">
        {React.Children.map(children, child => {
          return React.cloneElement(child, {
            setValue,
            value,
            validationResult: filter.validationResult
          });
        })}

        <ButtonGroup className="filter-options-wrap__buttons-wrap" fill>
          <Button
            intent={Intent.PRIMARY}
            icon={IconNames.CONFIRM}
            outlined
            onClick={() => {
              filter.setValue(value);
              filter.toggleActive(true);
            }}
          >
            {acceptLabel || "Применить"}
          </Button>
          <Button
            intent={Intent.PRIMARY}
            disabled={!filter.isActive}
            outlined
            icon={IconNames.REMOVE}
            onClick={() => {
              filter.setValue(null);
              filter.toggleActive(false);
            }}
          >
            {revokeLabel || "Отключить"}
          </Button>
        </ButtonGroup>
      </div>
    );
  }
);
