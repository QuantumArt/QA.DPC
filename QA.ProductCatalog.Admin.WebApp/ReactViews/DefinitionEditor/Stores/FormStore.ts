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
import { IReactionDisposer } from "mobx/lib/internal";

export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {
    this.singleRequestedEnums = new singleRequestedData(ApiService.getSelectEnums);
    this.initEnumsModel();
  }
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
  fetchFieldsReaction: IReactionDisposer;

  init = (cb: (onReactionAction: (nodeId: string) => Promise<void>) => IReactionDisposer) => {
    this.fetchFieldsReaction = cb(this.fetchFormFields);
  };

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
    const isFieldShouldBeHide = !this.inDefinitionModel?.value;
    const fieldValue = this.apiEditModel[field];

    switch (field) {
      case "InDefinition":
        if (
          !_.isNull(this.apiEditModel["IsFromDictionaries"]) &&
          !_.isUndefined(this.apiEditModel["IsFromDictionaries"])
        )
          return undefined;
        return this.inDefinitionModel;
      case "CacheEnabled":
        const mainCheckboxModel = new CheckboxParsedModel(
          field,
          field,
          fieldValue,
          "Cache",
          null,
          isFieldShouldBeHide,
          true
        );

        const subComponentInput = new InputParsedModel(
          "CachePeriod",
          null,
          this.apiEditModel["CachePeriod"],
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
        return new InputParsedModel(field, field, fieldValue, "", isFieldShouldBeHide);
      case "FieldTitle":
        if (_.isUndefined(fieldValue)) return undefined;
        return new InputParsedModel(
          field,
          this.settings.strings.FieldNameForCard,
          fieldValue,
          this.settings.strings.LabelText,
          isFieldShouldBeHide
        );
      case "RelateTo":
        if (!this.apiEditModel["RelatedContentName"] && !this.apiEditModel["RelatedContentId"])
          return undefined;
        return new TextParsedModel(
          field,
          fieldValue,
          `${this.apiEditModel["RelatedContentName"] || ""} ${this.apiEditModel[
            "RelatedContentId"
          ] || ""}`,
          isFieldShouldBeHide
        );
      case "FieldId":
      case "IsClassifier":
      case "ContentId":
        return new TextParsedModel(field, field, fieldValue, isFieldShouldBeHide);
      case "SkipCData":
      case "LoadLikeImage":
        return new CheckboxParsedModel(field, field, fieldValue, "", null, isFieldShouldBeHide);
      case "IsReadOnly":
      case "LoadAllPlainFields":
        if (this.apiEditModel["IsFromDictionaries"]) return undefined;
        return new CheckboxParsedModel(field, field, fieldValue, "", null, isFieldShouldBeHide);
      case "RelationConditionDescription":
        return new TextAreaParsedModel(
          "RelationCondition",
          "RelationCondition",
          this.apiEditModel["RelationCondition"],
          {
            rows: 6,
            placeholder: fieldValue,
            style: { resize: "none", fontFamily: "monospace" }
          },
          isFieldShouldBeHide
        );
      case "ClonePrototypeConditionDescription":
        return new TextAreaParsedModel(
          "ClonePrototypeCondition",
          "ClonePrototypeCondition",
          this.apiEditModel["ClonePrototypeCondition"],
          {
            rows: 6,
            placeholder: fieldValue,
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
          field,
          fieldValue,
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

  @action
  initInDefinitionModel = () => {
    const definitionFieldValue = this.apiEditModel["InDefinition"];
    if (!_.isNull(definitionFieldValue)) {
      runInAction(() => {
        this.inDefinitionModel = new CheckboxParsedModel(
          "InDefinition",
          "InDefinition",
          definitionFieldValue
        );
        console.log(this.inDefinitionModel);
      });
    }
  };

  parseEditFormDataToUIModel = (model: IEditFormModel): ParsedModelType[] => {
    const exceptionFields = [
      { onField: ["DefaultCachePeriod"], fieldsToRender: ["InDefinition", "DefaultCachePeriod"] }
    ];

    /**
     * исключение
     * если заполнено поле DefaultCachePeriod рендерим только его
     * */
    if (model["DefaultCachePeriod"]) {
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
  fetchFormFields = async (nodeId: string): Promise<void> => {
    const formData = new FormData();
    formData.append("path", nodeId.charAt(0) === "/" ? nodeId : `/${nodeId}`);
    formData.append("xml", this.xmlEditorStore.xml);
    try {
      if (!this.enumsModel) await this.initEnumsModel();
      const editForm = await ApiService.getEditForm(formData);
      this.apiEditModel = editForm;
      this.initInDefinitionModel();
      this.UIEditModel = _.compact(this.parseEditFormDataToUIModel(editForm));
    } catch (e) {
      console.log(e);
    }
  };
}
