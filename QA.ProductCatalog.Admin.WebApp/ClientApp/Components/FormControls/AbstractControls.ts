import { Component } from "react";
import { autorun, untracked, transaction, IReactionDisposer } from "mobx";
import { isPlainObject, isArray } from "Utils/TypeChecks";
import { ValidatableObject } from "Models/ValidatableMixin";

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

type Model = ValidatableObject & { [x: string]: any };
type Validator = (value: any) => string;

interface ValidatableProps {
  model: Model;
  validate?: Validator | Validator[];
}

export abstract class ValidatableControl<P = {}> extends AbstractControl<ValidatableProps & P> {
  private _validationDisposer: IReactionDisposer;

  componentDidMount() {
    const { model, name, validate } = this.props;

    const initialValue = untracked(() => model[name]);
    const hasEmptyInitialValue =
      initialValue == null ||
      initialValue === "" ||
      (isArray(initialValue) && initialValue.length === 0);

    if (!hasEmptyInitialValue) {
      model.setTouched(name, true);
    }

    if (validate) {
      const validators: Validator[] = isArray(validate) ? validate : [validate];

      this._validationDisposer = autorun(() => {
        const value = model[name];
        const errors = validators.map(validator => validator(value)).filter(Boolean);
        if (errors.length > 0) {
          model.addErrors(name, ...errors);
        }
      });
    }
  }

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

  componentWillUnmount() {
    if (this._validationDisposer) {
      this._validationDisposer();
    }
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
    let editValue = model[name];
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
