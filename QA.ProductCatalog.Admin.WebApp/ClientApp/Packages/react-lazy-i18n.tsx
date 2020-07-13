import {
  Component,
  ReactNode,
  ComponentType,
  createElement,
  createContext,
  useState,
  useContext,
  useRef
} from "react";
import hoistNonReactStatics from "hoist-non-react-statics";

/** React Context for user's locale */
export const LocaleContext = createContext<string | null>(null);

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
  /** Current Locale */
  locale: string | null;
  /** Translation Provider */
  Provider: (props: { children: ReactNode }) => ReactNode;
}

interface Resources {
  [key: string]: string | Function;
}

type LoadResources = (
  locale: string | null
) => Resources | Promise<Resources> | Promise<{ default: Resources }> | null;

interface Translatable {
  _locale: string | null;
  _translate: Translate;
}

/** Default translate function with empty resources */
const emptyTranslate = makeTranslate(null, null);

const TranslateContext = createContext(emptyTranslate);

/**
 * React 16.7 Hook for loading translate function
 * @example
 * const tr = useTranslation(lang => import(`./MyComponent.${lang}.jsx`));
 * return <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 */
export function useTranslation(load?: LoadResources) {
  if (!load) {
    return useContext(TranslateContext);
  }

  const forceUpdate = useForceUpdate();
  const locale = useContext(LocaleContext);
  const ref = useRef<Translatable>(null);
  // @ts-ignore: RefObject<T>.current isn't actually readonly
  const self = ref.current || (ref.current = { _locale: null, _translate: emptyTranslate });

  loadResources(self, locale, load, forceUpdate);

  return self._translate;
}

function useForceUpdate() {
  const [tick, setTick] = useState(0);
  return () => setTick((tick + 1) & 0x7fffffff);
}

interface TranslationProps {
  load?: LoadResources;
  children: (tr: Translate) => ReactNode;
}

/**
 * Component for translating it's descendants
 * @example
 * <Translation load={lang => import(`./MyResource.${lang}.jsx`)}>
 *   {tr => <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>}
 * </Translation>
 */
export class Translation extends Component<TranslationProps> {
  static contextType = LocaleContext;
  static displayName = "Translation";

  _locale: string | null = null;
  _translate = emptyTranslate;
  _forceUpdate = this.forceUpdate.bind(this);

  render() {
    const { load, children } = this.props;
    if (!load) {
      return createElement(TranslateContext.Consumer, null, tr => children(tr));
    }
    loadResources(this, this.context, load, this._forceUpdate);
    return createElement(
      TranslateContext.Provider,
      { value: this._translate },
      children(this._translate)
    );
  }
}

/**
 * HOC that injects `tr` prop
 * @example
 * const MyComponent = ({ name, tr }) => (
 *   <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 * )
 *
 * @withTranslation(lang => import(`./MyComponent.${lang}.jsx`))
 * class MyComponent extends Component {
 *   render() {
 *     const { name, tr } = this.props;
 *     return <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 *   }
 * }
 */
export function withTranslation(load?: LoadResources) {
  if (!load) {
    return function<T extends ComponentType<any>>(Wrapped: T): WithTranslation<T> {
      class Translation extends Component {
        static contextType = TranslateContext;
        static displayName = `withTranslation(${Wrapped.displayName || Wrapped.name})`;
        static WrappedComponent = Wrapped;

        render() {
          return createElement(Wrapped, { tr: this.context, ...this.props });
        }
      }
      return hoistNonReactStatics(Translation, Wrapped) as any;
    };
  }
  return function<T extends ComponentType<any>>(Wrapped: T): WithTranslation<T> {
    class Translation extends Component {
      static contextType = LocaleContext;
      static displayName = `withTranslation(${Wrapped.displayName || Wrapped.name})`;
      static WrappedComponent = Wrapped;

      _locale: string | null = null;
      _translate = emptyTranslate;
      _forceUpdate = this.forceUpdate.bind(this);

      render() {
        loadResources(this, this.context, load!, this._forceUpdate);
        return createElement(
          TranslateContext.Provider,
          { value: this._translate },
          createElement(Wrapped, { tr: this._translate, ...this.props })
        );
      }
    }
    return hoistNonReactStatics(Translation, Wrapped) as any;
  };
}

type WithTranslation<T> = T & {
  contextType: typeof LocaleContext;
  WrappedComponent: T;
};

function loadResources(
  self: Translatable,
  locale: string | null,
  loadResources: LoadResources,
  forceUpdate: () => void
) {
  if (self._locale !== locale) {
    self._locale = locale;
    const resourcesOrPromise = loadResources(locale);
    if (resourcesOrPromise instanceof Promise) {
      // @ts-ignore
      resourcesOrPromise.catch(() => null).then(resources => {
        if (self._locale === locale) {
          if (resources && isObject(resources.default)) {
            resources = resources.default as any;
          }
          self._translate = makeTranslate(locale, resources);
          forceUpdate();
        }
      });
    } else {
      self._translate = makeTranslate(locale, resourcesOrPromise);
    }
  }
}

function makeTranslate(locale: string | null, resources: Resources | null): Translate {
  resources = prepareKeys(resources);

  function tr(keyOrStrings: TemplateStringsArray | string, ...values: any[]) {
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
    return defaultTemplate(keyOrStrings, values);
  }

  tr.locale = locale;

  tr.Provider = ({ children }: { children: ReactNode }) =>
    createElement(TranslateContext.Provider, { value: tr }, children);

  return tr;
}

// React 16 already requires ES6 Map but not WeakMap
const resourcesCache =
  typeof WeakMap === "function"
    ? new WeakMap<Resources, Resources>()
    : new Map<Resources, Resources>();

function prepareKeys(resources: Resources | null): Resources | null {
  if (!resources) {
    return resources;
  }
  const cached = resourcesCache.get(resources);
  if (cached) {
    return cached;
  }
  const prepared: Resources = {};

  for (const resourceKey in resources) {
    const templateKey = resourceKey
      .replace(/\$\{\s*[A-Za-z0-9_]+\s*\}/g, "{*}")
      .replace(/\s+/g, " ");

    prepared[templateKey] = resources[resourceKey];
  }

  resourcesCache.set(resources, prepared);
  return prepared;
}

function defaultTemplate(strings: TemplateStringsArray, values: any[]) {
  const length = strings.length;
  switch (length) {
    case 1:
      return strings[0];
    case 2:
      return strings[0] + values[0] + strings[1];
  }
  const array = [];
  for (let i = 0; i < length; i++) {
    array.push(strings[i], values[i]);
  }
  array.pop();
  return array.join("");
}

function isString(arg: any): arg is string {
  return typeof arg === "string";
}

function isFunction(arg: any): arg is Function {
  return typeof arg === "function";
}

function isObject(arg: any): arg is Object {
  return arg && typeof arg === "object" && !Array.isArray(arg);
}

export { useTranslation as useTran, withTranslation as withTran, Translation as Tran };
