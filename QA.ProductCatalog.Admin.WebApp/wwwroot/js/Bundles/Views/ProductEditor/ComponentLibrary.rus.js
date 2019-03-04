import React from "react";
export default {
  "User Card":
    "\u041A\u0430\u0440\u0442\u043E\u0447\u043A\u0430 \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u044F",
  "First Name": "\u0418\u043C\u044F",
  "Last Name": "\u0424\u0430\u043C\u0438\u043B\u0438\u044F",
  "Full Name": "\u041F\u043E\u043B\u043D\u043E\u0435 \u0438\u043C\u044F",
  "Test...": "\u0422\u0435\u0441\u0442...",
  "Hello, ${name}!": function(name) {
    return "\u041F\u0440\u0438\u0432\u0435\u0442, " + name + "!";
  },
  helloTemplate: function(name) {
    return (
      "\u0417\u0434\u0440\u0430\u0432\u0441\u0442\u0432\u0443\u0439, " +
      name +
      "!"
    );
  },
  customMarkup: function(_a) {
    var firstName = _a.firstName,
      lastName = _a.lastName,
      fullName = _a.fullName;
    return React.createElement(
      "article",
      null,
      "\u041A\u0430\u0440\u0442\u043E\u0447\u043A\u0430 \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u044F",
      React.createElement("div", null, "\u0418\u043C\u044F: ", firstName),
      React.createElement(
        "div",
        null,
        "\u0424\u0430\u043C\u0438\u043B\u0438\u044F: ",
        lastName
      ),
      React.createElement(
        "div",
        null,
        "\u041F\u043E\u043B\u043D\u043E\u0435 \u0438\u043C\u044F: ",
        fullName
      )
    );
  }
};
//# sourceMappingURL=ComponentLibrary.rus.js.map
