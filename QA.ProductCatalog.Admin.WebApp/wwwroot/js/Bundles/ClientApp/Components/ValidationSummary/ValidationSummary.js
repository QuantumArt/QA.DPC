import React from "react";
import "./ValidationSummay.scss";
export var ValidationSummay = function(_a) {
  var errors = _a.errors;
  return React.createElement(
    React.Fragment,
    null,
    errors.map(function(articleError, i) {
      return React.createElement(
        "section",
        { key: i, className: "validation-summary__block" },
        React.createElement(
          "header",
          null,
          React.createElement("b", null, articleError.ContentName, ":"),
          " ",
          articleError.ServerId
        ),
        articleError.ArticleErrors.length > 0 &&
          React.createElement(
            "span",
            { className: "validation-summary__message" },
            articleError.ArticleErrors.join(", ")
          ),
        articleError.FieldErrors.length > 0 &&
          React.createElement(
            "main",
            null,
            articleError.FieldErrors.map(function(fieldError, j) {
              return React.createElement(
                "div",
                { key: j },
                React.createElement("u", null, fieldError.Name, ":"),
                " ",
                React.createElement(
                  "span",
                  { className: "validation-summary__message" },
                  fieldError.Messages.join(", ")
                )
              );
            })
          )
      );
    })
  );
};
//# sourceMappingURL=ValidationSummary.js.map
