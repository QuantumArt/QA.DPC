import { inject } from "react-ioc";
import { extendObservable } from "mobx";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import {
  ContentSchema,
  isExtensionField,
  isRelationField,
  RelationFieldSchema,
  PreloadingMode,
  PreloadingState
} from "Models/EditorSchemaModels";
import { TablesSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import { Mutable } from "Utils/TypeChecks";

export class DataSchemaLinker {
  @inject private _dataContext: DataContext;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataSerializer: DataSerializer;

  public addPreloadedArticlesToSnapshot(
    tablesSnapshot: Mutable<TablesSnapshot>,
    contentSchema: ContentSchema
  ) {
    const objectsByContent: {
      [contentName: string]: EntitySnapshot[];
    } = {};

    this.visitContentSchema(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const objects = objectsByContent[contentName] || (objectsByContent[contentName] = []);
        const articleObjects = this._dataSerializer.deserialize<EntitySnapshot[]>(
          fieldSchema.PreloadedArticles
        );
        objects.push(...articleObjects);
      }
    });

    const preloadedTablesSnapshot = this._dataNormalizer.normalizeTables(objectsByContent);

    Object.entries(preloadedTablesSnapshot).forEach(([contentName, preloadedArticlesById]) => {
      const articlesById = tablesSnapshot[contentName];
      if (articlesById) {
        Object.entries(preloadedArticlesById).forEach(([id, article]) => {
          if (!articlesById[id]) {
            articlesById[id] = article;
          }
        });
      } else {
        tablesSnapshot[contentName] = preloadedArticlesById;
      }
    });
  }

  public linkPreloadedArticles(contentSchema: ContentSchema) {
    this.visitContentSchema(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const entitiesMap = this._dataContext.tables[contentName];
        fieldSchema.PreloadingState = PreloadingState.Done;
        fieldSchema.PreloadedArticles = fieldSchema.PreloadedArticles.map(article =>
          entitiesMap.get(String(article._ServerId))
        );
      } else if (fieldSchema.PreloadingMode === PreloadingMode.Lazy) {
        const properties = {
          PreloadingState: PreloadingState.NotStarted
        };
        extendObservable(fieldSchema, properties, null, { deep: false });
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
