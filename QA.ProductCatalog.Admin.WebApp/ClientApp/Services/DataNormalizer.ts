import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { StoreSnapshot, ArticleSnapshot } from "Models/EditorDataModels";
import {
  ContentSchema,
  isSingleRelationField,
  isMultiRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";

const options = { idAttribute: "Id", mergeStrategy: deepMerge };

export class DataNormalizer {
  private _normalizrSchemas: {
    [name: string]: schema.Entity | schema.Object;
  } = {};

  public initSchema(mergedSchemas: { [name: string]: ContentSchema }) {
    const getName = (content: ContentSchema) => mergedSchemas[content.ContentId].ContentName;

    Object.values(mergedSchemas).forEach(content => {
      if (content.ForExtension) {
        this._normalizrSchemas[content.ContentName] = new schema.Object({});
      } else {
        this._normalizrSchemas[content.ContentName] = new schema.Entity(
          content.ContentName,
          {},
          options
        );
      }
    });

    Object.values(mergedSchemas).forEach(content => {
      const references = {};
      Object.values(content.Fields).forEach(field => {
        if (isExtensionField(field)) {
          const extReferences = {};
          Object.values(field.Contents).forEach(extContent => {
            extReferences[getName(extContent)] = this._normalizrSchemas[getName(extContent)];
          });
          references[`${field.FieldName}_Contents`] = extReferences;
        } else if (isSingleRelationField(field)) {
          references[field.FieldName] = this._normalizrSchemas[getName(field.Content)];
        } else if (isMultiRelationField(field)) {
          references[field.FieldName] = [this._normalizrSchemas[getName(field.Content)]];
        }
      });
      this._normalizrSchemas[content.ContentName].define(references);
    });
  }

  public normalize(articleObject: ArticleSnapshot, contentName: string): StoreSnapshot {
    return normalize(articleObject, this._normalizrSchemas[contentName]).entities;
  }
}
