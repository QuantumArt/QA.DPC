import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class InputSearch extends AbstractControl<InputHTMLAttributes<HTMLInputElement>> {
  @action
  handleChange(e: any) {
    super.handleChange(e);
    const { model, name } = this.props;
    model[name] = e.target.value;
  }

  render() {
    const { model, name, className, onFocus, onChange, onBlur, ...props } = this.props;
    const inputValue = model[name] != null ? model[name] : "";
    return (
      <div className={cn("bp3-input-group", className)}>
        <input
          type="search"
          className={cn("bp3-input", className)}
          value={inputValue}
          onFocus={this.handleFocus}
          onChange={this.handleChange}
          onBlur={this.handleBlur}
          {...props}
        />
        <span className="bp3-icon bp3-icon-search" />
      </div>
    );
  }
}
