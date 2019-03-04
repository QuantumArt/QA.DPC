import * as tslib_1 from "tslib";
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
import { LocaleContext, Translation, withTranslation } from "react-lazy-i18n";
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
var App = /** @class */ (function(_super) {
  tslib_1.__extends(App, _super);
  function App() {
    var _this = (_super !== null && _super.apply(this, arguments)) || this;
    _this.state = { lang: "eng" };
    return _this;
  }
  App.prototype.render = function() {
    var _this = this;
    var firstName = "John";
    var lastName = "Doe";
    var fullName = "John Doe";
    var lang = this.state.lang;
    return React.createElement(
      React.Fragment,
      null,
      React.createElement(
        "label",
        null,
        "English",
        " ",
        React.createElement("input", {
          type: "radio",
          value: "eng",
          checked: lang === "eng",
          onChange: function() {
            return _this.setState({ lang: "eng" });
          }
        })
      ),
      " ",
      React.createElement(
        "label",
        null,
        "\u0420\u0443\u0441\u0441\u043A\u0438\u0439",
        " ",
        React.createElement("input", {
          type: "radio",
          value: "rus",
          checked: lang === "rus",
          onChange: function() {
            return _this.setState({ lang: "rus" });
          }
        })
      ),
      React.createElement(Divider, null),
      React.createElement(
        LocaleContext.Provider,
        { value: lang },
        React.createElement("br", null),
        React.createElement(
          Translation,
          {
            load: function(lang) {
              return import(/* webpackChunkName: "i18n-" */ "./ComponentLibrary." +
                lang +
                ".jsx");
            }
          },
          function(tr) {
            return React.createElement(
              "article",
              {
                title: tr(
                  templateObject_1 ||
                    (templateObject_1 = tslib_1.__makeTemplateObject(
                      ["Hello, ", "!"],
                      ["Hello, ", "!"]
                    )),
                  firstName
                )
              },
              "locale: ",
              tr.locale,
              React.createElement("br", null),
              tr(
                templateObject_2 ||
                  (templateObject_2 = tslib_1.__makeTemplateObject(
                    ["User Card"],
                    ["User Card"]
                  ))
              ),
              React.createElement(
                "div",
                null,
                tr(
                  templateObject_3 ||
                    (templateObject_3 = tslib_1.__makeTemplateObject(
                      ["First Name"],
                      ["First Name"]
                    ))
                ),
                ": ",
                firstName
              ),
              React.createElement(
                "div",
                { title: tr("helloTemplate", lastName) },
                tr(
                  templateObject_4 ||
                    (templateObject_4 = tslib_1.__makeTemplateObject(
                      ["Last Name"],
                      ["Last Name"]
                    ))
                ),
                ": ",
                lastName
              ),
              React.createElement(
                "div",
                { title: tr("missingKey") || "Fallback" },
                tr(
                  templateObject_5 ||
                    (templateObject_5 = tslib_1.__makeTemplateObject(
                      ["Full Name"],
                      ["Full Name"]
                    ))
                ),
                ": ",
                fullName
              )
            );
          }
        ),
        React.createElement(Divider, null),
        React.createElement(LocalizedComponent, null),
        React.createElement(Divider, null),
        React.createElement("br", null)
      )
    );
  };
  return App;
})(React.Component);
var LocalizedComponent = /** @class */ (function(_super) {
  tslib_1.__extends(LocalizedComponent, _super);
  function LocalizedComponent() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  LocalizedComponent.prototype.render = function() {
    /** @type {Translate} */
    var tr = this.props.tr;
    var firstName = "Foo";
    var lastName = "Bar";
    var fullName = "Foo Bar";
    return (
      tr("customMarkup", {
        firstName: firstName,
        lastName: lastName,
        fullName: fullName
      }) ||
      React.createElement(
        "article",
        {
          title: tr(
            templateObject_6 ||
              (templateObject_6 = tslib_1.__makeTemplateObject(
                ["Hello, ", "!"],
                ["Hello, ", "!"]
              )),
            firstName
          )
        },
        "locale: ",
        tr.locale,
        React.createElement("br", null),
        tr(
          templateObject_7 ||
            (templateObject_7 = tslib_1.__makeTemplateObject(
              ["User Card"],
              ["User Card"]
            ))
        ),
        React.createElement(
          "div",
          null,
          tr(
            templateObject_8 ||
              (templateObject_8 = tslib_1.__makeTemplateObject(
                ["First Name"],
                ["First Name"]
              ))
          ),
          ": ",
          firstName
        ),
        React.createElement(
          "div",
          { title: tr("helloTemplate", lastName) },
          tr(
            templateObject_9 ||
              (templateObject_9 = tslib_1.__makeTemplateObject(
                ["Last Name"],
                ["Last Name"]
              ))
          ),
          ": ",
          lastName
        ),
        React.createElement(
          "div",
          { title: tr("missingKey") || "Fallback" },
          tr(
            templateObject_10 ||
              (templateObject_10 = tslib_1.__makeTemplateObject(
                ["Full Name"],
                ["Full Name"]
              ))
          ),
          ": ",
          fullName
        )
      )
    );
  };
  LocalizedComponent = tslib_1.__decorate(
    [
      withTranslation(function(lang) {
        return import(/* webpackChunkName: "i18n-" */ "./ComponentLibrary." +
          lang +
          ".jsx");
      })
    ],
    LocalizedComponent
  );
  return LocalizedComponent;
})(React.Component);
// const LocalizedHook = () => {
//   const tr = useTranslate(lang =>
//     import(/* webpackChunkName: "i18n-" */ `./ComponentLibrary.${lang}.jsx`)
//   );
//   const firstName = "Foo";
//   const lastName = "Bar";
//   const fullName = "Foo Bar";
//   return (
//     <article title={tr`Hello, ${firstName}!`}>
//       {tr`User Card`}
//       <div>
//         {tr`First Name`}: {firstName}
//       </div>
//       <div title={tr("helloTemplate", lastName)}>
//         {tr`Last Name`}: {lastName}
//       </div>
//       <div title={tr("missingKey") || "Fallback"}>
//         {tr`Full Name`}: {fullName}
//       </div>
//     </article>
//   );
// };
ReactDOM.render(
  React.createElement(
    Grid,
    { fluid: true },
    React.createElement("h4", null, "react-lazy-i18n"),
    React.createElement(App, null)
  ),
  document.getElementById("translate")
);
var Category = t
  .model("Category", {
    Id: t.identifierNumber,
    StringField: t.maybeNull(t.string)
  })
  .extend(validationMixin);
var Article = t
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
var article = Article.create({
  Id: 10,
  StringField: "foo",
  PhoneField: "",
  NumericField: 0,
  EnumField: "first",
  ArrayField: ["first"]
});
unprotect(article);
var category = Category.create({
  Id: 1,
  StringField: ""
});
unprotect(category);
configure({ enforceActions: "never" });
article.Category = category;
// prettier-ignore
var phoneMask = ["+", "7", " ", "(", /\d/, /\d/, /\d/, ")", " ", /\d/, /\d/, /\d/, "-", /\d/, /\d/, /\d/, /\d/];
var FormControlsBlock = observer(function() {
  return React.createElement(
    "div",
    null,
    React.createElement("h4", null, "FormControls"),
    React.createElement(Divider, null),
    React.createElement(
      Row,
      {
        className: cn("bp3-form-group", {
          "bp3-intent-danger": article.hasVisibleErrors("StringField")
        })
      },
      React.createElement(Col, { md: 3 }, "InputText [ pattern | required]"),
      React.createElement(
        Col,
        { md: 3, className: "bp3-form-content" },
        React.createElement(
          Validate,
          {
            model: article,
            name: "StringField",
            errorClassName: "bp3-form-helper-text",
            rules: [required, pattern(/^[A-Za-z0-9]+$/)]
          },
          React.createElement(InputText, {
            name: "StringField",
            model: article,
            placeholder: "StringField",
            className: cn({
              "bp3-intent-danger": article.hasVisibleErrors("StringField")
            })
          })
        )
      ),
      React.createElement(
        Col,
        { md: 3, className: "bp3-form-content" },
        React.createElement(InputText, {
          name: "StringField",
          model: article,
          placeholder: "StringField",
          className: cn({
            "bp3-intent-danger": article.hasVisibleErrors("StringField")
          })
        }),
        React.createElement(Validate, {
          model: article,
          name: "StringField",
          errorClassName: "bp3-form-helper-text",
          rules: required
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "InputText [normal | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputText, {
          name: "StringField",
          model: article,
          placeholder: "StringField"
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputText, {
          name: "StringField",
          model: article,
          placeholder: "StringField",
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "InputText [mask | readonly]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputText, {
          name: "PhoneField",
          model: article,
          placeholder: "PhoneField",
          mask: phoneMask
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputText, {
          name: "PhoneField",
          model: article,
          placeholder: "PhoneField",
          readOnly: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "InputNumber [normal | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputNumber, {
          name: "NumericField",
          model: article,
          placeholder: "NumericField"
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputNumber, {
          name: "NumericField",
          model: article,
          placeholder: "NumericField",
          isInteger: true,
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "InputSearch [normal | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputSearch, {
          name: "SearchField",
          model: article,
          placeholder: "SearchField"
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputSearch, {
          name: "SearchField",
          model: article,
          placeholder: "SearchField",
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      {
        className: cn("bp3-form-group", {
          "bp3-intent-danger": article.hasVisibleErrors("FileField")
        })
      },
      React.createElement(Col, { md: 3 }, "InputFile [required | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputFile, {
          name: "FileField",
          model: article,
          placeholder: "FileField",
          className: cn({
            "bp3-intent-danger": article.hasVisibleErrors("FileField")
          })
        }),
        React.createElement(Validate, {
          model: article,
          name: "FileField",
          errorClassName: "bp3-form-helper-text",
          rules: required
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(InputFile, {
          name: "FileField",
          model: article,
          placeholder: "FileField",
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      {
        className: cn("bp3-form-group", {
          "bp3-intent-danger": article.hasVisibleErrors("DateField")
        })
      },
      React.createElement(Col, { md: 3 }, "DatePicker [date | date required]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(DatePicker, {
          name: "DateField",
          model: article,
          type: "date",
          placeholder: "DateField"
        })
      ),
      React.createElement(
        Col,
        { md: 3, className: "bp3-form-content" },
        React.createElement(DatePicker, {
          name: "DateField",
          model: article,
          type: "date",
          className: cn({
            "bp3-intent-danger": article.hasVisibleErrors("DateField")
          }),
          placeholder: "DateField"
        }),
        React.createElement(Validate, {
          model: article,
          name: "DateField",
          errorClassName: "bp3-form-helper-text",
          rules: required
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "DatePicker [time | time disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(DatePicker, {
          name: "TimeField",
          model: article,
          type: "time",
          placeholder: "TimeField"
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(DatePicker, {
          name: "TimeField",
          model: article,
          type: "time",
          placeholder: "TimeField",
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "DatePicker [normal | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(DatePicker, {
          name: "DateTimeField",
          model: article,
          placeholder: "DateTimeField"
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(DatePicker, {
          name: "DateTimeField",
          model: article,
          placeholder: "DateTimeField",
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "CheckBox [normal | disabled] "),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(CheckBox, {
          name: "BooleanField",
          model: article,
          inline: true
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(CheckBox, {
          name: "BooleanField",
          model: article,
          inline: true,
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "Select [normal | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(Select, {
          name: "EnumField",
          model: article,
          placeholder: "EnumField",
          options: [
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ]
        })
      ),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(Select, {
          name: "EnumField",
          model: article,
          placeholder: "EnumField",
          options: [
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ],
          disabled: true
        })
      )
    ),
    React.createElement(
      Row,
      {
        className: cn("bp3-form-group", {
          "bp3-intent-danger": article.hasVisibleErrors("ArrayField")
        })
      },
      React.createElement(Col, { md: 3 }, "Select [required | multiple]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(Select, {
          name: "EnumField",
          model: article,
          placeholder: "EnumField",
          options: [
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ],
          required: true
        })
      ),
      React.createElement(
        Col,
        { md: 3, className: "bp3-form-content" },
        React.createElement(Select, {
          name: "ArrayField",
          model: article,
          className: cn({
            "bp3-intent-danger": article.hasVisibleErrors("ArrayField")
          }),
          placeholder: "ArrayField",
          options: [
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" }
          ],
          multiple: true
        }),
        React.createElement(Validate, {
          model: article,
          name: "ArrayField",
          errorClassName: "bp3-form-helper-text",
          rules: [required, maxCount(1)]
        })
      )
    ),
    React.createElement(
      Row,
      {
        className: cn("bp3-form-group", {
          "bp3-intent-danger": article.hasVisibleErrors("EnumField")
        })
      },
      React.createElement(Col, { md: 3 }, "RadioGroup [validation | disabled]"),
      React.createElement(
        Col,
        { md: 3 },
        React.createElement(RadioGroup, {
          name: "EnumField",
          model: article,
          placeholder: "EnumField",
          inline: true,
          className: cn({
            "bp3-intent-danger": article.hasVisibleErrors("EnumField")
          }),
          options: [
            { value: "first", label: "Первый" },
            { value: "second", label: "Второй" },
            { value: "third", label: "Третий", disabled: true }
          ]
        }),
        React.createElement(Validate, {
          model: article,
          name: "EnumField",
          errorClassName: "bp3-form-helper-text",
          rules: [required, pattern(/^[0-9]+$/)]
        })
      ),
      React.createElement(
        Col,
        { md: 6 },
        React.createElement(
          RadioGroup,
          {
            name: "EnumField",
            model: article,
            placeholder: "EnumField",
            inline: true,
            disabled: true
          },
          React.createElement(Radio, {
            label: "\u041F\u0435\u0440\u0432\u044B\u0439",
            value: "first"
          }),
          React.createElement(Radio, {
            label: "\u0412\u0442\u043E\u0440\u043E\u0439",
            value: "second"
          })
        )
      )
    ),
    React.createElement(
      Row,
      { className: "bp3-form-group" },
      React.createElement(Col, { md: 3 }, "TextArea [normal]"),
      React.createElement(
        Col,
        { md: true },
        React.createElement(TextArea, {
          name: "TextField",
          model: article,
          placeholder: "TextField"
        })
      )
    ),
    React.createElement(Divider, null),
    React.createElement(
      Row,
      null,
      React.createElement(
        Col,
        { md: true },
        React.createElement("div", null, "State:"),
        React.createElement("pre", null, JSON.stringify(toJS(article), null, 2))
      ),
      React.createElement(
        Col,
        { md: true },
        React.createElement("div", null, "All errors:"),
        React.createElement(
          "pre",
          null,
          JSON.stringify(toJS(article.getAllErrors()), null, 2)
        )
      ),
      React.createElement(
        Col,
        { md: true },
        React.createElement("div", null, "Visible errors:"),
        React.createElement(
          "pre",
          null,
          JSON.stringify(toJS(article.getAllVisibleErrors()), null, 2)
        )
      )
    )
  );
});
ReactDOM.render(
  React.createElement(
    Grid,
    { fluid: true },
    React.createElement(FormControlsBlock, null)
  ),
  document.getElementById("library")
);
var templateObject_1,
  templateObject_2,
  templateObject_3,
  templateObject_4,
  templateObject_5,
  templateObject_6,
  templateObject_7,
  templateObject_8,
  templateObject_9,
  templateObject_10;
//# sourceMappingURL=ComponentLibrary.js.map
