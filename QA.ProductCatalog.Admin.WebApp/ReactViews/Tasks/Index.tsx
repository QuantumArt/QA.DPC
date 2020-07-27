import React, { useEffect } from "react";
import {
  FilterTooltip,
  RerunCell,
  DateCell,
  ProgressBarCell,
  StatusCell,
  Grid,
  StatusFilterContent,
  ScheduleFilterContent,
  FilterButtonsWrapper
} from "./Components";
import { observer } from "mobx-react-lite";
import { useStore } from "./UseStore";
import { TaskGridFilterType } from "Shared/Enums";
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
        truncate: { onWidth: 120, possibleRows: 1 } //possible rows param max 2 default 1
      },
      {
        Header: (
          <FilterTooltip label={status} filter={store.filters.get(TaskGridFilterType.StatusFilter)}>
            <FilterButtonsWrapper
              acceptLabel={window.QP.Tasks.tableFilters.messages.filter}
              revokeLabel={window.QP.Tasks.tableFilters.messages.clear}
            >
              <StatusFilterContent options={statusValues} />
            </FilterButtonsWrapper>
          </FilterTooltip>
        ),
        accessor: "StateId",
        Cell: StatusCell
      },
      {
        Header: (
          <FilterTooltip
            label={schedule}
            filter={store.filters.get(TaskGridFilterType.ScheduleFilter)}
          >
            <FilterButtonsWrapper
              acceptLabel={window.QP.Tasks.tableFilters.messages.filter}
              revokeLabel={window.QP.Tasks.tableFilters.messages.clear}
            >
              <ScheduleFilterContent
                options={[{ label: "Да", value: "true" }, { label: "Нет", value: "false" }]}
              />
            </FilterButtonsWrapper>
          </FilterTooltip>
        ),
        accessor: "HasSchedule"
        //что-то сделать с этими параметрами
        // ScheduleCronExpression: null
        // ScheduledFromTaskId: null
      },
      {
        Header: progress,
        accessor: "Progress",
        Cell: (cellProps: any) => {
          const stateId = cellProps.cell.row.values.StateId;
          return <ProgressBarCell value={cellProps.value} stateId={stateId} />;
        }
      },
      {
        Header: name,
        accessor: "DisplayName",
        truncate: { onWidth: 120, possibleRows: 1 } //possible rows param max 2 default 1
      },
      {
        Header: created,
        accessor: "CreatedTime",
        Cell: DateCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110, possibleRows: 1 } //possible rows param max 2 default 1
      },
      {
        Header: lastStatusChange,
        accessor: "LastStatusChangeTime",
        Cell: DateCell,
        className: "grid__date-cell",
        truncate: { onWidth: 110, possibleRows: 1 } //possible rows param max 2 default 1
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
          return <RerunCell id={Id} method={store.rerun} />;
        }
      }
    ],
    []
  );

  return (
    <div className="task-wrapper">
      <Grid
        columns={gridColumns}
        data={store.getGridData}
        customPagination={store.pagination}
        total={store.total}
        isLoading={store.isLoading}
      />
    </div>
  );
});
