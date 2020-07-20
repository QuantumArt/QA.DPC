import React, { useEffect, useState } from "react";
import { Grid } from "Shared/Components";
import { StatusCell } from "Tasks/components/status-cell";
import { ProgressBarCell } from "Tasks/components/progress-bar-cell";

//copied from common.ts ClientApp
const urlFromHead = document.head.getAttribute("root-url") || "";
const rootUrl = urlFromHead.endsWith("/") ? urlFromHead.slice(0, -1) : urlFromHead;

const Task = () => {
  const requestUrl = `${rootUrl}/Task/TasksData?showOnlyMine=True&skip=0&take=20`;
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

  const gridColumns = React.useMemo(
    () => [
      {
        Header: "Id",
        accessor: "Id"
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
        Header: userName,
        accessor: "UserName"
      },
      {
        Header: name,
        accessor: "DisplayName"
      },
      {
        Header: created,
        accessor: "CreatedTime"
      },
      {
        Header: lastStatusChange,
        accessor: "LastStatusChangeTime"
      },
      {
        Header: message,
        accessor: "Message"
      }
      // {
      //   Header: '',
      //   accessor: ' '
      //   //кнопка обновления
      // },
    ],
    []
  );

  const [gridData, setData] = useState([]);

  useEffect(() => {
    fetch(requestUrl)
      .then(response => {
        return response.json();
      })
      .then(({ data }) => {
        console.log(data);
        setData(data.tasks);
      })
      .catch(e => {
        console.error(e.text());
      });
  }, []);

  return <Grid columns={gridColumns} data={gridData} />;
};

export default Task;
