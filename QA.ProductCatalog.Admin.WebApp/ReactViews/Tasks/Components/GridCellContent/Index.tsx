import React, { RefObject, useLayoutEffect, useRef, useState } from "react";
import { Tooltip } from "@blueprintjs/core";
import Truncate from "react-truncate";
import { IUntruncatedElementProps, UntruncatedElementWrap } from "./Subcomponents";
import "./Style.scss";

interface IProps extends IUntruncatedElementProps {
  value: JSX.Element | string;
  refBody: RefObject<any>;
  isLoading: boolean;
  truncateOnWidth: number;
  truncateRows: 1 | 2;
}

export const GridTruncatedCellContent = React.memo(
  ({ refBody, isLoading, truncateOnWidth, truncateRows, untruncatedElement, value }: IProps) => {
    const cellRef = useRef(null);
    const [isTruncate, setIsTruncate] = useState(false);
    const [isTooltip, setIsTooltip] = useState(false);
    //размер шрифта ~ 14 px
    const fontSizeOneRow = 16;
    const fontSizeTwoRows = 34;

    //проверяет ширину и высоту строки и схлопывает ее добавляя тултип если это нужно
    useLayoutEffect(
      () => {
        if (!cellRef.current) return;
        if (
          !isTooltip &&
          (cellRef.current.offsetWidth > truncateOnWidth ||
            (truncateRows === 1 && cellRef.current.offsetHeight > fontSizeOneRow) ||
            (truncateRows === 2 && cellRef.current.offsetHeight > fontSizeTwoRows))
        ) {
          setIsTruncate(true);
        }
        if (
          isTooltip &&
          (cellRef.current.offsetWidth < truncateOnWidth ||
            (truncateRows === 1 && cellRef.current.offsetHeight < fontSizeOneRow) ||
            (truncateRows === 2 && cellRef.current.offsetHeight < fontSizeTwoRows))
        ) {
          setIsTooltip(false);
        }
      },
      [isLoading, cellRef, isTooltip, truncateOnWidth, truncateRows]
    );

    const setTooltip = (isTruncated: boolean) => isTruncated && setIsTooltip(true);

    const TruncateString = props => {
      return (
        <>
          <Truncate
            {...props}
            lines={truncateRows || 1}
            className="truncate-cell"
            width={truncateOnWidth}
          >
            {value}
          </Truncate>
        </>
      );
    };

    if (isTooltip && !isLoading) {
      return (
        <UntruncatedElementWrap untruncatedElement={untruncatedElement} isLoading={isLoading}>
          <Tooltip position={"left"} usePortal content={value} portalContainer={refBody.current}>
            <TruncateString />
          </Tooltip>
        </UntruncatedElementWrap>
      );
    }

    if (isTruncate && !isLoading) {
      return (
        <UntruncatedElementWrap untruncatedElement={untruncatedElement} isLoading={isLoading}>
          <TruncateString
            onTruncate={param => {
              setTooltip(param);
            }}
          />
        </UntruncatedElementWrap>
      );
    }

    return (
      <UntruncatedElementWrap untruncatedElement={untruncatedElement} isLoading={isLoading}>
        <div ref={cellRef}>{value}</div>
      </UntruncatedElementWrap>
    );
  }
);
