import { Tag, ITagProps } from "@blueprintjs/core";
import React, { useMemo } from "react";
import { getChannelTagIntentDependsOnStatus } from "Shared/Utils";
import { ChannelStatuses } from "Shared/Enums";
import cn from "classnames";
import "./Style.scss";

export const StatusTag = ({ value, className }) => {
  const statusTagProps = useMemo(() => {
    const props: ITagProps = {
      round: true,
      icon: false,
      intent: getChannelTagIntentDependsOnStatus(value)
    };

    switch (value) {
      case ChannelStatuses.OK:
        Object.assign(props, { icon: "tick-circle" });
        break;
    }
    return props;
  }, [value]);

  return (
    <Tag {...statusTagProps} className={cn("status-cell-tag", className)}>
      {value}
    </Tag>
  );
};
