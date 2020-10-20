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

export const Notification = observer(() => {
  const store = useStore();

  useEffect(() => {
    store.initRequests();
    return store.breakRequests;
  }, []);
  console.log("rerender");

  return (
    <div className="channels-wrap">
      <ChannelsBlock header="System settings">
        <ChannelsSystemSettings settings={store.getSystemSettings} />
      </ChannelsBlock>

      <ChannelsBlock contentWithoutPadding header="Channels">
        <ChannelsGrid gridData={store.getChannels} />
      </ChannelsBlock>

      <ChannelsBlock header="Settings">
        <ChannelsGeneralSettings settings={store.getGeneralSettings} />
      </ChannelsBlock>
    </div>
  );
});
