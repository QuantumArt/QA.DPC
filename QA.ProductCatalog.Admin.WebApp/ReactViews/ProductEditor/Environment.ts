__webpack_public_path__ = `${require("ProductEditor/Utils/Common").rootUrl}/${process.env.OUT_DIR}/`;
// do not use ES6 import here as it breaks __webpack_public_path__
// https://github.com/webpack/webpack/issues/2776
require("ProductEditor/Styles/FlexboxGrid.scss");
require("ProductEditor/Styles/NProgress.scss");
require("ProductEditor/Styles/Tabs.scss");
if (DEBUG) {
  require("mobx").configure({ enforceActions: "observed" });
}
