import * as tslib_1 from "tslib";
import { observer } from "mobx-react";
import { AbstractEditor } from "./ArticleEditor";
import "./ArticleEditor.scss";
/** Компонент для отображения и редактирования статьи-расширения */
var ExtensionEditor = /** @class */ (function(_super) {
  tslib_1.__extends(ExtensionEditor, _super);
  function ExtensionEditor() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ExtensionEditor = tslib_1.__decorate([observer], ExtensionEditor);
  return ExtensionEditor;
})(AbstractEditor);
export { ExtensionEditor };
//# sourceMappingURL=ExtensionEditor.js.map
