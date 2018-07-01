__webpack_public_path__ = `${document.head.getAttribute("root-url") || ""}/${process.env.OUT_DIR}/`;
// do not use ES6 import here as it breaks __webpack_public_path__
// https://github.com/webpack/webpack/issues/2776
require("normalize.css");
require("@blueprintjs/core/lib/css/blueprint.css");
require("@blueprintjs/icons/lib/css/blueprint-icons.css");
require("reflect-metadata");
require("mobx").configure({ enforceActions: true });
