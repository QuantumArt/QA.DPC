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

class TestArticle {
  @observable StringField = "";
  @observable NumericField = 0;
  @observable BooleanField = null;
  @observable TextField = null;
  @observable DateField = null;
}

const testArticle = new TestArticle();

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
          model={testArticle}
          placeholder="StringField"
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
          model={testArticle}
          placeholder="NumericField"
          isInteger
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        DatePicker
      </Label>
      <Col sm={3}>
        <DatePicker
          name="DateField"
          model={testArticle}
          placeholder="DateField"
        />
      </Col>
      <Label sm={3} size="sm">
        DatePicker
      </Label>
      <Col sm={3}>
        <DatePicker
          name="DateField"
          model={testArticle}
          placeholder="DateField"
        />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        CheckBox
      </Label>
      <Col sm={3}>
        <CheckBox name="BooleanField" model={testArticle} />
      </Col>
    </FormGroup>

    <FormGroup row>
      <Label sm={3} size="sm">
        TextArea
      </Label>
      <Col sm={6}>
        <TextArea
          name="TextField"
          model={testArticle}
          placeholder="TextField"
        />
      </Col>
    </FormGroup>

    <hr />
    <code>
      <pre>{JSON.stringify(testArticle, null, 2)}</pre>
    </code>
  </div>
));

ReactDOM.render(
  <div>
    <FormControlsBlock />
  </div>,
  document.getElementById("library")
);
