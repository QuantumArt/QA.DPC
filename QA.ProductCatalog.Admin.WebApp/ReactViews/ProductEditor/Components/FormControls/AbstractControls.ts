import { Component } from "react";
import { transaction } from "mobx";
import { isPlainObject } from "ProductEditor/Utils/TypeChecks";
import { ValidatableObject } from "ProductEditor/Packages/mst-validation-mixin";

interface ControlProps {
  [x: string]: any;
  model: { [x: string]: any };
  name: string;
  className?: string;
  onFocus?: (...args) => void;
  onChange?: (...args) => void;
  onBlur?: (...args) => void;
}

export abstract class AbstractControl<P = {}> extends Component<ControlProps & P> {
  constructor(props: ControlProps & P, context?: any) {
    super(props, context);
    const { model, name } = props;
    if (!(name in model)) {
      const modelName = isPlainObject(model) ? "Object" : Object.getPrototypeOf(model).name;
      throw new TypeError(`Object [${modelName}] does not have property "${name}"`);
    }
    this.handleFocus = this.handleFocus.bind(this);
    this.handleChange = this.handleChange.bind(this);
    this.handleBlur = this.handleBlur.bind(this);
  }

  protected handleFocus(...args) {
    if (this.props.onFocus) {
      this.props.onFocus(...args);
    }
  }

  protected handleChange(...args) {
    if (this.props.onChange) {
      this.props.onChange(...args);
    }
  }

  protected handleBlur(...args) {
    if (this.props.onBlur) {
      this.props.onBlur(...args);
    }
  }
}

interface ValidatableProps {
  model: ValidatableObject & { [x: string]: any };
}

export abstract class ValidatableControl<P = {}> extends AbstractControl<ValidatableProps & P> {
  protected handleFocus(...args) {
    super.handleFocus(...args);
    const { model, name } = this.props;
    transaction(() => {
      model.setFocus(name, true);
      model.setTouched(name, true);
    });
  }

  protected handleChange(...args) {
    super.handleChange(...args);
    const { model, name } = this.props;
    model.setTouched(name, true);
  }

  protected handleBlur(...args) {
    super.handleBlur(...args);
    const { model, name } = this.props;
    model.setFocus(name, false);
  }
}

export abstract class ValidatableInput<P = {}> extends ValidatableControl<P> {
  readonly state = {
    hasFocus: false,
    editValue: ""
  };

  protected handleFocus(...args) {
    super.handleFocus(...args);
    const { model, name } = this.props;
    let editValue: string = model[name];
    if (editValue == null) {
      editValue = "";
    }
    this.setState({ hasFocus: true, editValue });
  }

  protected handleBlur(...args) {
    super.handleBlur(...args);
    this.setState({ hasFocus: false });
  }
}
