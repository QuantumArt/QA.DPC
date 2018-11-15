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

/** React Context for user's locale */
export const LocaleContext = createContext<string>(null);

/**
 * Translation function
 * @example
 * <div>
 *   {tr("nameKey")}: <input value={name} />
 *   {tr("greetingKey", name) || <span>Hello, {name}!</span>}
 * </div>
 *
 * <div>
 *   {tr`Name`}: <input value={name} />
 *   {tr`Hello, ${name}!`}
 * </div>
 */
export interface Translate {
  /** Find translation by key */
  (key: string, ...values: any[]): any;
  /** ES6 template tag */
  (strings: TemplateStringsArray, ...values: any[]): any;
}

interface Resources {
  [key: string]: string | Function;
}

type LoadResources = (
  locale: string
) => Resources | Promise<Resources> | Promise<{ default: Resources }>;

/** Default translate function with empty resources */
const emptyTranslate = makeTranslate(null);

/**
 * React 16.7 Hook for loading translate function
 * @example
 * const tr = useTranslate(lang => import(`./MyComponent.${lang}.jsx`));
 * return <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 */
export function useTranslate(load: LoadResources) {
  const forceUpdate = useForceUpdate();
  const locale = useContext(LocaleContext);
  const ref = useRef<{ locale: string; translate: Translate }>(null);
  const self = ref.current || (ref.current = { locale: null, translate: emptyTranslate });

  if (self.locale !== locale) {
    self.locale = locale;
    const resourcesOrPromise = load(locale);
    if (resourcesOrPromise instanceof Promise) {
      resourcesOrPromise.catch(() => null).then(resources => {
        if (self.locale === locale) {
          if (resources && isObject(resources.default)) {
            resources = resources.default;
          }
          self.translate = makeTranslate(resources);
          forceUpdate();
        }
      });
    } else {
      self.translate = makeTranslate(resourcesOrPromise);
    }
  }
  return self.translate;
}

function useForceUpdate() {
  const [tick, setTick] = useState(0);
  return () => setTick((tick + 1) & 0x7fffffff);
}

function makeTranslate(resources: Resources): Translate {
  resources = prepareKeys(resources);

  return (keyOrStrings: TemplateStringsArray | string, ...values: any[]) => {
    const templateKey = isString(keyOrStrings)
      ? keyOrStrings
      : keyOrStrings.join("{*}").replace(/\s+/g, " ");

    const template = resources && resources[templateKey];
    if (isString(template)) {
      return template;
    }
    if (isFunction(template)) {
      return template(...values);
    }
    if (isString(keyOrStrings)) {
      return null;
    }
    return defaultTemplate(keyOrStrings, ...values);
  };
}

// React 16 already requires ES6 Set but not WeakSet
const preparedResources =
  typeof WeakSet === "function" ? new WeakSet<Resources>() : new Set<Resources>();

function prepareKeys(resources: Resources): Resources {
  if (!resources || preparedResources.has(resources)) {
    return resources;
  }
  preparedResources.add(resources);

  Object.keys(resources).forEach(resourceKey => {
    const templateKey = resourceKey
      .replace(/\$\{\s*[A-Za-z0-9_]+\s*\}/g, "{*}")
      .replace(/\s+/g, " ");

    if (templateKey !== resourceKey) {
      resources[templateKey] = resources[resourceKey];
      delete resources[resourceKey];
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

function isString(arg): arg is string {
  return typeof arg === "string";
}

function isFunction(arg): arg is Function {
  return typeof arg === "function";
}

function isObject(arg: any): arg is Object {
  return arg && typeof arg === "object" && !Array.isArray(arg);
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

/**
 * Component for translating it's descendants
 * @example
 * <Localize load={lang => import(`./MyResource.${lang}.jsx`)}>
 *   {tr => <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>}
 * </Localize>
 */
export class Localize extends LocalizeComponent<LocalizeProps> {
  static displayName = "Localize";

  render() {
    const { load, children } = this.props;
    this.loadResources(this.context, load);
    return children(this._translate);
  }
}

/**
 * HOC for inject `translate` prop
 * @example
 * const MyComponent = ({ name, translate: tr }) => (
 *   <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 * )
 *
 * @localize(lang => import(`./MyComponent.${lang}.jsx`))
 * class MyComponent extends Component {
 *   render() {
 *     const { name, translate: tr } = this.props;
 *     return <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 *   }
 * }
 */
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
  // @ts-ignore
  return hoistNonReactStatics(Localize, Wrapped) as T;
};
