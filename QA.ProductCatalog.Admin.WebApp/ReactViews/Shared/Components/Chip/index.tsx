import React from "react";
import { Intent, Icon, Colors } from "@blueprintjs/core";
import { IconName } from "@blueprintjs/icons";

import "./Style.scss";

const getChipBackgroundColorDependsOnIntent = (intent: Intent): string => {
  switch (intent) {
    case Intent.PRIMARY:
      return Colors.BLUE3;
    case Intent.SUCCESS:
      return Colors.GREEN3;
    case Intent.WARNING:
      return Colors.ORANGE3;
    case Intent.DANGER:
      return Colors.RED3;
    case Intent.NONE:
    default:
      return Colors.GRAY3;
  }
};

type Props = {
  intent: Intent;
  iconName: IconName;
  text: string;
};

export default function Chip({ intent, iconName, text }: Props) {
  const backgroundColor = getChipBackgroundColorDependsOnIntent(intent);

  return (
    <span className="Chip" style={{ backgroundColor }}>
      <Icon
        iconSize={Icon.SIZE_STANDARD}
        icon={iconName}
        color={Colors.WHITE}
        className="Chip__icon"
      />
      <span>{text}</span>
    </span>
  );
}
