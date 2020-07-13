/* eslint-disable */
const merge = require("webpack-merge");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCssAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const { BundleAnalyzerPlugin } = require("webpack-bundle-analyzer");
const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
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
  output: {
    filename: "bundle.[hash].js",
    path: path.join(__dirname, "dist/app")
  },
  optimization: {
    minimize: true
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        exclude: /node_modules/,
        use: [
          {
            loader: "thread-loader",
            options: poolOptions
          },
          {
            loader: "ts-loader",
            options: {
              transpileOnly: true,
              happyPackMode: true
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
    new MiniCssExtractPlugin({
      filename: "[name].[hash].css",
      chunkFilename: "[id].[hash].css"
    }),
    new OptimizeCssAssetsPlugin({}),
    new BundleAnalyzerPlugin({
      analyzerMode: "static",
      openAnalyzer: false
    }),
    new HtmlWebpackPlugin({
      title: "Beeline IoT",
      baseHref: "/",
      template: "./src/assets/index.html"
    })
  ]
});
