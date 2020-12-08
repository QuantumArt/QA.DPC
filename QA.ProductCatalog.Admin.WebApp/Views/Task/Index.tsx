import React from "react";
import { render } from "react-dom";
import { Task } from "Tasks";
import { Stub } from "Shared/Components";

if (!window.task) {
  render(<Stub />, document.getElementById("task"));
} else {
  render(<Task />, document.getElementById("task"));
}
