import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import cn from "classnames";
import { Radio } from "@blueprintjs/core";
import { Grid, Row, Col } from "react-flexbox-grid";
import { types as t, unprotect } from "mobx-state-tree";
import { toJS } from "mobx";
import { observer } from "mobx-react";
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
import { validatableMixin } from "Models/ValidatableMixin";
import {
  LocaleContext,
  Localize,
  Translate,
  localize,
  id,
  fallback,
  TranslateFunction
} from "Utils/Localization";

// const resources = {
//   "Test...": "Тест...",
//   "Hello, ${name}!": name => `Привет, ${name}!`,
//   helloTemplate: name => `Здравствуй, ${name}!`,
//   customComponent: ({ firstName, lastName, fullName }) => (
//     <article key={1}>
//       Карточка пользователя
//       <div key={2}>Имя: {firstName}</div>
//       <div key={3}>Фамилия: {lastName}</div>
//       <div key={4}>Полное имя: {fullName}</div>
//     </article>
//   )
// };

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
        </label>
        <span />
        <label>
          Русский{" "}
          <input
            type="radio"
            value="rus"
            checked={lang === "rus"}
            onChange={() => this.setState({ lang: "rus" })}
          />
        </label>
        <hr />
        <LocaleContext.Provider value={lang}>
          <br />
          <Localize
            load={lang =>
              import(/* webpackChunkName: "i18n-" */ `./Foo.${lang}.jsx`)
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
          <hr />
          <LocalizedComponent />
          <br />
        </LocaleContext.Provider>
      </>
    );
  }
}

@localize(lang => import(/* webpackChunkName: "i18n-" */ `./Bar.${lang}.jsx`))
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

ReactDOM.render(<App />, document.getElementById("translate"));

const Category = t
  .model("Category", {
    Id: t.identifier(t.number),
    StringField: t.maybe(t.string)
  })
  .extend(validatableMixin);

const Article = t
  .model("Article", {
    Id: t.identifier(t.number),
    Category: t.maybe(t.reference(Category)),
    StringField: t.maybe(t.string),
    PhoneField: t.maybe(t.string),
    NumericField: t.maybe(t.number),
    SearchField: t.maybe(t.string),
    FileField: t.maybe(t.string),
    BooleanField: t.maybe(t.boolean),
    TextField: t.maybe(t.string),
    DateField: t.maybe(t.Date),
    TimeField: t.maybe(t.Date),
    DateTimeField: t.maybe(t.Date),
    EnumField: t.maybe(t.string),
    ArrayField: t.maybe(t.array(t.string))
  })
  .extend(validatableMixin);

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

article.Category = category;

// prettier-ignore
const phoneMask = ["+", "7", " ", "(", /\d/, /\d/, /\d/, ")", " ", /\d/, /\d/, /\d/, "-", /\d/, /\d/, /\d/, /\d/];

const FormControlsBlock = observer(() => (
  <div>
    <h4>FormControls</h4>
    <hr />

    <Row
      className={cn("pt-form-group", {
        "pt-intent-danger": article.hasVisibleErrors("StringField")
      })}
    >
      <Col md={3}>InputText [ pattern | required]</Col>
      <Col md={3} className="pt-form-content">
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          validate={[required, pattern(/^[A-Za-z0-9]+$/)]}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("StringField")
          })}
        />
        {article.hasVisibleErrors("StringField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("StringField")}
          </div>
        )}
      </Col>
      <Col md={3} className="pt-form-content">
        <InputText
          name="StringField"
          model={article}
          placeholder="StringField"
          validate={required}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("StringField")
          })}
        />
        {article.hasVisibleErrors("StringField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("StringField")}
          </div>
        )}
      </Col>
    </Row>

    <Row className="pt-form-group">
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

    <Row className="pt-form-group">
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

    <Row className="pt-form-group">
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

    <Row className="pt-form-group">
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
      className={cn("pt-form-group", {
        "pt-intent-danger": article.hasVisibleErrors("FileField")
      })}
    >
      <Col md={3}>InputFile [required | disabled]</Col>
      <Col md={3}>
        <InputFile
          name="FileField"
          model={article}
          placeholder="FileField"
          validate={required}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("FileField")
          })}
        />
        {article.hasVisibleErrors("FileField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("FileField")}
          </div>
        )}
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
      className={cn("pt-form-group", {
        "pt-intent-danger": article.hasVisibleErrors("DateField")
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
      <Col md={3} className="pt-form-content">
        <DatePicker
          name="DateField"
          model={article}
          type="date"
          validate={required}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("DateField")
          })}
          placeholder="DateField"
        />
        {article.hasVisibleErrors("DateField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("DateField")}
          </div>
        )}
      </Col>
    </Row>

    <Row className="pt-form-group">
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

    <Row className="pt-form-group">
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

    <Row className="pt-form-group">
      <Col md={3}>CheckBox [normal | disabled] </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline />
      </Col>
      <Col md={3}>
        <CheckBox name="BooleanField" model={article} inline disabled />
      </Col>
    </Row>

    <Row className="pt-form-group">
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
      className={cn("pt-form-group", {
        "pt-intent-danger": article.hasVisibleErrors("ArrayField")
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
      <Col md={3} className="pt-form-content">
        <Select
          name="ArrayField"
          model={article}
          validate={[required, maxCount(1)]}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("ArrayField")
          })}
          placeholder="ArrayField"
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]}
          multiple
        />
        {article.hasVisibleErrors("ArrayField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("ArrayField")}
          </div>
        )}
      </Col>
    </Row>

    <Row
      className={cn("pt-form-group", {
        "pt-intent-danger": article.hasVisibleErrors("EnumField")
      })}
    >
      <Col md={3}>RadioGroup [validation | disabled]</Col>
      <Col md={3}>
        <RadioGroup
          name="EnumField"
          model={article}
          placeholder="EnumField"
          inline
          validate={[required, pattern(/^[0-9]+$/)]}
          className={cn({
            "pt-intent-danger": article.hasVisibleErrors("EnumField")
          })}
          options={[
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" },
            { value: "third", label: "Третий", disabled: true }
          ]}
        />
        {article.hasVisibleErrors("EnumField") && (
          <div className="pt-form-helper-text">
            {article.getVisibleErrors("EnumField")}
          </div>
        )}
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

    <Row className="pt-form-group">
      <Col md={3}>TextArea [normal]</Col>
      <Col md>
        <TextArea name="TextField" model={article} placeholder="TextField" />
      </Col>
    </Row>

    <hr />

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

const ArticleEditorBlock = observer(() => (
  <div>
    <h4>ArticleEditor</h4>
    <hr />
  </div>
));

ReactDOM.render(
  <Grid fluid>
    <FormControlsBlock />
    <br />
    <ArticleEditorBlock />
  </Grid>,
  document.getElementById("library")
);
