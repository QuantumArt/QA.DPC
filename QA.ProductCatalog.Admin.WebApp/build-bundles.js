/* eslint-disable */
"use strict";

const fs = require("fs");
const path = require("path");
const postcss = require("postcss");
const cssnano = require("cssnano");
const { minify } = require("terser");

const root = __dirname;
const dev = process.argv.includes("--dev");

function read(file) {
  return fs.readFileSync(path.resolve(root, file), "utf8");
}

function write(file, content) {
  const dest = path.resolve(root, file);
  fs.mkdirSync(path.dirname(dest), { recursive: true });
  fs.writeFileSync(dest, content);
  console.log("  →", file);
}

async function buildCSS(outputFile, inputFiles) {
  const combined = inputFiles.map(read).join("\n");
  if (dev) {
    write(outputFile, combined);
    return;
  }
  const result = await postcss([cssnano({ preset: "default" })]).process(
    combined,
    { from: undefined },
  );
  write(outputFile, result.css);
}

async function buildJS(outputFile, inputFiles, alreadyMinified = false) {
  const combined = inputFiles.map(read).join("\n");
  if (dev || alreadyMinified) {
    write(outputFile, combined);
    return;
  }
  const result = await minify(combined);
  write(outputFile, result.code);
}

function copyDir(srcDir, destDir) {
  const src = path.resolve(root, srcDir);
  const dest = path.resolve(root, destDir);
  fs.mkdirSync(dest, { recursive: true });
  for (const file of fs.readdirSync(src)) {
    fs.copyFileSync(path.join(src, file), path.join(dest, file));
  }
  console.log("  →", destDir);
}

async function main() {
  console.log(`Building bundles (${dev ? "dev" : "prod"})...`);

  copyDir(
    "node_modules/@fortawesome/fontawesome-free/webfonts",
    "wwwroot/css/webfonts",
  );

  await buildCSS("wwwroot/css/blueprint.min.css", [
    "node_modules/normalize.css/normalize.css",
    "node_modules/@blueprintjs/core/lib/css/blueprint.css",
    "wwwroot/resources/fa.css",
    "wwwroot/resources/blueprint-icons.css",
  ]);

  await buildCSS("wwwroot/css/codemirror.min.css", [
    "wwwroot/js/codemirror/lib/codemirror.css",
    "wwwroot/js/codemirror/addon/fold/foldgutter.css",
    "wwwroot/css/XmlViewer.css",
  ]);

  await buildJS(
    "wwwroot/js/bundles/jquery.min.js",
    [
      "node_modules/jquery/dist/jquery.min.js",
      "node_modules/jquery.scrollto/jquery.scrollTo.min.js",
    ],
    true, // already minified, just concatenate
  );

  await buildJS("wwwroot/js/bundles/scripts.min.js", [
    "wwwroot/js/pmrpc.js",
    "wwwroot/js/qp/QP8BackendApi.Interaction.js",
    "wwwroot/js/qp/QA.Utils.js",
    "wwwroot/js/qp/QA.Integration.js",
  ]);

  await buildJS("wwwroot/js/bundles/codemirror.min.js", [
    "wwwroot/js/codemirror/lib/codemirror.js",
    "wwwroot/js/codemirror/addon/fold/xml-fold.js",
    "wwwroot/js/codemirror/addon/fold/brace-fold.js",
    "wwwroot/js/codemirror/addon/edit/matchtags.js",
    "wwwroot/js/codemirror/mode/xml/xml.js",
    "wwwroot/js/codemirror/mode/javascript/javascript.js",
    "wwwroot/js/codemirror/addon/fold/foldcode.js",
    "wwwroot/js/codemirror/addon/fold/foldgutter.js",
  ]);

  console.log("Done.");
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
