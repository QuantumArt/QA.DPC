import XmlEditorStore from "./XmlEditorStore";
import { action, observable, reaction, runInAction } from "mobx";
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
  @observable inDefinitionModel: CheckboxParsedModel;

  fetchFieldsReaction = reaction(
    () => this.nodeId,
    (nodeId: string) => {
      if (nodeId) this.fetchFormFields();
    }
  );

  initEnumsModel = async () => {
    try {
      this.enumsModel = await this.singleRequestedEnums.getData();
      console.log(this.enumsModel);
    } catch (e) {
      //TODO прикрутить попап ошибок
      this.formError = "Ошибка загрузки формы";
      this.operationState = OperationState.Error;
      throw e;
    }
  };

  getModelByFieldName = (field: string): ParsedModelType => {
    const isFieldShouldBeHide = !this.inDefinitionModel.value;

    switch (field) {
      case "CachePeriod":
        const mainCheckboxModel = new CheckboxParsedModel(
          field,
          this.apiEditModel["CacheEnabled"],
          "Cache",
          null,
          isFieldShouldBeHide,
          true
        );

        const subComponentInput = new InputParsedModel(
          "Cache",
          this.apiEditModel[field],
          "",
          !mainCheckboxModel.value,
          true
        );

        //TODO вынести в свойства стора для возможности отписки на unmount текущего ui форм
        reaction(
          () => mainCheckboxModel.value,
          (val: boolean) => {
            subComponentInput.isHide = !val;
          }
        );
        mainCheckboxModel.subComponentOnCheck = subComponentInput;

        return mainCheckboxModel;
      case "FieldName":
      case "ContentName":
      case "DefaultCachePeriod":
        if (!this.apiEditModel[field]) return undefined;
        return new InputParsedModel(field, this.apiEditModel[field], "", isFieldShouldBeHide);
      case "FieldTitle":
        return new InputParsedModel(
          this.settings.formControlStrings.fieldNameForCard,
          this.apiEditModel[field],
          this.settings.formControlStrings.labelText,
          isFieldShouldBeHide
        );
      case "RelateTo":
        if (!this.apiEditModel["RelatedContentName"] && !this.apiEditModel["RelatedContentId"])
          return undefined;
        return new TextParsedModel(
          this.apiEditModel[field],
          `${this.apiEditModel["RelatedContentName"] || ""} ${this.apiEditModel[
            "RelatedContentId"
          ] || ""}`,
          isFieldShouldBeHide
        );
      case "FieldId":
      case "IsClassifier":
      case "ContentId":
        if (!this.apiEditModel[field]) return undefined;
        return new TextParsedModel(field, this.apiEditModel[field], isFieldShouldBeHide);
      case "InDefinition":
        return this.inDefinitionModel;
      case "IsReadOnly":
      case "LoadAllPlainFields":
        if (this.apiEditModel["IsFromDictionaries"]) return undefined;
        return new CheckboxParsedModel(
          field,
          this.apiEditModel[field],
          "",
          null,
          isFieldShouldBeHide
        );
      case "RelationConditionDescription":
        if (!this.apiEditModel[field]) return undefined;
        return new TextAreaParsedModel(
          "RelationCondition",
          this.apiEditModel["RelationCondition"],
          {
            rows: 6,
            placeholder: this.apiEditModel[field],
            style: { resize: "none", fontFamily: "monospace" }
          },
          isFieldShouldBeHide
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
          },
          isFieldShouldBeHide
        );
      case "DeletingMode":
      case "UpdatingMode":
      case "CloningMode":
      case "PreloadingMode":
      case "PublishingMode":
        return new SelectParsedModel(
          field,
          this.apiEditModel[field],
          this.enumsModel[getBackendEnumTypeByFieldName(field)].map(option => {
            return {
              label: option.title,
              value: option.value
            };
          }),
          isFieldShouldBeHide
        );
      default:
        return undefined;
    }
  };

  parseEditFormDataToViewDataModel = (model: IEditFormModel): ParsedModelType[] => {
    if (this.apiEditModel["InDefinition"]) {
      runInAction(() => {
        this.inDefinitionModel = new CheckboxParsedModel(
          "InDefinition",
          this.apiEditModel["InDefinition"]
        );
      });
    }

    if (model["FieldType"] && model["DefaultCachePeriod"]) {
      const fields = ["InDefinition", "DefaultCachePeriod"];
      return fields.map(
        (fieldName): ParsedModelType => {
          return this.getModelByFieldName(fieldName);
        }
      );
    }

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
