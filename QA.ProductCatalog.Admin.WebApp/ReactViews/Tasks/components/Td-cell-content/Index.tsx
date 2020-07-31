import React, { useEffect, useRef, useState } from "react";
import { Tooltip } from "@blueprintjs/core";
import cn from "classnames";
import Truncate from "react-truncate";

export const TdCellContent = ({ cell, refBody, loading }) => {
  const cellRef = useRef(null);
  const [isTruncate, setIsTruncate] = useState(false);
  const [isTooltip, setIsTooltip] = useState(false);
  const isWithTruncate = !!cell.column.truncate;
  //размер шрифта ~ 14 px
  const fontSizeOneRow = 16;
  const fontSizeTwoRows = 34;

  //эффект который при рендере проверяет ширину и высоту строки и схлопывает ее добавляя тултип если это нужно
  useEffect(
    () => {
      if (!cell.value || !isWithTruncate || !cellRef.current) return;
      if (
        !isTooltip &&
        (cellRef.current.offsetWidth > cell.column.truncate.onWidth ||
          (cell.column.truncate.possibleRows === 1 &&
            cellRef.current.offsetHeight > fontSizeOneRow) ||
          (cell.column.truncate.possibleRows === 2 &&
            cellRef.current.offsetHeight > fontSizeTwoRows))
      ) {
        setIsTruncate(true);
        return;
      }
      if (
        isTooltip &&
        cellRef.current &&
        (cellRef.current.offsetWidth < cell.column.truncate.onWidth ||
          (cell.column.truncate.possibleRows === 1 &&
            cellRef.current.offsetHeight < fontSizeOneRow) ||
          (cell.column.truncate.possibleRows === 2 &&
            cellRef.current.offsetHeight < fontSizeTwoRows))
      ) {
        setIsTooltip(false);
      }
    },
    [cell.value, loading]
  );

  const setTooltip = (isTruncated: boolean) => isTruncated && setIsTooltip(true);

  const TruncateString = props => {
    const { possibleRows, onWidth } = cell.column.truncate;

    return (
      <Truncate {...props} lines={possibleRows || 1} className="truncate-cell" width={onWidth}>
        {cell.render("Cell")}
      </Truncate>
    );
  };

  if (isTooltip && !loading) {
    return (
      <Tooltip
        position={"left"}
        usePortal
        content={cell.render("Cell")}
        portalContainer={refBody.current}
      >
        <TruncateString />
      </Tooltip>
    );
  }

  if (isTruncate && !loading) {
    return (
      <TruncateString
        onTruncate={param => {
          setTooltip(param);
        }}
      />
    );
  }

  return (
    <span
      className={cn("inside-cell", {
        "inside-cell--hidden": cell.column.showOnHover,
        "bp3-skeleton truncate-cell": loading
      })}
      ref={cellRef}
    >
      {cell.render("Cell")}
    </span>
  );
};
