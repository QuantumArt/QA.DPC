import React from "react";
import { IChannel } from "Notification/ApiServices/ApiInterfaces";
import { Column, useTable } from "react-table";
import "./Style.scss";
import cn from "classnames";
import { StatusTag } from "Notification/Components/StatusTag";
import { format, isValid } from "date-fns";
import { l } from "Notification/Localization";
import { ChannelStatusTag } from "Notification/Components";

interface IProps {
  gridData: IChannel[];
}

const parseDate = ({ value }: { value: string }): string => {
  const date = new Date(value);
  return isValid(date) && format(date, "DD.MM.YYYY HH:mm:ss");
};

export const ChannelsGrid = React.memo(({ gridData }: IProps) => {
  const gridColumns = React.useMemo<Column<IChannel>[]>(
    () => [
      {
        Header: l("channel"),
        accessor: "Name"
      },
      {
        Header: l("queue"),
        accessor: "Count"
      },
      {
        Header: l("enqueueTime"),
        accessor: "LastQueued",
        Cell: parseDate
      },
      {
        Header: l("publishingTime"),
        accessor: "LastPublished",
        Cell: parseDate
      },
      {
        Header: l("productId"),
        accessor: "LastId"
      },
      {
        Header: l("publishingStatus"),
        accessor: "LastStatus",
        Cell: StatusTag
      },
      {
        Header: l("channelStatus"),
        accessor: "State",
        Cell: ChannelStatusTag
      }
    ],
    []
  );

  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable<IChannel>({
    columns: gridColumns,
    data: gridData
  });

  return (
    <div className="channels-grid">
      <table {...getTableProps()} className="channels-table">
        <thead className="grid-head">
          {headerGroups.map(headerGroup => (
            <tr {...headerGroup.getHeaderGroupProps()} className="grid-head__tr">
              {headerGroup.headers.map(column => (
                <th {...column.getHeaderProps()} className="grid-head__th grid__cell">
                  {column.render("Header")}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody {...getTableBodyProps()} className="grid-body">
          {rows.map((row, i) => {
            prepareRow(row);
            return (
              <tr {...row.getRowProps()} className="grid-body__tr">
                {row.cells.map(cell => {
                  return (
                    <td
                      {...cell.getCellProps()}
                      className={cn("grid-body__td grid__cell", cell.column.className)}
                    >
                      {cell.render("Cell")}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
});
