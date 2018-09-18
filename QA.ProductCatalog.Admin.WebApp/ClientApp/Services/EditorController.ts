import { inject } from "react-ioc";
import { EntitySnapshot, EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EditorSettings } from "Models/EditorSettings";
import { DataSerializer, IdMapping } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataValidator, ArticleErrors } from "Services/DataValidator";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { command } from "Utils/Command";

export class EditorController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataValidator: DataValidator;
  @inject private _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;

  private _query = document.location.search;
  private _rootUrl = document.head.getAttribute("root-url") || "";

  @command
  public async initialize() {
    const initSchemaTask = this.initSchema();

    if (this._editorSettings.ArticleId > 0) {
      const response = await fetch(
        `${this._rootUrl}/ProductEditor/GetEditorData${this._query}&productDefinitionId=${
          this._editorSettings.ProductDefinitionId
        }&articleId=${this._editorSettings.ArticleId}`
      );
      if (!response.ok) {
        throw new Error(await response.text());
      }
      const nestedObjectTree = this._dataSerializer.deserialize<EntitySnapshot>(
        await response.text()
      );

      await initSchemaTask;
      const contentName = this._schemaContext.rootSchema.ContentName;

      const flatObjectsByContent = this._dataNormalizer.normalize(nestedObjectTree, contentName);

      this._dataContext.initStore(flatObjectsByContent);
      return this._dataContext.store[contentName].get(String(nestedObjectTree._ClientId));
    } else {
      await initSchemaTask;
      const contentName = this._schemaContext.rootSchema.ContentName;

      this._dataContext.initStore({});
      return this._dataContext.createArticle(contentName);
    }
  }

  private async initSchema() {
    const response = await fetch(
      `${this._rootUrl}/ProductEditor/GetEditorSchema${this._query}&productDefinitionId=${
        this._editorSettings.ProductDefinitionId
      }`
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const schema = await response.json();
    this._schemaContext.initSchema(schema.EditorSchema, schema.MergedSchemas);
    this._dataContext.initSchema(this._schemaContext.contentSchemasById);
    this._dataNormalizer.initSchema(this._schemaContext.contentSchemasById);
  }

  @command
  public async savePartialProduct(article: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.validate(article, contentSchema);
    if (errors.length > 0) {
      window.alert(this.getErrorMessage(errors));
      return;
    }

    const partialProduct = this._dataSerializer.serialize(article, contentSchema);

    const response = await fetch(
      `${this._rootUrl}/ProductEditor/SavePartialProduct${this._query}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          ProductDefinitionId: this._editorSettings.ProductDefinitionId,
          ContentPath: contentSchema.ContentPath,
          PartialProduct: partialProduct
        })
      }
    );
    if (response.status === 409) {
      const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      if (this._dataMerger.storeHasConflicts(dataSnapshot)) {
        // TODO: React confirm dialog
        const serverWins = window.confirm(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Применить изменения с сервера?`
        );
        if (serverWins) {
          this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.ServerWins);
        } else {
          this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.ClientWins);
        }
        // TODO: React alert dialog
        window.alert(`Пожалуйста, проверьте корректность данных и сохраните статью снова.`);
      } else {
        this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.ClientWins);
        // TODO: React alert dialog
        window.alert(
          `Данные на сервере были изменены другим пользователем.\n` +
            `Пожалуйста, проверьте корректность данных и сохраните статью снова.`
        );
      }
      return;
    }
    if (!response.ok) {
      // TODO: React alert dialog
      window.alert("При сохранении произошла ошибка!");
      throw new Error(await response.text());
    }
    const okResponse: {
      IdMappings: IdMapping[];
      PartialProduct: Object;
    } = await response.json();

    this._dataSerializer.extendIdMappings(okResponse.IdMappings);

    const dataTreeJson = JSON.stringify(okResponse.PartialProduct);
    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(dataTreeJson);
    const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);
    this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.ServerWins);
  }

  private getErrorMessage(articleErrors: ArticleErrors[]) {
    return articleErrors
      .slice(0, 3)
      .map(
        articleError =>
          `${articleError.ContentName}: ${articleError.ServerId}\n` +
          (articleError.ArticleErrors.length > 0
            ? `${articleError.ArticleErrors.join(", ")}\n`
            : ``) +
          (articleError.FieldErrors.length > 0
            ? `${articleError.FieldErrors.map(
                fieldError => `${fieldError.Name}: ${fieldError.Messages.join(", ")}`
              )}\n`
            : ``)
      )
      .join("\n");
  }
}
