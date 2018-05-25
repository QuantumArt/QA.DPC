import { Component } from "react";
import PropTypes from "prop-types";
import { untracked } from "mobx";
import { isObject, isPlainObject } from "Utils/TypeChecks";

interface ControlProps {
  [x: string]: any;
  model: { [x: string]: any };
  name: string;
}

export abstract class AbstractControl<P = {}> extends Component<ControlProps & P> {
  static propTypes = {
    model: PropTypes.object.isRequired,
    name(props, _propName, componentName) {
      const { model, name } = props;
      if (!isObject(model) || name in model) {
        return null;
      }
      const modelName = isPlainObject(model) ? "Object" : Object.getPrototypeOf(model).name;
      return new Error(
        `Invalid prop \`name\` supplied to ${componentName}. Passed \`model\` [${modelName}] does not have property "${name}"`
      );
    }
  };
}

export abstract class AbstractInput<P = {}> extends AbstractControl<P> {
  state = {
    hasFocus: false,
    editValue: ""
  };

  handleFocus = () => {
    const { model, name } = this.props;
    let editValue = untracked(() => model[name]);
    if (editValue == null) {
      editValue = "";
    }
    this.setState({ hasFocus: true, editValue });
  };
}
