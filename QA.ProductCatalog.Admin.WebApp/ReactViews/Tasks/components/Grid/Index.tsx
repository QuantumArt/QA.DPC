import React, { useRef } from "react";
import { useTable, usePagination } from "react-table";
import cn from "classnames";
import { PaginationActions } from "Shared/Enums";
import { GridPagination, TdCellContent } from "../";
import "./Style.scss";
import { Pagination } from "Tasks/TaskStore";

interface IProps {
  isLoading: boolean;
  total: number;
  customPagination: Pagination;
  data: any[];
  /**
   * custom columns props
   * showOnHover: показывать поле только при наведении
   * truncate: {
   * onWidth: схлопывать при указананой ширине
   * possibleRows: возможных строк
   * }
   */
  columns: {
    showOnHover?: boolean;
    truncate?: { onWidth: number; possibleRows: 1 | 2 };
  };
}

export const Grid = ({ columns, data, customPagination, total, isLoading }: IProps) => {
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

  const gridBody = useRef(null);

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
                  return (
                    <td
                      {...cell.getCellProps()}
                      className={cn("grid-body__td grid__cell", cell.column.className)}
                    >
                      <TdCellContent cell={cell} refBody={gridBody} loading={isLoading} />
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
