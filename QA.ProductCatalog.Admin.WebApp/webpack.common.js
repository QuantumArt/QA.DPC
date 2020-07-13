/* eslint-disable */
const path = require("path");
const ForkTsCheckerWebpackPlugin = require("fork-ts-checker-webpack-plugin");
const ForkTsCheckerNotifierWebpackPlugin = require("fork-ts-checker-notifier-webpack-plugin");
const webpack = require("webpack");
const CopyPlugin = require("copy-webpack-plugin");

const assetProcessing = outDir => ({
  loader: "url-loader",
  options: {
    limit: 10000,
    fallback: "file-loader",
    outputPath: outDir
  }
});

module.exports = {
  entry: ["core-js/stable", "whatwg-fetch", "./src/index.tsx"],
  resolve: {
    extensions: [".ts", ".tsx", ".js", ".json", ".css", ".scss", ".svg"],
    alias: {
      app: path.resolve(__dirname, "src/app/"),
      assets: path.resolve(__dirname, "src/assets/")
    }
  },
  output: {
    globalObject: "this"
  },
  optimization: {
    chunkIds: "named",
    splitChunks: {
      cacheGroups: {
        default: false,
        vendors: false,
        common: {
          name: "common",
          minChunks: 3,
          priority: 1,
          chunks: "async",
          reuseExistingChunk: true,
          enforce: true
        },
        vendor: {
          name: "vendor",
          test: /[\\/]node_modules[\\/]/,
          priority: 10
        }
      }
    },
    runtimeChunk: true,
    usedExports: true,
    providedExports: true
  },
  module: {
    rules: [
      {
        test: /\.(jpg|jpeg|png|gif)?$/,
        use: assetProcessing("images")
      },
      {
        test: /\.svg?$/,
        oneOf: [
          {
            issuer: /\.(scss|css)?$/,
            use: assetProcessing("images")
          },
          {
            issuer: /\.tsx?$/,
            use: "@svgr/webpack"
          }
        ]
      },
      {
        test: /\.(woff|woff2|eot|ttf|otf)?$/,
        use: assetProcessing("fonts")
      }
    ]
  },
  plugins: [
    new ForkTsCheckerWebpackPlugin({
      checkSyntacticErrors: true,
      silent: false,
      async: false
    }),
    new ForkTsCheckerNotifierWebpackPlugin({
      title: "TypeScript",
      excludeWarnings: false,
      skipFirstNotification: true
    })
    // new CopyPlugin([
    //   {
    //     from: './src/assets/images',
    //     to: 'images',
    //   },
    // ]),
  ]
};
