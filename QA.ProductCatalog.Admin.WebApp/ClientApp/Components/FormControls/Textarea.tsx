import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import TextAreaAutosize from "react-textarea-autosize";
import { AbstractInput } from "./AbstractControls";

@observer
export class TextArea extends AbstractInput {
  handleChange = action((e: any) => {
    this.editValue = e.target.value;
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    this.hasFocus = false;
    model[name] = this.editValue;
  });

  render() {
    const { model, name, ...props } = this.props;
    const inputValue = this.hasFocus ? this.editValue : model[name] != null ? model[name] : "";
    return (
      <TextAreaAutosize
        useCacheForDOMMeasurements
        className="editor-textarea form-control form-control-sm"
        minRows={2}
        maxRows={6}
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
