import React, { Component } from "react";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import "./FormControls.scss";

export const Input = observer(({ model, name, ...props }) => {
  const value = model[name];
  return (
    <input
      type="text"
      className="editor-input"
      value={value == null ? "" : value}
      onInput={action(e => {
        // @ts-ignore
        model[name] = e.target.value;
      })}
      {...props}
    />
  );
});

type NumericProps = {
  model: any;
  name: string;
  [x: string]: any;
};

@observer
export class Numeric extends Component<NumericProps> {
  @observable pendingValue = "";

  handleChange = action(e => {
    // @ts-ignore
    const value = e.target.value;
    const { model, name } = this.props;
    if (value === "" || value === "-") {
      this.pendingValue = value;
      model[name] = null;
    } else {
      const number = Number(value);
      if (Number.isSafeInteger(number)) {
        this.pendingValue = "";
        model[name] = value;
      }
    }
  });

  handleBlur = action(e => {
    // @ts-ignore
    const value = e.target.value;
    const { model, name } = this.props;
    if (this.pendingValue) {
      this.pendingValue = "";
      model[name] = null;
    }
  });

  render() {
    const { model, name, ...props } = this.props;
    const value = model[name];
    const textValue = this.pendingValue || (value == null ? "" : String(value));
    return (
      <input
        type="text"
        className="editor-input"
        value={textValue}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}

// TODO: autosize
export const Textarea = observer(({ model, name, ...props }) => {
  const value = model[name];
  return (
    <textarea
      type="text"
      className="editor-textarea"
      value={value == null ? "" : value}
      onInput={action(e => {
        // @ts-ignore
        model[name] = e.target.value;
      })}
      {...props}
    />
  );
});

export const Checkbox = observer(({ model, name, ...props }) => (
  <input
    type="checkbox"
    className="editor-checkbox"
    checked={!!model[name]}
    onChange={action(e => {
      // @ts-ignore
      model[name] = !!e.target.checked;
    })}
    {...props}
  />
));
