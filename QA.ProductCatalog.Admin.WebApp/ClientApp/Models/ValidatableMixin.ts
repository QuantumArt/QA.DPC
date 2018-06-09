import {
  observable,
  computed,
  intercept,
  isObservableArray,
  isObservableMap,
  Lambda,
  observe
} from "mobx";

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
}

class FieldState {
  @observable.ref isTouched = false;
  @observable.ref hasFocus = false;
  errors = observable.array<string>(null, { deep: false });

  @computed
  get hasErrors() {
    return this.errors.length > 0;
  }

  @computed
  get hasVisibleErrors() {
    return this.isTouched && !this.hasFocus && this.hasErrors;
  }

  @computed
  get visibleErrors() {
    return this.hasVisibleErrors ? this.errors : null;
  }
}

export const validatableMixin = (self: Object) => {
  const fields = observable.map<string, FieldState>(null, { deep: false });

  const getOrAddFieldState = (name: string) => {
    let fieldState = fields.get(name);
    if (fieldState) {
      return fieldState;
    }
    fieldState = new FieldState();
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
    if (isObservableArray(value) || isObservableMap(value)) {
      console.log("addFieldInterceptor");
      fieldInterceptors[name] = intercept(value, change => {
        console.log("intercept field", name, change);
        clearErrors(name);
        return change;
      });
    }
  };

  const removeFieldInterceptor = (name: string) => {
    const fieldInterceptor = fieldInterceptors[name];
    if (fieldInterceptor) {
      console.log("removeFieldInterceptor");
      fieldInterceptor();
      delete fieldInterceptors[name];
    }
  };

  Object.entries(self).forEach(([name, value]) => {
    addFieldInterceptor(name, value);
  });

  intercept(self, change => {
    console.log("intercept model", change.name, change);
    clearErrors(change.name);
    return change;
  });

  observe(self, change => {
    console.log("observe model", change.name, change);
    removeFieldInterceptor(change.name);
    if (change.type !== "remove") {
      addFieldInterceptor(change.name, self[change.name]);
    }
  });

  return {
    actions: {
      addErrors(name: string, ...errors: string[]) {
        console.log("addErrors", name, errors);
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
        return fieldState ? fieldState.errors : null;
      },
      getVisibleErrors(name: string) {
        const fieldState = fields.get(name);
        return fieldState ? fieldState.visibleErrors : null;
      },
      getAllErrors() {
        const errors = {};
        let hasErrors = false;
        for (const [fieldName, fieldState] of fields.entries()) {
          if (fieldState.hasErrors) {
            errors[fieldName] = fieldState.errors;
            hasErrors = true;
          }
        }
        return hasErrors ? errors : null;
      },
      getAllVisibleErrors() {
        const errors = {};
        let hasErrors = false;
        for (const [fieldName, fieldState] of fields.entries()) {
          if (fieldState.hasVisibleErrors) {
            errors[fieldName] = fieldState.errors;
            hasErrors = true;
          }
        }
        return hasErrors ? errors : null;
      }
    }
  };
};
