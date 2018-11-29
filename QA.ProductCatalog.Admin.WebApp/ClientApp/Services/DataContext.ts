import { action } from "mobx";
import {
  types as t,
  unprotect,
  IModelType,
  onPatch,
  ModelProperties,
  getType,
  getSnapshot
} from "mobx-state-tree";
import { validationMixin } from "mst-validation-mixin";
import { isNumber, isString, isIsoDateString, isBoolean } from "Utils/TypeChecks";
import {
  TablesObject,
  TablesSnapshot,
  EntityObject,
  EntitySnapshot,
  ArticleObject
} from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  FieldExactTypes,
  ContentSchemasById,
  isExtensionField,
  isMultiRelationField,
  isSingleRelationField,
  isPlainField,
  isEnumField
} from "Models/EditorSchemaModels";

type ModelType<S, T> = IModelType<ModelProperties, any, S, S, T>;

export class DataContext<TTables extends TablesObject = TablesObject> {
  private _nextId = -1;
  private _defaultSnapshots: DefaultSnapshots = null;
  private _tablesType: ModelType<TablesSnapshot, TablesObject> = null;
  /** Набор таблиц статей-сущностей. Каждая таблица — ES6 Map */
  public tables: TTables = null;

  public initSchema(mergedSchemas: ContentSchemasById) {
    this._tablesType = compileTablesType(mergedSchemas, () => this._nextId--);
    this._defaultSnapshots = compileDefaultSnapshots(mergedSchemas);
  }

  public initTables(tablesSnapshot: TablesSnapshot) {
    this.tables = this._tablesType.create(tablesSnapshot) as TTables;

    // разрешаем изменения моделей из других сервисов и компонентов
    unprotect(this.tables);

    if (DEBUG) {
      // отладочный вывод в консоль
      const tables = this.tables;
      class SnapshotView {
        get snapshot() {
          return getSnapshot(tables);
        }
      }
      const isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
      if (isChrome) {
        onPatch(this.tables, patch => console.log(patch, new SnapshotView()));
      } else {
        onPatch(this.tables, patch => console.log(patch));
      }
    }
  }

  /**
   * Создать статью в указанном конетнте.
   * @param contentName Имя контента
   * @param properties Значения полей статьи.
   */
  @action
  public createEntity<T extends EntityObject = EntityObject>(
    contentName: string,
    properties?: { [key: string]: any }
  ): T {
    const entity = this.getContentType(contentName).create({
      _ClientId: this._nextId,
      ...this._defaultSnapshots[contentName],
      ...properties
    }) as T;

    this.tables[contentName].put(entity);
    return entity;
  }

  /**
   * Удалить статью. Статья удаляется только из памяти и НЕ УДАЛЯЕТСЯ НА СЕРВЕРЕ.
   * @param entity Статья для удаления
   */
  @action
  public deleteEntity(entity: EntityObject) {
    const contentName = getType(entity).name;
    this.tables[contentName].delete(String(entity._ClientId));
  }

  private getContentType(contentName: string): ModelType<EntitySnapshot, EntityObject> {
    const optionalType = this._tablesType.properties[contentName];
    if (!optionalType) {
      throw new TypeError(`Content "${contentName}" is not defined in this Tables schema`);
    }
    // @ts-ignore
    return optionalType.type.subType;
  }
}

/**
 * Компилирует MobX-модели из нормализованных схем контентов
 * @example
 *
 * interface Tables {
 *   Product: Map<string, Product>;
 *   Region: Map<string, Region>;
 * }
 * interface Product {
 *   _ClientId: number;
 *   Description: string;
 *   Regions: Region[];
 * }
 * interface Region {
 *   _ClientId: number;
 *   Title: string;
 *   Parent: Region;
 * }
 */
function compileTablesType(
  mergedSchemas: { [name: string]: ContentSchema },
  getNextId: () => number
) {
  // создаем словарь с моделями основных контентов
  const contentModels = {};

  const visitField = (field: FieldSchema, fieldModels: Object) => {
    if (isExtensionField(field)) {
      // создаем nullable поле-классификатор в виде enum
      fieldModels[field.FieldName] = t.maybeNull(
        t.enumeration(field.FieldName, Object.keys(field.ExtensionContents))
      );
      // создаем словарь с моделями контентов-расширений
      const extContentModels: { [x: string]: any } = {};
      // заполняем его, обходя field.Contents
      Object.entries(field.ExtensionContents).forEach(([extName, extContent]) => {
        // для каждого контента-расширения создаем словарь его полей
        const extFieldModels = {
          _ServerId: t.optional(t.number, getNextId),
          _Modified: t.maybeNull(t.Date),
          _IsExtension: t.optional(t.literal(true), true)
        };
        // заполняем словарь полей обходя все поля контента-расширения
        Object.values(extContent.Fields).forEach(field => visitField(field, extFieldModels));
        // создаем анонимную модель контента-расширения
        // prettier-ignore
        extContentModels[extName] = t.optional(
          t.model(extFieldModels).extend(validationMixin), {}
        );
      });
      // создаем анонимную модель словаря контентов-расширений
      // prettier-ignore
      fieldModels[`${field.FieldName}${ArticleObject._Extension}`] = t.optional(
        t.model(extContentModels), {}
      );
    } else if (isSingleRelationField(field)) {
      // для O2MRelation создаем nullable ссылку на сущность
      fieldModels[field.FieldName] = t.maybeNull(
        t.reference(t.late(() => contentModels[field.RelatedContent.ContentName]))
      );
    } else if (isMultiRelationField(field)) {
      // для M2MRelation и M2ORelation создаем массив ссылок на сущности
      // prettier-ignore
      fieldModels[field.FieldName] = t.optional(
        t.array(t.reference(t.late(() => contentModels[field.RelatedContent.ContentName]))), []
      );
    } else if (isEnumField(field)) {
      // создаем nullable строковое поле в виде enum
      fieldModels[field.FieldName] = t.maybeNull(
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
          fieldModels[field.FieldName] = t.maybeNull(t.string);
          break;
        case FieldExactTypes.Numeric:
          fieldModels[field.FieldName] = t.maybeNull(t.number);
          break;
        case FieldExactTypes.Boolean:
          fieldModels[field.FieldName] = t.maybeNull(t.boolean);
          break;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          fieldModels[field.FieldName] = t.maybeNull(t.Date);
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
        _ClientId: t.identifierNumber,
        _ServerId: t.optional(t.number, getNextId),
        _Modified: t.maybeNull(t.Date),
        _IsExtension: t.optional(t.literal(false), false),
        _IsVirtual: t.optional(t.boolean, false)
      };
      // заполняем поля модели объекта
      Object.values(content.Fields).forEach(field => visitField(field, fieldModels));
      // создаем именованную модель объекта на основе полей
      contentModels[content.ContentName] = t
        .model(content.ContentName, fieldModels)
        .extend(validationMixin);
    });

  const collectionModels = {};
  // заполняем словарь с моделями коллекций
  Object.entries(contentModels).forEach(([name, model]: [string, any]) => {
    collectionModels[name] = t.optional(t.map(model), {});
  });
  // создаем модель хранилища
  return t.model(collectionModels);
}

/** словарь со снапшотами-по умолчанию для создания новых статей */
interface DefaultSnapshots {
  readonly [contentName: string]: Readonly<Object>;
}

/**
 * Создает словарь со снапшотами-по умолчанию для создания новых статей.
 * Заполняет его на основе `FieldSchema.DefaultValue`.
 * @example
 *
 * {
 *   Product: {
 *     Type: "InternetTariff",
 *     Type_Extension: {
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
      Object.entries(field.ExtensionContents).forEach(([extName, extContent]) => {
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
        fieldValues[`${field.FieldName}${ArticleObject._Extension}`] = extContentSnapshots;
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
