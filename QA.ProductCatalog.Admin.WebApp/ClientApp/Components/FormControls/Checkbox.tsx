import React from "react";
import { Input } from "reactstrap";
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
    // @ts-ignore
    return (
      <div className="form-check">
        <Input type="checkbox" checked={!!model[name]} onChange={this.handleChange} {...props} />
      </div>
    );
  }
}
