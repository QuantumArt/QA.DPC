import { Component } from "react";
import { isPlainObject } from "Utils/TypeChecks";

interface ControlProps {
  [x: string]: any;
  model: { [x: string]: any };
  name: string;
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

export abstract class AbstractInput<P = {}> extends AbstractControl<{ required?: boolean } & P> {
  state = {
    hasFocus: false,
    editValue: ""
  };

  handleFocus = () => {
    const { model, name } = this.props;
    let editValue = model[name];
    if (editValue == null) {
      editValue = "";
    }
    this.setState({ hasFocus: true, editValue });
  };
}
