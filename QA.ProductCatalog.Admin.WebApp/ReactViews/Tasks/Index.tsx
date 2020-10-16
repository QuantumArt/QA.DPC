import React, { useEffect, useLayoutEffect } from "react";
import {
  GridHeadFilterTooltip,
  ProgressBarGridCell,
  StatusTag,
  Grid,
  MyLastTask,
  ScheduleGridCellCalendar,
  DateGridCell,
  RerunGridCell,
  ScheduleGridCellDescription,
  ErrorBoundary
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
import { l } from "Tasks/Localization";

/**
 * ColumnModel
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
  fixedWidth?: number;
  getClassNameByEnableSchedule?: (taskId: number) => string;
  truncate?: {
    onWidth?: number;
    noTruncateElement?: (taskId: number) => Element | String;
  };
}
export const Task = observer(() => {
  const store = useStore();
  const gridWrap = React.useRef(null);
  const [gridWidth, setGridWidth] = React.useState(1000);
  const { statusValues } = window.task;

  useEffect(() => {
    if (!store.isLoading) setGridWidth(gridWrap.current.scrollWidth);
  }, [gridWrap, store.isLoading]);

  const gridColumns = React.useMemo<Column<ColumnModel>[]>(
    () => [
      {
        Header: "Id",
        accessor: "Id"
      },
      {
        Header: l("userName"),
        accessor: "UserName",
        truncate: { onWidth: 120 }
      },
      {
        Header: (
          <GridHeadFilterTooltip label={l("status")}>
            <FilterButtonsWrapper
              acceptLabel={l("filter")}
              revokeLabel={l("clear")}
              filter={store.filters.get(TaskGridFilterType.StatusFilter)}
            >
              <StatusFilterContent options={statusValues} />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "StateId",
        Cell: (cellProps: any) => {
          const { State, StateId } = cellProps.data[cellProps.row.index] as GridTask;
          return <StatusTag state={State} stateId={StateId} />;
        }
      },
      {
        Header: (
          <GridHeadFilterTooltip label={l("schedule")}>
            <FilterButtonsWrapper
              acceptLabel={l("filter")}
              revokeLabel={l("clear")}
              filter={store.filters.get(TaskGridFilterType.ScheduleFilter)}
            >
              <ScheduleFilterContent
                options={[
                  { label: l("isTrue"), value: ScheduleFilterValues.YES },
                  { label: l("isFalse"), value: ScheduleFilterValues.NO }
                ]}
              />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "HasSchedule",
        Cell: (cellProps: any) => {
          const cronExpression = cellProps.data[cellProps.row.index].ScheduleCronExpression;
          return <ScheduleGridCellDescription cronExpression={cronExpression} />;
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
                taskIdNumber={gridElement.Id}
                scheduleCronExpression={gridElement.ScheduleCronExpression}
                hasSchedule={gridElement.HasSchedule}
                isScheduleEnabled={gridElement.ScheduleEnabled}
              />
            );
          }
        }
      },
      {
        Header: l("progress"),
        accessor: "Progress",
        Cell: (cellProps: any) => {
          const stateId = cellProps.cell.row.values.StateId;
          return <ProgressBarGridCell value={cellProps.value} stateId={stateId} />;
        }
      },
      {
        Header: l("name"),
        accessor: "DisplayName",
        truncate: { onWidth: 120 }
      },
      {
        Header: l("created"),
        accessor: "CreatedTime",
        Cell: DateGridCell,
        truncate: { onWidth: 130 }
      },
      {
        Header: l("lastStatusChange"),
        accessor: "LastStatusChangeTime",
        Cell: DateGridCell,
        truncate: { onWidth: 130 }
      },
      {
        Header: l("message"),
        accessor: "Message",
        truncate: { onWidth: 140 }
      },
      {
        Header: "",
        accessor: "IsCancellationRequested",
        className: "grid__rerun-cell",
        Cell: (cellProps: any) => {
          const { Id, StateId, IsCancellationRequested } = cellProps.row.values as Partial<
            GridTask
          >;
          return (
            <RerunGridCell
              id={Id}
              onRerun={store.fetchRerunTask}
              stateId={StateId}
              onCancel={store.fetchCancelRerun}
              IsCancellationRequested={IsCancellationRequested}
            />
          );
        }
      }
    ],
    []
  );

  return (
    <div className="task-wrapper" ref={gridWrap}>
      <ErrorBoundary>
        <MyLastTask task={store.lastTask} width={gridWidth} />
      </ErrorBoundary>

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
