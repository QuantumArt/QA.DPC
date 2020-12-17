import React from "react";
import { render } from "react-dom";
import { Notification } from "Notification";
import { Stub } from "Shared/Components";

if (!window.notification) {
  render(<Stub />, document.getElementById("notification"));
} else {
  render(<Notification />, document.getElementById("notification"));
}
