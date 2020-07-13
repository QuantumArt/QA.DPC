/* eslint-disable */
const merge = require("webpack-merge");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const common = require("./webpack.common");
const threadLoader = require("thread-loader");

const poolOptions = {
  workerParallelJobs: 50,
  poolTimeout: Infinity,
  name: "Typescript",
  workerNodeArgs: ["--max-old-space-size=4096"]
};

threadLoader.warmup(poolOptions, ["ts-loader", "url-loader"]);

module.exports = merge(common, {
  mode: "development",
  devtool: "source-map",
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
  devServer: {
    hot: true,
    port: 3001,
    open: true,
    historyApiFallback: {
      disableDotRule: true
    },
    stats: "minimal",
    clientLogLevel: "warning"
  },
  node: {
    // workaround for webpack-dev-server issue
    // https://github.com/webpack/webpack-dev-server/issues/60#issuecomment-103411179
    fs: "empty",
    net: "empty"
  },
  plugins: [
    new HtmlWebpackPlugin({
      title: "Beeline IoT",
      baseHref: "/",
      template: "./src/assets/index.html"
    })
  ]
});
