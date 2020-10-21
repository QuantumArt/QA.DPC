import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { useStore } from "Notification/UseStore";
import {
  ChannelsBlock,
  ChannelsGeneralSettings,
  ChannelsGrid,
  ChannelsSystemSettings
} from "./Components";
import "./Root.scss";
import { l } from "Notification/Localization";

export const Notification = observer(() => {
  const store = useStore();

  useEffect(() => {
    store.initRequests();
    return store.breakRequests;
  }, []);

  return (
    <div className="channels-wrap">
      <ChannelsBlock header={l("sSettings")}>
        <ChannelsSystemSettings settings={store.getSystemSettings} />
      </ChannelsBlock>

      <ChannelsBlock contentWithoutPadding header={l("channels")}>
        <ChannelsGrid gridData={store.getChannels} />
      </ChannelsBlock>

      <ChannelsBlock header={l("gSettings")}>
        <ChannelsGeneralSettings settings={store.getGeneralSettings} />
      </ChannelsBlock>
    </div>
  );
});
