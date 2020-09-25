import { BackendEnumType, FormFieldType } from "DefinitionEditor/Enums";
import React from "react";
import { EnumBackendModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { action, observable } from "mobx";

export declare type ParsedModelType =
  | ICheckboxParsedModel
  | IInputParsedModel
  | ISelectParsedModel
  | ITextParsedModel
  | ITextAreaParsedModel;

export function isCheckboxParsedModel(model: ParsedModelType): model is ICheckboxParsedModel {
  return (<ICheckboxParsedModel>model).subModel !== undefined;
}

export interface IBaseParsedModel {
  type: FormFieldType;
  label: string | null;
  name: string;
  value: string | boolean | number;
  isHide: boolean;
  toggleIsHide: () => void;
  isInline: boolean;
}
interface IInputParsedModel extends IBaseParsedModel {
  type: FormFieldType.Input;
  value: string;
  placeholder?: string;
  name: string;
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
}
interface ITextParsedModel extends IBaseParsedModel {
  type: FormFieldType.Text;
  value: string;
}
interface ICheckboxParsedModel extends IBaseParsedModel {
  type: FormFieldType.Checkbox;
  value: boolean;
  subString?: string;
  subModel?: ParsedModelType;
  onChangeCb: () => void;
}

export abstract class BaseAbstractParsedModel implements IBaseParsedModel {
  protected constructor(name, label, value, isHide = false, isInline = false) {
    this.name = name;
    this.label = label;
    this.value = value;
    this.isHide = isHide;
    this.isInline = isInline;
  }
  readonly name: string;
  readonly type: FormFieldType;
  readonly label: string;
  @observable isHide: boolean;
  @action
  toggleIsHide = () => (this.isHide = !this.isHide);
  readonly isInline: boolean;
  value: string | boolean | number;
}

export class TextParsedModel extends BaseAbstractParsedModel implements ITextParsedModel {
  constructor(name, label, value, isHide?, isInline?) {
    super(name, label, value, isHide, isInline);
  }
  readonly type = FormFieldType.Text;
  readonly value;
}

export class SelectParsedModel extends BaseAbstractParsedModel implements ISelectParsedModel {
  constructor(name, label, value, options, isHide?, isInline?) {
    super(name, label, value, isHide, isInline);
    this.options = options;
  }
  readonly type = FormFieldType.Select;
  value;
  readonly options;
}

export class CheckboxParsedModel extends BaseAbstractParsedModel implements ICheckboxParsedModel {
  constructor(
    name,
    label,
    value,
    onChangeCb = null,
    subString = "",
    subComponentOnCheck = null,
    isHide?,
    isInline?
  ) {
    super(name, label, value, isHide, isInline);
    this.subString = subString;
    this.subModel = subComponentOnCheck;
    this.onChangeCb = onChangeCb;
  }
  readonly type = FormFieldType.Checkbox;
  readonly onChangeCb;
  value;
  subModel;
  readonly subString;
}

export class TextAreaParsedModel extends BaseAbstractParsedModel implements ITextAreaParsedModel {
  constructor(name, label, value, extraOptions, isHide?, isInline?) {
    super(name, label, value, isHide, isInline);
    this.extraOptions = extraOptions;
  }
  readonly type = FormFieldType.Textarea;
  readonly value;
  readonly extraOptions;
}

export class InputParsedModel extends BaseAbstractParsedModel implements IInputParsedModel {
  constructor(name, label, value, placeholder = "", isHide?, isInline?) {
    super(name, label, value, isHide, isInline);
    this.placeholder = placeholder;
  }
  readonly type = FormFieldType.Input;
  readonly value;
  readonly placeholder;
}

export const getBackendEnumTypeByFieldName = (
  field: "DeletingMode" | "UpdatingMode" | "CloningMode" | "PreloadingMode" | "PublishingMode"
): BackendEnumType => {
  switch (field) {
    case "DeletingMode":
      return BackendEnumType.Delete;
    case "CloningMode":
      return BackendEnumType.Clone;
    case "UpdatingMode":
      return BackendEnumType.Update;
    case "PreloadingMode":
      return BackendEnumType.Preload;
    case "PublishingMode":
      return BackendEnumType.Publish;
  }
};
