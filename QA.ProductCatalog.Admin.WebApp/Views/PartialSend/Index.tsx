import React from "react";
import { render } from "react-dom";

import { PartialSend, PartialSendStore } from "PartialSend";
import { Stub } from "Shared/Components";

if (!window.partialSend) {
  render(<Stub />, document.getElementById("root"));
} else {
  const store = new PartialSendStore();
  render(<PartialSend store={store} />, document.getElementById("root"));
}
