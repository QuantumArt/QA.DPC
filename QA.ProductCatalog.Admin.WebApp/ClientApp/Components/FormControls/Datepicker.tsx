import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import DateTime from "react-datetime";
import moment from "moment";
import "moment/locale/ru";
import "react-datetime/css/react-datetime.css";

@observer
export class DatePicker extends Component<{
  model: any;
  name: string;
  type?: "date" | "time";
  [x: string]: any;
}> {
  @observable pendingValue = "";

  handleChange = action((momentValue: string | moment.Moment) => {
    const { model, name } = this.props;
    console.log(this.pendingValue, model[name]);
    if (typeof momentValue === "string") {
      this.pendingValue = momentValue;
      model[name] = null;
    } else {
      this.pendingValue = "";
      model[name] = momentValue.toISOString();
    }
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    if (this.pendingValue) {
      this.pendingValue = "";
      model[name] = null;
    }
  });

  render() {
    const { model, name, type, ...props } = this.props;
    const value = model[name];
    const momentValue = this.pendingValue || (value == null ? null : moment(value));
    return (
      <DateTime
        className="editor-datepicker"
        locale="ru-ru"
        dateFormat={type !== "time"}
        timeFormat={type !== "date"}
        value={momentValue}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
