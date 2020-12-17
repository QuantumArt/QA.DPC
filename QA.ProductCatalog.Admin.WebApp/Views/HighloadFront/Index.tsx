import React from "react";
import { render } from "react-dom";

import { HighloadFront, HighloadFrontStore } from "../../ReactViews/HighloadFront";
import { Stub } from "../../ReactViews/Shared/Components";

if (!window.highloadFront) {
  render(<Stub />, document.getElementById("root"));
} else {
  const store = new HighloadFrontStore();
  render(<HighloadFront store={store} />, document.getElementById("root"));
}
