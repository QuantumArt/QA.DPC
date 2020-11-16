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
  Lambda,
  IObservableArray
} from "mobx";
import {
  getSnapshot,
  isStateTreeNode,
  getChildType,
  resolveIdentifier,
  isReferenceType,
  clone,
  IAnyModelType,
  IStateTreeNode
} from "mobx-state-tree";
import { isArray } from "Utils/TypeChecks";

/**
 * Функция-валидатор.
 * @returns Строка ошибки или `undefined` при отсутствии ошибки.
 * @example
 * const required = value => value ? undefined : "Поле обязательно для заполнения";
 *
 * const pattern = (regExp: RegExp) => value =>
 *   regExp.test(value) ? undefined : "Поле не соответствует шаблону";
 */
export type Validator = (value: any) => string | undefined;

/**
 * Объект, имеющий состояние редактирования и валидации.
 * При первом изменении поля его значение сохраняется во внутренних структурах,
 * и доступно с помощью методов
 * @see getBaseValue
 * @see restoreBaseValue
 * @see restoreBaseValues
 * Ошибка, привязанная к полю, считается видимой, если поле ранее было выбрано
 * в фокус
 * @see isTouched
 * и изменено
 * @see isChanged
 */
export interface ValidatableObject {
  /** Поле объекта (или любое из полей объекта) ранее было выбрано в фокус и изменено @see isTouched @see isChanged */
  isEdited<K extends keyof this>(name?: K): boolean;
  /** Поле объекта (или любое из полей объекта) ранее было выбрано в фокус */
  isTouched<K extends keyof this>(name?: K): boolean;
  /**
   * Присвоить признак @see isTouched полю объекта
   * @param isTouched @default true
   */
  setTouched<K extends keyof this>(name: K, isTouched?: boolean): this;
  /** Сбросить признак @see isTouched для всех полей объекта */
  setUntouched(): this;
  /** Поле объекта (или любое из полей объекта) ранее было изменено */
  isChanged<K extends keyof this>(name?: K): boolean;
  /**
   * Присвоить признак @see isChanged полю объекта. При установке признака isChanged
   * в false, текущие значения полей становятся базовыми (изначальными).
   * @param isChanged @default true
   */
  setChanged<K extends keyof this>(name: K, isChanged?: boolean): this;
  /**
   * Сбросить признак @see isChanged для всех полей объекта. При установке признака isChanged
   * в false, текущие значения полей становятся базовыми (изначальными).
   */
  setUnchanged(): this;
  /** Поле объекта находится в фокусе */
  hasFocus<K extends keyof this>(name: K): boolean;
  /**
   * Присвоить признак @see hasFocus полю объекта
   * @param hasFocus @default true
   */
  setFocus<K extends keyof this>(name: K, hasFocus?: boolean): this;
  /**
   * Получить изначальное значение поля объекта (до редактирования)
   * или текущее, если поле не было изменено
   */
  getBaseValue<K extends keyof this>(name: K): Readonly<this[K]>;
  /** Восстановить изначальное значение поля объеткта */
  restoreBaseValue<K extends keyof this>(name: K): this;
  /** Восстановить изначальные значения всех полей объеткта */
  restoreBaseValues(): this;
  /** Имеет ли ошибки указанное поле объекта (или хотя бы одно из полей) */
  hasErrors<K extends keyof this>(name?: K): boolean;
  /** Имеет ли видимые ошибки указанное поле объекта (или хотя бы одно из полей) */
  hasVisibleErrors<K extends keyof this>(name?: K): boolean;
  /** Получить ошибки по полю, если они есть */
  getErrors<K extends keyof this>(name: K): string[] | null;
  /** Получить видимые ошибки по полю, если они есть */
  getVisibleErrors<K extends keyof this>(name: K): string[] | null;
  /** Получить ошибки по все полям, если они есть */
  getAllErrors(): { [field: string]: string[] } | null;
  /** Получить видимые ошибки по все полям, если они есть */
  getAllVisibleErrors(): { [field: string]: string[] } | null;
  /** Добавить ошибки к полю. Эти ошибки будут стерты при любом изменении поля. */
  addErrors<K extends keyof this>(name: K, ...errors: string[]): this;
  /** Очистить ошибки привязанные к полю (или по всем полям) */
  clearErrors<K extends keyof this>(name?: K): this;
  /**
   * Добавить функции валидации к полю. Они выполняются реактивно,
   * при любом изменении зартагиваемых полей.
   */
  addValidators<K extends keyof this>(name: K, ...validators: Validator[]): this;
  /** Удалить ранее добавленные валидаторы к полю */
  removeValidators<K extends keyof this>(name: K, ...validators: Validator[]): this;
}

const SHALLOW = { deep: false };

class FieldState {
  @observable.ref isTouched = false;
  @observable.ref isChanged = false;
  @observable.ref hasFocus = false;
  @observable.ref baseValue = undefined;
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

  get isEdited() {
    return this.isTouched && this.isChanged;
  }

  get hasErrors() {
    return this.allErrors.length > 0;
  }

  get hasVisibleErrors() {
    return this.isTouched && !this.hasFocus && this.hasErrors;
  }

  get visibleErrors() {
    return this.hasVisibleErrors ? this.allErrors : null;
  }
}

/**
 * Mixin для MobX State Tree, реализующий интерфейс @see ValidatableObject
 * @example
 * types.model("Product", {
 *   Title: types.string,
 *   Order: types.number,
 * }).extend(validationMixin);
 */
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

  const handleFieldChange = (
    name: string,
    oldValue: any,
    isCollectionChange = isObservableArray(oldValue) || isObservableMap(oldValue)
  ) => {
    const fieldState = getOrAddFieldState(name);
    if (!fieldState.isChanged) {
      fieldState.isChanged = true;

      if (isCollectionChange) {
        if (isStateTreeNode(oldValue)) {
          const elementType = getChildType(oldValue) as IAnyModelType;
          if (isReferenceType(elementType)) {
            if (isObservableArray(oldValue)) {
              fieldState.baseValue = getSnapshot(oldValue as IStateTreeNode).map(id =>
                resolveIdentifier(elementType, self, id)
              );
            } else if (isObservableMap(oldValue)) {
              const mapSnapshot = getSnapshot(oldValue as IStateTreeNode);
              fieldState.baseValue = new Map(
                Object.keys(mapSnapshot).map(
                  key =>
                    [key, resolveIdentifier(elementType, self, mapSnapshot[key])] as [string, any]
                )
              );
            }
          } else {
            // this cloned node is detached and can be used only for restore base value
            fieldState.baseValue = clone(oldValue);
          }
        } else if (isObservableArray(oldValue)) {
          fieldState.baseValue = observable.array(oldValue.peek());
        } else if (isObservableMap(oldValue)) {
          fieldState.baseValue = observable.map(oldValue.entries());
        }
      } else {
        fieldState.baseValue = oldValue;
      }
    }
    if (fieldState.errors.length > 0) {
      fieldState.errors.clear();
    }
  };

  const fieldInterceptors: { [name: string]: Lambda } = Object.create(null);

  const addFieldInterceptor = (name: string, value: any) => {
    if (isObservableArray(value)) {
      fieldInterceptors[name] = intercept(value, change => {
        if (change.type === "splice") {
          if (change.removedCount > 0 || change.added.length > 0) {
            handleFieldChange(name, value, true);
          }
        } else if (change.type === "update") {
          if (!sutructuralEquals(value[change.index], change.newValue.storedValue)) {
            handleFieldChange(name, value, true);
          }
        }
        return change;
      });
    } else if (isObservableMap(value)) {
      fieldInterceptors[name] = intercept(value, change => {
        if (change.type === "add") {
          handleFieldChange(name, value, true);
        } else if (change.type === "update") {
          if (!sutructuralEquals(value.get(change.name), change.newValue.storedValue)) {
            handleFieldChange(name, value, true);
          }
        } else if (change.type === "delete") {
          if (value.has(change.name)) {
            handleFieldChange(name, value, true);
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
    const oldValue = self[change.name];
    if (change.type === "remove" || !sutructuralEquals(oldValue, change.newValue)) {
      handleFieldChange(change.name, oldValue);
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
        return self as ValidatableObject;
      },
      clearErrors(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          if (fieldState && fieldState.errors.length > 0) {
            fieldState.errors.clear();
          }
        } else {
          fields.forEach(fieldState => {
            if (fieldState.errors.length > 0) {
              fieldState.errors.clear();
            }
          });
        }
        return self as ValidatableObject;
      },
      addValidators(name: string, ...validators: Validator[]) {
        getOrAddFieldState(name).validators.push(...validators);
        return self as ValidatableObject;
      },
      removeValidators(name: string, ...validators: Validator[]) {
        const fieldState = fields.get(name);
        if (fieldState) {
          validators.forEach(validator => {
            fieldState.validators.remove(validator);
          });
        }
        return self as ValidatableObject;
      },
      setTouched(name: string, isTouched = true) {
        if (isTouched) {
          getOrAddFieldState(name).isTouched = true;
        } else {
          const fieldState = fields.get(name);
          if (fieldState) {
            fieldState.isTouched = false;
          }
        }
        return self as ValidatableObject;
      },
      setUntouched() {
        fields.forEach(fieldState => {
          fieldState.isTouched = false;
        });
        return self as ValidatableObject;
      },
      setChanged(name: string, isChanged = true) {
        if (isChanged) {
          const oldValue = self[name];
          handleFieldChange(name, oldValue);
        } else {
          const fieldState = fields.get(name);
          if (fieldState) {
            fieldState.isChanged = false;
            fieldState.baseValue = undefined;
          }
        }
        return self as ValidatableObject;
      },
      setUnchanged() {
        fields.forEach(fieldState => {
          fieldState.isChanged = false;
          fieldState.baseValue = undefined;
        });
        return self as ValidatableObject;
      },
      restoreBaseValue(name: string) {
        const fieldState = fields.get(name);
        if (fieldState && fieldState.isChanged) {
          self[name] = fieldState.baseValue;
          fieldState.isChanged = false;
          fieldState.baseValue = undefined;
        }
        return self as ValidatableObject;
      },
      restoreBaseValues() {
        fields.forEach((fieldState, name) => {
          if (fieldState.isChanged) {
            self[name] = fieldState.baseValue;
            fieldState.isChanged = false;
            fieldState.baseValue = undefined;
          }
        });
        return self as ValidatableObject;
      },
      setFocus(name: string, hasFocus = true) {
        if (hasFocus) {
          getOrAddFieldState(name).hasFocus = true;
        } else {
          const fieldState = fields.get(name);
          if (fieldState) {
            fieldState.hasFocus = false;
          }
        }
        return self as ValidatableObject;
      }
    },
    views: {
      isEdited(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return fieldState ? fieldState.isEdited : false;
        }
        for (const fieldState of fields.values()) {
          if (fieldState.isEdited) {
            return true;
          }
        }
        return false;
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
      isChanged(name?: string) {
        if (name) {
          const fieldState = fields.get(name);
          return fieldState ? fieldState.isChanged : false;
        }
        for (const fieldState of fields.values()) {
          if (fieldState.isChanged) {
            return true;
          }
        }
        return false;
      },
      getBaseValue(name: string) {
        const fieldState = fields.get(name);
        return fieldState && fieldState.isChanged ? fieldState.baseValue : self[name];
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

/**
 * Утилита для MobX, добавляющая интерфейс @see ValidatableObject
 * @example
 * const product = validatable(observable({
 *   Title: "",
 *   Order: 0,
 * }))
 */
export function validatable<T extends Object>(target: T): T & ValidatableObject {
  const { actions, views } = validationMixin(target);
  for (const name in views) {
    target[name] = views[name];
  }
  extendObservable(target, actions, actionDecorators, SHALLOW);
  return target as any;
}

type Constructor<T = any> = { new (...args: any[]): T };

/**
 * Mixin для MobX, реализующий интерфейс @see ValidatableObject
 * @example
 * class Entity {
 *   Id: number;
 * }
 *
 * class Product extends Validatable(Entity) {
 *   @observable Title: string;
 *   @observable Order: number;
 * }
 */
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

/** Props для компонента Validate */
interface ValidateProps {
  /** Объект для валидации */
  model: ValidatableObject & { [x: string]: any };
  /** Имя поля для валидаци */
  name: string;
  /** Класс для элемента-обертки включающего поле ввода + сообщения об ошибках */
  className?: string;
  /** Класс для отображения ошибок */
  errorClassName?: string;
  /** Не отображать сообщения об ошибках */
  silent?: boolean;
  /** Правила валидации значения поля */
  rules?: Validator | Validator[];
  /** Render Callback для отображения ошибок */
  renderErrors?: (...errors: string[]) => ReactNode;
}

/**
 * React компонент для декларативного описания валидации поля
 * @example
 * <Validate silent model={article} name="Title" rules={[required, pattern(/^[A-Z]+$/i)]} />
 *
 * <Validate name="Title" model={article} rules={required}>
 *   <input value={article.Title} onChange={e => (article.Title = e.target.value)} />
 * </Validate>
 */
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
        <div className={errorClassName}>
          {errors.map((error, i) => (
            <div key={i}>{error}</div>
          ))}
        </div>
      );
    }
    return null;
  }
}
