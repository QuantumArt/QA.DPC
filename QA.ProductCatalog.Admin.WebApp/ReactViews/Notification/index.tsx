import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { useStore } from "Notification/UseStore";
import {
  ChannelsBlock,
  ChannelsGeneralSettings,
  ChannelsGrid,
  ChannelsLoader,
  ChannelsSystemSettings
} from "./Components";
import "./Root.scss";
import { l } from "Notification/Localization";
import { Callout, Intent } from "@blueprintjs/core";

const srtWithUpperFirstLetter = function(str) {
  return `${str.charAt(0).toUpperCase()}${str.slice(1)}`;
};

export const Notification = observer(() => {
  const store = useStore();

  useEffect(() => {
    store.initRequests();
    return store.breakRequests;
  }, []);

  return (
    <div className="channels-wrap">
      <ChannelsLoader isActive={store.isLoading} />

      {!store.isActual && (
        <ChannelsBlock className={store.isLoading && "display-none"}>
          <Callout intent={Intent.WARNING}>
            {l("notificationSenderChanged")}.{" "}
            <a onClick={() => store.priorityRequestInSameTime(store.updateConfiguration)}>
              {srtWithUpperFirstLetter(l("update"))}
            </a>
          </Callout>
        </ChannelsBlock>
      )}

      <ChannelsBlock header={l("sSettings")} className={store.isLoading && "display-none"}>
        <ChannelsSystemSettings settings={store.getSystemSettings} />
      </ChannelsBlock>

      <ChannelsBlock
        contentWithoutPadding
        header={l("channels")}
        className={store.isLoading && "display-none"}
      >
        <ChannelsGrid gridData={store.getChannels} />
      </ChannelsBlock>

      <ChannelsBlock header={l("gSettings")} className={store.isLoading && "display-none"}>
        <ChannelsGeneralSettings settings={store.getGeneralSettings} />
      </ChannelsBlock>
    </div>
  );
});
