import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { apiService } from "Notification/ApiServices";
import { useStore } from "Notification/UseStore";
import { InputGroup } from "@blueprintjs/core";
import { SelectRow } from "./Components";
import "./Root.scss";
import { CycleDataFetch } from "Shared/Utils";

export const Notification = observer(() => {
  const store = useStore();

  useEffect(() => {
    const cycleFetch = new CycleDataFetch(
      data => console.log(data),
      apiService.getModel.bind(apiService),
      5000,
      15000
    );
    try {
      cycleFetch.initCyclingFetch();
    } catch (e) {
      console.error(e);
    }
    return cycleFetch.breakCycling;
  }, []);

  return (
    <div className="channels-wrap">
      <h2 className="bp3-heading">System settings</h2>
      <SelectRow label={"test1"}>
        <InputGroup disabled={true} value={"test"} />
      </SelectRow>
      <SelectRow label={"test2"}>
        <InputGroup disabled={true} value={"test"} />
      </SelectRow>
      <h2 className="bp3-heading">Channels</h2>
      <span>table</span>
      <h2 className="bp3-heading">Settings</h2>
    </div>
  );
});
