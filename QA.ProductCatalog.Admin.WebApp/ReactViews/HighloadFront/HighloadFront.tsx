import React, { ReactNode, Component } from "react";
import { observer } from "mobx-react";
import { locale } from "moment";
import { Checkbox } from "@blueprintjs/core";

import { getTaskIntentDependsOnState } from "Shared/Utils";

import Store from "./store";

import { ProgressBar } from "Shared/Components";

type Props = {
  store: Store;
};

@observer
export default class HighloadFront extends Component<Props> {
  constructor(props) {
    super(props);
    locale(window.highloadFront.culture);
  }

  componentDidMount() {
    this.props.store.cyclicFetch();
  }

  componentWillUnmount() {
    this.props.store.clearTimeout();
  }

  render(): ReactNode {
    const {
      tasks,
      getFormattedChannelDate,
      getTimePassed,
      isIndexingAvailable,
      handleIndexChannel
    } = this.props.store;
    const { highloadFront } = window;
    const { columnHeaders } = highloadFront;

    return (
      <div className="formLayout">
        <fieldset>
          <legend>{highloadFront.legend}</legend>
        </fieldset>
        <table className="inner-groupping-table">
          <colgroup>
            <col style={{ width: "15%" }} />
            <col style={{ width: "10%" }} />
            <col style={{ width: "9%" }} />
            <col style={{ width: "15%" }} />
            <col style={{ width: "14%" }} />
            <col style={{ width: "14%" }} />
            <col style={{ width: "23%" }} />
          </colgroup>
          <thead>
            <tr>
              <th>{columnHeaders.default}</th>
              <th>{columnHeaders.language}</th>
              <th>{columnHeaders.type}</th>
              <th>{columnHeaders.date}</th>
              <th>{columnHeaders.processing}</th>
              <th>{columnHeaders.updating}</th>
              <th>{columnHeaders.progress}</th>
            </tr>
          </thead>
          <tbody>
            {tasks &&
              tasks.length > 0 &&
              tasks.map((task, index) => (
                <tr key={index}>
                  <td>
                    <Checkbox disabled checked={task.IsDefault} />
                  </td>
                  <td>{task.ChannelLanguage}</td>
                  <td>{task.ChannelState}</td>
                  <td>{getFormattedChannelDate(task.ChannelDate)}</td>
                  <td>
                    {isIndexingAvailable(task.TaskState) && (
                      <a href="#" onClick={() => handleIndexChannel(task)}>
                        {highloadFront.processingIndex}
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
  }
}
