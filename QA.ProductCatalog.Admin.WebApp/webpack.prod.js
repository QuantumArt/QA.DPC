/* eslint-disable */
const { merge } = require("webpack-merge");
const webpack = require("webpack");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const { BundleAnalyzerPlugin } = require("webpack-bundle-analyzer");
const common = require("./webpack.common");

module.exports = merge(common, {
  mode: "production",
  devtool: "cheap-source-map",
  optimization: {
    minimize: true,
    minimizer: [new CssMinimizerPlugin()],
  },
  module: {
    rules: [
      {
        test: /\.(tsx?|jsx?)$/,
        exclude: /node_modules/,
        include: /(Views|ReactViews)/,
        use: [
          {
            loader: "ts-loader",
            options: {
              transpileOnly: true,
              logLevel: "error",
            },
          },
        ],
      },
      {
        test: /\.(scss|css)?$/,
        use: [
          { loader: MiniCssExtractPlugin.loader },
          {
            loader: "css-loader",
            options: {
              importLoaders: 1,
            },
          },
          { loader: "postcss-loader" },
          { loader: "resolve-url-loader" },
          { loader: "sass-loader", options: { sourceMap: true } },
        ],
      },
    ],
  },
  plugins: [
    new webpack.DefinePlugin({
      "process.env.NODE_ENV": JSON.stringify("production"),
      DEBUG: false,
    }),
    new MiniCssExtractPlugin({
      filename: "../../css/[name].css",
      chunkFilename: "[id].[contenthash].css",
    }),
    new BundleAnalyzerPlugin({
      analyzerMode: "static",
      openAnalyzer: false,
    }),
  ],
});
