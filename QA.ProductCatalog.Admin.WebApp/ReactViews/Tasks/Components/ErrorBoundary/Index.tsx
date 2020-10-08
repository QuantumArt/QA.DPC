import React from "react";
import "./Style.scss";
import { l } from "Tasks/Localization";

export class ErrorBoundary extends React.Component {
  state = { hasError: false };

  constructor(props) {
    super(props);
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return <h3 className="color-danger ui-error">{l("typicalError")}</h3>;
    }

    return this.props.children;
  }
}
