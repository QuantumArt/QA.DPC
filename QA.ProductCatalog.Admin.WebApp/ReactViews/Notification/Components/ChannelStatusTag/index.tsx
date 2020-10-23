import { Tag, ITagProps } from "@blueprintjs/core";
import React, { useMemo } from "react";
import { getChannelStatusDescription, getChannelTagIntentDependsOnStatus } from "Shared/Utils";
import { ChannelStatuses } from "Shared/Enums";
import cn from "classnames";
import "./Style.scss";
import { isNull, isUndefined } from "lodash";

interface IProps {
  value: ChannelStatuses;
  className: string;
}

export const ChannelStatusTag = ({ value, className }: IProps) => {
  if (isNull(value) || isUndefined(value) || value === ChannelStatuses.Actual) return "";

  const statusTagProps = useMemo(() => {
    const props: ITagProps = {
      round: true,
      icon: false,
      intent: getChannelTagIntentDependsOnStatus(value)
    };

    switch (value) {
      case ChannelStatuses.Changed:
        Object.assign(props, { icon: "refresh" });
        break;
      case ChannelStatuses.Deleted:
        Object.assign(props, { icon: "disable" });
        break;
      case ChannelStatuses.New:
        Object.assign(props, { icon: "add" });
        break;
    }
    return props;
  }, [value]);

  return (
    <Tag {...statusTagProps} className={cn("status-cell-tag", className)}>
      {getChannelStatusDescription(value)}
    </Tag>
  );
};
