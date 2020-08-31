import React from "react";
import { observer } from "mobx-react-lite";
import { Button, FormGroup, InputGroup, Intent } from "@blueprintjs/core";

interface Props {
  nodeId: string;
}

const StubFrom = observer<Props>(({ nodeId }) => {
  return (
    <form>
      <FormGroup inline label="Some field">
        <InputGroup placeholder={nodeId} fill />
      </FormGroup>
    </form>
  );
});

export default StubFrom;
