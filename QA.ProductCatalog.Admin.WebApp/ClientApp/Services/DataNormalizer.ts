import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { ArticleObject, TablesSnapshot, EntitySnapshot } from "Models/EditorDataModels";
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
  private _tablesSchema: schema.Object;

  public initSchema(mergedSchemas: ContentSchemasById) {
    Object.values(mergedSchemas).forEach(content => {
      this._entitySchemas[content.ContentName] = new schema.Entity(
        content.ContentName,
        {},
        options
      );
      this._objectSchemas[content.ContentName] = new ObjectSchema({});
    });

    this._tablesSchema = new ObjectSchema({});

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
      this._tablesSchema.define({
        [content.ContentName]: [this._entitySchemas[content.ContentName]]
      });
    });
  }

  public normalize(articleObject: EntitySnapshot, contentName: string): TablesSnapshot {
    return normalize(articleObject, this._entitySchemas[contentName]).entities;
  }

  public normalizeAll(articleObjects: EntitySnapshot[], contentName: string): TablesSnapshot {
    return normalize(articleObjects, [this._entitySchemas[contentName]]).entities;
  }

  public normalizeTables(articleObjectsByContent: {
    [contentName: string]: EntitySnapshot[];
  }): TablesSnapshot {
    return normalize(articleObjectsByContent, this._tablesSchema).entities;
  }
}

/**
 * `schema.Object` that preserves `null` in property values
 * https://github.com/paularmstrong/normalizr/issues/332
 */
class ObjectSchema extends schema.Object {
  schema: object;

  normalize(input, _parent, _key, visit, addEntity) {
    const object = { ...input };
    Object.keys(this.schema).forEach(key => {
      const localSchema = this.schema[key];
      const value = visit(input[key], input, key, localSchema, addEntity);
      if (value === undefined) {
        delete object[key];
      } else {
        object[key] = value;
      }
    });
    return object;
  }
}
