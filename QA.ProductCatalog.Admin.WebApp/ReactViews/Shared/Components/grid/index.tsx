import React, { useRef } from "react";
import { useTable, usePagination } from "react-table";
import cn from "classnames";
import { PaginationActions } from "Shared/Enums";
import { TdCellContent } from "Shared/Components";
import { Pagination } from "Shared/Components/pagination";
import "./_reset.scss";
import "./style.scss";

export function Grid({ columns, data, paginationOptions, loading }) {
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
      initialState: { pageIndex: 0, pageSize: paginationOptions.pagination.take }
    },
    usePagination
  );
  const canNextPage = () =>
    data.length + paginationOptions.pagination.skip !== paginationOptions.total;
  const canPreviousPage = () => paginationOptions.pagination.skip !== 0;

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
              // className="bp3-skeleton" для прелоадера
              <tr {...row.getRowProps()} className="grid-body__tr">
                {row.cells.map(cell => {
                  return (
                    <td
                      {...cell.getCellProps()}
                      className={cn("grid-body__td grid__cell", cell.column.className)}
                    >
                      <TdCellContent cell={cell} refBody={gridBody} loading={loading} />
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>

      <Pagination
        paginationOptions={{
          previousPage: () => {
            paginationOptions.fetchData(PaginationActions.DecrementPage);
            previousPage();
          },
          nextPage: () => {
            nextPage();
            paginationOptions.fetchData(PaginationActions.IncrementPage);
          },
          canNextPage: canNextPage(),
          canPreviousPage: canPreviousPage(),
          total: paginationOptions.total,
          showFrom: paginationOptions.pagination.skip,
          showTo: data.length + paginationOptions.pagination.skip
        }}
        loading={loading}
      />
    </>
  );
}
