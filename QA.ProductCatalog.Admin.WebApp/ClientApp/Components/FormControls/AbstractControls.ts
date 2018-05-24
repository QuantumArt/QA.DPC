import { Component } from "react";
import { observable, action } from "mobx";

interface ControlProps {
  [x: string]: any;
  model: { [x: string]: any };
  name: string;
}

export abstract class AbstractControl<P = {}> extends Component<ControlProps & P> {
  componentDidMount() {
    const { model, name } = this.props;
    if (!(name in model)) {
      throw new TypeError(
        `The model ${Object.getPrototypeOf(model).name} does not have property ${name}`
      );
    }
  }
}

export abstract class AbstractInput<P = {}> extends AbstractControl<P> {
  @observable hasFocus = false;
  @observable editValue: any = "";

  handleFocus = action(() => {
    const { model, name } = this.props;
    this.hasFocus = true;
    this.editValue = model[name];
    if (this.editValue == null) {
      this.editValue = "";
    }
  });
}
