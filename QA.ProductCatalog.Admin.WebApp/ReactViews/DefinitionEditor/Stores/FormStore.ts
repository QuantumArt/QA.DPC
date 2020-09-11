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
import { l } from "DefinitionEditor/Localization";

//TODO доделать:
// 1. при вводе значения в пустой инпут и переключениии ноды, значение сохраняется
// 2. сделать обработку ошибок на всех уровнях работы с формой
// 3. прикрутить лоадер
// 4. добавить локализацию
export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {
    this.singleRequestedEnums = new singleRequestedData(ApiService.getSelectEnums);
    this.initEnumsModel();
  }

  @observable inDefinitionModel: CheckboxParsedModel;
  @observable UIEditModel: ParsedModelType[];
  private apiEditModel: IEditFormModel;
  private singleRequestedEnums: ISingleRequestedData<
    { [key in BackendEnumType]: EnumBackendModel[] }
  >;
  private enumsModel: { [key in BackendEnumType]: EnumBackendModel[] };
  public formData: {};
  private readonly excludeFieldsFromNewFormData: string[] = ["RelateTo"];

  @observable operationState: OperationState = OperationState.None;
  @observable isLeaveWithoutSaveDialog: boolean = false;
  @observable warningPopupOnExitCb: () => void;
  @observable errorText: string = null;

  fetchFieldsReaction: IReactionDisposer;
  otherFieldReactions: IReactionDisposer[] = [];

  @action
  resetErrorState = () => {
    this.operationState = OperationState.None;
    this.errorText = null;
  };

  @action
  toggleLeaveWithoutSaveDialog = () => {
    this.isLeaveWithoutSaveDialog = !this.isLeaveWithoutSaveDialog;
  };

  @action
  setError = (errText?: string) => {
    this.operationState = OperationState.Error;
    this.errorText = errText ?? "Error";
  };

  setFormData = (newFormData: object) => {
    this.formData = Object.keys(newFormData).reduce((acc, key) => {
      if (!this.excludeFieldsFromNewFormData.includes(key)) acc[key] = newFormData[key];
      return acc;
    }, {});
  };

  init = (cb: (onReactionAction: (nodeId: string) => Promise<void>) => IReactionDisposer) => {
    this.fetchFieldsReaction = cb(this.fetchFormFields);
  };

  disposeOtherFieldReactions = () => {
    if (this.otherFieldReactions && this.otherFieldReactions.length) {
      this.otherFieldReactions.forEach(reaction => reaction());
      this.otherFieldReactions = [];
    }
  };

  addFieldReaction = (reaction: IReactionDisposer) => this.otherFieldReactions.push(reaction);

  initEnumsModel = async () => {
    try {
      this.enumsModel = await this.singleRequestedEnums.getData();
    } catch (e) {
      this.setError("Ошибка загрузки формы");
      throw e;
    }
  };

  getModelByFieldName = (field: string): ParsedModelType => {
    const fieldValue = this.apiEditModel[field];

    switch (field) {
      /**
       * checkbox models
       * */
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
          false,
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
        this.addFieldReaction(
          reaction(
            () => mainCheckboxModel.value,
            (val: boolean) => {
              subComponentInput.isHide = !val;
            }
          )
        );
        mainCheckboxModel.subComponentOnCheck = subComponentInput;
        return mainCheckboxModel;
      case "SkipCData":
      case "LoadLikeImage":
        return new CheckboxParsedModel(field, field, fieldValue);
      case "IsReadOnly":
      case "LoadAllPlainFields":
        if (this.apiEditModel["IsFromDictionaries"]) return undefined;
        return new CheckboxParsedModel(field, field, fieldValue);
      case "FieldName":
      case "ContentName":
      case "DefaultCachePeriod":
        return new InputParsedModel(field, field, fieldValue);
      case "FieldTitle":
        if (_.isUndefined(fieldValue)) return undefined;
        return new InputParsedModel(
          field,
          this.settings.strings.FieldNameForCard,
          fieldValue,
          l("LabelText")
        );
      /**
       * text models
       * */
      case "RelateTo":
        if (!this.apiEditModel["RelatedContentName"] && !this.apiEditModel["RelatedContentId"])
          return undefined;
        return new TextParsedModel(
          field,
          fieldValue,
          `${this.apiEditModel["RelatedContentName"] || ""} ${this.apiEditModel[
            "RelatedContentId"
          ] || ""}`
        );
      case "FieldId":
      case "IsClassifier":
      case "ContentId":
        return new TextParsedModel(field, field, fieldValue);
      /**
       * textarea models
       * */
      case "RelationConditionDescription":
        return new TextAreaParsedModel(
          "RelationCondition",
          "RelationCondition",
          this.apiEditModel["RelationCondition"],
          {
            rows: 6,
            placeholder: fieldValue,
            style: { resize: "none", fontFamily: "monospace" }
          }
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
          }
        );
      /**
       * select models
       * */
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
          })
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
      });
    }
  };

  parseEditFormDataToUIModel = (model: IEditFormModel): ParsedModelType[] => {
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
  saveForm = async (nodeId): Promise<void> => {
    try {
      const formData = new FormData();
      formData.append("path", nodeId.charAt(0) === "/" ? nodeId : `/${nodeId}`);
      const validation = this.xmlEditorStore.validateXml();
      if (validation !== true) {
        throw validation;
      }
      formData.append("xml", this.xmlEditorStore.xml);
      Object.keys(this.formData).forEach(fieldKey => {
        formData.append(fieldKey, _.isNull(this.formData[fieldKey]) ? "" : this.formData[fieldKey]);
      });
      const newEditForm = await ApiService.saveField(formData);
      this.apiEditModel = newEditForm;
      this.initInDefinitionModel();
      this.UIEditModel = _.compact(this.parseEditFormDataToUIModel(newEditForm));
      this.xmlEditorStore.setXml(newEditForm.Xml);
    } catch (e) {
      this.setError("Ошибка сохранения формы");
      console.error(e);
    }
  };

  isEqualFormDataWithOriginalModel = (): boolean => {
    if (!this.formData) return true;
    const overlapFields = Object.keys(this.formData).reduce((acc, fieldKey) => {
      const formDataValue = this.formData[fieldKey];
      const modelValue = this.apiEditModel[fieldKey];
      if (formDataValue !== modelValue) acc.push(fieldKey);
      return acc;
    }, [] as string[]);
    return !overlapFields.length;
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
      this.formData = null;
      this.disposeOtherFieldReactions();
      this.initInDefinitionModel();
      this.UIEditModel = _.compact(this.parseEditFormDataToUIModel(editForm));
    } catch (e) {
      this.setError("Ошибка загрузки формы");
      console.log(e);
    }
  };
}
