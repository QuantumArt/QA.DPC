import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputNumber extends AbstractInput<{ isInteger?: boolean }> {
  handleChange = e => {
    const editValue = e.target.value;
    if (
      editValue === "" ||
      editValue === "-" ||
      (this.props.isInteger
        ? Number.isSafeInteger(Number(editValue))
        : Number.isFinite(Number(editValue)))
    ) {
      this.setState({ editValue });
    }
  };

  handleBlur = action(() => {
    const { model, name, required } = this.props;
    const { editValue } = this.state;
    if (!required && editValue === "") {
      model[name] = null;
    } else if (editValue !== "-") {
      model[name] = Number(editValue);
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, isInteger, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <input
        type="text"
        className="form-control"
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
