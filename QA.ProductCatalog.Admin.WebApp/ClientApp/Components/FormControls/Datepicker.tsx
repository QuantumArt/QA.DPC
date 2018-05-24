import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import DateTime from "react-datetime";
import moment from "moment";
import "moment/locale/ru";
import "react-datetime/css/react-datetime.css";
import { AbstractInput } from "./AbstractControls";

@observer
export class DatePicker extends AbstractInput<{ type?: "date" | "time" }> {
  handleChange = action((momentValue: string | moment.Moment) => {
    this.editValue = momentValue;
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    this.hasFocus = false;
    if (moment.isMoment(this.editValue)) {
      model[name] = this.editValue.toDate();
    } else if (this.editValue === "") {
      model[name] = null;
    }
  });

  render() {
    const { model, name, type, ...props } = this.props;
    const inputValue = this.hasFocus ? this.editValue : model[name] != null ? model[name] : null;
    return (
      <DateTime
        className="editor-datepicker"
        inputProps={inputProps}
        locale="ru-ru"
        dateFormat={type !== "time"}
        timeFormat={type !== "date"}
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}

const inputProps = {
  className: "form-control form-control-sm"
};
