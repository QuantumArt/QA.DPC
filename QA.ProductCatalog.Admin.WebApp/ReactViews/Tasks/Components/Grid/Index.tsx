import React, { useRef } from "react";
import { useTable, usePagination } from "react-table";
import cn from "classnames";
import { PaginationActions } from "Shared/Enums";
import { GridPagination, GridTruncatedCellContent } from "../";
import "./Style.scss";
import { Pagination } from "Tasks/TaskStore";
import { Task } from "Tasks/ApiServices/DataContracts";

interface IProps {
  isLoading: boolean;
  total: number;
  customPagination: Pagination;
  data: Task[];
  /**
   * custom columns props
   * showOnHover: показывать поле только при наведении
   * getClassNameByEnableSchedule параметр для EnableSchedule
   * truncate: {
   * onWidth: схлопывать при указананой ширине
   * possibleRows: возможных строк
   * noTruncateElement возвращает реакт элемент который не будет схлопываться
   * }
   */
  // columns: {
  //   Header: any;
  //   accessor: any;
  //   Cell?: any;
  //   showOnHover?: boolean;
  //   fixedWidth?: number;
  //   getClassNameByEnableSchedule?: (taskId: number) => string;
  //   truncate?: {
  //     onWidth?: number;
  //     possibleRows?: 1 | 2;
  //     noTruncateElement?: (taskId: number) => Element | String;
  //   };
  // }[];
  columns: any[];
}

export const Grid = ({ columns, data, customPagination, total, isLoading }: IProps) => {
  const gridBody = useRef(null);
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    page, // Instead of using 'rows', we'll use page,
    nextPage,
    previousPage
  } = useTable(
    {
      columns,
      data,
      initialState: {
        pageIndex: 0,
        pageSize: customPagination.initPaginationOptions.take
      }
    },
    usePagination
  );
  const { skip } = customPagination.getPaginationOptions;
  const canNextPage = () => data.length + skip !== total;
  const canPreviousPage = () => skip !== 0;

  return (
    <>
      <table {...getTableProps()} className="grid">
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

        <tbody {...getTableBodyProps()} className="grid-body" ref={gridBody}>
          {page.map(row => {
            prepareRow(row);
            return (
              <tr {...row.getRowProps()} className="grid-body__tr">
                {row.cells.map(cell => {
                  const classNameByEnableSchedule = !!cell.column.getClassNameByEnableSchedule
                    ? cell.column.getClassNameByEnableSchedule(cell.row.values.Id)
                    : "";
                  const renderCell = () => {
                    if (!!cell.column.truncate) {
                      const cellElement = cell.render("Cell");
                      return (
                        <GridTruncatedCellContent
                          value={cellElement}
                          truncateOnWidth={cell.column.truncate.onWidth}
                          truncateRows={cell.column.truncate.possibleRows}
                          untruncatedElement={
                            cell.column.truncate.noTruncateElement
                              ? cell.column.truncate.noTruncateElement(cell.row.values.Id)
                              : null
                          }
                          refBody={gridBody}
                          isLoading={isLoading}
                        />
                      );
                    }

                    return (
                      <div
                        className={cn("inside-cell", {
                          "inside-cell--hidden": cell.column.showOnHover,
                          "bp3-skeleton truncate-cell": isLoading
                        })}
                      >
                        {cell.render("Cell")}
                      </div>
                    );
                  };

                  return (
                    <td
                      {...cell.getCellProps()}
                      className={cn(
                        "grid-body__td grid__cell",
                        cell.column.className,
                        classNameByEnableSchedule
                      )}
                    >
                      <div style={cell.column.fixedWidth && { width: cell.column.fixedWidth }}>
                        {renderCell()}
                      </div>
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>

      <GridPagination
        paginationOptions={{
          previousPage: () => {
            customPagination.changePage(PaginationActions.DecrementPage);
            previousPage();
          },
          nextPage: () => {
            nextPage();
            customPagination.changePage(PaginationActions.IncrementPage);
          },
          canNextPage: canNextPage(),
          canPreviousPage: canPreviousPage(),
          total: total,
          showFrom: skip,
          showTo: data.length + skip
        }}
        isLoading={isLoading}
      />
    </>
  );
};
