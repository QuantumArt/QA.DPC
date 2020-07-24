import React from "react";
import { Button } from "@blueprintjs/core";
import "./style.scss";

interface IPaginationOptions {
  previousPage: () => void;
  canPreviousPage: boolean;
  nextPage: () => void;
  canNextPage: boolean;
  total: number;
  showFrom: number;
  showTo: number;
}

export const Pagination = ({
  paginationOptions,
  loading
}: {
  paginationOptions: IPaginationOptions;
  loading: boolean;
}) => {
  const {
    previousPage,
    nextPage,
    total,
    showFrom,
    showTo,
    canPreviousPage,
    canNextPage
  } = paginationOptions;

  return (
    <div className="pagination">
      <div className="pagination__info-container">
        <span className="pagination__info">
          {showFrom === 0 ? 1 : showFrom} - {showTo} из {total}
        </span>
      </div>
      <div className="pagination__buttons-container">
        <Button
          className="pagination__button"
          icon="chevron-left"
          onClick={previousPage}
          disabled={!canPreviousPage || loading}
          outlined
        />
        <Button
          className="pagination__button"
          icon="chevron-right"
          onClick={nextPage}
          disabled={!canNextPage || loading}
          outlined
        />
      </div>
    </div>
  );
};
