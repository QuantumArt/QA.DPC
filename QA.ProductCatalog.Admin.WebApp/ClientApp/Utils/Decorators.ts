import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { Intent } from "@blueprintjs/core";
import { NotificationPresenter } from "Services/OverlayPresenter";
import { isPromiseLike } from "Utils/TypeChecks";

/** Trace method calls and execution time to console in DEBUG mode */
export function trace(target: Object, key: string, descriptor: PropertyDescriptor) {
  if (DEBUG) {
    const methodName = `${target.constructor.name || ""}.${key}`;
    let index = 0;
    return intercept(
      descriptor,
      () => {
        const currentName = `${methodName} #${++index}`;
        console.log(`${currentName}: start`);
        console.time(currentName);
      },
      () => console.timeEnd(`${methodName} #${index}`)
    );
  }
  return descriptor;
}

let modalRunningCount = 0;

/** Block all user's input while executing method */
export function modal(_target: Object, _key: string, descriptor: PropertyDescriptor) {
  const backdrop = document.createElement("div");
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
    () => {
      if (modalRunningCount === 0) {
        document.body.appendChild(backdrop);
      }
      modalRunningCount++;
    },
    () => {
      modalRunningCount--;
      if (modalRunningCount === 0) {
        document.body.removeChild(backdrop);
      }
    }
  );
}

let progressRunningCount = 0;

/** Show NProgress bar while exeduting method */
export function progress(_target: Object, _key: string, descriptor: PropertyDescriptor) {
  return intercept(
    descriptor,
    () => {
      if (progressRunningCount === 0) {
        NProgress.start();
      }
      progressRunningCount++;
    },
    () => {
      progressRunningCount--;
      if (progressRunningCount === 0) {
        NProgress.done();
      }
    }
  );
}

const handledErrors = new WeakSet();

/** Show notification about error */
export function handleError(_target: Object, _key: string, descriptor: PropertyDescriptor) {
  return intercept(descriptor, null, error => {
    if (error instanceof Error && !handledErrors.has(error)) {
      handledErrors.add(error);
      NotificationPresenter.show({
        intent: Intent.DANGER,
        message: "Произошла ошибка",
        timeout: 15000
      });
    }
  });
}

/** Attach `before` and `after` callbacks to method or async method */
const intercept = (
  descriptor: PropertyDescriptor,
  before: () => void,
  after: (error?: any) => void
) => ({
  ...descriptor,
  value() {
    if (before) {
      before();
    }
    let isPromise = false;
    let error = undefined;
    try {
      const result = descriptor.value.apply(this, arguments);
      isPromise = isPromiseLike(result);
      if (isPromise && after) {
        result.then(() => after(), error => after(error));
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
