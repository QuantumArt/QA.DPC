import React, { useEffect, useRef, useState } from "react";
import { Intent } from "@blueprintjs/core";
import { Column } from "react-table";
import { observer } from "mobx-react-lite";
import {
  DateGridCell,
  ErrorBoundary,
  Grid,
  GridHeadFilterTooltip,
  MyLastTask,
  ProgressBarGridCell,
  RerunGridCell,
  ScheduleGridCellCalendar,
  ScheduleGridCellDescription,
  StatusTag
} from "./Components";
import {
  DateFilter,
  FilterButtonsWrapper,
  NameFilterContent,
  ScheduleFilterContent,
  StatusFilterContent
} from "./Components/GridHeadFilterTooltip/Subcomponents";
import { useStore } from "./UseStore";
import { ScheduleFilterValues, TaskGridFilterType } from "Shared/Enums";
import { getClassnameByIntent } from "Shared/Utils";
import { Task } from "Tasks/ApiServices/DataContracts";
import { l } from "Tasks/Localization";
import "./Root.scss";

export const Tasks = observer(() => {
  const store = useStore();
  const gridWrap = useRef(null);
  const [gridWidth, setGridWidth] = useState(1000);
  const { statusValues } = window.task;

  useEffect(() => {
    if (!store.isLoading) setGridWidth(gridWrap.current.scrollWidth);
  }, [gridWrap, store.isLoading]);

  const gridColumns = React.useMemo<Column<Task>[]>(
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
          <GridHeadFilterTooltip
            label={l("status")}
            filter={store.filters.get(TaskGridFilterType.StatusFilter)}
            pagination={store.pagination}
          >
            <FilterButtonsWrapper acceptLabel={l("filter")} revokeLabel={l("clear")}>
              <StatusFilterContent options={statusValues} />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "StateId",
        Cell: (cellProps: any) => {
          const { State, StateId } = cellProps.data[cellProps.row.index] as Task;
          return <StatusTag state={State} stateId={StateId} />;
        }
      },
      {
        Header: (
          <GridHeadFilterTooltip
            label={l("schedule")}
            filter={store.filters.get(TaskGridFilterType.ScheduleFilter)}
            pagination={store.pagination}
          >
            <FilterButtonsWrapper acceptLabel={l("filter")} revokeLabel={l("clear")}>
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
        getClassNameByEnableSchedule: (gridElement: Task) => {
          if (gridElement.ScheduleEnabled) {
            return getClassnameByIntent("color", Intent.SUCCESS);
          }
          return getClassnameByIntent("color", Intent.NONE);
        },
        truncate: {
          onWidth: 100,
          noTruncateElementWidth: 30,
          noTruncateElement: (gridElement: Task) => {
            const { allowSchedule } = window.task;
            const showSchedule =
              (allowSchedule && gridElement.ScheduledFromTaskId === null) ||
              gridElement.HasSchedule;
            return (
              <ScheduleGridCellCalendar
                taskIdNumber={gridElement.Id}
                scheduleCronExpression={gridElement.ScheduleCronExpression}
                hasSchedule={showSchedule}
                isScheduleEnabled={gridElement.ScheduleEnabled}
                intent={gridElement.ScheduleEnabled ? Intent.SUCCESS : Intent.PRIMARY}
                allowConcurrentTasks={gridElement.AllowConcurrentTasks}
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
        Header: (
          <GridHeadFilterTooltip
            label={l("name")}
            filter={store.filters.get(TaskGridFilterType.NameFilter)}
            pagination={store.pagination}
          >
            <FilterButtonsWrapper acceptLabel={l("filter")} revokeLabel={l("clear")}>
              <NameFilterContent />
            </FilterButtonsWrapper>
          </GridHeadFilterTooltip>
        ),
        accessor: "DisplayName",
        truncate: { onWidth: 120 }
      },
      {
        Header: (
          <DateFilter
            label={l("created")}
            acceptLabel={l("filter")}
            revokeLabel={l("clear")}
            filterFrom={store.filters.get(TaskGridFilterType.DateFilterFrom)}
            filterTo={store.filters.get(TaskGridFilterType.DateFilterTo)}
            pagination={store.pagination}
          />
        ),
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
          const { Id, StateId, IsCancellationRequested } = cellProps.row.values as Partial<Task>;
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
