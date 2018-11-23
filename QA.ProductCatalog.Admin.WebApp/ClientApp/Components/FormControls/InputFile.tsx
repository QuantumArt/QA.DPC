import React, { createRef, InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";

@observer
export class InputFile extends ValidatableControl<InputHTMLAttributes<HTMLInputElement>> {
  inputRef = createRef<HTMLInputElement>();

  @action
  handleChange(e: any) {
    super.handleChange(e);
    const { model, name } = this.props;
    const files: FileList = e.target.files;
    if (files.length > 0) {
      model[name] = files[0].name;
    } else {
      model[name] = null;
    }
  }

  @action
  handleClear = (e: any) => {
    e.preventDefault();
    const { model, name } = this.props;
    model.setTouched(name, true);
    this.inputRef.current.value = null;
    model[name] = null;
  };

  render() {
    const {
      model,
      name,
      className,
      readOnly,
      onFocus,
      onChange,
      onBlur,
      placeholder,
      ...props
    } = this.props;
    const fileName = model[name];
    return (
      <label
        className={cn("bp3-file-input bp3-fill editor-input-file", className)}
        title={fileName}
      >
        <input
          ref={this.inputRef}
          type={readOnly ? "text" : "file"}
          readOnly={readOnly}
          onFocus={this.handleFocus}
          onChange={this.handleChange}
          onBlur={this.handleBlur}
          {...props}
        />
        <span
          className={cn("bp3-file-upload-input", {
            "editor-input-file__placeholder": !fileName
          })}
        >
          {fileName ? fileName : placeholder}
          <span
            className="editor-input-file__clear bp3-icon bp3-icon-cross"
            title="Очистить"
            onClick={this.handleClear}
          />
        </span>
      </label>
    );
  }
}
