import { inject } from "react-ioc";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import {
  ContentSchema,
  isExtensionField,
  isRelationField,
  RelationFieldSchema
} from "Models/EditorSchemaModels";
import { StoreSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import { Mutable } from "Utils/TypeChecks";

export class DataSchemaLinker {
  @inject private _dataContext: DataContext;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataSerializer: DataSerializer;

  public addPreloadedArticlesToSnapshot(
    storeSnapshot: Mutable<StoreSnapshot>,
    contentSchema: ContentSchema
  ) {
    const objectsByContent: {
      [contentName: string]: EntitySnapshot[];
    } = {};

    this.visitContentSchema(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadedArticles.length > 0) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const objects = objectsByContent[contentName] || (objectsByContent[contentName] = []);
        const articleObjects = this._dataSerializer.deserialize<EntitySnapshot[]>(
          fieldSchema.PreloadedArticles
        );
        objects.push(...articleObjects);
      }
    });

    const preloadedStoreSnapshot = this._dataNormalizer.normalizeStore(objectsByContent);

    Object.entries(preloadedStoreSnapshot).forEach(([contentName, preloadedArticlesById]) => {
      const articlesById = storeSnapshot[contentName];
      if (articlesById) {
        Object.entries(preloadedArticlesById).forEach(([id, article]) => {
          if (!articlesById[id]) {
            articlesById[id] = article;
          }
        });
      } else {
        storeSnapshot[contentName] = preloadedArticlesById;
      }
    });
  }

  public linkPreloadedArticles(contentSchema: ContentSchema) {
    this.visitContentSchema(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadedArticles.length > 0) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const entitiesMap = this._dataContext.store[contentName];
        // @ts-ignore
        fieldSchema.PreloadedArticles = fieldSchema.PreloadedArticles.map(article =>
          entitiesMap.get(String(article._ServerId))
        );
      }
    });
  }

  private visitContentSchema(
    contentSchema: ContentSchema,
    action: (fieldSchema: RelationFieldSchema) => void,
    visited = new Set<ContentSchema>()
  ) {
    if (visited.has(contentSchema)) {
      return;
    }
    visited.add(contentSchema);
    Object.values(contentSchema.Fields).forEach(fieldSchema => {
      if (isRelationField(fieldSchema)) {
        action(fieldSchema);
        this.visitContentSchema(fieldSchema.RelatedContent, action, visited);
      } else if (isExtensionField(fieldSchema)) {
        Object.values(fieldSchema.ExtensionContents).forEach(extensionSchema => {
          this.visitContentSchema(extensionSchema, action, visited);
        });
      }
    });
  }
}
