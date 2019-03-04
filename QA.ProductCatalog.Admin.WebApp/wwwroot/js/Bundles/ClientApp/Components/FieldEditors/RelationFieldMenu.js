import React from "react";
import { ButtonGroup, Button, Intent } from "@blueprintjs/core";
/** Строка кнопок-действий с полем-связью */
export var RelationFieldMenu = function(_a) {
  var onCreate = _a.onCreate,
    onClonePrototype = _a.onClonePrototype,
    onSelect = _a.onSelect,
    onClear = _a.onClear,
    onReload = _a.onReload,
    children = _a.children;
  return React.createElement(
    ButtonGroup,
    null,
    onCreate &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          rightIcon: "add",
          intent: Intent.SUCCESS,
          onClick: onCreate,
          title:
            "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u043F\u0443\u0441\u0442\u0443\u044E \u0441\u0432\u044F\u0437\u0430\u043D\u043D\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E \u0441 \u043D\u0443\u043B\u044F"
        },
        "\u0421\u043E\u0437\u0434\u0430\u0442\u044C"
      ),
    onClonePrototype &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          rightIcon: "add",
          intent: Intent.SUCCESS,
          onClick: onClonePrototype,
          title:
            "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u0441\u0432\u044F\u0437\u0430\u043D\u043D\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E \u043F\u043E \u043E\u0431\u0440\u0430\u0437\u0446\u0443"
        },
        "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u043F\u043E \u043E\u0431\u0440\u0430\u0437\u0446\u0443"
      ),
    onSelect &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          rightIcon: "th-derived",
          intent: Intent.PRIMARY,
          onClick: onSelect,
          title:
            "\u0412\u044B\u0431\u0440\u0430\u0442\u044C \u043E\u0434\u043D\u0443 \u0438\u043B\u0438 \u043D\u0435\u0441\u043A\u043E\u043B\u044C\u043A\u043E \u0441\u0442\u0430\u0442\u0435\u0439 \u0438 \u0441\u0432\u044F\u0437\u0430\u0442\u044C \u0441 \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u043E\u043C"
        },
        "\u0412\u044B\u0431\u0440\u0430\u0442\u044C"
      ),
    onClear &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          rightIcon: "eraser",
          intent: Intent.DANGER,
          onClick: onClear,
          title:
            "\u041E\u0447\u0438\u0441\u0442\u0438\u0442\u044C \u0441\u0432\u044F\u0437\u044C \u0441\u0442\u0430\u0442\u044C\u0438 (\u043D\u0435\u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u043D\u044B\u0435 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u043F\u043E\u043B\u0435\u0439 \u043F\u043E\u0442\u0435\u0440\u044F\u044E\u0442\u0441\u044F)"
        },
        "\u041E\u0447\u0438\u0441\u0442\u0438\u0442\u044C"
      ),
    onReload &&
      React.createElement(
        Button,
        {
          minimal: true,
          small: true,
          rightIcon: "automatic-updates",
          intent: Intent.WARNING,
          onClick: onReload,
          title:
            "\u041F\u0435\u0440\u0435\u0437\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C \u0441\u0432\u044F\u0437\u044C \u0441\u0442\u0430\u0442\u044C\u0438 \u0441 \u0441\u0435\u0440\u0432\u0435\u0440\u0430 (\u043D\u0435\u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u043D\u044B\u0435 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u043F\u043E\u043B\u0435\u0439 \u043F\u043E\u0442\u0435\u0440\u044F\u044E\u0442\u0441\u044F)"
        },
        "\u041F\u0435\u0440\u0435\u0437\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C"
      ),
    children
  );
};
//# sourceMappingURL=RelationFieldMenu.js.map
