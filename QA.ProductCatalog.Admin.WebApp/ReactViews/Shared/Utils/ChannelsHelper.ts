import { ChannelStatuses } from "Shared/Enums";
import { Intent } from "@blueprintjs/core";

export const getChannelTagIntentDependsOnStatus = (status: ChannelStatuses) => {
  switch (status) {
    case ChannelStatuses.NotFound:
      return Intent.NONE;
    case ChannelStatuses.OK:
      return Intent.SUCCESS;
  }
};
