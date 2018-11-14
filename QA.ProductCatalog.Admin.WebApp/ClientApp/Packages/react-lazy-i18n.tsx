import React, {
  Component,
  ReactNode,
  ComponentType,
  createContext,
  useState,
  useContext,
  useRef
} from "react";
import hoistNonReactStatics from "hoist-non-react-statics";

interface Resources {
  [id: string]: string | Function;
}

export interface Translate {
  (id: string, ...values: any[]): any;
  (strings: TemplateStringsArray, ...values: any[]): any;
}

function isString(arg): arg is string {
  return typeof arg === "string";
}

function isFunction(arg): arg is Function {
  return typeof arg === "function";
}

function isObject(arg: any): arg is Object {
  return arg && typeof arg === "object" && !Array.isArray(arg);
}

function makeTranslate(resources: Resources): Translate {
  resources = prepareKeys(resources);

  return (idOrStrings: TemplateStringsArray | string, ...values: any[]) => {
    const templateId = isString(idOrStrings)
      ? idOrStrings
      : idOrStrings.join("{*}").replace(/\s+/g, " ");

    const template = resources && resources[templateId];
    if (isString(template)) {
      return template;
    }
    if (isFunction(template)) {
      return template(...values);
    }
    if (isString(idOrStrings)) {
      return null;
    }
    return defaultTemplate(idOrStrings, ...values);
  };
}

const preparedResources =
  typeof WeakSet === "function" ? new WeakSet<Resources>() : new Set<Resources>();

function prepareKeys(resources: Resources): Resources {
  if (!resources || preparedResources.has(resources)) {
    return resources;
  }
  preparedResources.add(resources);

  Object.keys(resources).forEach(resourceId => {
    const templateId = resourceId.replace(/\$\{\s*[A-Za-z0-9_]+\s*\}/g, "{*}").replace(/\s+/g, " ");
    if (templateId !== resourceId) {
      resources[templateId] = resources[resourceId];
      delete resources[resourceId];
    }
  });

  return resources;
}

function defaultTemplate(strings: TemplateStringsArray, ...values: any[]) {
  switch (strings.length) {
    case 1:
      return strings[0];
    case 2:
      return strings[0] + values[0] + strings[1];
  }
  const arr = [];
  strings.forEach((str, i) => {
    arr.push(str, values[i]);
  });
  arr.pop();
  return arr.join("");
}

export const LocaleContext = createContext<string>(null);

type LoadResources = (
  locale: string
) => Resources | Promise<Resources> | Promise<{ default: Resources }>;

const emptyTranslate = makeTranslate(null);

function useForceUpdate() {
  const [tick, setTick] = useState(0);
  return () => setTick((tick + 1) & 0x7fffffff);
}

export function useTranslate(load: LoadResources) {
  const forceUpdate = useForceUpdate();
  const locale = useContext(LocaleContext);
  const ref = useRef<{ _locale: string; _translate: Translate }>(null);
  const self = ref.current || (ref.current = { _locale: null, _translate: emptyTranslate });

  if (self._locale !== locale) {
    self._locale = locale;
    const resources = load(locale);
    if (resources instanceof Promise) {
      resources.catch(() => null).then(resources => {
        if (self._locale === locale) {
          if (resources && isObject(resources.default)) {
            resources = resources.default;
          }
          self._translate = makeTranslate(resources);
          forceUpdate();
        }
      });
    } else {
      self._translate = makeTranslate(resources);
    }
  }
  return self._translate;
}

abstract class LocalizeComponent<T = {}> extends Component<T> {
  static contextType = LocaleContext;
  protected _locale: string = null;
  protected _translate = emptyTranslate;

  protected loadResources(locale: string, load: LoadResources) {
    if (this._locale !== locale) {
      this._locale = locale;
      const resources = load(locale);
      if (resources instanceof Promise) {
        resources.catch(() => null).then(resources => {
          if (this._locale === locale) {
            if (resources && isObject(resources.default)) {
              resources = resources.default;
            }
            this._translate = makeTranslate(resources);
            this.forceUpdate();
          }
        });
      } else {
        this._translate = makeTranslate(resources);
      }
    }
  }
}

interface LocalizeProps {
  load: LoadResources;
  children: (translate: Translate) => ReactNode;
}

export class Localize extends LocalizeComponent<LocalizeProps> {
  static displayName = "Localize";

  render() {
    const { load, children } = this.props;
    this.loadResources(this.context, load);
    return children(this._translate);
  }
}

export const localize = (load: LoadResources) => <T extends ComponentType>(Wrapped: T) => {
  class Localize extends LocalizeComponent {
    static displayName = `Localize(${Wrapped.displayName || Wrapped.name})`;
    static WrappedComponent = Wrapped;

    render() {
      this.loadResources(this.context, load);
      // @ts-ignore
      return <Wrapped translate={this._translate} {...this.props} />;
    }
  }
  // static fields from component should be visible on the generated Component
  return hoistNonReactStatics(Localize, Wrapped) as T;
};
