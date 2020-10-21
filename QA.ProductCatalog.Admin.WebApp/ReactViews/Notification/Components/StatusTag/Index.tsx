import { Tag, ITagProps } from "@blueprintjs/core";
import React, { useMemo } from "react";
import { getPublishTagIntentDependsOnStatus } from "Shared/Utils";
import { ChannelPublishStatuses } from "Shared/Enums";
import cn from "classnames";
import "./Style.scss";
import { isNull, isUndefined } from "lodash";

export const StatusTag = ({ value, className }) => {
  if (isNull(value) || isUndefined(value)) return "";
  const statusTagProps = useMemo(() => {
    const props: ITagProps = {
      round: true,
      icon: false,
      intent: getPublishTagIntentDependsOnStatus(value)
    };

    if (value === ChannelPublishStatuses.OK) {
      Object.assign(props, { icon: "tick-circle" });
    }
    return props;
  }, [value]);

  return (
    <Tag {...statusTagProps} className={cn("status-cell-tag", className)}>
      {value}
    </Tag>
  );
};
