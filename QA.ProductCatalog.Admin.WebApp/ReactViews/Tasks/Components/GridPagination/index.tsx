import React from "react";
import { Button, ButtonGroup, Spinner } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import { l } from "Tasks/Localization";
import "./Style.scss";

interface IPaginationOptions {
  previousPage: () => void;
  canPreviousPage: boolean;
  nextPage: () => void;
  canNextPage: boolean;
  gotoLastPage: () => void;
  gotoFirstPage: () => void;
  total: number;
  showFrom: number;
  showTo: number;
}

export const GridPagination = ({
  paginationOptions,
  isLoading
}: {
  paginationOptions: IPaginationOptions;
  isLoading: boolean;
}) => {
  const {
    previousPage,
    nextPage,
    total,
    showFrom,
    showTo,
    canPreviousPage,
    canNextPage,
    gotoFirstPage,
    gotoLastPage
  } = paginationOptions;

  return (
    <div className="pagination">
      <ButtonGroup className="pagination__buttons-container">
        <Button
          className="pagination__button"
          icon={IconNames.CHEVRON_BACKWARD}
          onClick={gotoFirstPage}
          disabled={!canPreviousPage || isLoading}
        />
        <Button
          className="pagination__button"
          icon={IconNames.CHEVRON_LEFT}
          onClick={previousPage}
          disabled={!canPreviousPage || isLoading}
        />
        <div className="pagination__info-container">
          {isLoading ? (
            <Spinner size={Spinner.SIZE_SMALL} />
          ) : (
            <span className="pagination__info">
              {showFrom === 0 ? 1 : showFrom} - {showTo} {l("paginationOf")} {total}
            </span>
          )}
        </div>
        <Button
          className="pagination__button"
          icon={IconNames.CHEVRON_RIGHT}
          onClick={nextPage}
          disabled={!canNextPage || isLoading}
        />
        <Button
          className="pagination__button"
          icon={IconNames.CHEVRON_FORWARD}
          onClick={gotoLastPage}
          disabled={!canNextPage || isLoading}
        />
        {/*<Button onClick={() => store.toggleLoading()}>Loading</Button>*/}
        {/*<Button onClick={() => store.setGridData([])}>Empty</Button>*/}
        {/*<Button onClick={() => store.fetchGridData()}>Load</Button>*/}
      </ButtonGroup>
    </div>
  );
};
