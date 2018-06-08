import { observable } from "mobx";

export interface ValidatableObject {
  addErrors(name: string, ...errors: string[]): void;
  clearErrors(name?: string): void;
  hasErrors(name?: string): boolean;
  hasVisibleErrors(name?: string): boolean;
  getErrors(name: string): string[] | null;
  getVisibleErrors(name: string): string[] | null;
  getAllErrors(): { [field: string]: string[] } | null;
  getAllVisibleErrors(): { [field: string]: string[] } | null;
  setTouched(name: string, isTouched: boolean): void;
  isTouched(name?: string): boolean;
  setFocus(name: string, hasFocus: boolean): void;
  hasFocus(name: string): boolean;
}

class FieldState {
  @observable.ref public isTouched = false;
  @observable.ref public hasFocus = false;
  public errors = observable.array<string>(null, { deep: false });
}

export const validatableMixin = () => {
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
      hasErrors(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return !!(fieldState && fieldState.errors.length > 0);
        }
        for (const fieldState of fields.values()) {
          if (fieldState.errors.length > 0) {
            return true;
          }
        }
        return false;
      },
      hasVisibleErrors(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return !!(
            fieldState &&
            fieldState.isTouched &&
            !fieldState.hasFocus &&
            fieldState.errors.length > 0
          );
        }
        for (const fieldState of fields.values()) {
          if (fieldState.isTouched && !fieldState.hasFocus && fieldState.errors.length > 0) {
            return true;
          }
        }
        return false;
      },
      getErrors(name: string) {
        const fieldState = fields.get(name);
        return fieldState && fieldState.errors.length > 0 ? fieldState.errors : null;
      },
      getVisibleErrors(name: string) {
        const fieldState = fields.get(name);
        const hasVisibleErrors =
          fieldState &&
          fieldState.isTouched &&
          !fieldState.hasFocus &&
          fieldState.errors.length > 0;
        return hasVisibleErrors ? fieldState.errors : null;
      },
      getAllErrors() {
        const errors = {};
        let hasErrors = false;
        for (const [fieldName, fieldState] of fields.entries()) {
          if (fieldState.errors.length > 0) {
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
          if (fieldState.isTouched && !fieldState.hasFocus && fieldState.errors.length > 0) {
            errors[fieldName] = fieldState.errors;
            hasErrors = true;
          }
        }
        return hasErrors ? errors : null;
      },
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
      }
    }
  };
};
