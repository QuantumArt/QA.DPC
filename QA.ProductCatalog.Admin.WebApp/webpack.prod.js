/* eslint-disable */
const { merge } = require("webpack-merge");
const webpack = require("webpack");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCssAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const { BundleAnalyzerPlugin } = require("webpack-bundle-analyzer");
const path = require("path");
const threadLoader = require("thread-loader");
const common = require("./webpack.common");

const poolOptions = {
  workerParallelJobs: 50,
  poolTimeout: 2000,
  name: "Typescript",
  workerNodeArgs: ["--max-old-space-size=4096"]
};

threadLoader.warmup(poolOptions, ["ts-loader", "url-loader"]);

module.exports = merge(common, {
  mode: "production",
  devtool: "cheap-source-map",
  optimization: {
    minimize: true
  },
  module: {
    rules: [
      {
        test: /\.(tsx?|jsx?)$/,
        exclude: /node_modules/,
        include: /(Views|ReactViews)/,
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
          { loader: MiniCssExtractPlugin.loader },
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
      "process.env.NODE_ENV": JSON.stringify("production"),
      DEBUG: false
    }),
    new MiniCssExtractPlugin({
      filename: "../../css/[name].css",
      chunkFilename: "[id].[hash].css",
    }),
    new OptimizeCssAssetsPlugin({}),
    new BundleAnalyzerPlugin({
      analyzerMode: "static",
      openAnalyzer: false
    }),
  ]
});
