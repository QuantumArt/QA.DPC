import { Button, ButtonGroup, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { observer } from "mobx-react-lite";
import { IconNames } from "@blueprintjs/icons";
import { Filter, Pagination } from "Tasks/TaskStore";
import "./Style.scss";
import { PaginationActions } from "Shared/Enums";

interface Props {
  filter?: Filter;
  pagination?: Pagination;
  children: React.ReactElement;
  acceptLabel: string;
  revokeLabel: string;
}

export const FilterButtonsWrapper = observer(
  ({ filter, pagination, children, acceptLabel, revokeLabel }: Props) => {
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
            onClick={async () => {
              filter.setValue(value);
              await filter.toggleActive(true);
              pagination.changePage(PaginationActions.FirstPage);
            }}
          >
            {acceptLabel}
          </Button>
          <Button
            intent={Intent.PRIMARY}
            disabled={!filter.isActive}
            outlined
            icon={IconNames.REMOVE}
            onClick={async () => {
              filter.setValue(null);
              await filter.toggleActive(false);
              pagination.changePage(PaginationActions.FirstPage);
            }}
          >
            {revokeLabel}
          </Button>
        </ButtonGroup>
      </div>
    );
  }
);
