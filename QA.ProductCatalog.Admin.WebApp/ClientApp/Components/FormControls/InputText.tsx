import React from "react";
import MaskedInput, { MaskedInputProps } from "react-text-mask";
import { action } from "mobx";
import { observer } from "mobx-react";
import { isString } from "Utils/TypeChecks";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputText extends AbstractInput<{ pattern?: string | RegExp } & MaskedInputProps> {
  handleChange = e => {
    this.setState({ editValue: e.target.value });
  };

  handleBlur = action(() => {
    let { model, name, required, pattern } = this.props;
    const { editValue } = this.state;
    if (pattern) {
      const regExp = isString(pattern) ? new RegExp(pattern) : pattern;
      if (regExp.test(editValue) || (!required && editValue === "")) {
        model[name] = editValue;
      }
    } else if (!required || editValue !== "") {
      model[name] = editValue;
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    if (props.pattern) {
      // @ts-ignore
      props.pattern = String(props.pattern);
    }
    return props.mask ? (
      <MaskedInput
        className="form-control"
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    ) : (
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
