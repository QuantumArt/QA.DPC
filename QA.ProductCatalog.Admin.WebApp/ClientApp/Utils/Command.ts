import { observable, action } from "mobx";
import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { isObject, isFunction } from "./TypeChecks";

const DEBUG = process.env.NODE_ENV.toLowerCase() !== "production";

const commandState = observable({
  runningCount: 0,
  get isRunning() {
    return this.runningCount > 0;
  }
});

interface CommandDecorator {
  (target: Object, key: string, descriptor: PropertyDescriptor): PropertyDescriptor;
  readonly isRunning: boolean;
}

function commandDecorator(target: Object, key: string, descriptor: PropertyDescriptor) {
  const commandName = `${target.constructor.name || ""}.${key}`;
  let commandNumber = 0;
  const startCommand = action(`${commandName}: start`, () => {
    if (DEBUG) {
      const currentName = `${commandName} #${++commandNumber}`;
      console.log(`${currentName}: start`);
      console.time(currentName);
    }
    if (commandState.runningCount === 0) {
      NProgress.start();
    }
    commandState.runningCount++;
  });
  const finishCommand = action(`${commandName}: finish`, () => {
    if (DEBUG) {
      console.timeEnd(`${commandName} #${commandNumber}`);
    }
    commandState.runningCount--;
    if (commandState.runningCount === 0) {
      NProgress.done();
    }
  });
  return {
    ...descriptor,
    value(...args: any[]) {
      startCommand();
      let isPromise = false;
      try {
        const result = descriptor.value.apply(this, args);
        isPromise = isObject(result) && isFunction(result.then);
        if (isPromise) {
          result.then(finishCommand, finishCommand);
        }
        return result;
      } finally {
        if (!isPromise) {
          finishCommand();
        }
      }
    }
  };
}

Object.defineProperty(commandDecorator, "isRunning", {
  get: () => commandState.isRunning
});

export const command = commandDecorator as CommandDecorator;
