import React, { ReactNode } from "react";
import "./Style.scss";
import cn from "classnames";

export interface IUntruncatedElementProps {
  untruncatedElement?: JSX.Element | string;
  children?: ReactNode;
  isLoading: boolean;
}

export const UntruncatedElementWrap = ({
  children,
  untruncatedElement,
  isLoading
}: IUntruncatedElementProps) => {
  const renderChild = (childs: ReactNode) =>
    React.Children.map(childs, child =>
      React.cloneElement(child as React.ReactElement, {
        className: cn({ "bp3-skeleton truncate-cell ": isLoading })
      })
    );

  if (!untruncatedElement) return <>{renderChild(children)}</>;
  return (
    <div className="with-untruncated-element">
      {renderChild(children)}
      {untruncatedElement && renderChild(untruncatedElement)}
    </div>
  );
};
