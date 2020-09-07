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

export interface IBaseParsedModel {
  type: FormFieldType;
  label: string | null;
  value: string | boolean | number;
  isHide: boolean;
  isInline: boolean;
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
}
interface ITextParsedModel extends IBaseParsedModel {
  type: FormFieldType.Text;
  value: string;
}
interface ICheckboxParsedModel extends IBaseParsedModel {
  type: FormFieldType.Checkbox;
  value: boolean;
  subString?: string;
  subComponentOnCheck?: ParsedModelType;
  toggleValue?: () => void;
}

export abstract class BaseAbstractParsedModel implements IBaseParsedModel {
  protected constructor(label, value, isHide = false, isInline = false) {
    this.label = label;
    this.value = value;
    this.isHide = isHide;
    this.isInline = isInline;
  }
  readonly type: FormFieldType;
  readonly label: string;
  isHide: boolean;
  readonly isInline: boolean;
  value: string | boolean | number;
}

export class TextParsedModel extends BaseAbstractParsedModel implements ITextParsedModel {
  constructor(label, value, isHide?, isInline?) {
    super(label, value, isHide, isInline);
  }
  readonly type = FormFieldType.Text;
  readonly value;
}

export class SelectParsedModel extends BaseAbstractParsedModel implements ISelectParsedModel {
  constructor(label, value, options, isHide?, isInline?) {
    super(label, value, isHide, isInline);
    this.options = options;
  }
  readonly type = FormFieldType.Select;
  value;
  readonly options;
}

export class CheckboxParsedModel extends BaseAbstractParsedModel implements ICheckboxParsedModel {
  constructor(label, value, subString = "", subComponentOnCheck = null, isHide?, isInline?) {
    super(label, value, isHide, isInline);
    this.subString = subString;
    this.subComponentOnCheck = subComponentOnCheck;
  }
  readonly type = FormFieldType.Checkbox;
  @observable value;
  subComponentOnCheck;
  @action
  toggleValue = () => {
    this.value = !this.value;
  };
  readonly subString;
}

export class TextAreaParsedModel extends BaseAbstractParsedModel implements ITextAreaParsedModel {
  constructor(label, value, extraOptions, isHide?, isInline?) {
    super(label, value, isHide, isInline);
    this.extraOptions = extraOptions;
  }
  readonly type = FormFieldType.Textarea;
  readonly value;
  readonly extraOptions;
}

export class InputParsedModel extends BaseAbstractParsedModel implements IInputParsedModel {
  constructor(label, value, placeholder = "", isHide?, isInline?) {
    super(label, value, isHide, isInline);
    this.placeholder = placeholder;
  }
  @observable isHide: boolean;
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
