import React, { Component, ReactNode } from "react";
import {
  observable,
  computed,
  action,
  transaction,
  intercept,
  observe,
  comparer,
  extendObservable,
  isObservableArray,
  isObservableMap,
  Lambda
} from "mobx";
import { getSnapshot, isStateTreeNode } from "mobx-state-tree";
import { isArray } from "Utils/TypeChecks";

export type Validator = (value: any) => string;

export interface ValidatableObject {
  isTouched(name?: string): boolean;
  setTouched(name: string, isTouched: boolean): void;
  hasFocus(name: string): boolean;
  setFocus(name: string, hasFocus: boolean): void;
  hasErrors(name?: string): boolean;
  hasVisibleErrors(name?: string): boolean;
  getErrors(name: string): string[] | null;
  getVisibleErrors(name: string): string[] | null;
  getAllErrors(): { [field: string]: string[] } | null;
  getAllVisibleErrors(): { [field: string]: string[] } | null;
  addErrors(name: string, ...errors: string[]): void;
  clearErrors(name?: string): void;
  addValidators(name: string, ...validators: Validator[]): void;
  removeValidators(name: string, ...validators: Validator[]): void;
}

const SHALLOW = { deep: false };

class FieldState {
  @observable.ref isTouched = false;
  @observable.ref hasFocus = false;
  validators = observable.array<Validator>(null, SHALLOW);
  errors = observable.array<string>(null, SHALLOW);

  constructor(private _model: Object, private _name: string) {}

  @computed
  get allErrors() {
    const fieldErrors: string[] = [];
    const value = this._model[this._name];
    this.validators.forEach(validator => {
      const error = validator(value);
      if (error && !fieldErrors.includes(error)) {
        fieldErrors.push(error);
      }
    });
    fieldErrors.push(...this.errors);
    return fieldErrors;
  }

  @computed
  get hasErrors() {
    return this.allErrors.length > 0;
  }

  @computed
  get hasVisibleErrors() {
    return this.isTouched && !this.hasFocus && this.hasErrors;
  }

  @computed
  get visibleErrors() {
    return this.hasVisibleErrors ? this.allErrors : null;
  }
}

export const validationMixin = (self: Object) => {
  const fields = observable.map<string, FieldState>(null, SHALLOW);

  const getOrAddFieldState = (name: string) => {
    let fieldState = fields.get(name);
    if (fieldState) {
      return fieldState;
    }
    fieldState = new FieldState(self, name);
    fields.set(name, fieldState);
    return fieldState;
  };

  const clearErrors = (name: string) => {
    const fieldState = fields.get(name);
    if (fieldState && fieldState.errors.length > 0) {
      fieldState.errors.clear();
    }
  };

  const fieldInterceptors: { [name: string]: Lambda } = {};

  const addFieldInterceptor = (name: string, value: any) => {
    if (isObservableArray(value)) {
      fieldInterceptors[name] = intercept(value, change => {
        if (change.type === "splice") {
          if (change.removedCount > 0 || change.added.length > 0) {
            clearErrors(name);
          }
        } else if (change.type === "update") {
          if (!sutructuralEquals(value[change.index], change.newValue.storedValue)) {
            clearErrors(name);
          }
        }
        return change;
      });
    } else if (isObservableMap(value)) {
      fieldInterceptors[name] = intercept(value, change => {
        if (change.type === "add") {
          clearErrors(name);
        } else if (change.type === "update") {
          if (!sutructuralEquals(value.get(change.name), change.newValue.storedValue)) {
            clearErrors(name);
          }
        } else if (change.type === "delete") {
          if (value.has(change.name)) {
            clearErrors(name);
          }
        }
        return change;
      });
    }
  };

  const removeFieldInterceptor = (name: string) => {
    const fieldInterceptor = fieldInterceptors[name];
    if (fieldInterceptor) {
      fieldInterceptor();
      delete fieldInterceptors[name];
    }
  };

  intercept(self, change => {
    if (change.type === "remove" || !sutructuralEquals(self[change.name], change.newValue)) {
      clearErrors(change.name);
    }
    return change;
  });

  // otherwise, model can not be created from snapshot
  Promise.resolve().then(() => {
    Object.keys(self).forEach(name => {
      addFieldInterceptor(name, self[name]);
    });
  });

  observe(self, change => {
    removeFieldInterceptor(change.name);
    if (change.type !== "remove") {
      addFieldInterceptor(change.name, self[change.name]);
    }
  });

  return {
    actions: {
      addErrors(name: string, ...errors: string[]) {
        const fieldErrors = getOrAddFieldState(name).errors;
        errors.forEach(error => {
          if (!fieldErrors.includes(error)) {
            fieldErrors.push(error);
          }
        });
      },
      clearErrors(name?: string) {
        if (name) {
          clearErrors(name);
        } else {
          fields.forEach(fieldState => {
            if (fieldState.errors.length > 0) {
              fieldState.errors.clear();
            }
          });
        }
      },
      addValidators(name: string, ...validators: Validator[]) {
        getOrAddFieldState(name).validators.push(...validators);
      },
      removeValidators(name: string, ...validators: Validator[]) {
        const fieldState = fields.get(name);
        if (fieldState) {
          validators.forEach(validator => {
            fieldState.validators.remove(validator);
          });
        }
      },
      setTouched(name: string, isTouched: boolean) {
        getOrAddFieldState(name).isTouched = isTouched;
      },
      setFocus(name: string, hasFocus: boolean) {
        getOrAddFieldState(name).hasFocus = hasFocus;
      }
    },
    views: {
      isTouched(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return fieldState ? fieldState.isTouched : false;
        }
        for (const fieldState of fields.values()) {
          if (fieldState.isTouched) {
            return true;
          }
        }
        return false;
      },
      hasFocus(name: string) {
        const fieldState = fields.get(name);
        return fieldState ? fieldState.hasFocus : false;
      },
      hasErrors(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return fieldState ? fieldState.hasErrors : false;
        }
        for (const fieldState of fields.values()) {
          if (fieldState.hasErrors) {
            return true;
          }
        }
        return false;
      },
      hasVisibleErrors(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return fieldState ? fieldState.hasVisibleErrors : false;
        }
        for (const fieldState of fields.values()) {
          if (fieldState.hasVisibleErrors) {
            return true;
          }
        }
        return false;
      },
      getErrors(name: string) {
        const fieldState = fields.get(name);
        return fieldState ? fieldState.allErrors : null;
      },
      getVisibleErrors(name: string) {
        const fieldState = fields.get(name);
        return fieldState ? fieldState.visibleErrors : null;
      },
      getAllErrors() {
        const modelErrors = {};
        let hasErrors = false;
        for (const [fieldName, fieldState] of fields.entries()) {
          if (fieldState.hasErrors) {
            modelErrors[fieldName] = fieldState.allErrors;
            hasErrors = true;
          }
        }
        return hasErrors ? modelErrors : null;
      },
      getAllVisibleErrors() {
        const modelErrors = {};
        let hasErrors = false;
        for (const [fieldName, fieldState] of fields.entries()) {
          if (fieldState.hasVisibleErrors) {
            modelErrors[fieldName] = fieldState.allErrors;
            hasErrors = true;
          }
        }
        return hasErrors ? modelErrors : null;
      }
    }
  };
};

function sutructuralEquals(first: any, second: any): boolean {
  if (first === second) {
    return true;
  }
  const firstSnapshot = isStateTreeNode(first) ? getSnapshot(first) : first;
  const secondSnapshot = isStateTreeNode(second) ? getSnapshot(second) : second;
  return comparer.structural(firstSnapshot, secondSnapshot);
}

const actionDecorators = {};

Object.keys(validationMixin(observable({})).actions).forEach(key => {
  actionDecorators[key] = action;
});

export function validatable<T extends Object>(target: T): T & ValidatableObject {
  const { actions, views } = validationMixin(target);
  for (const name in views) {
    target[name] = views[name];
  }
  extendObservable(target, actions, actionDecorators, SHALLOW);
  return target as any;
}

type Constructor<T = any> = { new (...args: any[]): T };

export function Validatable<T extends Constructor = Constructor<Object>>(
  constructor?: T
): T & Constructor<ValidatableObject> {
  if (!constructor) {
    constructor = class {} as any;
  }
  return class extends constructor {
    constructor(...args: any[]) {
      super(...args);
      validatable(this);
    }
  };
}

interface ValidateProps {
  model: ValidatableObject & { [x: string]: any };
  name: string;
  className?: string;
  errorClassName?: string;
  silent?: boolean;
  rules?: Validator | Validator[];
  renderErrors?: (...errors: string[]) => ReactNode;
}

export class Validate extends Component<ValidateProps> {
  private _validators: Validator[];

  private handleFocus = () => {
    const { model, name } = this.props;
    transaction(() => {
      model.setFocus(name, true);
      model.setTouched(name, true);
    });
  };

  private handleChange = () => {
    const { model, name } = this.props;
    model.setTouched(name, true);
  };

  private handleBlur = () => {
    const { model, name } = this.props;
    model.setFocus(name, false);
  };

  componentDidMount() {
    const { model, name, rules } = this.props;
    if (rules) {
      const validators = (isArray(rules) ? rules : [rules]).filter(Boolean);
      if (validators.length > 0) {
        this._validators = validators;
        model.addValidators(name, ...this._validators);
      }
    }
  }

  componentWillUnmount() {
    if (this._validators) {
      const { model, name } = this.props;
      model.removeValidators(name, ...this._validators);
    }
  }

  render() {
    const { silent, children } = this.props;
    if (silent) {
      return children ? this.renderWrapper(children) : null;
    }
    if (children) {
      return this.renderWrapper(children, this.renderErrors());
    }
    return this.renderErrors();
  }

  renderWrapper(children: ReactNode, errors?: ReactNode) {
    const { className } = this.props;
    return (
      <div
        className={className}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
      >
        {children}
        {errors}
      </div>
    );
  }

  renderErrors() {
    const { model, name, errorClassName, renderErrors } = this.props;
    if (model.hasVisibleErrors(name)) {
      const errors = model.getVisibleErrors(name);
      return renderErrors ? (
        renderErrors(...errors)
      ) : (
        <div className={errorClassName}>{errors.map((error, i) => <div key={i}>{error}</div>)}</div>
      );
    }
    return null;
  }
}
