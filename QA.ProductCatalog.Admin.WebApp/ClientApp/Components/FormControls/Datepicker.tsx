import React from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import DateTime from "react-datetime";
import moment from "moment";
import "moment/locale/ru";
import "react-datetime/css/react-datetime.css";
import { AbstractInput } from "./AbstractControls";

interface DatePickerProps {
  id?: string;
  type?: "date" | "time";
  placeholder?: string;
  disabled?: boolean;
  readOnly?: boolean;
}

@observer
export class DatePicker extends AbstractInput<DatePickerProps> {
  handleChange(editValue: string | moment.Moment) {
    super.handleChange(editValue);
    this.setState({ editValue });
  }

  @action
  handleBlur(e: any) {
    super.handleBlur(e);
    const { model, name } = this.props;
    const { editValue } = this.state;
    if (moment.isMoment(editValue)) {
      model[name] = editValue.toDate();
    } else if (editValue === "") {
      model[name] = null;
    }
  }

  render() {
    const {
      model,
      name,
      className,
      onFocus,
      onChange,
      onBlur,
      validate,
      id,
      type,
      placeholder,
      disabled,
      readOnly,
      ...props
    } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : null;
    return (
      <div className={cn("pt-input-group", { "pt-fill": type !== "time" }, className)}>
        <DateTime
          className="editor-datepicker"
          inputProps={{
            className: "pt-input",
            id,
            placeholder,
            disabled,
            readOnly
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
        <span className={cn("pt-icon", type === "time" ? "pt-icon-time" : "pt-icon-calendar")} />
      </div>
    );
  }
}
