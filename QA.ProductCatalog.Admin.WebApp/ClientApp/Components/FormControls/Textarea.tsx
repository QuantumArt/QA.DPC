import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import TextareaAutosize from "react-textarea-autosize";

@observer
export class Textarea extends Component<{
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
      <TextareaAutosize
        useCacheForDOMMeasurements
        minRows={2}
        maxRows={6}
        className="editor-textarea"
        value={value == null ? "" : value}
        onChange={this.handleChange}
        {...props}
      />
    );
  }
}
