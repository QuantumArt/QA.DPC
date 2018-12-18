import React from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { LocaleContext } from "react-lazy-i18n";
import DateTime from "react-datetime";
import moment from "moment";
import "react-datetime/css/react-datetime.css";
import { ValidatableInput } from "./AbstractControls";

interface DatePickerProps {
  id?: string;
  type?: "date" | "time";
  placeholder?: string;
  disabled?: boolean;
  readOnly?: boolean;
}

@observer
export class DatePicker extends ValidatableInput<DatePickerProps> {
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
      <div className={cn("bp3-input-group", { "bp3-fill": type !== "time" }, className)}>
        <LocaleContext.Consumer>
          {locale => (
            <DateTime
              className="editor-datepicker"
              inputProps={{
                className: "bp3-input",
                id,
                placeholder,
                disabled,
                readOnly
              }}
              locale={(locale && locale.slice(0, 2).toLowerCase()) || "en"}
              dateFormat={type !== "time"}
              timeFormat={type !== "date"}
              value={inputValue}
              onFocus={this.handleFocus}
              onChange={this.handleChange}
              onBlur={this.handleBlur}
              {...props}
            />
          )}
        </LocaleContext.Consumer>
        <span className={cn("bp3-icon", type === "time" ? "bp3-icon-time" : "bp3-icon-calendar")} />
      </div>
    );
  }
}
