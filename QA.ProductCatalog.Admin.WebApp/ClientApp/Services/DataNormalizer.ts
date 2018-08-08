import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { ArticleObject, StoreSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import {
  ContentSchemasById,
  isSingleRelationField,
  isMultiRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";

const options = { idAttribute: ArticleObject._ClientId, mergeStrategy: deepMerge };

export class DataNormalizer {
  private _entitySchemas: {
    [contentName: string]: schema.Entity;
  } = {};
  private _objectSchemas: {
    [contentName: string]: schema.Object;
  } = {};

  public initSchema(mergedSchemas: ContentSchemasById) {
    Object.values(mergedSchemas).forEach(content => {
      this._entitySchemas[content.ContentName] = new schema.Entity(
        content.ContentName,
        {},
        options
      );
      this._objectSchemas[content.ContentName] = new schema.Object({});
    });

    Object.values(mergedSchemas).forEach(content => {
      const references = {};
      Object.values(content.Fields).forEach(field => {
        if (isExtensionField(field)) {
          const extReferences = {};
          Object.values(field.ExtensionContents).forEach(extContent => {
            extReferences[extContent.ContentName] = this._objectSchemas[extContent.ContentName];
          });
          references[`${field.FieldName}${ArticleObject._Contents}`] = extReferences;
        } else if (isSingleRelationField(field)) {
          references[field.FieldName] = this._entitySchemas[field.RelatedContent.ContentName];
        } else if (isMultiRelationField(field)) {
          references[field.FieldName] = [this._entitySchemas[field.RelatedContent.ContentName]];
        }
      });
      this._entitySchemas[content.ContentName].define(references);
      this._objectSchemas[content.ContentName].define(references);
    });
  }

  public normalize(articleObject: EntitySnapshot, contentName: string): StoreSnapshot {
    return normalize(articleObject, this._entitySchemas[contentName]).entities;
  }

  public normalizeAll(articleObjects: EntitySnapshot[], contentName: string): StoreSnapshot {
    return normalize(articleObjects, [this._entitySchemas[contentName]]).entities;
  }
}
