import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";

@observer
export class Numeric extends Component<{
  model: any;
  name: string;
  [x: string]: any;
}> {
  @observable pendingValue = "";

  handleChange = action(e => {
    // @ts-ignore
    const value = e.target.value;
    const { model, name } = this.props;
    if (value === "" || value === "-") {
      this.pendingValue = value;
      model[name] = null;
    } else {
      const number = Number(value);
      if (Number.isSafeInteger(number)) {
        this.pendingValue = "";
        model[name] = value;
      }
    }
  });

  handleBlur = action(e => {
    // @ts-ignore
    const value = e.target.value;
    const { model, name } = this.props;
    if (this.pendingValue) {
      this.pendingValue = "";
      model[name] = null;
    }
  });

  render() {
    const { model, name, ...props } = this.props;
    const value = model[name];
    const textValue = this.pendingValue || (value == null ? "" : String(value));
    return (
      <input
        type="text"
        className="editor-input"
        value={textValue}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
