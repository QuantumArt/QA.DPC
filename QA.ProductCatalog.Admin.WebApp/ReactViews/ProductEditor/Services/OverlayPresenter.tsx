import React, { ReactNode } from "react";
import { observable, action } from "mobx";
import { Intent, IToastProps, Toaster, Position } from "@blueprintjs/core";
import { AlertWrapper } from "ProductEditor/Components/Overlay/AlertWrapper";

export class OverlayPresenter {
  public overlays = observable.array<ReactNode>();

  @action
  public alert(message: ReactNode, button: string) {
    const index = this.overlays.length;

    return new Promise<void>(resolve => {
      this.overlays.push(
        <AlertWrapper
          key={index}
          canEscapeKeyCancel
          canOutsideClickCancel
          confirmButtonText={button}
          onClose={() => {
            resolve();
            setTimeout(action(() => this.overlays.splice(index, 1)), 300);
          }}
        >
          {message}
        </AlertWrapper>
      );
    });
  }

  @action
  public confirm(message: ReactNode, confirmButton: string, cancelButton: string) {
    const index = this.overlays.length;

    return new Promise<boolean>(resolve => {
      this.overlays.push(
        <AlertWrapper
          key={index}
          confirmButtonText={confirmButton}
          cancelButtonText={cancelButton}
          intent={Intent.PRIMARY}
          onClose={confirmed => {
            resolve(confirmed);
            setTimeout(action(() => this.overlays.splice(index, 1)), 300);
          }}
        >
          {message}
        </AlertWrapper>
      );
    });
  }

  public notify(toast: IToastProps) {
    NotificationPresenter.show(toast);
  }
}

export const NotificationPresenter = Toaster.create({
  position: Position.BOTTOM_RIGHT
});
