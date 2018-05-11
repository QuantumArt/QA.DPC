import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";

@observer
export class Checkbox extends Component<{
  model: any;
  name: string;
  [x: string]: any;
}> {
  handleChange = action((e: any) => {
    const { model, name } = this.props;
    model[name] = !!e.target.checked;
  });

  render() {
    const { model, name, ...props } = this.props;
    return (
      <input
        type="checkbox"
        className="editor-checkbox"
        checked={!!model[name]}
        onChange={this.handleChange}
        {...props}
      />
    );
  }
}
