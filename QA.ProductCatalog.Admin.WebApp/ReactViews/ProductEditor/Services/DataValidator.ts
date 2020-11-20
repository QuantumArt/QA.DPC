import { ArticleObject, EntityObject } from "ProductEditor/Models/EditorDataModels";
import {
  ContentSchema,
  isExtensionField,
  UpdatingMode,
  isSingleRelationField,
  isMultiRelationField
} from "ProductEditor/Models/EditorSchemaModels";

export interface ArticleErrors {
  ServerId: number;
  ContentName: string;
  ArticleErrors: string[];
  FieldErrors: {
    Name: string;
    Messages: string[];
  }[];
}

export class DataValidator {
  private _isEdited: boolean;
  private _errors: ArticleErrors[];

  public collectErrors(
    article: ArticleObject,
    contentSchema: ContentSchema,
    reuqireChanges: boolean
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
  }

  private visitArticle(
    article: ArticleObject,
    contentSchema: ContentSchema,
    visitedArticlesByContent = new Map<ContentSchema, Set<ArticleObject>>()
  ) {
    let visitedArticles = visitedArticlesByContent.get(contentSchema);
    if (!visitedArticles) {
      visitedArticles = new Set<ArticleObject>();
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

    for (const fieldName in contentSchema.Fields) {
      const fieldValue = article[fieldName];
      if (fieldValue == null) {
        continue;
      }

      const fieldSchema = contentSchema.Fields[fieldName];
      if (isSingleRelationField(fieldSchema)) {
        const relatedEntity = fieldValue as EntityObject;
        if (fieldSchema.UpdatingMode === UpdatingMode.Update) {
          this.visitArticle(relatedEntity, fieldSchema.RelatedContent, visitedArticlesByContent);
        } else if (relatedEntity._ServerId < 0) {
          this.addNotSavedRelationError(relatedEntity, fieldSchema.RelatedContent);
        }
      } else if (isMultiRelationField(fieldSchema)) {
        const relatedCollection = fieldValue as EntityObject[];
        if (fieldSchema.UpdatingMode === UpdatingMode.Update) {
          relatedCollection.forEach(entity =>
            this.visitArticle(entity, fieldSchema.RelatedContent, visitedArticlesByContent)
          );
        } else {
          relatedCollection.forEach(entity => {
            if (entity._ServerId < 0) {
              this.addNotSavedRelationError(entity, fieldSchema.RelatedContent);
            }
          });
        }
      } else if (isExtensionField(fieldSchema)) {
        const extensionFieldName = `${fieldName}${ArticleObject._Extension}`;
        const extensionArticle = article[extensionFieldName][fieldValue] as ArticleObject;
        const extensionContentSchema = fieldSchema.ExtensionContents[fieldValue];
        this.visitArticle(extensionArticle, extensionContentSchema, visitedArticlesByContent);
      }
    }
  }

  private addFieldErrors(article: ArticleObject, contentSchema: ContentSchema) {
    this._errors.push({
      ServerId: article._ServerId,
      ContentName: contentSchema.ContentName,
      ArticleErrors: [],
      FieldErrors: Object.entries(article.getAllErrors()).map(([fieldName, messages]) => {
        article.setTouched(fieldName, true);
        const fieldSchema = contentSchema.Fields[fieldName];
        return {
          Name: fieldSchema.FieldTitle || fieldSchema.FieldName,
          Messages: messages
        };
      })
    });
  }

  private addNotSavedRelationError(article: ArticleObject, contentSchema: ContentSchema) {
    this._errors.push({
      ServerId: article._ServerId,
      ContentName: contentSchema.ContentName,
      ArticleErrors: ["Новая зависимая статья не была сохранена на сервере"],
      FieldErrors: []
    });
  }
}
