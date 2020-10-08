import React from "react";
import { Icon } from "@blueprintjs/core";
import "./Style.scss";
import { Intent } from "@blueprintjs/core";
import { TaskStatuses } from "Shared/Enums";

interface IProps {
  id: number;
  stateId: number;
  IsCancellationRequested: boolean;
  onRerun: (id: number) => void;
  onCancel: (id: number) => void;
}

export const RerunGridCell = React.memo(
  ({ id, onRerun, onCancel, stateId, IsCancellationRequested }: IProps) => {
    return (
      <div className="rerun-container">
        {!IsCancellationRequested && stateId !== TaskStatuses.Progress ? (
          <Icon
            className="rerun-container__icon"
            intent={Intent.PRIMARY}
            onClick={() => onRerun(id)}
            icon="refresh"
          />
        ) : (
          <Icon
            className="rerun-container__icon"
            intent={Intent.DANGER}
            onClick={() => onCancel(id)}
            icon="disable"
          />
        )}
      </div>
    );
  }
);
