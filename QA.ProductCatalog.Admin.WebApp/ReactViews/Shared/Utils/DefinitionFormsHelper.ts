import { BackendEnumType, FormFieldType } from "DefinitionEditor/Enums";
import React from "react";
import { EnumBackendModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { action, observable } from "mobx";
import { IOptionProps } from "@blueprintjs/core";

export declare type ParsedModelType =
  | ICheckboxParsedModel
  | IInputParsedModel
  | ISelectParsedModel
  | ITextParsedModel
  | ITextAreaParsedModel;

export interface IBaseParsedModel {
  type: FormFieldType;
  label: string;
  value: string | boolean | number;
}
interface IInputParsedModel extends IBaseParsedModel {
  type: FormFieldType.Input;
  value: string;
  placeholder?: string;
}
interface ITextAreaParsedModel extends IBaseParsedModel {
  type: FormFieldType.Textarea;
  value: string | number;
  extraOptions?: {
    Cols?: number;
    Rows?: number;
    Placeholder?: string;
    Style?: React.CSSProperties;
  };
}
interface ISelectParsedModel extends IBaseParsedModel {
  type: FormFieldType.Select;
  options: EnumBackendModel[];
  value: number;
  getParsedOptions: () => IOptionProps[];
  getParsedValue: (optValue: number) => string;
  setValueByTitle: (valueTitle: string) => void;
}
interface ITextParsedModel extends IBaseParsedModel {
  type: FormFieldType.Text;
  value: string;
}
interface ICheckboxParsedModel extends IBaseParsedModel {
  type: FormFieldType.Checkbox;
  value: boolean;
  toggleValue: () => void;
  subString?: string;
}

export abstract class BaseAbstractParsedModel implements IBaseParsedModel {
  protected constructor(label, value) {
    this.label = label;
    this.value = value;
  }
  readonly type: FormFieldType;
  readonly label: string;
  value: string | boolean | number;
}

export class TextParsedModel extends BaseAbstractParsedModel implements ITextParsedModel {
  constructor(label, value) {
    super(label, value);
  }
  readonly type = FormFieldType.Text;
  readonly value;
}

export class SelectParsedModel extends BaseAbstractParsedModel implements ISelectParsedModel {
  constructor(label, value, options) {
    super(label, value);
    this.options = options;
  }
  readonly type = FormFieldType.Select;
  @observable value;
  getParsedValue = (optValue: number): string => {
    return this.options.find(option => option.value === optValue)?.title;
  };
  getParsedOptions = (): IOptionProps[] => {
    return this.options.map(option => {
      return {
        label: option.title,
        value: option.value
      };
    });
  };
  @action
  setValueByTitle = (title: string) => {
    if (typeof title === "string") {
      this.value = this.options.find(option => option.title === title)?.value;
    }
  };
  readonly options;
}

export class CheckboxParsedModel extends BaseAbstractParsedModel implements ICheckboxParsedModel {
  constructor(label, value, subString = "") {
    super(label, value);
    this.subString = subString;
  }
  readonly type = FormFieldType.Checkbox;
  @observable value;
  @action
  toggleValue = (): void => {
    this.value = !this.value;
    console.log(this.value);
  };
  readonly subString;
}

export class TextAreaParsedModel extends BaseAbstractParsedModel implements ITextAreaParsedModel {
  constructor(label, value, extraOptions) {
    super(label, value);
    this.extraOptions = extraOptions;
  }
  readonly type = FormFieldType.Textarea;
  readonly value;
  readonly extraOptions;
}

export class InputParsedModel extends BaseAbstractParsedModel implements IInputParsedModel {
  constructor(label, value, placeholder = "") {
    super(label, value);
    this.placeholder = placeholder;
  }
  readonly type = FormFieldType.Input;
  readonly value;
  readonly placeholder;
}

export const getBackendEnumTypeByFieldName = (
  field: "DeletingMode" | "UpdatingMode" | "CloningMode"
): BackendEnumType => {
  switch (field) {
    case "DeletingMode":
      return BackendEnumType.Delete;
    case "CloningMode":
      return BackendEnumType.Preload;
    case "UpdatingMode":
      return BackendEnumType.Update;
  }
};
