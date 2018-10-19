import { observable, action } from "mobx";
import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { isObject, isFunction } from "Utils/TypeChecks";

const commandState = observable({
  runningCount: 0,
  get isRunning() {
    return this.runningCount > 0;
  }
});

interface CommandDecorator {
  (target: Object, key: string, descriptor: PropertyDescriptor): PropertyDescriptor;
  readonly isRunning: boolean;
  alertErrors: boolean;
}

function commandDecorator(target: Object, key: string, descriptor: PropertyDescriptor) {
  const commandName = `${target.constructor.name || ""}.${key}`;
  let commandNumber = 0;
  const backdrop = document.createElement("div");
  Object.assign(backdrop.style, {
    position: "fixed",
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    zIndex: 20
  });
  const startCommand = action(`${commandName}: start`, () => {
    if (DEBUG) {
      const currentName = `${commandName} #${++commandNumber}`;
      console.log(`${currentName}: start`);
      console.time(currentName);
    }
    if (commandState.runningCount === 0) {
      document.body.appendChild(backdrop);
      NProgress.start();
    }
    commandState.runningCount++;
  });
  const finishCommand = action(`${commandName}: finish`, (error?: any) => {
    if (DEBUG) {
      console.timeEnd(`${commandName} #${commandNumber}`);
    }
    commandState.runningCount--;
    if (commandState.runningCount === 0) {
      document.body.removeChild(backdrop);
      NProgress.done();
    }
    if (error instanceof Error) {
      if (command.alertErrors) {
        alert("Произошла ошибка");
      }
      throw error;
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

Object.defineProperties(commandDecorator, {
  alertErrors: {
    value: false,
    writable: true
  },
  isRunning: {
    get: () => commandState.isRunning
  }
});

/**
 * Декоратор для пользовательских действий, требующих асинхронных вызовов.
 * Блокирует ввод данных от пользователя, не блокируя при этом UI браузера.
 * Включает полосу NProgress. Логирует время выполнения операции в DEBUG-режиме.
 */
export const command = commandDecorator as CommandDecorator;
