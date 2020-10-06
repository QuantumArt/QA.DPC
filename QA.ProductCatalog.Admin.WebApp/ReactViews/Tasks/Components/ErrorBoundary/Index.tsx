import React from "react";
import "./Style.scss";

export class ErrorBoundary extends React.Component {
  state = { hasError: false };

  constructor(props) {
    super(props);
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  render() {
    const { typicalError } = window.task.schedule;

    if (this.state.hasError) {
      return <h3 className="color-danger ui-error">{typicalError}</h3>;
    }

    return this.props.children;
  }
}
