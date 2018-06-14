import { types as t, unprotect, IModelType } from "mobx-state-tree";
import { isNumber, isString, isIsoDateString, isBoolean } from "Utils/TypeChecks";
import {
  StoreObject,
  StoreSnapshot,
  ArticleObject,
  ArticleSnapshot
} from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  FieldExactTypes,
  isExtensionField,
  isMultiRelationField,
  isSingleRelationField,
  isPlainField,
  isEnumField
} from "Models/EditorSchemaModels";
import { validatableMixin } from "Models/ValidatableMixin";

export class DataContext {
  private _nextId = -1;
  private _defaultSnapshots: { [content: string]: Object } = null;
  private _storeType: IModelType<StoreSnapshot, StoreObject> = null;
  public store: StoreObject = null;

  public initSchema(mergedSchemas: { [name: string]: ContentSchema }) {
    this._storeType = compileStoreType(mergedSchemas);
    this._defaultSnapshots = compileDefaultSnapshots(mergedSchemas);
  }

  public initStore(storeSnapshot: StoreSnapshot) {
    this.store = this._storeType.create(storeSnapshot);
    // разрешаем изменения моделей из других сервисов и компонентов
    unprotect(this.store);
  }

  public createArticle<T extends ArticleObject = ArticleObject>(contentName: string): T {
    const article = this.getContentType(contentName).create({
      ...this._defaultSnapshots[contentName],
      Id: this._nextId--
    }) as T;

    this.store[contentName].put(article);
    return article;
  }

  private getContentType(contentName: string): IModelType<ArticleSnapshot, ArticleObject> {
    const optionalType = this._storeType.properties[contentName];
    if (!optionalType) {
      throw new TypeError(`Content "${contentName}" is not defined in this Store schema`);
    }
    // @ts-ignore
    return optionalType.type.subType;
  }
}

/**
 * Компилирует MobX-модели из нормализованных схем контентов
 * @example
 *
 * interface Store {
 *   Product: Map<string, Product>;
 *   Region: Map<string, Region>;
 * }
 * interface Product {
 *   Id: number;
 *   Description: string;
 *   Regions: Region[];
 * }
 * interface Region {
 *   Id: number;
 *   Title: string;
 *   Parent: Region;
 * }
 */
function compileStoreType(mergedSchemas: { [name: string]: ContentSchema }) {
  // создаем словарь с моделями основных контентов
  const contentModels = {};

  const getName = (content: ContentSchema) => mergedSchemas[content.ContentId].ContentName;
  const getFields = (content: ContentSchema) => mergedSchemas[content.ContentId].Fields;
  const getModel = (content: ContentSchema) => contentModels[getName(content)];

  const visitField = (field: FieldSchema, fieldModels: Object) => {
    if (isExtensionField(field)) {
      // создаем nullable поле-классификатор в виде enum
      fieldModels[field.FieldName] = t.maybe(
        t.enumeration(field.FieldName, Object.keys(field.Contents))
      );
      // создаем словарь с моделями контентов-расширений
      const extContentModels: Object = {};
      // заполняем его, обходя field.Contents
      Object.entries(field.Contents).forEach(([extName, extContent]) => {
        // для каждого контента-расширения создаем словарь его полей
        const extFieldModels = {
          ContentName: t.optional(t.literal(extName), extName)
        };
        // заполняем словарь полей обходя все поля контента-расширения
        Object.values(getFields(extContent)).forEach(field => visitField(field, extFieldModels));
        // создаем анонимную модель контента-расширения
        // prettier-ignore
        extContentModels[extName] = t.optional(
          t.model(extFieldModels).extend(validatableMixin), {}
        );
      });
      // создаем анонимную модель словаря контентов-расширений
      fieldModels[`${field.FieldName}_Contents`] = t.optional(t.model(extContentModels), {});
    } else if (isSingleRelationField(field)) {
      // для O2MRelation создаем nullable ссылку на сущность
      fieldModels[field.FieldName] = t.maybe(t.reference(t.late(() => getModel(field.Content))));
    } else if (isMultiRelationField(field)) {
      // для M2MRelation и M2ORelation создаем массив ссылок на сущности
      // prettier-ignore
      fieldModels[field.FieldName] = t.optional(
        t.array(t.reference(t.late(() => getModel(field.Content)))), []
      );
    } else if (isEnumField(field)) {
      // создаем nullable строковое поле в виде enum
      fieldModels[field.FieldName] = t.maybe(
        t.enumeration(field.FieldName, field.Items.map(item => item.Value))
      );
    } else if (isPlainField(field)) {
      switch (field.FieldType) {
        case FieldExactTypes.String:
        case FieldExactTypes.Textbox:
        case FieldExactTypes.VisualEdit:
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
        case FieldExactTypes.Classifier:
          fieldModels[field.FieldName] = t.maybe(t.string);
          break;
        case FieldExactTypes.Numeric:
          fieldModels[field.FieldName] = t.maybe(t.number);
          break;
        case FieldExactTypes.Boolean:
          fieldModels[field.FieldName] = t.maybe(t.boolean);
          break;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          fieldModels[field.FieldName] = t.maybe(t.Date);
          break;
      }
    }
    if (!fieldModels[field.FieldName]) {
      throw new Error(
        `Field "${field.FieldName}" has unsupported ClassName ${
          field.ClassNames.slice(-1)[0]
        } or FieldType FieldExactTypes.${field.FieldType}`
      );
    }
  };

  Object.values(mergedSchemas)
    .filter(content => !content.ForExtension)
    .forEach(content => {
      const fieldModels = {
        Id: t.identifier(t.number),
        ContentName: t.optional(t.literal(content.ContentName), content.ContentName),
        Modified: t.maybe(t.Date)
      };
      // заполняем поля модели объекта
      Object.values(content.Fields).forEach(field => visitField(field, fieldModels));
      // создаем именованную модель объекта на основе полей
      contentModels[content.ContentName] = t
        .model(content.ContentName, fieldModels)
        .extend(validatableMixin);
    });

  const collectionModels = {};
  // заполняем словарь с моделями коллекций
  Object.entries(contentModels).forEach(([name, model]: [string, any]) => {
    collectionModels[name] = t.optional(t.map(model), {});
  });
  // создаем модель хранилища
  return t.model(collectionModels);
}

/**
 * Создает словарь со снапшотами-по умолчанию для создания новых статей.
 * Заполняет его на основе `FieldSchema.DefaultValue`.
 * @example
 *
 * {
 *   Product: {
 *     Type: "InternetTariff",
 *     Type_Contents: {
 *       InternetTariff: {
 *         Description: "Тариф интернет"
 *       }
 *     }
 *   },
 *   Region: {
 *     Title: "Москва"
 *   }
 * }
 */
function compileDefaultSnapshots(mergedSchemas: { [name: string]: ContentSchema }) {
  const getFields = (content: ContentSchema) => mergedSchemas[content.ContentId].Fields;

  const visitField = (field: FieldSchema, fieldValues: Object) => {
    if (isExtensionField(field)) {
      // заполняем значение по-умолчанию для поля-классификатора
      if (isString(field.DefaultValue)) {
        fieldValues[field.FieldName] = field.DefaultValue;
      }
      // создаем словарь со снапшотами по-умолчанию контентов-расширений
      const extContentSnapshots = {};
      // заполняем его, обходя field.Contents
      Object.entries(field.Contents).forEach(([extName, extContent]) => {
        // для каждого контента-расширения создаем словарь значений его полей
        const extFieldValues = {};
        // заполняем словарь полей обходя все поля контента-расширения
        Object.values(getFields(extContent)).forEach(field => visitField(field, extFieldValues));
        // запоминаем объект с полями контента, если они определены
        if (Object.keys(extFieldValues).length > 0) {
          extContentSnapshots[extName] = extFieldValues;
        }
      });
      // запоминаем объект со со снапшотами контентов-расширений, если они определены
      if (Object.keys(extContentSnapshots).length > 0) {
        fieldValues[`${field.FieldName}_Contents`] = extContentSnapshots;
      }
    } else if (isPlainField(field)) {
      switch (field.FieldType) {
        case FieldExactTypes.String:
        case FieldExactTypes.Textbox:
        case FieldExactTypes.VisualEdit:
        case FieldExactTypes.Classifier:
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
        case FieldExactTypes.StringEnum:
          if (isString(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Numeric:
          if (isNumber(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Boolean:
          if (isBoolean(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          if (isIsoDateString(field.DefaultValue)) {
            fieldValues[field.FieldName] = new Date(field.DefaultValue);
          }
          break;
      }
    }
  };

  const contentSnapshots = {};
  // заполняем словарь со снапшотами по-умолчанию для основных контентов
  Object.values(mergedSchemas)
    .filter(content => !content.ForExtension)
    .forEach(content => {
      const fieldValues = {};
      Object.values(content.Fields).forEach(field => visitField(field, fieldValues));
      contentSnapshots[content.ContentName] = fieldValues;
    });

  return contentSnapshots;
}
