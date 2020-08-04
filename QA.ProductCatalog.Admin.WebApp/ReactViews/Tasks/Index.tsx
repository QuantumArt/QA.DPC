import React, { useEffect } from "react";
import {
  GridHeadFilterTooltip,
  ProgressBarGridCell,
  StatusCell,
  Grid,
  MyLastTask,
  ScheduleGridCell,
  DateGridCell,
  RerunGridCell
} from "./Components";
import {
  StatusFilterContent,
  ScheduleFilterContent,
  FilterButtonsWrapper
} from "./Components/GridHeadFilterTooltip/Subcomponents";
import { observer } from "mobx-react-lite";
import { useStore } from "./UseStore";
import { ScheduleFilterValues, TaskGridFilterType } from "Shared/Enums";
import "./_reset.scss";
import "./Style.scss";

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

  console.log(window.QP.Tasks);

  useEffect(() => {
    store.init();
  }, []);

  const gridColumns = React.useMemo(
    () => [
      {
        Header: "Id",
        accessor: "Id"
      },
      {
        Header: userName,
        accessor: "UserName",
        truncate: { onWidth: 120, possibleRows: 1 }
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
        Cell: StatusCell
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
        // Cell: ScheduleCell,
        Cell: (cellProps: any) => {
          const { Id } = cellProps.row.values;
          return (
            <ScheduleGridCell
              taskId={Id}
              scheduleCronExpression={cellProps.data[cellProps.row.index].ScheduleCronExpression}
              hasSchedule={cellProps.value}
            />
          );
        }
        //что-то сделать с этими параметрами
        // ScheduleCronExpression: null
        // ScheduledFromTaskId: null
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
        truncate: { onWidth: 120, possibleRows: 1 }
      },
      {
        Header: created,
        accessor: "CreatedTime",
        Cell: DateGridCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110, possibleRows: 1 }
      },
      {
        Header: lastStatusChange,
        accessor: "LastStatusChangeTime",
        Cell: DateGridCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110, possibleRows: 1 }
      },
      {
        Header: message,
        accessor: "Message",
        truncate: { onWidth: 140, possibleRows: 2 }
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
      <MyLastTask task={store.lastTask} />

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
