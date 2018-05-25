import "bootstrap/dist/css/bootstrap.css";
import React from "react";
import ReactDOM from "react-dom";
import { Col, FormGroup, Label } from "reactstrap";
import { observable } from "mobx";
import { observer } from "mobx-react";
import {
  InputText,
  InputNumber,
  CheckBox,
  TextArea,
  DatePicker
} from "Components/FormControls/FormControls";

const article = observable({
  StringField: "",
  NumericField: 0,
  BooleanField: null,
  TextField: null,
  DateField: null
});

const FormControlsBlock = observer(() => (
  <div>
    <h4>FormControls</h4>
    <hr />

    <FormGroup row>
      <Label sm={3} size="sm">
        InputText
      </Label>
      <Col sm={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
        />
      </Col>
      <Col sm={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        InputNumber
      </Label>
      <Col sm={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
        />
      </Col>
      <Col sm={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
          isInteger
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        DatePicker
      </Label>
      <Col sm={3}>
        <DatePicker name="DateField" model={article} placeholder="DateField" />
      </Col>
      <Col sm={3}>
        <DatePicker
          name="DateField"
          model={article}
          placeholder="DateField"
          disabled
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        CheckBox
      </Label>
      <Col sm={3}>
        <CheckBox name="BooleanField" model={article} />
      </Col>
      <Col sm={3}>
        <CheckBox name="BooleanField" model={article} disabled />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        TextArea
      </Label>
      <Col sm={6}>
        <TextArea name="TextField" model={article} placeholder="TextField" />
      </Col>
    </FormGroup>

    <hr />
    <code>
      <pre>{JSON.stringify(article, null, 2)}</pre>
    </code>
  </div>
));

ReactDOM.render(
  <div>
    <FormControlsBlock />
  </div>,
  document.getElementById("library")
);
