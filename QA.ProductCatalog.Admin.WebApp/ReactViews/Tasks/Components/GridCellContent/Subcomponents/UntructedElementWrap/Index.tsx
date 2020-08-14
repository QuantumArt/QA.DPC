import React, { ReactNode } from "react";
import "./Style.scss";
import cn from "classnames";

export interface IUntruncatedElementProps {
  untruncatedElement?: JSX.Element | string;
  children?: ReactNode;
  isLoading: boolean;
  width?: number;
}

const renderChild = (node: ReactNode, isLoading: boolean, width: number) =>
  React.Children.map(node, child =>
    React.cloneElement(child as React.ReactElement, {
      style: width && isLoading ? { width: width } : null,
      className: cn("truncate-cell", { "bp3-skeleton": isLoading })
    })
  );

export const UntruncatedElementWrap = ({
  children,
  untruncatedElement,
  isLoading,
  width
}: IUntruncatedElementProps) => {
  if (!untruncatedElement) return <>{renderChild(children, isLoading, width)}</>;

  return (
    <div className="with-untruncated-element">
      {renderChild(children, isLoading, width)}
      {untruncatedElement && renderChild(untruncatedElement, isLoading, width)}
    </div>
  );
};
