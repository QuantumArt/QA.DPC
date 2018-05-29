import { types as t, getType, unprotect, onPatch, resolvePath, IModelType } from "mobx-state-tree";
import { IDisposer } from "mobx-state-tree/dist/utils";
import {
  isObject,
  isInteger,
  isIsoDateString,
  isString,
  isNumber,
  isBoolean
} from "Utils/TypeChecks";
import {
  StoreObject,
  StoreSnapshot,
  ArticleObject,
  ArticleSnapshot,
  FileType
} from "Models/EditorDataModels";
import {
  ContentSchema,
  isRelationField,
  isBackwardField,
  isExtensionField,
  FieldSchema,
  isEnumField
} from "Models/EditorSchemaModels";

export class DataContext {
  private _nextId = -1;
  private _patchListener: IDisposer = null;
  private _defaultSnapshots: { [content: string]: Object } = null;
  private _storeType: IModelType<StoreSnapshot, StoreObject> = null;
  public store: StoreObject = null;

  public initSchema(mergedSchemas: { [name: string]: ContentSchema }) {
    this._storeType = compileStoreType(mergedSchemas);
    this._defaultSnapshots = compileDefaultSnapshots(mergedSchemas);
    console.log({ snapshots: this._defaultSnapshots }); // TODO: remove
  }

  public initStore(storeSnapshot: StoreSnapshot) {
    this.store = this._storeType.create(storeSnapshot);
    // разрешаем изменения моделей из других сервисов и компонентов
    unprotect(this.store);
    // подписываемся на добавление новых элементов в дерево
    this._patchListener = onPatch(this.store, patch => {
      // console.log(patch);
      const { op, path } = patch;
      // проверяем, что элемент пришел из глубины дерева
      if ((op === "add" || op === "replace") && path.match(/\//g).length > 2) {
        const object = resolvePath(this.store, path);
        // проверяем, что элемент является сущностью с Id
        if (isObject(object) && isInteger(object.Id)) {
          // и добавляем его в соответствующую коллекцию
          this.store[getType(object).name].put(object);
        }
      }
    });
  }

  // TODO: возможно стоит добавлять объекты в store при создании, а не по патчам ?
  public createArticle<T extends ArticleObject = ArticleObject>(contentName: string): T {
    return this.getContentType(contentName).create({
      ...this._defaultSnapshots[contentName],
      Id: this._nextId--
    }) as T;
  }

  private getContentType(contentName: string): IModelType<ArticleSnapshot, ArticleObject> {
    const optionalType = this._storeType.properties[contentName];
    if (!optionalType) {
      throw new TypeError(`Content "${contentName}" is not defined in this Store schema`);
    }
    // @ts-ignore
    return optionalType.type.subType;
  }

  public dispose() {
    if (this._patchListener) {
      this._patchListener();
    }
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
        extContentModels[extName] = t.optional(t.model(extFieldModels), {});
      });
      // создаем анонимную модель словаря контентов-расширений
      fieldModels[`${field.FieldName}_Contents`] = t.optional(t.model(extContentModels), {});
    } else if (isBackwardField(field)) {
      // для BackwardRelation создаем массив ссылок на сущности
      // prettier-ignore
      fieldModels[field.FieldName] = t.optional(
        t.array(t.reference(t.late(() => getModel(field.Content)))), []
      );
    } else if (isRelationField(field)) {
      switch (field.FieldType) {
        case "M2MRelation":
        case "M2ORelation":
          // для M2MRelation и M2ORelation создаем массив ссылок на сущности
          // prettier-ignore
          fieldModels[field.FieldName] = t.optional(
            t.array(t.reference(t.late(() => getModel(field.Content)))), []
          );
          break;
        case "O2MRelation":
          // для O2MRelation создаем nullable ссылку на сущность
          fieldModels[field.FieldName] = t.maybe(
            t.reference(t.late(() => getModel(field.Content)))
          );
          break;
      }
    } else if (isEnumField(field)) {
      // создаем nullable строковое поле в виде enum
      fieldModels[field.FieldName] = t.maybe(
        t.enumeration(field.FieldName, field.Items.map(item => item.Value))
      );
    } else {
      switch (field.FieldType) {
        case "String":
        case "Textbox":
        case "VisualEdit":
        case "Classifier":
          fieldModels[field.FieldName] = t.maybe(t.string);
          break;
        case "Numeric":
        case "O2MRelation":
          fieldModels[field.FieldName] = t.maybe(t.number);
          break;
        case "Boolean":
          fieldModels[field.FieldName] = t.maybe(t.boolean);
          break;
        case "Date":
        case "Time":
        case "DateTime":
          fieldModels[field.FieldName] = t.maybe(t.Date);
          break;
        case "File":
        case "Image":
          fieldModels[field.FieldName] = t.maybe(FileType);
          break;
        case "DynamicImage":
        default:
          throw new Error(
            `Field "${field.FieldName}" has unsupported type FieldExactTypes.${field.FieldType}`
          );
      }
    }
  };

  Object.values(mergedSchemas)
    .filter(content => !content.ForExtension)
    .forEach(content => {
      const fieldModels = {
        Id: t.identifier(t.number),
        ContentName: t.optional(t.literal(content.ContentName), content.ContentName),
        Timestamp: t.maybe(t.Date)
      };
      // заполняем поля модели объекта
      Object.values(content.Fields).forEach(field => visitField(field, fieldModels));
      // создаем именованную модель объекта на основе полей
      contentModels[content.ContentName] = t.model(content.ContentName, fieldModels);
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
    } else if (!isBackwardField(field) && !isRelationField(field)) {
      switch (field.FieldType) {
        case "String":
        case "Textbox":
        case "VisualEdit":
        case "Classifier":
        case "StringEnum":
          if (isString(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case "Numeric":
          if (isNumber(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case "Boolean":
          if (isBoolean(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case "Date":
        case "Time":
        case "DateTime":
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
