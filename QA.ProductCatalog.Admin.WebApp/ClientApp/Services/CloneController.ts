import { inject } from "react-ioc";
import { runInAction } from "mobx";
import { command } from "Utils/Command";
import { rootUrl } from "Utils/Common";
import { isArray } from "Utils/TypeChecks";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { DataContext } from "Services/DataContext";
import {
  RelationFieldSchema,
  isMultiRelationField,
  isSingleRelationField
} from "Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject, ArticleObject } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";

export class CloneController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;
  @inject private _dataContext: DataContext;

  private _query = document.location.search;

  @command
  public async cloneRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ): Promise<EntityObject> {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(`${rootUrl}/ProductEditor/ClonePartialProduct${this._query}`, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        ProductDefinitionId: this._editorSettings.ProductDefinitionId,
        ContentPath: contentSchema.ContentPath,
        CloneArticleId: entity._ServerId
      })
    });
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());

    return runInAction("cloneRelatedEntity", () => {
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);

      const cloneId = String(dataTree._ClientId);
      const clonedEntity = this._dataContext.tables[contentSchema.ContentName].get(cloneId);

      const relation = parent[fieldSchema.FieldName];
      if (isMultiRelationField(fieldSchema) && isArray(relation)) {
        relation.push(clonedEntity);
        // parent.setTouched(fieldSchema.FieldName) — не нужен,
        // так как клонированный продукт уже сохранен на сервере
      }
      return clonedEntity;
    });
  }

  @command
  public async cloneProductPrototype(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema
  ): Promise<EntityObject> {
    const contentSchema = fieldSchema.RelatedContent;

    const response = await fetch(
      `${rootUrl}/ProductEditor/ClonePartialProductPrototype${this._query}`,
      {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          ProductDefinitionId: this._editorSettings.ProductDefinitionId,
          ContentPath: fieldSchema.ParentContent.ContentPath,
          RelationFieldName: fieldSchema.FieldName,
          ParentArticleId: parent._ServerId
        })
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    const dataTree = this._dataSerializer.deserialize<EntitySnapshot>(await response.text());

    return runInAction("cloneProductPrototype", () => {
      const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

      this._dataMerger.mergeTables(dataSnapshot, MergeStrategy.ServerWins);

      const cloneId = String(dataTree._ClientId);
      const clonedEntity = this._dataContext.tables[contentSchema.ContentName].get(cloneId);

      const relation = parent[fieldSchema.FieldName];
      if (isMultiRelationField(fieldSchema) && isArray(relation)) {
        relation.push(clonedEntity);
        // parent.setTouched(fieldSchema.FieldName) — не нужен,
        // так как клонированный продукт уже сохранен на сервере
      } else if (isSingleRelationField(fieldSchema) && !relation) {
        parent[fieldSchema.FieldName] = clonedEntity;
      }
      return clonedEntity;
    });
  }
}
