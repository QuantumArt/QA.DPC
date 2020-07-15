/* eslint-disable */
const path = require("path");
const { merge } = require("webpack-merge");
const webpack = require("webpack");
const common = require("./webpack.common");
const threadLoader = require("thread-loader");

const poolOptions = {
  workerParallelJobs: 50,
  poolTimeout: 2000,
  name: "Typescript",
  workerNodeArgs: ["--max-old-space-size=4096"]
};

threadLoader.warmup(poolOptions, ["ts-loader", "url-loader"]);

module.exports = merge(common, {
  mode: "development",
  devtool: "eval-source-map",
  module: {
    rules: [
      {
        test: /\.(tsx?|jsx?)$/,
        exclude: /node_modules/,
        include: /(ClientApp|Views|ReactViews)/,
        use: [
          {
            loader: "thread-loader",
            options: poolOptions
          },
          {
            loader: "ts-loader",
            options: {
              transpileOnly: true,
              happyPackMode: true,
              configFile: path.resolve(__dirname, "tsconfig.json"),
              logLevel: "error"
            }
          }
        ]
      },
      {
        test: /\.(scss|css)?$/,
        use: [
          { loader: "style-loader" },
          {
            loader: "css-loader",
            options: {
              importLoaders: 1
            }
          },
          { loader: "postcss-loader" },
          { loader: "resolve-url-loader" },
          { loader: "sass-loader", options: { sourceMap: true } }
        ]
      }
    ]
  },
  plugins: [
    new webpack.DefinePlugin({
      "process.env.NODE_ENV": JSON.stringify("development"),
      DEBUG: true
    }),
    new webpack.WatchIgnorePlugin([/\.js$/, /\.d\.ts$/])
  ]
});
