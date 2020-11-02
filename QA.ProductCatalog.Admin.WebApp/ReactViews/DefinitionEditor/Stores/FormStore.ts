import XmlEditorStore from "./XmlEditorStore";
import { action, observable, reaction, runInAction } from "mobx";
import ApiService from "DefinitionEditor/ApiService";
import { EnumBackendModel, IEditFormModel } from "DefinitionEditor/ApiService/ApiInterfaces";
import { BackendEnumType, ModelType } from "DefinitionEditor/Enums";
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
import { keys, forIn, assign, isNull, isUndefined } from "lodash";
import { IReactionDisposer } from "mobx/lib/internal";
import { l } from "DefinitionEditor/Localization";
import qs from "qs";

export default class FormStore {
  constructor(private settings: DefinitionEditorSettings, private xmlEditorStore: XmlEditorStore) {
    this.singleRequestedEnums = new singleRequestedData(ApiService.getSelectEnums);
    this.initEnumsModel();
  }

  @observable UIEditModel: { [key in string]: ParsedModelType };
  ModelType: ModelType;
  private singleRequestedEnums: ISingleRequestedData<
    { [key in BackendEnumType]: EnumBackendModel[] }
  >;
  private enumsModel: { [key in BackendEnumType]: EnumBackendModel[] };
  public finalFormData: {};
  subFieldsForSave: Partial<IEditFormModel> = {};

  @observable operationState: OperationState = OperationState.None;
  @observable errorText: string = null;

  onChangeNodeIdReaction: IReactionDisposer;

  resetErrorState = () => {
    runInAction(() => {
      this.operationState = OperationState.Success;
      this.errorText = null;
    });
  };

  setError = (errText?: string) => {
    runInAction(() => {
      this.operationState = OperationState.Error;
      this.errorText = errText ?? "Error";
    });
  };

  setFormData = (newFormData: object | null = null): void => {
    const excludeFieldsFromNewFormData: string[] = ["RelateTo", "IsClassifier"];
    if (!newFormData) {
      this.finalFormData = newFormData;
      return;
    }
    this.finalFormData = keys(newFormData).reduce((acc, key) => {
      if (!excludeFieldsFromNewFormData.includes(key)) acc[key] = newFormData[key];
      return acc;
    }, {});
  };

  init = (
    onChangeNodeIdReaction: (
      onReactionAction: (nodeId: string) => Promise<void>
    ) => IReactionDisposer
  ) => {
    this.onChangeNodeIdReaction = onChangeNodeIdReaction(async (nodeId: string) => {
      if (!this.enumsModel) await this.initEnumsModel();
      await this.fetchFormFields(nodeId);
    });
  };

  initEnumsModel = async () => {
    try {
      this.enumsModel = await this.singleRequestedEnums.getData();
    } catch (e) {
      this.setError(l("FormLoadError"));
      throw e;
    }
  };

  /**
   * @param deps - массив зависимостей содержащий name полей, которые не будут скрыты
   * @param reverseLogic - массив deps работает наоборот,
   * @param value - вместо противоположного значения будет установлено переданное,
   * */
  hideUiFields = (deps: string[] = [], reverseLogic: boolean = false, value?: boolean) => {
    forIn(this.UIEditModel, async model => {
      if (
        (!deps.includes(model.name) && !reverseLogic) ||
        (reverseLogic && deps.includes(model.name))
      ) {
        model?.toggleIsHide(value);
      }
    });
  };

  parseEditFormDataToUIModel = (model: IEditFormModel): { [key in string]: ParsedModelType } => {
    let exceptionFields = [];
    /**
     * исключение
     * если заполнено поле DefaultCachePeriod рендерим только поля exceptionFields
     * */
    if (model["DefaultCachePeriod"]) {
      exceptionFields = ["InDefinition", "DefaultCachePeriod"];
    }
    /**
     * исключение
     * если поле FieldType равняется нулю рендерим только поля exceptionFields
     * */
    if (model["FieldType"] === 0) {
      exceptionFields = [
        "InDefinition",
        "FieldId",
        "FieldName",
        "FieldTitle",
        "SkipCData",
        "LoadLikeImage"
      ];
    }

    return this.getParsedUIModelFromApiFields(model, exceptionFields);
  };

  getXmlAndPathObj = (nodeId: string): { xml: string; path: string } => {
    return {
      xml: this.xmlEditorStore.xml,
      path: nodeId.charAt(0) === "/" ? nodeId : `/${nodeId}`
    };
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
      const formDataValue =
        this.finalFormData[fieldKey] === "" ? null : this.finalFormData[fieldKey];
      const modelValue = comparableModel[fieldKey]?.value;
      if (formDataValue !== modelValue) acc.push(fieldKey);
      return acc;
    }, [] as string[]);

    return !overlapFields.length;
  };

  isFormTheSame = (): boolean => {
    if (this.isEqualFormDataWithOriginalModel()) {
      this.setError(l("SameForm"));
      return true;
    }
    return false;
  };

  setModelTypeByFieldType = (fieldType: number | undefined): void => {
    if (!isNull(fieldType) && !isUndefined(fieldType)) {
      this.ModelType = ModelType.Field;
    } else {
      this.ModelType = ModelType.Content;
    }
  };

  getApiMethodByModelType = (): ((body: string) => Promise<IEditFormModel>) => {
    if (this.ModelType === ModelType.Field) return ApiService.saveField;
    if (this.ModelType === ModelType.Content) return ApiService.saveContent;
    return null;
  };

  setSubFieldsForSave = (fields: Partial<IEditFormModel>): void => {
    keys(fields).map(field => {
      const value = fields[field];
      if (!isNull(value) && !isUndefined(value)) this.subFieldsForSave[field] = value;
    });
  };

  @action
  fetchFormFields = async (nodeId: string): Promise<void> => {
    try {
      this.operationState = OperationState.Pending;
      const editForm = await ApiService.getEditForm(qs.stringify(this.getXmlAndPathObj(nodeId)));
      this.setModelTypeByFieldType(editForm?.FieldType);
      this.setSubFieldsForSave({ FieldType: editForm?.FieldType });
      this.UIEditModel = this.parseEditFormDataToUIModel(editForm);
      this.operationState = OperationState.Success;
    } catch (e) {
      this.setError(l("FormLoadError"));
      console.error(e);
    }
  };

  @action
  saveForm = async (nodeId: string): Promise<void> => {
    try {
      this.operationState = OperationState.Pending;
      const newEditForm = await this.getApiMethodByModelType()(
        qs.stringify(
          assign({}, this.finalFormData, this.getXmlAndPathObj(nodeId), this.subFieldsForSave)
        )
      );
      this.UIEditModel = this.parseEditFormDataToUIModel(newEditForm);
      this.xmlEditorStore.setXml(newEditForm.Xml);
      this.xmlEditorStore.setLastLocalSavedXml(newEditForm.Xml);
      this.operationState = OperationState.Success;
    } catch (e) {
      this.setError(l("FormSaveError"));
      console.error(e);
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
    const hideIfInDefinition =
      isUndefined(fieldsModel["InDefinition"]) || isNull(fieldsModel["InDefinition"])
        ? false
        : !fieldsModel["InDefinition"];
    return keys(fieldsModel).reduce((acc, field) => {
      const fieldValue = fields[field];

      switch (field) {
        /**
         * checkbox models
         * */
        case "InDefinition":
          acc[field] = new CheckboxParsedModel(field, l(field), fieldValue, () =>
            this.hideUiFields([field])
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
            hideIfInDefinition,
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
          acc[field] = new CheckboxParsedModel(
            field,
            l("DontWrapInCData"),
            fieldValue,
            null,
            "",
            null,
            hideIfInDefinition
          );
          break;
        case "LoadLikeImage":
          acc[field] = new CheckboxParsedModel(
            field,
            l("LoadAsImage"),
            fieldValue,
            null,
            "",
            null,
            hideIfInDefinition
          );
          break;

        case "IsReadOnly":
        case "LoadAllPlainFields":
          if (fields["IsFromDictionaries"]) return acc;
          acc[field] = new CheckboxParsedModel(
            field,
            l(field),
            fieldValue,
            null,
            "",
            null,
            hideIfInDefinition
          );
          break;

        case "FieldName":
        case "ContentName":
        case "DefaultCachePeriod":
          acc[field] = new InputParsedModel(field, l(field), fieldValue, "", hideIfInDefinition);
          break;
        case "VirtualPath":
          acc[field] = new InputParsedModel(field, l("Path"), fieldValue, "", hideIfInDefinition);
          break;

        case "FieldTitle":
          acc[field] = new InputParsedModel(
            field,
            l("FieldNameForCard"),
            fieldValue,
            l("LabelText"),
            hideIfInDefinition
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
            `${fields["RelatedContentName"] || ""} ${fields["RelatedContentId"] || ""}`,
            hideIfInDefinition
          );
          break;

        case "FieldId":
          acc[field] = new TextParsedModel(field, l(field), fieldValue, hideIfInDefinition);
          break;

        case "IsClassifier":
        case "ContentId":
          acc[field] = new TextParsedModel(field, field, fieldValue, hideIfInDefinition);
          break;

        /**
         * textarea models
         * */
        case "RelationCondition":
          if (fields["FieldType"] === 3 || fields["FieldType"] === 7) break;
          acc[field] = new TextAreaParsedModel(
            field,
            l(field),
            fieldValue,
            {
              rows: 6,
              placeholder: l("RelationConditionDescription"),
              style: { resize: "none", fontFamily: "monospace" }
            },
            hideIfInDefinition
          );
          break;

        case "ClonePrototypeCondition":
          if (fields["FieldType"] === 3 || fields["FieldType"] === 7) break;
          acc[field] = new TextAreaParsedModel(
            field,
            l(field),
            fieldValue,
            {
              rows: 6,
              placeholder: l("ClonePrototypeConditionDescription"),
              style: { resize: "none", fontFamily: "monospace" }
            },
            hideIfInDefinition
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
            }),
            hideIfInDefinition
          );
          break;
        default:
          return acc;
      }
      return acc;
    }, {});
  };
}
