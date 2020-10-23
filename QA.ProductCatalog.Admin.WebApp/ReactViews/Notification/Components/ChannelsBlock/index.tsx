import React, { ReactNode } from "react";
import cn from "classnames";
import "./Style.scss";

export const ChannelsBlock = ({
  className,
  contentWithoutPadding,
  header,
  headerClassName,
  children
}: {
  className?: string;
  header?: string;
  contentWithoutPadding?: boolean;
  headerClassName?: string;
  children: ReactNode;
}) => {
  return (
    <div
      className={cn("channels-block", className, {
        "remove-content-pd": contentWithoutPadding
      })}
    >
      {header && <h3 className={cn("bp3-heading", headerClassName)}>{header}</h3>}
      {children}
    </div>
  );
};
