import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { StoreSnapshot, ArticleSnapshot } from "Models/EditorDataModels";
import {
  ContentSchema,
  isSingleRelationField,
  isMultiRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";

const options = { idAttribute: "_ClientId", mergeStrategy: deepMerge };

export class DataNormalizer {
  private _entitySchemas: {
    [contentName: string]: schema.Entity;
  } = {};
  private _objectSchemas: {
    [contentName: string]: schema.Object;
  } = {};

  public initSchema(mergedSchemas: { [name: string]: ContentSchema }) {
    const getName = (content: ContentSchema) => mergedSchemas[content.ContentId].ContentName;

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
          Object.values(field.Contents).forEach(extContent => {
            extReferences[getName(extContent)] = this._objectSchemas[getName(extContent)];
          });
          references[`${field.FieldName}_Contents`] = extReferences;
        } else if (isSingleRelationField(field)) {
          references[field.FieldName] = this._entitySchemas[getName(field.Content)];
        } else if (isMultiRelationField(field)) {
          references[field.FieldName] = [this._entitySchemas[getName(field.Content)]];
        }
      });
      this._entitySchemas[content.ContentName].define(references);
      this._objectSchemas[content.ContentName].define(references);
    });
  }

  public normalize(articleObject: ArticleSnapshot, contentName: string): StoreSnapshot {
    return normalize(articleObject, this._entitySchemas[contentName]).entities;
  }

  public normalizeAll(articleObjects: ArticleSnapshot[], contentName: string): StoreSnapshot {
    return normalize(articleObjects, [this._entitySchemas[contentName]]).entities;
  }
}
