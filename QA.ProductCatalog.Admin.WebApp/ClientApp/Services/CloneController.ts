import { inject } from "react-ioc";
import { runInAction } from "mobx";
import { command } from "Utils/Command";
import { rootUrl } from "Utils/Common";
import { isArray } from "Utils/TypeChecks";
import { DataSerializer } from "Services/DataSerializer";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataMerger, MergeStrategy } from "Services/DataMerger";
import { RelationFieldSchema, isMultiRelationField } from "Models/EditorSchemaModels";
import { EntitySnapshot, EntityObject, ArticleObject } from "Models/EditorDataModels";
import { EditorSettings } from "Models/EditorSettings";

export class CloneController {
  @inject private _editorSettings: EditorSettings;
  @inject private _dataSerializer: DataSerializer;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataMerger: DataMerger;

  private _query = document.location.search;

  @command
  public async cloneRelatedEntity(
    parent: ArticleObject,
    fieldSchema: RelationFieldSchema,
    entity: EntityObject
  ) {
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

    const dataSnapshot = this._dataNormalizer.normalize(dataTree, contentSchema.ContentName);

    runInAction("cloneRelatedEntity", () => {
      this._dataMerger.mergeStore(dataSnapshot, MergeStrategy.ServerWins);

      const relation = parent[fieldSchema.FieldName];
      if (isMultiRelationField(fieldSchema) && isArray(relation)) {
        relation.push(dataTree._ClientId);
        parent.setTouched(fieldSchema.FieldName, true);
      }
    });
  }
}
