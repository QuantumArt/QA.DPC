import React from "react";
import { action } from "mobx";

export const Input = ({ model, name, ...props }) => (
  <input
    type="text"
    value={model[name] == null ? "" : model[name]}
    onInput={action(e => {
      // @ts-ignore
      model[name] = e.target.value;
    })}
    {...props}
  />
);

export const Checkbox = ({ model, name, ...props }) => (
  <input
    type="checkbox"
    checked={!!model[name]}
    onChange={action(e => {
      // @ts-ignore
      model[name] = !!e.target.checked;
    })}
    {...props}
  />
);
