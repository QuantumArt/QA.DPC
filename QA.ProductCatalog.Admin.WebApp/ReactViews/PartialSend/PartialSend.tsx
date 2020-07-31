import React, { Component, ReactNode } from "react";
import { observer } from "mobx-react";

import { SendForm, Result } from "./Containers";

import Store from "./PartialSendStore";

import { CurrentStep } from "./Enums";

import "./style.css";

type Props = {
  store: Store;
};

@observer
export default class PartialSend extends Component<Props> {
  componentDidMount() {
    this.props.store.partialSendRefreshData();
  }

  render(): ReactNode {
    const { currentStep } = this.props.store;

    if (currentStep === CurrentStep.Empty) {
      return null;
    }

    return currentStep === CurrentStep.SendForm ? (
      <SendForm store={this.props.store} />
    ) : (
      <Result store={this.props.store} />
    );
  }
}
