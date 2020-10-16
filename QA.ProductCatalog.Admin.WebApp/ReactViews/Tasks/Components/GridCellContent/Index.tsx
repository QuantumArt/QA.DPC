import React, { RefObject, useEffect, useRef, useState } from "react";
import { Tooltip } from "@blueprintjs/core";
import { IUntruncatedElementProps, UntruncatedElementWrap } from "./Subcomponents";
import "./Style.scss";

interface IProps extends IUntruncatedElementProps {
  value: JSX.Element | string;
  refBody: RefObject<any>;
  isLoading: boolean;
  truncateOnWidth: number;
}

export const GridTruncatedCellContent = React.memo(
  ({ refBody, isLoading, truncateOnWidth, untruncatedElement, value }: IProps) => {
    const cellRef = useRef(null);
    const cellRefTruncated = useRef(null);
    const [isTruncate, setIsTruncate] = useState(false);

    //проверяет ширину и высоту строки и схлопывает ее добавляя тултип если это нужно
    useEffect(() => {
      if (!isTruncate && cellRef.current.offsetWidth > truncateOnWidth) {
        setIsTruncate(true);
        return;
      }
      if (
        isTruncate &&
        cellRefTruncated.current &&
        cellRefTruncated.current.offsetWidth < truncateOnWidth
      ) {
        setIsTruncate(false);
      }
    }, [isLoading, cellRef, truncateOnWidth, isTruncate, cellRefTruncated]);

    if (isTruncate && !isLoading) {
      return (
        <UntruncatedElementWrap
          untruncatedElement={untruncatedElement}
          isLoading={isLoading}
          width={truncateOnWidth}
        >
          <Tooltip position={"left"} usePortal content={value} portalContainer={refBody.current}>
            <div
              style={truncateOnWidth && { width: truncateOnWidth }}
              className="truncate-cell truncate-string"
            >
              <span ref={cellRefTruncated}> {value && value}</span>
            </div>
          </Tooltip>
        </UntruncatedElementWrap>
      );
    }

    return (
      <UntruncatedElementWrap
        untruncatedElement={untruncatedElement}
        isLoading={isLoading}
        width={truncateOnWidth}
      >
        <span ref={cellRef}>{value}</span>
      </UntruncatedElementWrap>
    );
  }
);
