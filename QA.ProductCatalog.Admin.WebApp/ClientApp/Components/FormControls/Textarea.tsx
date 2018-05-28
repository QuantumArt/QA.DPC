import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import TextAreaAutosize from "react-textarea-autosize";
import { AbstractInput } from "./AbstractControls";

@observer
export class TextArea extends AbstractInput {
  handleChange = e => {
    this.setState({ editValue: e.target.value });
  };

  handleBlur = action(() => {
    const { model, name } = this.props;
    model[name] = this.state.editValue;
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <TextAreaAutosize
        useCacheForDOMMeasurements
        className="editor-textarea form-control"
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
