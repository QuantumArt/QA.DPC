import React, { useEffect } from "react";
import {
  GridHeadFilterTooltip,
  ProgressBarGridCell,
  StatusTag,
  Grid,
  MyLastTask,
  ScheduleGridCellCalendar,
  DateGridCell,
  RerunGridCell,
  ScheduleGridCellDescription
} from "./Components";
import {
  StatusFilterContent,
  ScheduleFilterContent,
  FilterButtonsWrapper
} from "./Components/GridHeadFilterTooltip/Subcomponents";
import { observer } from "mobx-react-lite";
import { useStore } from "./UseStore";
import { ScheduleFilterValues, TaskGridFilterType } from "Shared/Enums";
import { getClassnameByIntent } from "Shared/Utils";
import { Column, Accessor } from "react-table";
import { Task as GridTask } from "Tasks/ApiServices/DataContracts";
import { Intent } from "@blueprintjs/core";
import "./Root.scss";

/**
 * ColumnModel
 * showOnHover: показывать поле только при наведении
 * getClassNameByEnableSchedule: параметр для EnableSchedule
 * truncate: {
 * onWidth: схлопывать при указананой ширине
 * noTruncateElement возвращает реакт элемент который не будет схлопываться
 * }
 */
export interface ColumnModel {
  Header: any;
  accessor: Accessor;
  Cell?: any;
  showOnHover?: boolean;
  fixedWidth?: number;
  getClassNameByEnableSchedule?: (taskId: number) => string;
  truncate?: {
    onWidth?: number;
    noTruncateElement?: (taskId: number) => Element | String;
  };
}
export const Task = observer(() => {
  const store = useStore();

  const {
    userName,
    status,
    schedule,
    progress,
    name,
    created,
    lastStatusChange,
    message,
    statusValues
  } = window.QP.Tasks.tableFields;

  useEffect(() => {
    store.init();
  }, []);

  const gridColumns = React.useMemo<Column<ColumnModel>[]>(
    () => [
      {
        Header: "Id",
        accessor: "Id"
      },
      {
        Header: userName,
        accessor: "UserName",
        truncate: { onWidth: 120 }
      },
      {
        Header: (
          <GridHeadFilterTooltip
            label={status}
            filter={store.filters.get(TaskGridFilterType.StatusFilter)}
          >
            <FilterButtonsWrapper
              acceptLabel={window.QP.Tasks.tableFilters.messages.filter}
              revokeLabel={window.QP.Tasks.tableFilters.messages.clear}
            >
              <StatusFilterContent options={statusValues} />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "StateId",
        Cell: StatusTag
      },
      {
        Header: (
          <GridHeadFilterTooltip
            label={schedule}
            filter={store.filters.get(TaskGridFilterType.ScheduleFilter)}
          >
            <FilterButtonsWrapper
              acceptLabel={window.QP.Tasks.tableFilters.messages.filter}
              revokeLabel={window.QP.Tasks.tableFilters.messages.clear}
            >
              <ScheduleFilterContent
                options={[
                  { label: "Да", value: ScheduleFilterValues.YES },
                  { label: "Нет", value: ScheduleFilterValues.NO }
                ]}
              />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "HasSchedule",
        Cell: (cellProps: any) => {
          return (
            <ScheduleGridCellDescription
              cronExpression={cellProps.data[cellProps.row.index].ScheduleCronExpression}
            />
          );
        },
        getClassNameByEnableSchedule: (gridElement: GridTask) => {
          if (gridElement.ScheduleEnabled) {
            return getClassnameByIntent("color", Intent.SUCCESS, "-");
          }
          return getClassnameByIntent("color", Intent.NONE, "-");
        },
        truncate: {
          onWidth: 120,

          noTruncateElement: (gridElement: GridTask) => {
            return (
              <ScheduleGridCellCalendar
                taskId={gridElement.Id}
                scheduleCronExpression={gridElement.ScheduleCronExpression}
                hasSchedule={gridElement.HasSchedule}
                scheduleEnabled={gridElement.ScheduleEnabled}
              />
            );
          }
        }
      },
      {
        Header: progress,
        accessor: "Progress",
        Cell: (cellProps: any) => {
          const stateId = cellProps.cell.row.values.StateId;
          return <ProgressBarGridCell value={cellProps.value} stateId={stateId} />;
        }
      },
      {
        Header: name,
        accessor: "DisplayName",
        truncate: { onWidth: 120 }
      },
      {
        Header: created,
        accessor: "CreatedTime",
        Cell: DateGridCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110 }
      },
      {
        Header: lastStatusChange,
        accessor: "LastStatusChangeTime",
        Cell: DateGridCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110 }
      },
      {
        Header: message,
        accessor: "Message",
        truncate: { onWidth: 140 }
      },
      {
        Header: "",
        accessor: " ",
        showOnHover: true,
        className: "grid__rerun-cell",
        Cell: (cellProps: any) => {
          const Id = cellProps.row.values.Id;
          return <RerunGridCell id={Id} method={store.fetchRerunTask} />;
        }
      }
    ],
    []
  );

  return (
    <div className="task-wrapper">
      <MyLastTask task={store.lastTask} width={1240} />

      <Grid
        columns={gridColumns}
        data={store.getGridData}
        customPagination={store.pagination}
        total={store.getTotal}
        isLoading={store.isLoading}
      />
    </div>
  );
});
