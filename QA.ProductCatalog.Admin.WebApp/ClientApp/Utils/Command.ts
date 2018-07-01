import { observable, runInAction } from "mobx";
import NProgress from "nprogress";
import "nprogress/nprogress.css";

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
  return {
    ...descriptor,
    async value(...args: any[]) {
      try {
        if (process.env.NODE_ENV.toLowerCase() !== "production") {
          console.log(`${target.constructor.name || ""}.${key}: started`);
          console.time(`${target.constructor.name || ""}.${key}`);
        }
        runInAction(() => {
          if (commandState.runningCount === 0) {
            NProgress.start();
          }
          commandState.runningCount++;
        });
        return await descriptor.value.apply(this, args);
      } finally {
        if (process.env.NODE_ENV.toLowerCase() !== "production") {
          console.timeEnd(`${target.constructor.name || ""}.${key}`);
        }
        runInAction(() => {
          commandState.runningCount--;
          if (commandState.runningCount === 0) {
            NProgress.done();
          }
        });
      }
    }
  };
}

Object.defineProperty(commandDecorator, "isRunning", {
  get: () => commandState.isRunning
});

export const command = commandDecorator as CommandDecorator;
