import { ParsedModelType } from "Shared/Utils";
import React, { ReactNode } from "react";
import cn from "classnames";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import "./Style.scss";

interface IProps {
  model: ParsedModelType;
  children: ReactNode;
}

const FormFieldWrapper = observer(({ model, children }: IProps) => {
  const { formStore } = useStores();

  return (
    <div
      className={cn("form-field", {
        "form-field--hide": !formStore.inDefinitionModel.value && model.label !== "InDefinition"
      })}
    >
      {model.label && <label className="form-field__label">{model.label}</label>}
      <div className="form-field-element-wrap">
        {React.Children.map(children, child => {
          return React.cloneElement(child as React.ReactElement, {
            model: model
          });
        })}
      </div>
    </div>
  );
});
export default FormFieldWrapper;
