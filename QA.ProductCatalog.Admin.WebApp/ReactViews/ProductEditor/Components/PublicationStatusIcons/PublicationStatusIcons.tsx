import React, { Component } from "react";
import cn from "classnames";
import { observer } from "mobx-react";
import { ContentSchema } from "ProductEditor/Models/EditorSchemaModels";
import { PublicationContext } from "ProductEditor/Services/PublicationContext";
import { EntityObject } from "ProductEditor/Models/EditorDataModels";
import "./PublicationStatusIcons.scss";

interface PublicationStatusIconsProps {
  /** Статья, для которой считается время последней модификации */
  model: EntityObject;
  /**
   * Статья, которая публикуется как продукт DPC
   * @default model
   */
  product?: EntityObject;
  /** Схема контента для статьи `model` */
  contentSchema: ContentSchema;
  /** Контекст статусов публикации */
  publicationContext: PublicationContext;
}

@observer
export class PublicationStatusIcons extends Component<PublicationStatusIconsProps> {
  render() {
    const { model, product = model, contentSchema, publicationContext } = this.props;
    const lastModified = contentSchema.getLastModified(model);
    const liveTime = publicationContext.getLiveTime(product);
    const stageTime = publicationContext.getStageTime(product);
    const liveIsSync = liveTime && liveTime >= lastModified;
    const stageIsSync = stageTime && stageTime >= lastModified;
    return (
      <>
        {stageTime && (
          <div
            className={cn("publication-status-icon", {
              "publication-status-icon--sync": stageIsSync
            })}
            title={stageIsSync ? "Опубликовано на STAGE" : "Требуется публикация на STAGE"}
          >
            S
          </div>
        )}
        {liveTime && (
          <div
            className={cn("publication-status-icon", {
              "publication-status-icon--sync": liveIsSync
            })}
            title={liveIsSync ? "Опубликовано на LIVE" : "Требуется публикация на LIVE"}
          >
            L
          </div>
        )}
      </>
    );
  }
}
