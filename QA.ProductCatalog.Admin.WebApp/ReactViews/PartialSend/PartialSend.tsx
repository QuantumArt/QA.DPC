import React, { Component, ReactNode } from "react";
import { when } from "mobx";
import { observer } from "mobx-react";
import { Intent, Spinner } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";

import { Toaster } from "Shared/Components";
import { MAX_FETCH_COUNT } from "Shared/Constants";
import { FetchStatus } from "Shared/Enums";

import { SendForm, Result } from "./Containers";

import Store from "./PartialSendStore";

import { CurrentStep } from "./Enums";

import "./Style.scss";

type Props = {
  store: Store;
};

@observer
export default class PartialSend extends Component<Props> {
  componentDidMount() {
    const { store } = this.props;

    store.cyclicFetchActiveTask();
    when(
      () => store.fetchStatus === FetchStatus.Success || store.failureFetchCount >= MAX_FETCH_COUNT,
      () => {
        if (store.taskId) {
          store.setCurrentStep(CurrentStep.Result);
        } else {
          store.setCurrentStep(CurrentStep.SendForm);
        }
      }
    );
    when(
      () => store.fetchStatus === FetchStatus.Failure && store.failureFetchCount >= MAX_FETCH_COUNT,
      () => {
        Toaster.show({
          icon: IconNames.WARNING_SIGN,
          message: store.fetchError,
          intent: Intent.DANGER,
          timeout: 10000
        });
      }
    );
  }

  render(): ReactNode {
    let { currentStep } = this.props.store;
    if (currentStep === CurrentStep.Empty) {
      return (
        <div className="formLayout centered PartialSend">
          <Spinner />
        </div>
      );
    }

    return currentStep === CurrentStep.SendForm ? (
      <SendForm store={this.props.store} />
    ) : (
      <Result store={this.props.store} />
    );
  }
}
