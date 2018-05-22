import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { StoreSnapshot, ArticleSnapshot } from "Models/StoreModels";
import {
  ContentSchema,
  isRelationField,
  isBackwardField,
  isExtensionField
} from "Models/EditorSchemaModels";

const options = { idAttribute: "Id", mergeStrategy: deepMerge };

export class NormalizationService {
  private _normalizrSchemas: {
    [name: string]: schema.Entity | schema.Object;
  } = {};

  public initialize(mergedSchemas: { [name: string]: ContentSchema }) {
    const contentName = ({ ContentId }) => mergedSchemas[ContentId].ContentName;

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
            extReferences[contentName(extContent)] = this._normalizrSchemas[
              contentName(extContent)
            ];
          });
          references[`${field.FieldName}_Contents`] = extReferences;
        } else if (isBackwardField(field)) {
          references[field.FieldName] = [this._normalizrSchemas[contentName(field.Content)]];
        } else if (isRelationField(field)) {
          switch (field.FieldType) {
            case "M2MRelation":
            case "M2ORelation":
              references[field.FieldName] = [this._normalizrSchemas[contentName(field.Content)]];
              break;
            case "O2MRelation":
              references[field.FieldName] = this._normalizrSchemas[contentName(field.Content)];
              break;
          }
        }
      });
      this._normalizrSchemas[content.ContentName].define(references);
    });
  }

  public normalize(data: ArticleSnapshot, contentName: string): StoreSnapshot {
    return normalize(data, this._normalizrSchemas[contentName]).entities;
  }
}
