import React, { useState } from "react";
import cn from "classnames";
import { Popover, Icon, Position, Intent, ButtonGroup, Button } from "@blueprintjs/core";
import { observer } from "mobx-react-lite";
import { Filter } from "Tasks/TaskStore";
import { getClassnameByIntent } from "Shared/Utils";
import { IconNames } from "@blueprintjs/icons";
import { DateRange, DateRangeInput } from "@blueprintjs/datetime";
import moment from "moment";
import MomentLocaleUtils from "react-day-picker/moment";
import { useStore } from "Tasks/UseStore";
import { l } from "Tasks/Localization";

interface Props {
  label: string | number;
  filterFrom: Filter;
  filterTo: Filter;
  acceptLabel: string;
  revokeLabel: string;
}

export const DateFilter = observer(
  ({ label, filterFrom, filterTo, acceptLabel, revokeLabel }: Props) => {
    const intent = filterFrom.isActive && filterTo.isActive ? Intent.SUCCESS : Intent.PRIMARY;
    const [dates, setDates] = useState<DateRange>([null, null]);
    const { withLoader, fetchGridData } = useStore();
    return (
      <Popover position={Position.BOTTOM} minimal>
        <span className={cn("filter-cell", getClassnameByIntent("color", intent))}>
          {label} <Icon className="filter-cell__icon" iconSize={14} icon="filter" intent={intent} />
        </span>
        <div className="filter-options-wrap">
          <DateRangeInput
            onChange={(range: DateRange) => {
              setDates(range);
            }}
            formatDate={date =>
              moment(date)
                .locale(window.task.locale)
                .format("DD.MM.YYYY")
            }
            parseDate={str =>
              moment(str, "DD.MM.YYYY")
                .locale(window.task.locale)
                .toDate()
            }
            minDate={moment()
              .subtract(2, "years")
              .toDate()}
            maxDate={moment().toDate()}
            locale={window.task.locale}
            localeUtils={MomentLocaleUtils}
            highlightCurrentDay
            shortcuts={false}
            startInputProps={{
              placeholder: l("fromDate")
            }}
            endInputProps={{
              placeholder: l("toDate")
            }}
            value={dates}
          />
          <ButtonGroup className="filter-options-wrap__buttons-wrap" fill>
            <Button
              intent={Intent.PRIMARY}
              icon={IconNames.CONFIRM}
              outlined
              onClick={() => {
                filterFrom.setValue(dates[0].toLocaleDateString(window.task.locale));
                filterTo.setValue(dates[1].toLocaleDateString(window.task.locale));
                filterFrom.toggleActive(true);
                filterTo.toggleActive(true);
                withLoader(() => fetchGridData());
              }}
            >
              {acceptLabel}
            </Button>
            <Button
              intent={Intent.PRIMARY}
              disabled={!filterTo.isActive && !filterFrom.isActive}
              outlined
              icon={IconNames.REMOVE}
              onClick={() => {
                filterFrom.setValue(null);
                filterTo.setValue(null);
                setDates([null, null]);
                filterFrom.toggleActive(false);
                filterTo.toggleActive(false);
              }}
            >
              {revokeLabel}
            </Button>
          </ButtonGroup>
        </div>
      </Popover>
    );
  }
);
