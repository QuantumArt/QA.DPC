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
      captureUserInput(document.body, true);
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
      captureUserInput(document.body, false);
      NProgress.done();
    }
    if (error instanceof Error) {
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

Object.defineProperty(commandDecorator, "isRunning", {
  get: () => commandState.isRunning
});

/**
 * Декоратор для пользовательских действий, требующих асинхронных вызовов.
 * Блокирует ввод данных от пользователя, не блокируя при этом UI браузера.
 * Включает полосу NProgress. Логирует время выполнения операции в DEBUG-режиме.
 */
export const command = commandDecorator as CommandDecorator;

/** Prevent all user input to some element and all it's descentants */
function captureUserInput(element: Node, capture: boolean): void {
  ["mousedown", "click", "contextmenu", "focus"].forEach(event => {
    if (capture) {
      element.addEventListener(event, preventInput, true);
    } else {
      element.removeEventListener(event, preventInput, true);
    }
  });
  if (capture) {
    (document.activeElement as HTMLElement).blur();
  }
}

function preventInput(event: Event): void {
  event.stopPropagation();
  event.preventDefault();
  if (event.type === "focus") {
    (event.target as HTMLElement).blur();
  }
}
