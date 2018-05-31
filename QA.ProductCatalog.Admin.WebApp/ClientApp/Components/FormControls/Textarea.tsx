import React, { TextareaHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import TextAreaAutosize from "react-textarea-autosize";
import { AbstractInput } from "./AbstractControls";

@observer
export class TextArea extends AbstractInput<TextareaHTMLAttributes<HTMLTextAreaElement>> {
  handleChange = e => {
    const { onChange } = this.props;
    if (onChange) {
      onChange(e);
    }
    this.setState({ editValue: e.target.value });
  };

  handleBlur = action((e: any) => {
    const { model, name, onBlur } = this.props;
    model[name] = this.state.editValue;
    if (onBlur) {
      onBlur(e);
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, className, onChange, onFocus, onBlur, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <TextAreaAutosize
        useCacheForDOMMeasurements
        className={cn("editor-textarea form-control", className)}
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
