import React from "react";
import { useTable, usePagination } from "react-table";
import { Button } from "@blueprintjs/core";
import "./_reset.scss";
import "./style.scss";
// import cn from "classnames";

export function Grid({ columns, data }) {
  // Use the state and functions returned from useTable to build your UI
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    page, // Instead of using 'rows', we'll use page,
    // which has only the rows for the active page

    // The rest of these things are super handy, too ;)
    canPreviousPage,
    canNextPage,
    // pageOptions,
    // pageCount,
    // gotoPage,
    nextPage,
    previousPage,
    state: { pageIndex, pageSize }
  } = useTable(
    {
      columns,
      data,
      initialState: { pageIndex: 0, pageSize: 10 }
    },
    usePagination
  );

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

        <tbody {...getTableBodyProps()} className="grid-body">
          {page.map(row => {
            prepareRow(row);
            return (
              // className="bp3-skeleton" для прелоадера
              <tr {...row.getRowProps()} className="grid-body__tr">
                {row.cells.map(cell => {
                  return (
                    <td {...cell.getCellProps()} className="grid-body__td grid__cell">
                      {cell.render("Cell")}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>

      <div className="pagination">
        <span>
          {pageIndex + 1} - {pageSize} из {data.length}
        </span>
        <Button
          icon="chevron-left"
          onClick={() => previousPage()}
          disabled={!canPreviousPage}
          outlined
        />
        <Button icon="chevron-right" onClick={() => nextPage()} disabled={!canNextPage} outlined />

        {/*default btns*/}
        {/*<button onClick={() => previousPage()} disabled={!canPreviousPage}>*/}
        {/*  <Icon icon="chevron-left" intent={Intent.NONE} />*/}
        {/*</button>*/}
        {/*<button onClick={() => nextPage()} disabled={!canNextPage}>*/}
        {/*  <Icon icon="chevron-right" intent={Intent.NONE} />*/}
        {/*</button>*/}
      </div>
    </>
  );
}
