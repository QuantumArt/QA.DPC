import * as tslib_1 from "tslib";
import { ArticleObject } from "Models/EditorDataModels";
import {
  isExtensionField,
  UpdatingMode,
  isSingleRelationField,
  isMultiRelationField
} from "Models/EditorSchemaModels";
var DataValidator = /** @class */ (function() {
  function DataValidator() {}
  DataValidator.prototype.collectErrors = function(
    article,
    contentSchema,
    reuqireChanges
  ) {
    this._isEdited = false;
    this._errors = [];
    this.visitArticle(article, contentSchema);
    if (reuqireChanges && !this._isEdited && this._errors.length === 0) {
      this._errors.push({
        ServerId: article._ServerId,
        ContentName: contentSchema.ContentName,
        ArticleErrors: ["Статья не была изменена"],
        FieldErrors: []
      });
    }
    return this._errors;
  };
  DataValidator.prototype.visitArticle = function(
    article,
    contentSchema,
    visitedArticlesByContent
  ) {
    var _this = this;
    if (visitedArticlesByContent === void 0) {
      visitedArticlesByContent = new Map();
    }
    var visitedArticles = visitedArticlesByContent.get(contentSchema);
    if (!visitedArticles) {
      visitedArticles = new Set();
      visitedArticlesByContent.set(contentSchema, visitedArticles);
    }
    if (visitedArticles.has(article)) {
      return;
    }
    visitedArticles.add(article);
    if (article.isEdited()) {
      this._isEdited = true;
    }
    if (article.hasErrors()) {
      this.addFieldErrors(article, contentSchema);
    }
    var _loop_1 = function(fieldName) {
      var fieldValue = article[fieldName];
      if (fieldValue == null) {
        return "continue";
      }
      var fieldSchema = contentSchema.Fields[fieldName];
      if (isSingleRelationField(fieldSchema)) {
        var relatedEntity = fieldValue;
        if (fieldSchema.UpdatingMode === UpdatingMode.Update) {
          this_1.visitArticle(
            relatedEntity,
            fieldSchema.RelatedContent,
            visitedArticlesByContent
          );
        } else if (relatedEntity._ServerId < 0) {
          this_1.addNotSavedRelationError(
            relatedEntity,
            fieldSchema.RelatedContent
          );
        }
      } else if (isMultiRelationField(fieldSchema)) {
        var relatedCollection = fieldValue;
        if (fieldSchema.UpdatingMode === UpdatingMode.Update) {
          relatedCollection.forEach(function(entity) {
            return _this.visitArticle(
              entity,
              fieldSchema.RelatedContent,
              visitedArticlesByContent
            );
          });
        } else {
          relatedCollection.forEach(function(entity) {
            if (entity._ServerId < 0) {
              _this.addNotSavedRelationError(
                entity,
                fieldSchema.RelatedContent
              );
            }
          });
        }
      } else if (isExtensionField(fieldSchema)) {
        var extensionFieldName = "" + fieldName + ArticleObject._Extension;
        var extensionArticle = article[extensionFieldName][fieldValue];
        var extensionContentSchema = fieldSchema.ExtensionContents[fieldValue];
        this_1.visitArticle(
          extensionArticle,
          extensionContentSchema,
          visitedArticlesByContent
        );
      }
    };
    var this_1 = this;
    for (var fieldName in contentSchema.Fields) {
      _loop_1(fieldName);
    }
  };
  DataValidator.prototype.addFieldErrors = function(article, contentSchema) {
    this._errors.push({
      ServerId: article._ServerId,
      ContentName: contentSchema.ContentName,
      ArticleErrors: [],
      FieldErrors: Object.entries(article.getAllErrors()).map(function(_a) {
        var _b = tslib_1.__read(_a, 2),
          fieldName = _b[0],
          messages = _b[1];
        article.setTouched(fieldName, true);
        var fieldSchema = contentSchema.Fields[fieldName];
        return {
          Name: fieldSchema.FieldTitle || fieldSchema.FieldName,
          Messages: messages
        };
      })
    });
  };
  DataValidator.prototype.addNotSavedRelationError = function(
    article,
    contentSchema
  ) {
    this._errors.push({
      ServerId: article._ServerId,
      ContentName: contentSchema.ContentName,
      ArticleErrors: ["Новая зависимая статья не была сохранена на сервере"],
      FieldErrors: []
    });
  };
  return DataValidator;
})();
export { DataValidator };
//# sourceMappingURL=DataValidator.js.map
