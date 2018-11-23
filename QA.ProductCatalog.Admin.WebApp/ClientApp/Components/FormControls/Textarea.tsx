import React, { TextareaHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import TextAreaAutosize from "react-textarea-autosize";
import { ValidatableInput } from "./AbstractControls";

@observer
export class TextArea extends ValidatableInput<TextareaHTMLAttributes<HTMLTextAreaElement>> {
  handleChange(e: any) {
    super.handleChange(e);
    this.setState({ editValue: e.target.value });
  }

  @action
  handleBlur(e: any) {
    super.handleBlur(e);
    const { model, name } = this.props;
    model[name] = this.state.editValue;
  }

  render() {
    const { model, name, className, onFocus, onChange, onBlur, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <TextAreaAutosize
        useCacheForDOMMeasurements
        className={cn("bp3-input bp3-fill editor-textarea", className)}
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
