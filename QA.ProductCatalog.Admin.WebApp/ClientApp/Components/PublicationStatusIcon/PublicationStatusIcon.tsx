import React from "react";
import cn from "classnames";
import { ContentSchema } from "Models/EditorSchemaModels";
import { PublicationContext } from "Services/PublicationContext";
import { EntityObject } from "ClientApp/Models/EditorDataModels";
import "./PublicationStatusIcon.scss";

export const makePublicatoinStatusIcons = (
  publicationContext: PublicationContext,
  contentSchema: ContentSchema
) => (product: EntityObject) => {
  const liveTime = publicationContext.getLiveTime(product);
  const stageTime = publicationContext.getStageTime(product);
  const lastModified = contentSchema.getLastModified(product);
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
};
