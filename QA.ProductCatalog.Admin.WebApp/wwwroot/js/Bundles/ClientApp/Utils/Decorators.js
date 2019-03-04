import * as tslib_1 from "tslib";
import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { Intent } from "@blueprintjs/core";
import { NotificationPresenter } from "Services/OverlayPresenter";
import { isPromiseLike } from "Utils/TypeChecks";
/** Trace method calls and execution time to console in DEBUG mode */
export function trace(target, key, descriptor) {
  if (DEBUG) {
    var methodName_1 = (target.constructor.name || "") + "." + key;
    var index_1 = 0;
    return intercept(
      descriptor,
      function() {
        var currentName = methodName_1 + " #" + ++index_1;
        console.log(currentName + ": start");
        console.time(currentName);
      },
      function() {
        return console.timeEnd(methodName_1 + " #" + index_1);
      }
    );
  }
  return descriptor;
}
var modalRunningCount = 0;
/** Block all user's input while executing method */
export function modal(_target, _key, descriptor) {
  var backdrop = document.createElement("div");
  Object.assign(backdrop.style, {
    position: "fixed",
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    zIndex: 20
  });
  return intercept(
    descriptor,
    function() {
      if (modalRunningCount === 0) {
        document.body.appendChild(backdrop);
      }
      modalRunningCount++;
    },
    function() {
      modalRunningCount--;
      if (modalRunningCount === 0) {
        document.body.removeChild(backdrop);
      }
    }
  );
}
var progressRunningCount = 0;
/** Show NProgress bar while exeduting method */
export function progress(_target, _key, descriptor) {
  return intercept(
    descriptor,
    function() {
      if (progressRunningCount === 0) {
        NProgress.start();
      }
      progressRunningCount++;
    },
    function() {
      progressRunningCount--;
      if (progressRunningCount === 0) {
        NProgress.done();
      }
    }
  );
}
var visitedErrors = new WeakSet();
/** Show notification about error */
export function handleError(_target, _key, descriptor) {
  if (handleError.silent) {
    return descriptor;
  }
  return intercept(descriptor, null, function(error) {
    if (error instanceof Error && !visitedErrors.has(error)) {
      visitedErrors.add(error);
      NotificationPresenter.show({
        intent: Intent.DANGER,
        message: "Произошла ошибка",
        timeout: 15000
      });
    }
  });
}
/** Disable all notifications about errors */
handleError.silent = false;
/** Attach `before` and `after` callbacks to method or async method */
var intercept = function(descriptor, before, after) {
  return tslib_1.__assign({}, descriptor, {
    value: function() {
      if (before) {
        before();
      }
      var isPromise = false;
      var error = undefined;
      try {
        var result = descriptor.value.apply(this, arguments);
        isPromise = isPromiseLike(result);
        if (isPromise && after) {
          result.then(
            function() {
              return after();
            },
            function(error) {
              return after(error);
            }
          );
        }
        return result;
      } catch (e) {
        error = e;
        throw e;
      } finally {
        if (!isPromise && after) {
          after(error);
        }
      }
    }
  });
};
//# sourceMappingURL=Decorators.js.map
