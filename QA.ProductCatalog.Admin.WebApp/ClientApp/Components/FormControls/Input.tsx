import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";

@observer
export class Input extends Component<{
  model: any;
  name: string;
  [x: string]: any;
}> {
  handleChange = action((e: any) => {
    const { model, name } = this.props;
    model[name] = e.target.value;
  });

  render() {
    const { model, name, ...props } = this.props;
    const value = model[name];
    return (
      <input
        type="text"
        className="editor-input"
        value={value == null ? "" : value}
        onChange={this.handleChange}
        {...props}
      />
    );
  }
}
