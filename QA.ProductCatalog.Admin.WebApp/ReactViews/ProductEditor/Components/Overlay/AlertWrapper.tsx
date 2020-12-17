import React, { Component } from "react";
import { Alert, IAlertProps, Intent, Icon } from "@blueprintjs/core";
import "./AlertWrapper.scss";

interface AlertWrapperProps extends Partial<IAlertProps> {
  onClose: IAlertProps["onClose"];
}

export class AlertWrapper extends Component<AlertWrapperProps> {
  readonly state = { isOpen: true };

  private handleClose = (confirmed: boolean) => {
    this.setState({ isOpen: false });
    this.props.onClose(confirmed);
  };

  render() {
    const { isOpen } = this.state;
    return (
      <Alert
        {...this.props}
        isOpen={isOpen}
        onClose={this.handleClose}
        icon={<Icon icon="warning-sign" iconSize={Icon.SIZE_LARGE} intent={Intent.WARNING} />}
      />
    );
  }
}
