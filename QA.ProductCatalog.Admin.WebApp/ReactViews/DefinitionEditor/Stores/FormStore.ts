import XmlEditorStore from "./XmlEditorStore";
import { action, computed, observable, reaction } from "mobx";
import ApiService from "DefinitionEditor/ApiService";
import { EnumBackendModel, IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { BackendEnumType } from "DefinitionEditor/Enums";
import {
  CheckboxParsedModel,
  getBackendEnumTypeByFieldName,
  InputParsedModel,
  ISingleRequestedData,
  ParsedModelType,
  SelectParsedModel,
  singleRequestedData,
  TextAreaParsedModel,
  TextParsedModel
} from "Shared/Utils";
import { OperationState } from "Shared/Enums";
import _ from "lodash";

export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {
    this.singleRequestedEnums = new singleRequestedData(ApiService.getSelectEnums);
    this.initEnumsModel();
  }
  @observable private nodeId;
  @observable UIEditModel: ParsedModelType[];
  private apiEditModel: IEditFormModel;
  private singleRequestedEnums: ISingleRequestedData<
    { [key in BackendEnumType]: EnumBackendModel[] }
  >;
  private enumsModel: { [key in BackendEnumType]: EnumBackendModel[] };

  @observable operationState: OperationState = OperationState.None;
  @observable formError: string;
  @observable errorText: string = null;
  @observable errorLog: string = null;

  fetchFieldsReaction = reaction(
    () => this.nodeId,
    (nodeId: string) => {
      if (nodeId) this.fetchFormFields();
    }
  );

  initEnumsModel = async () => {
    try {
      this.enumsModel = await this.singleRequestedEnums.getData();
    } catch (e) {
      //TODO прикрутить попап ошибок
      this.formError = "Ошибка загрузки формы";
      this.operationState = OperationState.Error;
      throw e;
    }
  };

  getModelByFieldName = (field: string): ParsedModelType => {
    switch (field) {
      case "FieldName":
        return new InputParsedModel(field, this.apiEditModel[field]);
      case "FieldTitle":
        return new InputParsedModel(field, this.apiEditModel[field], "ControlStrings.LabelText");
      case "RelateTo":
        return new TextParsedModel(
          this.apiEditModel[field],
          `${this.apiEditModel["RelatedContentName"]} ${this.apiEditModel["RelatedContentId"]}`
        );
      case "FieldId":
        return new TextParsedModel(field, this.apiEditModel[field]);
      case "InDefinition":
        return new CheckboxParsedModel(field, this.apiEditModel[field]);
      case "RelationConditionDescription":
        if (!this.apiEditModel[field]) return undefined;
        return new TextAreaParsedModel(
          "RelationCondition",
          this.apiEditModel["RelationCondition"],
          {
            rows: 6,
            placeholder: this.apiEditModel[field],
            style: { resize: "none", fontFamily: "monospace" }
          }
        );
      case "ClonePrototypeConditionDescription":
        if (!this.apiEditModel[field]) return undefined;
        return new TextAreaParsedModel(
          "ClonePrototypeCondition",
          this.apiEditModel["ClonePrototypeCondition"],
          {
            rows: 6,
            placeholder: this.apiEditModel[field],
            style: { resize: "none", fontFamily: "monospace" }
          }
        );
      case "DeletingMode":
      case "UpdatingMode":
      case "CloningMode":
        return new SelectParsedModel(
          field,
          this.apiEditModel[field],
          this.enumsModel[getBackendEnumTypeByFieldName(field)]
        );
      default:
        return undefined;
    }
  };

  parseEditFormDataToViewDataModel = (model: IEditFormModel): ParsedModelType[] => {
    return Object.keys(model).map(
      (fieldName): ParsedModelType => {
        return this.getModelByFieldName(fieldName);
      }
    );
  };

  @action
  fetchFormFields = async (): Promise<void> => {
    const formData = new FormData();
    formData.append("path", this.nodeId.charAt(0) === "/" ? this.nodeId : `/${this.nodeId}`);
    formData.append("xml", this.xmlEditorStore.xml);
    try {
      if (!this.enumsModel) await this.initEnumsModel();
      const editForm = await ApiService.getEditForm(formData);
      this.apiEditModel = editForm;
      this.UIEditModel = _.compact(this.parseEditFormDataToViewDataModel(editForm));
    } catch (e) {
      console.log(e);
    }
  };

  @action
  setNodeId = (nodeId: string) => (this.nodeId = nodeId);
}
