import React, { ReactNode, RefObject, useLayoutEffect, useRef, useState, memo } from "react";
import { Tooltip, Position } from "@blueprintjs/core";
import { IUntruncatedElementProps, UntruncatedElementWrap } from "./Subcomponents";
import "./Style.scss";

interface IProps extends IUntruncatedElementProps {
  value: ReactNode;
  refBody: RefObject<any>;
  isLoading: boolean;
  truncateOnWidth: number;
}

export const GridTruncatedCellContent = memo(
  ({ refBody, isLoading, truncateOnWidth, untruncatedElement, value }: IProps) => {
    const cellRef = useRef(null);
    const [isTruncate, setIsTruncate] = useState(false);

    useLayoutEffect(() => {
      if (!isTruncate && cellRef.current.offsetHeight > 20 && !isLoading) {
        setIsTruncate(true);
      }
    }, [isLoading, cellRef, isTruncate]);

    useLayoutEffect(() => {
      if (isTruncate && isLoading) {
        setIsTruncate(false);
      }
    }, [isLoading, isTruncate]);

    if (isTruncate && !isLoading) {
      return (
        <UntruncatedElementWrap
          untruncatedElement={untruncatedElement}
          isLoading={isLoading}
          width={truncateOnWidth}
        >
          <Tooltip
            position={Position.LEFT}
            usePortal
            content={value as string}
            portalContainer={refBody.current}
          >
            <div
              style={truncateOnWidth && { width: truncateOnWidth }}
              className="truncate-cell truncate-string"
            >
              <span>{value && value}</span>
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
