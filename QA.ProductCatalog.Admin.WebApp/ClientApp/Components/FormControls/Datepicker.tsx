import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import DateTime from "react-datetime";
import moment from "moment";
import "moment/locale/ru";
import "react-datetime/css/react-datetime.css";
import { AbstractInput } from "./AbstractControls";

@observer
export class DatePicker extends AbstractInput<{
  type?: "date" | "time";
  placeholder?: string;
  disabled?: boolean;
}> {
  handleChange = action((editValue: string | moment.Moment) => {
    this.setState({ editValue });
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    const { editValue } = this.state;
    if (moment.isMoment(editValue)) {
      model[name] = editValue.toDate();
    } else if (editValue === "") {
      model[name] = null;
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, type, disabled, placeholder, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : null;
    return (
      <DateTime
        className="editor-datepicker"
        inputProps={{
          className: "form-control form-control-sm",
          placeholder,
          disabled
        }}
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
