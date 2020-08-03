import React, { Component, ReactNode } from "react";
import { observer } from "mobx-react";
import { FormGroup, TextArea, Checkbox, Button, Intent } from "@blueprintjs/core";

import { FetchStatus } from "Shared/Enums";

import Store from "../PartialSendStore";
import { CurrentStep } from "../enums";

type Props = {
  store: Store;
};

@observer
export default class SendForm extends Component<Props> {
  render(): ReactNode {
    const {
      hidden: { ignoredStatuses, localize },
      legend,
      description,
      processSpecialStatusesCheckbox,
      sendOnStageOnlyCheckbox,
      sendButton
    } = window.partialSend.sendForm;

    const {
      currentStep,
      ids,
      idsValidationError,
      setIds,
      processSpecialStatuses,
      setProcessSpecialStatuses,
      sendOnStageOnly,
      setSendOnStageOnly,
      isValidForm,
      fetchStatus
    } = this.props.store;

    if (currentStep !== CurrentStep.SendForm) {
      return null;
    }

    return (
      <div className="formLayout PartialSend">
        <fieldset>
          <legend>{legend}</legend>
          <p className="PartialSend__description">{description}</p>
          <form
            onSubmit={event => {
              event.preventDefault();
              const form = event.target as HTMLFormElement;
              this.props.store.handleSubmit(form);
            }}
          >
            <div>
              {ignoredStatuses &&
                ignoredStatuses.map((status, index) => (
                  <input key={index} name="IgnoredStatus" type="hidden" value={status} />
                ))}
              <input name="Localize" type="hidden" value={localize} />
            </div>
            <FormGroup
              intent={isValidForm ? Intent.NONE : Intent.DANGER}
              helperText={idsValidationError}
            >
              <TextArea
                name="idsStr"
                intent={isValidForm ? Intent.NONE : Intent.DANGER}
                cols={120}
                rows={10}
                value={ids}
                onChange={event => setIds(event.target.value)}
                className="PartialSend__ids"
              />
            </FormGroup>
            {!sendOnStageOnly && (
              <Checkbox
                name="proceedIgnoredStatus"
                value={`${processSpecialStatuses}`}
                checked={processSpecialStatuses}
                onChange={() => setProcessSpecialStatuses(!processSpecialStatuses)}
                label={processSpecialStatusesCheckbox}
              />
            )}
            <input type="hidden" name="proceedIgnoredStatus" value="false" />
            <Checkbox
              name="stageOnly"
              value={`${sendOnStageOnly}`}
              checked={sendOnStageOnly}
              onChange={() => {
                setSendOnStageOnly(!sendOnStageOnly);
                setProcessSpecialStatuses(!processSpecialStatuses);
              }}
              label={sendOnStageOnlyCheckbox}
            />
            <input type="hidden" name="stageOnly" value="false" />
            <Button
              type="submit"
              intent={Intent.PRIMARY}
              disabled={fetchStatus === FetchStatus.Failure}
              text={sendButton}
            />
          </form>
        </fieldset>
      </div>
    );
  }
}
