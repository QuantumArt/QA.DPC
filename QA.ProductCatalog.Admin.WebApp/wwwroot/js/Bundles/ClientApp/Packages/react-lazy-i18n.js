import * as tslib_1 from "tslib";
import {
  Component,
  createElement,
  createContext,
  useState,
  useContext,
  useRef
} from "react";
import hoistNonReactStatics from "hoist-non-react-statics";
/** React Context for user's locale */
export var LocaleContext = createContext(null);
/** Default translate function with empty resources */
var emptyTranslate = makeTranslate(null, null);
var TranslateContext = createContext(emptyTranslate);
/**
 * React 16.7 Hook for loading translate function
 * @example
 * const tr = useTranslation(lang => import(`./MyComponent.${lang}.jsx`));
 * return <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>
 */
export function useTranslation(load) {
  if (!load) {
    return useContext(TranslateContext);
  }
  var forceUpdate = useForceUpdate();
  var locale = useContext(LocaleContext);
  var ref = useRef(null);
  // @ts-ignore: RefObject<T>.current isn't actually readonly
  var self =
    ref.current ||
    (ref.current = { _locale: null, _translate: emptyTranslate });
  loadResources(self, locale, load, forceUpdate);
  return self._translate;
}
function useForceUpdate() {
  var _a = tslib_1.__read(useState(0), 2),
    tick = _a[0],
    setTick = _a[1];
  return function() {
    return setTick((tick + 1) & 0x7fffffff);
  };
}
/**
 * Component for translating it's descendants
 * @example
 * <Translation load={lang => import(`./MyResource.${lang}.jsx`)}>
 *   {tr => <span title={tr`Greeting`}>{tr`Hello, ${name}!`}</span>}
 * </Translation>
 */
var Translation = /** @class */ (function(_super) {
  tslib_1.__extends(Translation, _super);
  function Translation() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this._locale = null;
    _this._translate = emptyTranslate;
    _this._forceUpdate = _this.forceUpdate.bind(_this);
    return _this;
  }
  Translation.prototype.render = function() {
    var _a = this.props,
      load = _a.load,
      children = _a.children;
    if (!load) {
      return createElement(TranslateContext.Consumer, null, function(tr) {
        return children(tr);
      });
    }
    loadResources(this, this.context, load, this._forceUpdate);
    return createElement(
      TranslateContext.Provider,
      { value: this._translate },
      children(this._translate)
    );
  };
  Translation.contextType = LocaleContext;
  Translation.displayName = "Translation";
  return Translation;
})(Component);
export { Translation };
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
export function withTranslation(load) {
  if (!load) {
    return function(Wrapped) {
      var Translation = /** @class */ (function(_super) {
        tslib_1.__extends(Translation, _super);
        function Translation() {
          return (_super !== null && _super.apply(this, arguments)) || this;
        }
        Translation.prototype.render = function() {
          return createElement(
            Wrapped,
            tslib_1.__assign({ tr: this.context }, this.props)
          );
        };
        Translation.contextType = TranslateContext;
        Translation.displayName =
          "withTranslation(" + (Wrapped.displayName || Wrapped.name) + ")";
        Translation.WrappedComponent = Wrapped;
        return Translation;
      })(Component);
      return hoistNonReactStatics(Translation, Wrapped);
    };
  }
  return function(Wrapped) {
    var Translation = /** @class */ (function(_super) {
      tslib_1.__extends(Translation, _super);
      function Translation() {
        var _this = (_super !== null && _super.apply(this, arguments)) || this;
        _this._locale = null;
        _this._translate = emptyTranslate;
        _this._forceUpdate = _this.forceUpdate.bind(_this);
        return _this;
      }
      Translation.prototype.render = function() {
        loadResources(this, this.context, load, this._forceUpdate);
        return createElement(
          TranslateContext.Provider,
          { value: this._translate },
          createElement(
            Wrapped,
            tslib_1.__assign({ tr: this._translate }, this.props)
          )
        );
      };
      Translation.contextType = LocaleContext;
      Translation.displayName =
        "withTranslation(" + (Wrapped.displayName || Wrapped.name) + ")";
      Translation.WrappedComponent = Wrapped;
      return Translation;
    })(Component);
    return hoistNonReactStatics(Translation, Wrapped);
  };
}
function loadResources(self, locale, loadResources, forceUpdate) {
  if (self._locale !== locale) {
    self._locale = locale;
    var resourcesOrPromise = loadResources(locale);
    if (resourcesOrPromise instanceof Promise) {
      resourcesOrPromise
        .catch(function() {
          return null;
        })
        .then(function(resources) {
          if (self._locale === locale) {
            if (resources && isObject(resources.default)) {
              resources = resources.default;
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
function makeTranslate(locale, resources) {
  resources = prepareKeys(resources);
  function tr(keyOrStrings) {
    var values = [];
    for (var _i = 1; _i < arguments.length; _i++) {
      values[_i - 1] = arguments[_i];
    }
    var templateKey = isString(keyOrStrings)
      ? keyOrStrings
      : keyOrStrings.join("{*}").replace(/\s+/g, " ");
    var template = resources && resources[templateKey];
    if (isString(template)) {
      return template;
    }
    if (isFunction(template)) {
      return template.apply(void 0, tslib_1.__spread(values));
    }
    if (isString(keyOrStrings)) {
      return null;
    }
    return defaultTemplate(keyOrStrings, values);
  }
  tr.locale = locale;
  tr.Provider = function(_a) {
    var children = _a.children;
    return createElement(TranslateContext.Provider, { value: tr }, children);
  };
  return tr;
}
// React 16 already requires ES6 Map but not WeakMap
var resourcesCache = typeof WeakMap === "function" ? new WeakMap() : new Map();
function prepareKeys(resources) {
  if (!resources) {
    return resources;
  }
  var cached = resourcesCache.get(resources);
  if (cached) {
    return cached;
  }
  var prepared = {};
  for (var resourceKey in resources) {
    var templateKey = resourceKey
      .replace(/\$\{\s*[A-Za-z0-9_]+\s*\}/g, "{*}")
      .replace(/\s+/g, " ");
    prepared[templateKey] = resources[resourceKey];
  }
  resourcesCache.set(resources, prepared);
  return prepared;
}
function defaultTemplate(strings, values) {
  var length = strings.length;
  switch (length) {
    case 1:
      return strings[0];
    case 2:
      return strings[0] + values[0] + strings[1];
  }
  var array = [];
  for (var i = 0; i < length; i++) {
    array.push(strings[i], values[i]);
  }
  array.pop();
  return array.join("");
}
function isString(arg) {
  return typeof arg === "string";
}
function isFunction(arg) {
  return typeof arg === "function";
}
function isObject(arg) {
  return arg && typeof arg === "object" && !Array.isArray(arg);
}
export {
  useTranslation as useTran,
  withTranslation as withTran,
  Translation as Tran
};
//# sourceMappingURL=react-lazy-i18n.js.map
