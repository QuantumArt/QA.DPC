import { Component } from "react";
import { isPlainObject } from "Utils/TypeChecks";

interface ControlProps {
  [x: string]: any;
  model: { [x: string]: any };
  name: string;
  className?: string;
  onChange?: (e: any) => void;
}

export abstract class AbstractControl<P = {}> extends Component<ControlProps & P> {
  constructor(props: ControlProps & P, context?: any) {
    super(props, context);
    const { model, name } = props;
    if (!(name in model)) {
      const modelName = isPlainObject(model) ? "Object" : Object.getPrototypeOf(model).name;
      throw new TypeError(`Object [${modelName}] does not have property "${name}"`);
    }
  }
}

interface InputProps {
  onFocus?: (e: any) => void;
  onBlur?: (e: any) => void;
}

export abstract class AbstractInput<P = {}> extends AbstractControl<InputProps & P> {
  state = {
    hasFocus: false,
    editValue: ""
  };

  handleFocus = e => {
    const { model, name, onFocus } = this.props;
    let editValue = model[name];
    if (editValue == null) {
      editValue = "";
    }
    if (onFocus) {
      onFocus(e);
    }
    this.setState({ hasFocus: true, editValue });
  };
}
