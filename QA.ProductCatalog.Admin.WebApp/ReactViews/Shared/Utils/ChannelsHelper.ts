import { ChannelPublishStatuses, ChannelStatuses } from "Shared/Enums";
import { Intent } from "@blueprintjs/core";

export const getPublishTagIntentDependsOnStatus = (status: ChannelPublishStatuses): Intent => {
  switch (status) {
    case ChannelPublishStatuses.NotFound:
      return Intent.DANGER;
    case ChannelPublishStatuses.OK:
      return Intent.SUCCESS;
    default:
      return Intent.DANGER;
  }
};

export const getChannelTagIntentDependsOnStatus = (status: ChannelStatuses): Intent => {
  switch (status) {
    case ChannelStatuses.Changed:
      return Intent.PRIMARY;
    case ChannelStatuses.Deleted:
      return Intent.DANGER;
    case ChannelStatuses.New:
      return Intent.SUCCESS;
    default:
      return Intent.NONE;
  }
};

export const getChannelStatusDescription = (status: ChannelStatuses): string => {
  switch (status) {
    case ChannelStatuses.Changed:
      return "Changed";
    case ChannelStatuses.Deleted:
      return "Disconnected";
    case ChannelStatuses.New:
      return "Added";
    case ChannelStatuses.Actual:
      return "Actual";
    default:
      return "";
  }
};

export const firstLetterUp = function(str) {
  return `${str.charAt(0).toUpperCase()}${str.slice(1)}`;
};
export const firstLetterLow = function(str) {
  return `${str.charAt(0).toLowerCase()}${str.slice(1)}`;
};
