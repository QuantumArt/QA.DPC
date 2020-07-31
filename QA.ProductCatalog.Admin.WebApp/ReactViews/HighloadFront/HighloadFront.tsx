import React, { ReactNode, Component } from "react";
import { observer } from "mobx-react";
import { locale } from "moment";

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
          <thead>
            <tr>
              <th>{columnHeaders.default}</th>
              <th width="100px">{columnHeaders.language}</th>
              <th width="100px">{columnHeaders.type}</th>
              <th width="150px">{columnHeaders.date}</th>
              <th>{columnHeaders.processing}</th>
              <th width="200px">{columnHeaders.updating}</th>
              <th width="300px">{columnHeaders.progress}</th>
            </tr>
          </thead>
          <tbody>
            {tasks &&
              tasks.length > 0 &&
              tasks.map((task, index) => (
                <tr key={index}>
                  <td>
                    <input disabled type="checkbox" checked={task.IsDefault} />
                  </td>
                  <td>{task.ChannelLanguage}</td>
                  <td>{task.ChannelState}</td>
                  <td>{getFormattedChannelDate(task.ChannelDate)}</td>
                  <td width="100px">
                    {isIndexingAvailable(task.TaskState) && (
                      <a href="#" onClick={() => handleIndexChannel(task)}>
                        {highloadFront.processingIndex}
                      </a>
                    )}
                  </td>
                  <td>{getTimePassed(task.TaskStart, task.TaskEnd)}</td>
                  <td>
                    <ProgressBar barWidth="120px" defaultBarProps={{ value: task.TaskProgress }} />
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    );
  }
}
