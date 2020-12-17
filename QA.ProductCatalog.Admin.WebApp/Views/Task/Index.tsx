import React from "react";
import { render } from "react-dom";
import { Tasks } from "Tasks";
import { Stub } from "Shared/Components";

if (!window.task) {
  render(<Stub />, document.getElementById("task"));
} else {
  render(<Tasks />, document.getElementById("task"));
}
