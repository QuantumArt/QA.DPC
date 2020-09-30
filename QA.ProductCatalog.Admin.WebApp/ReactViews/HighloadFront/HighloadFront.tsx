import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { locale } from "moment";
import { Checkbox } from "@blueprintjs/core";
import { getTaskIntentDependsOnState } from "Shared/Utils";
import Store from "./HighloadFrontStore";
import { l } from "./Localization";
import { ProgressBar } from "Shared/Components";
import "./Style.scss";

type Props = {
  store: Store;
};

const HighloadFront = observer<Props>(({ store }) => {
  useEffect(() => {
    locale(window.highloadFront.Culture);
    store.cyclicFetch();
    return () => {
      store.clearTimeout();
    };
  }, []);
  const {
    tasks,
    getFormattedChannelDate,
    getTimePassed,
    isIndexingAvailable,
    handleIndexChannel
  } = store;
  return (
    <div className="container">
      <h3 className="bp3-heading">{l("HighloadFront")}</h3>
      <table className="table">
        <colgroup>
          <col style={{ width: "10%" }} />
          <col style={{ width: "10%" }} />
          <col style={{ width: "9%" }} />
          <col style={{ width: "15%" }} />
          <col style={{ width: "14%" }} />
          <col style={{ width: "14%" }} />
          <col style={{ width: "23%" }} />
        </colgroup>
        <thead>
          <tr>
            <th>{l("Default")}</th>
            <th>{l("Language")}</th>
            <th>{l("Type")}</th>
            <th>{l("Date")}</th>
            <th>{l("Processing")}</th>
            <th>{l("Updating")}</th>
            <th>{l("Progress")}</th>
          </tr>
        </thead>
        <tbody>
          {tasks &&
            tasks.length > 0 &&
            tasks.map((task, index) => (
              <tr key={index}>
                <td>
                  <Checkbox disabled inline checked={task.IsDefault} />
                </td>
                <td>{task.ChannelLanguage}</td>
                <td>{task.ChannelState}</td>
                <td>{getFormattedChannelDate(task.ChannelDate)}</td>
                <td>
                  {isIndexingAvailable(task.TaskState) && (
                    <a href="#" onClick={() => handleIndexChannel(task)}>
                      {l("ProceedIndexing")}
                    </a>
                  )}
                </td>
                <td>{getTimePassed(task.TaskStart, task.TaskEnd)}</td>
                <td>
                  <ProgressBar
                    barWidth="120px"
                    defaultBarProps={{
                      value: task.TaskProgress,
                      intent: getTaskIntentDependsOnState(task.TaskState),
                      animate: false,
                      stripes: false
                    }}
                  />
                </td>
              </tr>
            ))}
        </tbody>
      </table>
    </div>
  );
});

export default HighloadFront;
