import {
  types as t,
  getType,
  unprotect,
  onPatch,
  resolvePath,
  IExtendedObservableMap,
  IModelType
} from "mobx-state-tree";
import { IDisposer } from "mobx-state-tree/dist/utils";
import { isObject, isInteger } from "Utils/TypeChecks";
import { StoreSnapshot, ArticleSnapshot, FileType } from "Models/EditorDataModels";
import {
  ContentSchema,
  isRelationField,
  isBackwardField,
  isExtensionField,
  FieldSchema,
  isEnumField
} from "Models/EditorSchemaModels";

interface Store {
  [name: string]: IExtendedObservableMap<ArticleSnapshot>;
}

export class DataContext {
  private _nextId = -1;
  private _patchListener: IDisposer = null;
  private _storeType: IModelType<any, any> = null;
  public store: Store = null;

  public initSchema(mergedSchemas: { [name: string]: ContentSchema }) {
    this._storeType = compileStoreType(mergedSchemas);
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
  public createArticle<T = ArticleSnapshot>(contentName: string): T {
    return this.getContentType(contentName).create({ Id: this._nextId-- });
  }

  private getContentType(contentName: string): IModelType<any, any> {
    const optionalType = this._storeType.properties[contentName];
    if (!optionalType) {
      throw new Error(`Content "${contentName}" is not defined in this Store schema`);
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

export default new DataContext();

/**
 * Компилирует MobX-модели из нормализованных схем контентов
 */
function compileStoreType(mergedSchemas: { [name: string]: ContentSchema }) {
  const contentModels = {};
  const getName = (content: ContentSchema) => mergedSchemas[content.ContentId].ContentName;
  const getFields = (content: ContentSchema) => mergedSchemas[content.ContentId].Fields;
  const getModel = (content: ContentSchema) => contentModels[getName(content)];

  const visitField = (field: FieldSchema, fieldModels: any) => {
    if (isExtensionField(field)) {
      // создаем nullable поле-классификатор в виде enum
      fieldModels[field.FieldName] = t.maybe(
        t.enumeration(field.FieldName, Object.keys(field.Contents))
      );
      // создаем словарь с моделями контентов-расширений
      const extContentModels = {} as any;
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
        case "Image":
        case "Textbox":
        case "VisualEdit":
        case "DynamicImage":
        case "Classifier":
          fieldModels[field.FieldName] = t.maybe(t.string);
          break;
        case "Numeric":
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
          fieldModels[field.FieldName] = t.maybe(FileType);
          break;
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

      Object.values(content.Fields).forEach(field => visitField(field, fieldModels));

      contentModels[content.ContentName] = t.model(content.ContentName, fieldModels);
    });

  const collectionModels = {};
  Object.entries(contentModels).forEach(([name, model]: [string, any]) => {
    collectionModels[name] = t.optional(t.map(model), {});
  });

  return t.model(collectionModels);
}
