/* eslint-disable */
const path = require("path");
const { sync: globSync } = require("glob");
const webpack = require("webpack");
const ForkTsCheckerWebpackPlugin = require("fork-ts-checker-webpack-plugin");
const { TsconfigPathsPlugin } = require("tsconfig-paths-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

const outDir = "wwwroot/js/bundles";
const relOutDir = "js/bundles";
const outPath = path.resolve(__dirname, outDir);
const viewsPath = path.resolve(__dirname, "Views");
const views = new Set(globSync(path.resolve(viewsPath, "**/*.cshtml")));
const entries = globSync(path.resolve(viewsPath, "**/*.@(js|ts)"))
  .filter((page) => views.has(page.slice(0, -3) + ".cshtml"))
  .concat(
    globSync(path.resolve(viewsPath, "**/*.@(jsx|tsx)")).filter((page) =>
      views.has(page.slice(0, -4) + ".cshtml"),
    ),
  )
  .reduce((entries, page) => {
    const name = /(.*)\.(js|ts|jsx|tsx)$/.exec(page.slice(viewsPath.length))[1];
    entries[name] = page;
    return entries;
  }, {});

module.exports = {
  entry: {
    "core-js/stable": require.resolve("core-js/stable"),
    "whatwg-fetch": require.resolve("whatwg-fetch"),
    ...entries,
  },
  resolve: {
    extensions: [
      ".js",
      ".ts",
      ".tsx",
      ".jsx",
      ".json",
      ".css",
      ".scss",
      ".svg",
    ],
    plugins: [
      new TsconfigPathsPlugin({
        extensions: [".js", ".ts", ".jsx", ".tsx"],
        configFile: path.resolve(__dirname, "tsconfig.json"),
      }),
    ],
  },
  output: {
    clean: true,
    globalObject: "this",
    filename: "[name].js",
    path: outPath,
  },
  ignoreWarnings: [/export .* was not found in/],
  optimization: {
    minimizer: [
      new TerserPlugin({
        parallel: true,
        terserOptions: {
          compress: {
            // inline is buggy as of uglify-es 3.3.9
            // https://github.com/mishoo/UglifyJS2/issues/2842
            inline: 1,
          },
        },
      }),
    ],
  },
  module: {
    rules: [
      {
        test: /\.(jpg|jpeg|png|gif)$/,
        type: "asset",
        parser: { dataUrlCondition: { maxSize: 10000 } },
        generator: { filename: "images/[name][ext]" },
      },
      {
        test: /\.(woff|woff2|eot|ttf|otf)$/,
        type: "asset",
        parser: { dataUrlCondition: { maxSize: 10000 } },
        generator: { filename: "fonts/[name][ext]" },
      },
    ],
  },
  plugins: [
    new ForkTsCheckerWebpackPlugin(),
    new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /ru|kk/),
    new webpack.DefinePlugin({
      "process.env.OUT_DIR": JSON.stringify(relOutDir),
    }),
  ],
};
