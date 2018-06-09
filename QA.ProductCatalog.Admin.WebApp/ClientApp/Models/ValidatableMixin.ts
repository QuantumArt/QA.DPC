import { observable, observe, computed } from "mobx";

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

  observe(self, change => {
    const fieldState = fields.get(change.name);
    if (fieldState && fieldState.errors.length > 0) {
      fieldState.errors.clear();
    }
  });

  const getOrAddFieldState = (name: string) => {
    let fieldState = fields.get(name);
    if (fieldState) {
      return fieldState;
    }
    fieldState = new FieldState();
    fields.set(name, fieldState);
    return fieldState;
  };

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
          getOrAddFieldState(name).errors.clear();
        } else {
          fields.forEach(fieldState => {
            fieldState.errors.clear();
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
