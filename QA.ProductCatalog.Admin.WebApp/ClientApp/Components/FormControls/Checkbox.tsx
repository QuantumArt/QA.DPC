import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class CheckBox extends AbstractControl {
  handleChange = action((e: any) => {
    const { model, name } = this.props;
    model[name] = !!e.target.checked;
  });

  render() {
    const { model, name, ...props } = this.props;
    return (
      <div className="form-check">
        <input
          type="checkbox"
          className="form-check-input"
          checked={!!model[name]}
          onChange={this.handleChange}
          {...props}
        />
      </div>
    );
  }
}
