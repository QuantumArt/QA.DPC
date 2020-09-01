import XmlEditorStore from "./XmlEditorStore";
import { action, observable, reaction } from "mobx";
import ApiService from "DefinitionEditor/ApiService";
import { IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { FormFieldType } from "DefinitionEditor/Enums";
import React from "react";

declare type ParsedModelType =
  | ICheckboxParsedModel
  | IInputParsedModel
  | ISelectParsedModel
  | ITextParsedModel
  | ITextAreaParsedModel;

interface IBaseParsedModel {
  type: FormFieldType;
  label: string;
  value: string | boolean | number;
}
interface IInputParsedModel extends IBaseParsedModel {
  type: FormFieldType.Input;
  value: string | number;
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
  options: any[];
}
interface ITextParsedModel extends IBaseParsedModel {
  type: FormFieldType.Text;
  value: string | number;
}
interface ICheckboxParsedModel extends IBaseParsedModel {
  type: FormFieldType.Checkbox;
  value: boolean;
  subString?: string;
}
//___________________________________________________

abstract class BaseAbstractParsedModel implements IBaseParsedModel {
  protected constructor(label) {
    this.label = label;
  }
  readonly type: FormFieldType;
  readonly label: string;
  value: string | boolean | number;
}

class TextParsedModel extends BaseAbstractParsedModel implements ITextParsedModel {
  constructor(label, value) {
    super(label);
    this.value = value;
  }
  readonly type = FormFieldType.Text;
  readonly value;
}

class CheckboxParsedModel extends BaseAbstractParsedModel implements ICheckboxParsedModel {
  constructor(label, value, subString) {
    super(label);
    this.value = value;
    this.subString = subString;
  }
  readonly type = FormFieldType.Checkbox;
  readonly value;
  readonly subString;
}

class TextAreaParsedModel extends BaseAbstractParsedModel implements ITextAreaParsedModel {
  constructor(label, value, extraOptions) {
    super(label);
    this.value = value;
    this.extraOptions = extraOptions;
  }
  readonly type = FormFieldType.Textarea;
  readonly value;
  readonly extraOptions;
}

class InputParsedModel extends BaseAbstractParsedModel implements IInputParsedModel {
  constructor(label, value, placeholder) {
    super(label);
    this.value = value;
    this.placeholder = placeholder;
  }
  readonly type = FormFieldType.Input;
  readonly value;
  readonly placeholder;
}
//___________________________________________________

export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {}
  @observable nodeId;
  @observable.ref UIEditModel: ParsedModelType[];
  private apiEditModel: IEditFormModel;

  fetchFieldsReaction = reaction(
    () => this.nodeId,
    (nodeId: string) => {
      this.fetchFormFields();
    }
  );

  parseEditFormDataToViewDataModel = (model: IEditFormModel): ParsedModelType[] => {
    const initInputModel = (value, label, placeholder = ""): IInputParsedModel => {
      return new InputParsedModel(label, value, placeholder);
    };
    const initTextModel = (label, value): ITextParsedModel => {
      return new TextParsedModel(label, value);
    };
    const initCheckboxModel = (value, label, subString = null): ICheckboxParsedModel => {
      return new CheckboxParsedModel(label, value, subString);
    };
    const initTextAreaParsedModel = (value, label, extraOptions = {}): ITextAreaParsedModel => {
      return new TextAreaParsedModel(label, value, extraOptions);
    };

    const getModelByFieldName = (
      field: string
      // @ts-ignore
    ): ParsedModelType => {
      switch (field) {
        case "FieldName":
          return initInputModel(this.apiEditModel[field], field);
        case "FieldTitle":
          return initInputModel(this.apiEditModel[field], field, "ControlStrings.LabelText");
        case "RelateTo":
          return initTextModel(
            this.apiEditModel[field],
            `${this.apiEditModel["RelatedContentName"]} ${this.apiEditModel["RelatedContentId"]}`
          );
        case "FieldId":
          return initTextModel(field, this.apiEditModel[field]);
        case "InDefinition":
          return initCheckboxModel(this.apiEditModel[field], field);
        case "RelationConditionDescription":
          if (!this.apiEditModel[field]) return undefined;
          return initTextAreaParsedModel(
            this.apiEditModel["RelationCondition"],
            "RelationCondition",
            {
              rows: 6,
              placeholder: this.apiEditModel[field],
              style: { resize: "none", fontFamily: "monospace" }
            }
          );
        case "ClonePrototypeConditionDescription":
          if (!this.apiEditModel[field]) return undefined;
          return initTextAreaParsedModel(
            this.apiEditModel["ClonePrototypeCondition"],
            "ClonePrototypeCondition",
            {
              rows: 6,
              placeholder: this.apiEditModel[field],
              style: { resize: "none", fontFamily: "monospace" }
            }
          );
        // case "DeletingMode":
        // case "UpdatingMode":
        // case "CloningMode":
        //   return FormFieldType.Select;
        // default:
        //   return FormFieldType.Text;
      }
    };
    return Object.keys(model).map(
      (fieldName): ParsedModelType => {
        return getModelByFieldName(fieldName);
      }
    );
  };

  @action
  fetchFormFields = async (): Promise<void> => {
    const formData = new FormData();
    formData.append("path", this.nodeId.charAt(0) === "/" ? this.nodeId : `/${this.nodeId}`);
    formData.append("xml", this.xmlEditorStore.xml);
    try {
      const editForm = await ApiService.getEditForm(formData);
      this.apiEditModel = editForm;
      this.UIEditModel = this.parseEditFormDataToViewDataModel(editForm);
      //parse model to ui form
    } catch (e) {}
  };

  @action
  setNodeId = (nodeId: string) => (this.nodeId = nodeId);
}
