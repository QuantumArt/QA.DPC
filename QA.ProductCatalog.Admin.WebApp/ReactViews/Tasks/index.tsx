import React, { useEffect, useState } from "react";
import qs from "qs";
import { Grid } from "Shared/Components";
import { StatusCell } from "Tasks/components/status-cell";
import { ProgressBarCell } from "Tasks/components/progress-bar-cell";
import { DateCell } from "Tasks/components/date-cell";
import { RerunCell } from "Tasks/components/rerun-cell";
import { PaginationActions } from "Shared/Enums";

interface IGridResponse {
  data: {
    tasks: any[];
    totalTasks: number;
  };
}
interface IFetchDataOptions {
  skip: number;
  take: number;
  showOnlyMine: boolean;
}

//copied from common.ts ClientApp
const urlFromHead = document.head.getAttribute("root-url") || "";
const rootUrl = urlFromHead.endsWith("/") ? urlFromHead.slice(0, -1) : urlFromHead;

const Task = () => {
  const {
    userName,
    status,
    schedule,
    progress,
    name,
    created,
    lastStatusChange,
    message
  } = window.QP.Tasks.tableFields;

  const initPaginationOptions = {
    skip: 0,
    take: 10,
    showOnlyMine: true
  };

  useEffect(() => {
    fetchData();
  }, []);

  const [gridData, setGridData] = useState([]);
  const [pagination, setPagination] = useState(initPaginationOptions);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);

  const getCalculatedPagination = (operation: PaginationActions): IFetchDataOptions => {
    if (operation === PaginationActions.None) return pagination;
    return Object.assign(pagination, {
      skip:
        operation === PaginationActions.IncrementPage
          ? (pagination.skip += initPaginationOptions.take)
          : (pagination.skip -= initPaginationOptions.take)
    });
  };

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
        Header: status,
        accessor: "StateId",
        Cell: StatusCell
      },
      {
        Header: schedule,
        accessor: "HasSchedule"
        //что-то сделать с этими параметрами
        // ScheduleCronExpression: null
        // ScheduledFromTaskId: null
      },
      {
        Header: progress,
        accessor: "Progress",
        Cell: (cellProps: any) => {
          const stateId = cellProps.row.values.StateId;
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
          return <RerunCell id={Id} method={rerun} />;
        }
      }
    ],
    []
  );

  const rerun = (taskId: number) => {
    const queryStr: string = qs.stringify({
      taskId
    });
    const requestUrl = `${rootUrl}/Task/Rerun?${queryStr}`;

    fetch(requestUrl, { method: "POST" })
      .then(response => {
        return response.json();
      })
      .then(response => {
        console.log(response);
      })
      .catch(e => {
        console.error(e.text());
      });
  };

  const fetchData = (operation: PaginationActions = PaginationActions.None) => {
    setLoading(true);
    const paginationOptions = getCalculatedPagination(operation);
    const queryStr: string = qs.stringify(paginationOptions);
    const requestUrl = `${rootUrl}/Task/TasksData?${queryStr}`;
    fetch(requestUrl)
      .then(response => {
        return response.json();
      })
      .then(({ data }: IGridResponse) => {
        setGridData(data.tasks);
        setTotal(data.totalTasks);
        setPagination(paginationOptions);
        setLoading(false);
      })
      .catch(e => {
        setLoading(false);
        console.error(e.text());
      });
  };

  const customPaginationOptions = {
    pagination,
    total: total,
    fetchData
  };

  return (
    <Grid
      columns={gridColumns}
      data={gridData}
      paginationOptions={customPaginationOptions}
      loading={loading}
    />
  );
};

export default Task;
