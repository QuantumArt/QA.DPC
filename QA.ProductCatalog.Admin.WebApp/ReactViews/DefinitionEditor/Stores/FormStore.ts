import XmlEditorStore from "./XmlEditorStore";
import { action, observable, reaction, runInAction } from "mobx";
import ApiService from "DefinitionEditor/ApiService";
import { EnumBackendModel, IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { BackendEnumType } from "DefinitionEditor/Enums";
import {
  CheckboxParsedModel,
  getBackendEnumTypeByFieldName,
  InputParsedModel,
  isCheckboxParsedModel,
  ISingleRequestedData,
  ParsedModelType,
  SelectParsedModel,
  singleRequestedData,
  TextAreaParsedModel,
  TextParsedModel
} from "Shared/Utils";
import { OperationState } from "Shared/Enums";
import _, { keys, forIn, assign } from "lodash";
import { IReactionDisposer } from "mobx/lib/internal";
import { l } from "DefinitionEditor/Localization";

//TODO доделать:
// 2. сделать обработку ошибок на всех уровнях работы с формой
// 4. добавить локализацию
// 6. сохранение пока работает только с definitionfieldInfo. Для ContentInfo форм будет другой метод и соответствующая проверка.
export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {
    this.singleRequestedEnums = new singleRequestedData(ApiService.getSelectEnums);
    this.initEnumsModel();
  }

  @observable UIEditModel: { [key in string]: ParsedModelType };
  private singleRequestedEnums: ISingleRequestedData<
    { [key in BackendEnumType]: EnumBackendModel[] }
  >;
  private enumsModel: { [key in BackendEnumType]: EnumBackendModel[] };
  public finalFormData: {};
  private readonly excludeFieldsFromNewFormData: string[] = ["RelateTo"];

  @observable operationState: OperationState = OperationState.None;
  @observable isLeaveWithoutSaveDialog: boolean = false;
  @observable warningPopupOnExitCb: () => void;
  @observable errorText: string = null;

  onChangeNodeIdReaction: IReactionDisposer;
  otherFieldReactions: IReactionDisposer[] = [];

  @action
  resetErrorState = () => {
    this.operationState = OperationState.Success;
    this.errorText = null;
  };

  @action
  toggleLeaveWithoutSaveDialog = () => {
    this.isLeaveWithoutSaveDialog = !this.isLeaveWithoutSaveDialog;
  };

  setError = (errText?: string) => {
    runInAction(() => {
      this.operationState = OperationState.Error;
      this.errorText = errText ?? "Error";
    });
  };

  setFormData = (newFormData: object | null = null): void => {
    if (!newFormData) {
      this.finalFormData = newFormData;
      return;
    }
    this.finalFormData = Object.keys(newFormData).reduce((acc, key) => {
      if (!this.excludeFieldsFromNewFormData.includes(key)) acc[key] = newFormData[key];
      return acc;
    }, {});
  };

  init = (
    reactionCb: (onReactionAction: (nodeId: string) => Promise<void>) => IReactionDisposer
  ) => {
    this.onChangeNodeIdReaction = reactionCb(async (nodeId: string) => {
      this.disposeOtherFieldReactions();
      if (!this.enumsModel) await this.initEnumsModel();
      await this.fetchFormFields(nodeId);
    });
  };

  initEnumsModel = async () => {
    try {
      this.enumsModel = await this.singleRequestedEnums.getData();
    } catch (e) {
      this.setError("Ошибка загрузки формы");
      throw e;
    }
  };

  /**
   * @param deps - массив зависимостей содержащий name полей, которые не будут скрыты
   * @param reverseLogic - массив deps работает наоборот,
   * */
  hideUiFields = (deps: string[] = [], reverseLogic: boolean = false) => {
    forIn(this.UIEditModel, async model => {
      if (
        (!deps.includes(model.name) && !reverseLogic) ||
        (reverseLogic && deps.includes(model.name))
      ) {
        model?.toggleIsHide();
      }
    });
  };

  parseEditFormDataToUIModel = (model: IEditFormModel): { [key in string]: ParsedModelType } => {
    /**
     * исключение
     * если заполнено поле DefaultCachePeriod рендерим только  2 поля ниже
     * */
    if (model["DefaultCachePeriod"]) {
      const exceptionFields = ["InDefinition", "DefaultCachePeriod"];
      return this.getParsedUIModelFromApiFields(model, exceptionFields);
    }

    return this.getParsedUIModelFromApiFields(model);
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
      //TODO не уверен, что нужно отправлять  модель xml из xmlEditorStore т.к. мы работаем с формой, эта уже может быть отредактирована и не сохранена.
      formData.append("xml", this.xmlEditorStore.xml);
      keys(this.finalFormData).forEach(fieldKey => {
        formData.append(
          fieldKey,
          _.isNull(this.finalFormData[fieldKey]) ? "" : String(this.finalFormData[fieldKey])
        );
      });
      const newEditForm = await ApiService.saveField(formData);
      this.UIEditModel = this.parseEditFormDataToUIModel(newEditForm);
      this.xmlEditorStore.setXml(newEditForm.Xml);
    } catch (e) {
      this.setError("Ошибка сохранения формы");
      console.error(e);
    }
  };

  getSubModelsFromCheckboxModels = (): { [key in string]: ParsedModelType } => {
    return keys(this.UIEditModel).reduce((acc, fieldKey) => {
      const model = this.UIEditModel[fieldKey];
      if (isCheckboxParsedModel(model)) {
        if (model.subModel) acc[model.subModel.name] = model.subModel;
      }
      return acc;
    }, {});
  };

  isEqualFormDataWithOriginalModel = (): boolean => {
    if (!this.finalFormData) return true;
    const comparableModel = assign({}, this.UIEditModel, this.getSubModelsFromCheckboxModels());

    const overlapFields = keys(this.finalFormData).reduce((acc, fieldKey) => {
      const formDataValue = this.finalFormData[fieldKey];
      const modelValue = comparableModel[fieldKey]?.value;
      if (formDataValue !== modelValue) acc.push(fieldKey);
      return acc;
    }, [] as string[]);

    debugger;
    return !overlapFields.length;
  };

  @action
  fetchFormFields = async (nodeId: string): Promise<void> => {
    const formData = new FormData();
    formData.append("path", nodeId.charAt(0) === "/" ? nodeId : `/${nodeId}`);
    formData.append("xml", this.xmlEditorStore.xml);
    try {
      this.operationState = OperationState.Pending;
      const editForm = await ApiService.getEditForm(formData);
      this.UIEditModel = this.parseEditFormDataToUIModel(editForm);
      this.operationState = OperationState.Success;
    } catch (e) {
      this.setError("Ошибка загрузки формы");
      console.log(e);
    }
  };

  addFieldReaction = (reaction: IReactionDisposer) => this.otherFieldReactions.push(reaction);

  disposeOtherFieldReactions = () => {
    if (this.otherFieldReactions && this.otherFieldReactions.length) {
      this.otherFieldReactions.forEach(reaction => reaction());
      this.otherFieldReactions = [];
    }
  };

  getParsedUIModelFromApiFields = (
    fields: IEditFormModel,
    exceptionFields: string[] = []
  ): { [key in string]: ParsedModelType } => {
    const OnlyExceptionsFields = exceptionFields.reduce((model, field) => {
      model[field] = fields[field];
      return model;
    }, {});

    const fieldsModel = exceptionFields.length ? OnlyExceptionsFields : fields;
    return keys(fieldsModel).reduce((acc, field) => {
      const fieldValue = fields[field];

      switch (field) {
        /**
         * checkbox models
         * */
        case "InDefinition":
          if (
            !_.isNull(fields["IsFromDictionaries"]) &&
            !_.isUndefined(fields["IsFromDictionaries"])
          )
            return acc;
          acc[field] = new CheckboxParsedModel(field, l(field), fieldValue, () =>
            this.hideUiFields([field])
          );
          this.addFieldReaction(
            reaction(
              () => this.UIEditModel,
              () => {
                if (fieldValue === false) this.hideUiFields([field]);
              }
            )
          );
          break;
        case "CacheEnabled":
          const mainCheckboxModel = new CheckboxParsedModel(
            field,
            l("CacheSettings"),
            fieldValue,
            () => {
              subComponentInput.toggleIsHide();
            },
            l("ProceedCaching"),
            null,
            false,
            true
          );
          const subComponentInput = new InputParsedModel(
            "CachePeriod",
            null,
            fields["CachePeriod"],
            "",
            !mainCheckboxModel.value,
            true
          );
          mainCheckboxModel.subModel = subComponentInput;
          acc[field] = mainCheckboxModel;
          break;

        case "SkipCData":
          acc[field] = new CheckboxParsedModel(field, l("DontWrapInCData"), fieldValue);
          break;
        case "LoadLikeImage":
          acc[field] = new CheckboxParsedModel(field, l("LoadAsImage"), fieldValue);
          break;

        case "IsReadOnly":
        case "LoadAllPlainFields":
          if (fields["IsFromDictionaries"]) return acc;
          acc[field] = new CheckboxParsedModel(field, l(field), fieldValue);
          break;

        case "FieldName":
        case "ContentName":
        case "DefaultCachePeriod":
          acc[field] = new InputParsedModel(field, l(field), fieldValue);
          break;

        case "FieldTitle":
          if (_.isUndefined(fieldValue)) return acc;
          acc[field] = new InputParsedModel(
            field,
            l("FieldNameForCard"),
            fieldValue,
            l("LabelText")
          );
          break;

        /**
         * text models
         * */
        case "RelateTo":
          if (!fields["RelatedContentName"] && !fields["RelatedContentId"]) return acc;
          acc[field] = new TextParsedModel(
            field,
            fieldValue,
            `${fields["RelatedContentName"] || ""} ${fields["RelatedContentId"] || ""}`
          );
          break;

        case "FieldId":
        case "IsClassifier":
        case "ContentId":
          acc[field] = new TextParsedModel(field, l(field), fieldValue);
          break;

        /**
         * textarea models
         * */
        case "RelationConditionDescription":
          acc["RelationCondition"] = new TextAreaParsedModel(
            "RelationCondition",
            l("RelationCondition"),
            fields["RelationCondition"],
            {
              rows: 6,
              placeholder: fieldValue,
              style: { resize: "none", fontFamily: "monospace" }
            }
          );
          break;

        case "ClonePrototypeConditionDescription":
          acc["ClonePrototypeCondition"] = new TextAreaParsedModel(
            "ClonePrototypeCondition",
            l("ClonePrototypeCondition"),
            fields["ClonePrototypeCondition"],
            {
              rows: 6,
              placeholder: fieldValue,
              style: { resize: "none", fontFamily: "monospace" }
            }
          );
          break;

        /**
         * select models
         * */
        case "DeletingMode":
        case "UpdatingMode":
        case "CloningMode":
        case "PreloadingMode":
        case "PublishingMode":
          acc[field] = new SelectParsedModel(
            field,
            l(field),
            fieldValue,
            this.enumsModel[getBackendEnumTypeByFieldName(field)].map(option => {
              return {
                label: option.title,
                value: option.value
              };
            })
          );
          break;
        default:
          return acc;
      }
      return acc;
    }, {});
  };
}
