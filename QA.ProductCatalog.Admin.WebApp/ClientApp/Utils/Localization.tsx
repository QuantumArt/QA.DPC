import React, {
  Component,
  StatelessComponent,
  cloneElement,
  createContext,
  ReactNode
} from "react";
import hoistNonReactStatics from "hoist-non-react-statics";
import { isObject, isString, isFunction } from "Utils/TypeChecks";

type Tempalte = (...args: any[]) => string;

interface Resources {
  [id: string]: string | Tempalte | StatelessComponent;
}

export interface TranslateFunction {
  (id: string, fallback: FallbackArgs): string;
  (id: string, ...values: any[]): string;
  (strings: TemplateStringsArray, ...values: any[]): string;
}

export function makeTranslate(resources: Resources, shouldPrepareKeys = true): TranslateFunction {
  if (shouldPrepareKeys) {
    resources = prepareKeys(resources);
  }

  return function translate(
    idOrStrings: TemplateStringsArray | string,
    ...fallbackOrValues: any[]
  ) {
    const templateId = isString(idOrStrings)
      ? idOrStrings
      : idOrStrings.join("{*}").replace(/\s+/g, " ");

    const template = resources && (resources[templateId] as string | Tempalte);
    if (isString(template)) {
      return template;
    }
    const fallbackArgs = fallbackOrValues[0];
    if (fallbackArgs instanceof FallbackArgs) {
      const { strings, values } = fallbackArgs;
      if (isFunction(template)) {
        return template(...values);
      }
      return defaultTemplate(strings, ...values);
    }
    if (isFunction(template)) {
      return template(...fallbackOrValues);
    }
    if (isString(idOrStrings)) {
      return idOrStrings;
    }
    return defaultTemplate(idOrStrings, ...fallbackOrValues);
  };
}

function prepareKeys(resources: Resources): Resources {
  if (!resources) {
    return resources;
  }

  const templates = {};

  Object.keys(resources).forEach(resourceId => {
    const templateId = resourceId.replace(/\$\{[A-Za-z0-9_]+\}/g, "{*}").replace(/\s+/g, " ");
    templates[templateId] = resources[resourceId];
  });

  return templates;
}

export function id(strings: TemplateStringsArray) {
  return strings[0];
}

export function fallback(strings: TemplateStringsArray, ...values: any[]) {
  return new FallbackArgs(strings, values);
}

class FallbackArgs {
  constructor(public strings: TemplateStringsArray, public values: any[]) {}
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

const ResourcesContext = createContext<Resources>(null);

export class Translate extends Component<{ id: string }> {
  private _params: { [name: string]: any };
  private _propsByKey: { [key: string]: Object };

  private clearData() {
    this._params = {};
    this._propsByKey = {};
  }

  private collectData = (node: any) => {
    if (Array.isArray(node)) {
      node.forEach(this.collectData);
    } else if (isObject(node)) {
      const { type, props, key } = node;
      if (type && props) {
        if (key != null) {
          const storedProps = { ...props };
          delete storedProps.children;
          this._propsByKey[key] = storedProps;
          this.collectData(props.children);
        }
      } else {
        Object.assign(this._params, node);
      }
    }
  };

  private visitNode = (node: any) => {
    if (Array.isArray(node)) {
      return node.map(this.visitNode);
    }
    if (isObject(node)) {
      const { type, props, key } = node;
      if (type && props) {
        const { children } = props;
        const newProps = (key != null && this._propsByKey[key]) || props;
        const newChildren = this.visitNode(children);
        if (props === newProps && children === newChildren) {
          return node;
        }
        if (typeof newChildren === "undefined") {
          return cloneElement(node, newProps);
        }
        if (Array.isArray(newChildren)) {
          return cloneElement(node, newProps, ...newChildren);
        }
        return cloneElement(node, newProps, newChildren);
      }
      for (const key in node) {
        return node[key];
      }
    }
    return node;
  };

  private renderChildren = (resources: Resources) => {
    const { id, children } = this.props;
    this.clearData();
    this.collectData(children);
    const resource = resources && (resources[id] as StatelessComponent);
    const nodes = resource ? resource(this._params) : children;
    return this.visitNode(nodes) || null;
  };

  render() {
    return <ResourcesContext.Consumer>{this.renderChildren}</ResourcesContext.Consumer>;
  }
}

export const LocaleContext = createContext<string>("");

interface LocalizeProps {
  load: (locale: string) => Resources | Promise<Resources> | Promise<{ default: Resources }>;
  defaultLocale?: string;
  defaultResources?: Resources;
  children: (translate: TranslateFunction) => ReactNode;
}

interface LocalizeState {
  resources: Resources;
  translate: TranslateFunction;
}

export class Localize extends Component<LocalizeProps, LocalizeState> {
  private _locale = this.props.defaultLocale;
  state = {
    resources: this.props.defaultResources,
    translate: makeTranslate(this.props.defaultResources, true)
  };

  private updateResources(resources: Resources) {
    resources = prepareKeys(resources);
    const translate = makeTranslate(resources, false);
    this.setState({ resources, translate });
  }

  private loadResources(locale: string) {
    let resources = this.props.load(locale);
    if (resources instanceof Promise) {
      resources.catch(() => null).then(resources => {
        if (this._locale === locale) {
          if (resources && isObject(resources.default)) {
            resources = resources.default;
          }
          this.updateResources(resources);
        }
      });
    } else {
      this.updateResources(resources);
    }
  }

  private renderChildren = (locale: string) => {
    const { children } = this.props;
    const { resources, translate } = this.state;
    if (this._locale !== locale) {
      this._locale = locale;
      this.loadResources(locale);
    }
    const nodes = children(translate);
    return <ResourcesContext.Provider value={resources}>{nodes}</ResourcesContext.Provider>;
  };

  render() {
    return <LocaleContext.Consumer>{this.renderChildren}</LocaleContext.Consumer>;
  }
}

interface LocalizeConfig {
  load: (locale: string) => Resources | Promise<Resources> | Promise<{ default: Resources }>;
  defaultLocale?: string;
  defaultResources?: Resources;
}

export function localize(
  load: LocalizeConfig["load"]
): <T extends typeof Component>(Wrapped: T) => T;
export function localize(config: LocalizeConfig): <T extends typeof Component>(Wrapped: T) => T;
export function localize(argument: any) {
  let load, defaultLocale, defaultResources;
  if (isFunction(argument)) {
    load = argument;
  } else {
    ({ load, defaultLocale, defaultResources } = argument);
  }
  return Wrapped => {
    class Localized extends Component {
      static displayName = `Localized(${getDisplayName(Wrapped)})`;

      render() {
        return (
          <Localize load={load} defaultLocale={defaultLocale} defaultResources={defaultResources}>
            {this.renderComponent}
          </Localize>
        );
      }
      // hoist Localized render callback
      renderComponent = translate => <Wrapped translate={translate} {...this.props} />;
    }
    // static fields from component should be visible on the generated Component
    hoistNonReactStatics(Localized, Wrapped);

    return Localized;
  };
}

function getDisplayName(Component) {
  return Component.displayName || Component.name || "Component";
}
