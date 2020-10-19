import React, { Component, ReactNode } from "react";
import { observer } from "mobx-react";
import moment from "moment";
import { Button, Intent, Spinner } from "@blueprintjs/core";

import { Chip, ProgressBar } from "Shared/Components";

import Store from "../PartialSendStore";

import { getStateInfoByStateId } from "../Utils";

import { CurrentStep } from "../Enums";

type Props = {
  store: Store;
};

@observer
export default class Result extends Component<Props> {
  componentDidMount() {
    this.props.store.cyclicFetchTask();
  }

  componentWillUnmount() {
    clearTimeout(this.props.store.timerId);
  }

  renderValue = (value: any): any => {
    return value != null ? value : <span>&nbsp;</span>;
  };

  render(): ReactNode {
    const { legend, labels, sendNewPackageButton } = window.partialSend.result;
    const {
      displayName,
      id,
      createdDate,
      userName,
      state,
      progress,
      lastStatusChangeTime,
      message
    } = labels;

    const { currentStep, task, taskProcessingFinished, handleSendNewPackage } = this.props.store;

    if (currentStep !== CurrentStep.Result) {
      return null;
    }

    if (!task) {
      return (
        <div className="formLayout centered PartialSend">
          <Spinner />
        </div>
      );
    }

    const [stateIntent, stateIconName] = getStateInfoByStateId(task.stateId);

    return (
      <div className="formLayout PartialSend">
        <fieldset>
          <h3 className="bp3-heading">{legend}</h3>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{displayName}</dt>
            <dd className="plain-field-value field">{this.renderValue(task.displayName)}</dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{id}</dt>
            <dd className="plain-field-value field">{this.renderValue(task.id)}</dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{createdDate}</dt>
            <dd className="plain-field-value field">
              {task.createdTime ? (
                <>
                  <span>{moment(task.createdTime).format("DD MMM YYYY")}</span>
                  &nbsp;
                  <span className="half-transparent">
                    {moment(task.createdTime).format("HH:mm")}
                  </span>
                </>
              ) : (
                <span>&nbsp;</span>
              )}
            </dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{userName}</dt>
            <dd className="plain-field-value field">{this.renderValue(task.userName)}</dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{state}</dt>
            <dd className="plain-field-value field">
              <Chip
                intent={stateIntent}
                iconName={stateIconName}
                text={this.renderValue(task.state)}
              />
            </dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{progress}</dt>
            <dd className="plain-field-value field">
              <ProgressBar
                barWidth="120px"
                defaultBarProps={{
                  value: task.progress,
                  animate: false,
                  stripes: false,
                  intent: stateIntent
                }}
              />
            </dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{lastStatusChangeTime}</dt>
            <dd className="plain-field-value field">
              {task.lastStatusChangeTime ? (
                <>
                  <span>{moment(task.lastStatusChangeTime).format("DD MMM YYYY")}</span>
                  &nbsp;
                  <span className="half-transparent">
                    {moment(task.lastStatusChangeTime).format("HH:mm")}
                  </span>
                </>
              ) : (
                <span>&nbsp;</span>
              )}
            </dd>
          </dl>
          <dl className="plain-field row">
            <dt className="plain-field-capture label">{message}</dt>
            <dd className="plain-field-value field">{this.renderValue(task.message)}</dd>
          </dl>
          {taskProcessingFinished && (
            <dl className="plain-field row">
              <dt className="plain-field-capture label" />
              <dd className="plain-field-value field transparent">
                <Button
                  intent={Intent.PRIMARY}
                  text={sendNewPackageButton}
                  onClick={handleSendNewPackage}
                />
              </dd>
            </dl>
          )}
        </fieldset>
      </div>
    );
  }
}
