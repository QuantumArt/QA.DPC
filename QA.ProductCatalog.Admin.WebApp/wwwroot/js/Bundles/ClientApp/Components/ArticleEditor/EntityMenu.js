import React from "react";
import {
  Button,
  Menu,
  MenuItem,
  Intent,
  Popover,
  Position,
  Icon
} from "@blueprintjs/core";
import "./ArticleEditor.scss";
/** Выпадающее меню действий со статьей */
export var EntityMenu = function(_a) {
  var small = _a.small,
    onSave = _a.onSave,
    onDetach = _a.onDetach,
    onRemove = _a.onRemove,
    onRefresh = _a.onRefresh,
    onReload = _a.onReload,
    onClone = _a.onClone,
    onPublish = _a.onPublish,
    children = _a.children;
  return React.createElement(
    Popover,
    { position: Position.BOTTOM_RIGHT, interactionKind: "hover" },
    React.createElement(
      Button,
      {
        minimal: true,
        small: small,
        icon: "caret-down",
        intent: Intent.PRIMARY
      },
      "\u0414\u0435\u0439\u0441\u0442\u0432\u0438\u044F"
    ),
    React.createElement(
      Menu,
      null,
      onSave &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "floppy-disk" }),
          intent: Intent.PRIMARY,
          onClick: onSave,
          text: "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C",
          title:
            "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C \u0442\u0435\u043A\u0443\u0449\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E"
        }),
      onClone &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "duplicate" }),
          intent: Intent.SUCCESS,
          onClick: onClone,
          text:
            "\u041A\u043B\u043E\u043D\u0438\u0440\u043E\u0432\u0430\u0442\u044C",
          title:
            "\u041A\u043B\u043E\u043D\u0438\u0440\u043E\u0432\u0430\u0442\u044C \u0442\u0435\u043A\u0443\u0449\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E"
        }),
      onRefresh &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "refresh" }),
          intent: Intent.WARNING,
          onClick: onRefresh,
          text: "\u041E\u0431\u043D\u043E\u0432\u0438\u0442\u044C",
          title:
            "\u0417\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430 \u0441 \u0441\u0435\u0440\u0432\u0435\u0440\u0430 (\u043D\u0435\u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u043D\u044B\u0435 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u043F\u043E\u043B\u0435\u0439 \u043E\u0441\u0442\u0430\u043D\u0443\u0442\u0441\u044F)"
        }),
      onReload &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, {
            icon: "automatic-updates"
          }),
          intent: Intent.WARNING,
          onClick: onReload,
          text:
            "\u041F\u0435\u0440\u0435\u0437\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C",
          title:
            "\u041F\u0435\u0440\u0435\u0437\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C \u0447\u0430\u0441\u0442\u044C \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430 \u0441 \u0441\u0435\u0440\u0432\u0435\u0440\u0430 (\u043D\u0435\u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u043D\u044B\u0435 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u044F \u043F\u043E\u043B\u0435\u0439 \u043F\u043E\u0442\u0435\u0440\u044F\u044E\u0442\u0441\u044F)"
        }),
      onRemove &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "delete" }),
          intent: Intent.DANGER,
          onClick: onRemove,
          text: "\u0423\u0434\u0430\u043B\u0438\u0442\u044C",
          title:
            "\u0423\u0434\u0430\u043B\u0438\u0442\u044C (\u0430\u0440\u0445\u0438\u0432\u0438\u0440\u043E\u0432\u0430\u0442\u044C) \u0442\u0435\u043A\u0443\u0449\u0443\u044E \u0441\u0442\u0430\u0442\u044C\u044E"
        }),
      onDetach &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "remove" }),
          intent: Intent.DANGER,
          onClick: onDetach,
          text: "\u041E\u0442\u0432\u044F\u0437\u0430\u0442\u044C",
          title:
            "\u0423\u0434\u0430\u043B\u0438\u0442\u044C \u0441\u0432\u044F\u0437\u044C \u0441 \u0442\u0435\u043A\u0443\u0449\u0435\u0439 \u0441\u0442\u0430\u0442\u044C\u0435\u0439"
        }),
      onPublish &&
        React.createElement(MenuItem, {
          labelElement: React.createElement(Icon, { icon: "share" }),
          intent: Intent.NONE,
          onClick: onPublish,
          text:
            "\u041E\u043F\u0443\u0431\u043B\u0438\u043A\u043E\u0432\u0430\u0442\u044C",
          title:
            "\u041E\u043F\u0443\u0431\u043B\u0438\u043A\u043E\u0432\u0430\u0442\u044C \u0447\u0430\u0441\u0442\u044C \u043F\u0440\u043E\u0434\u0443\u043A\u0442\u0430"
        }),
      children
    )
  );
};
//# sourceMappingURL=EntityMenu.js.map
