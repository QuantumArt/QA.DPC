import {
  observable,
  computed,
  intercept,
  observe,
  comparer,
  isObservableArray,
  isObservableMap,
  Lambda
} from "mobx";
import { getSnapshot, isStateTreeNode } from "mobx-state-tree";

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

class FieldState {
  @observable.ref isTouched = false;
  @observable.ref hasFocus = false;
  validators = observable.array<Validator>(null, { deep: false });
  errors = observable.array<string>(null, { deep: false });

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

export const validatableMixin = (self: Object) => {
  const fields = observable.map<string, FieldState>(null, { deep: false });

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

  Object.entries(self).forEach(([name, value]) => {
    addFieldInterceptor(name, value);
  });

  intercept(self, change => {
    if (change.type === "remove" || !sutructuralEquals(self[change.name], change.newValue)) {
      clearErrors(change.name);
    }
    return change;
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
