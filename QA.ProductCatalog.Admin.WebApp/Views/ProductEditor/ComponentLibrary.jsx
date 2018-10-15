import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import cn from "classnames";
import { Radio, Divider } from "@blueprintjs/core";
import { Grid, Row, Col } from "react-flexbox-grid";
import { types as t, unprotect } from "mobx-state-tree";
import { toJS, configure } from "mobx";
import { observer } from "mobx-react";
import { validationMixin, Validate } from "mst-validation-mixin";
import {
  LocaleContext,
  Localize,
  Translate,
  localize,
  id,
  fallback,
  TranslateFunction
} from "react-lazy-i18n";
import {
  InputText,
  InputNumber,
  InputSearch,
  InputFile,
  CheckBox,
  TextArea,
  DatePicker,
  Select,
  RadioGroup
} from "Components/FormControls/FormControls";
import { required, pattern, maxCount } from "Utils/Validators";

class App extends React.Component {
  state = { lang: "eng" };

  render() {
    const firstName = "John";
    const lastName = "Doe";
    const fullName = "John Doe";
    const { lang } = this.state;
    return (
      <>
        <label>
          English{" "}
          <input
            type="radio"
            value="eng"
            checked={lang === "eng"}
            onChange={() => this.setState({ lang: "eng" })}
          />
        </label>{" "}
        <label>
          Русский{" "}
          <input
            type="radio"
            value="rus"
            checked={lang === "rus"}
            onChange={() => this.setState({ lang: "rus" })}
          />
        </label>
        <Divider />
        <LocaleContext.Provider value={lang}>
          <br />
          <Localize
            load={lang =>
              import(/* webpackChunkName: "i18n-" */ `./ComponentLibrary.${lang}.jsx`)
            }
          >
            {tran => (
              <Translate id="customComponent">
                <article key={1} title={tran`Test...`}>
                  User Card
                  <div key={2} title={tran`Hello, ${firstName}!`}>
                    First Name: {{ firstName }}
                  </div>
                  <div key={3} title={tran(`helloTemplate`, lastName)}>
                    Last Name: {{ lastName }}
                  </div>
                  <div
                    key={4}
                    title={tran(id`missingKey`, fallback`Hello, ${fullName}!`)}
                  >
                    Full Name: {{ fullName }}
                  </div>
                </article>
              </Translate>
            )}
          </Localize>
          <Divider />
          <LocalizedComponent />
          <br />
        </LocaleContext.Provider>
      </>
    );
  }
}

@localize(lang =>
  import(/* webpackChunkName: "i18n-" */ `./ComponentLibrary.${lang}.jsx`)
)
class LocalizedComponent extends React.Component {
  render() {
    /** @type {TranslateFunction} */
    const tran = this.props.translate;

    const firstName = "Foo";
    const lastName = "Bar";
    const fullName = "Foo Bar";
    return (
      <Translate id="customComponent">
        <article key={1} title={tran`Test...`}>
          User Card
          <div key={2} title={tran`Hello, ${firstName}!`}>
            First Name: {{ firstName }}
          </div>
          <div key={3} title={tran(`helloTemplate`, lastName)}>
            Last Name: {{ lastName }}
          </div>
          <div
            key={4}
            title={tran(id`missingKey`, fallback`Hello, ${fullName}!`)}
          >
            Full Name: {{ fullName }}
          </div>
        </article>
      </Translate>
    );
  }
}

ReactDOM.render(
  <Grid fluid>
    <h4>react-lazy-i18n</h4>
    <App />
  </Grid>,
  document.getElementById("translate")
);

const Category = t
  .model("Category", {
    Id: t.identifierNumber,
    StringField: t.maybeNull(t.string)
  })
  .extend(validationMixin);

const Article = t
  .model("Article", {
    Id: t.identifierNumber,
    Category: t.maybeNull(t.reference(Category)),
    StringField: t.maybeNull(t.string),
    PhoneField: t.maybeNull(t.string),
    NumericField: t.maybeNull(t.number),
    SearchField: t.maybeNull(t.string),
    FileField: t.maybeNull(t.string),
    BooleanField: t.maybeNull(t.boolean),
    TextField: t.maybeNull(t.string),
    DateField: t.maybeNull(t.Date),
    TimeField: t.maybeNull(t.Date),
    DateTimeField: t.maybeNull(t.Date),
    EnumField: t.maybeNull(t.string),
    ArrayField: t.maybeNull(t.array(t.string))
  })
  .extend(validationMixin);

const article = Article.create({
  Id: 10,
  StringField: "foo",
  PhoneField: "",
  NumericField: 0,
  EnumField: "first",
  ArrayField: ["first"]
});

unprotect(article);

const category = Category.create({
  Id: 1,
  StringField: ""
});

unprotect(category);

configure({ enforceActions: "never" });

article.Category = category;

// prettier-ignore
const phoneMask = ["+", "7", " ", "(", /\d/, /\d/, /\d/, ")", " ", /\d/, /\d/, /\d/, "-", /\d/, /\d/, /\d/, /\d/];

const FormControlsBlock = observer(() => (
  <div>
    <h4>FormControls</h4>
    <Divider />

    <Row
      className={cn("bp3-form-group", {
        "bp3-intent-danger": article.hasVisibleErrors("StringField")
      })}
    >
      <Col md={3}>InputText [ pattern | required]</Col>
      <Col md={3} className="bp3-form-content">
        <Validate
          model={article}
          name="StringField"
          errorClassName="bp3-form-helper-text"
          rules={[required, pattern(/^[A-Za-z0-9]+$/)]}
        >
          <InputText
            name="StringField"
            model={article}
            placeholder="StringField"
            className={cn({
              "bp3-intent-danger": article.hasVisibleErrors("StringField")
            })}
          />
        </Validate>
      </Col>
      <Col md={3} className="bp3-form-content">
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          className={cn({
            "bp3-intent-danger": article.hasVisibleErrors("StringField")
          })}
        />
        <Validate
          model={article}
          name="StringField"
          errorClassName="bp3-form-helper-text"
          rules={required}
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>InputText [normal | disabled]</Col>
      <Col md={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
        />
      </Col>
      <Col md={3}>
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          disabled
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>InputText [mask | readonly]</Col>
      <Col md={3}>
        <InputText
          name="PhoneField"
          model={article}
          placeholder="PhoneField"
          mask={phoneMask}
        />
      </Col>
      <Col md={3}>
        <InputText
          name="PhoneField"
          model={article}
          placeholder="PhoneField"
          readOnly
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>InputNumber [normal | disabled]</Col>
      <Col md={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
        />
      </Col>
      <Col md={3}>
        <InputNumber
          name="NumericField"
          model={article}
          placeholder="NumericField"
          isInteger
          disabled
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>InputSearch [normal | disabled]</Col>
      <Col md={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
        />
      </Col>
      <Col md={3}>
        <InputSearch
          name="SearchField"
          model={article}
          placeholder="SearchField"
          disabled
        />
      </Col>
    </Row>

    <Row
      className={cn("bp3-form-group", {
        "bp3-intent-danger": article.hasVisibleErrors("FileField")
      })}
    >
      <Col md={3}>InputFile [required | disabled]</Col>
      <Col md={3}>
        <InputFile
          name="FileField"
          model={article}
          placeholder="FileField"
          className={cn({
            "bp3-intent-danger": article.hasVisibleErrors("FileField")
          })}
        />
        <Validate
          model={article}
          name="FileField"
          errorClassName="bp3-form-helper-text"
          rules={required}
        />
      </Col>
      <Col md={3}>
        <InputFile
          name="FileField"
          model={article}
          placeholder="FileField"
          disabled
        />
      </Col>
    </Row>

    <Row
      className={cn("bp3-form-group", {
        "bp3-intent-danger": article.hasVisibleErrors("DateField")
      })}
    >
      <Col md={3}>DatePicker [date | date required]</Col>
      <Col md={3}>
        <DatePicker
          name="DateField"
          model={article}
          type="date"
          placeholder="DateField"
        />
      </Col>
      <Col md={3} className="bp3-form-content">
        <DatePicker
          name="DateField"
          model={article}
          type="date"
          className={cn({
            "bp3-intent-danger": article.hasVisibleErrors("DateField")
          })}
          placeholder="DateField"
        />
        <Validate
          model={article}
          name="DateField"
          errorClassName="bp3-form-helper-text"
          rules={required}
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>DatePicker [time | time disabled]</Col>
      <Col md={3}>
        <DatePicker
          name="TimeField"
          model={article}
          type="time"
          placeholder="TimeField"
        />
      </Col>
      <Col md={3}>
        <DatePicker
          name="TimeField"
          model={article}
          type="time"
          placeholder="TimeField"
          disabled
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>DatePicker [normal | disabled]</Col>
      <Col md={3}>
        <DatePicker
          name="DateTimeField"
          model={article}
          placeholder="DateTimeField"
        />
      </Col>
      <Col md={3}>
        <DatePicker
          name="DateTimeField"
          model={article}
          placeholder="DateTimeField"
          disabled
        />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>CheckBox [normal | disabled] </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline />
      </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline disabled />
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>Select [normal | disabled]</Col>
      <Col md={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
        />
      </Col>
      <Col md={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          disabled
        />
      </Col>
    </Row>

    <Row
      className={cn("bp3-form-group", {
        "bp3-intent-danger": article.hasVisibleErrors("ArrayField")
      })}
    >
      <Col md={3}>Select [required | multiple]</Col>
      <Col md={3}>
        <Select
          name="EnumField"
          model={article}
          placeholder="EnumField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          required
        />
      </Col>
      <Col md={3} className="bp3-form-content">
        <Select
          name="ArrayField"
          model={article}
          className={cn({
            "bp3-intent-danger": article.hasVisibleErrors("ArrayField")
          })}
          placeholder="ArrayField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          multiple
        />
        <Validate
          model={article}
          name="ArrayField"
          errorClassName="bp3-form-helper-text"
          rules={[required, maxCount(1)]}
        />
      </Col>
    </Row>

    <Row
      className={cn("bp3-form-group", {
        "bp3-intent-danger": article.hasVisibleErrors("EnumField")
      })}
    >
      <Col md={3}>RadioGroup [validation | disabled]</Col>
      <Col md={3}>
        <RadioGroup
          name="EnumField"
          model={article}
          placeholder="EnumField"
          inline
          className={cn({
            "bp3-intent-danger": article.hasVisibleErrors("EnumField")
          })}
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" },
            { value: "third", label: "Третий", disabled: true }
          ]}
        />
        <Validate
          model={article}
          name="EnumField"
          errorClassName="bp3-form-helper-text"
          rules={[required, pattern(/^[0-9]+$/)]}
        />
      </Col>
      <Col md={6}>
        <RadioGroup
          name="EnumField"
          model={article}
          placeholder="EnumField"
          inline
          disabled
        >
          <Radio label="Первый" value="first" />
          <Radio label="Второй" value="second" />
        </RadioGroup>
      </Col>
    </Row>

    <Row className="bp3-form-group">
      <Col md={3}>TextArea [normal]</Col>
      <Col md>
        <TextArea name="TextField" model={article} placeholder="TextField" />
      </Col>
    </Row>

    <Divider />

    <Row>
      <Col md>
        <div>State:</div>
        <pre>{JSON.stringify(toJS(article), null, 2)}</pre>
      </Col>
      <Col md>
        <div>All errors:</div>
        <pre>{JSON.stringify(toJS(article.getAllErrors()), null, 2)}</pre>
      </Col>
      <Col md>
        <div>Visible errors:</div>
        <pre>
          {JSON.stringify(toJS(article.getAllVisibleErrors()), null, 2)}
        </pre>
      </Col>
    </Row>
  </div>
));

ReactDOM.render(
  <Grid fluid>
    <FormControlsBlock />
  </Grid>,
  document.getElementById("library")
);
