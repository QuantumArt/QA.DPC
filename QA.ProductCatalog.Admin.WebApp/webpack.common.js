/* eslint-disable */
const path = require("path");
const glob = require("glob");
const webpack = require("webpack");
const ForkTsCheckerWebpackPlugin = require("fork-ts-checker-webpack-plugin");
const ForkTsCheckerNotifierWebpackPlugin = require("fork-ts-checker-notifier-webpack-plugin");
const CleanWebpackPlugin = require("clean-webpack-plugin");
const { TsconfigPathsPlugin } = require("tsconfig-paths-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

const outDir = "wwwroot/js/bundles";
const relOutDir = "js/bundles";
const outPath = path.resolve(__dirname, outDir);
const viewsPath = path.resolve(__dirname, "Views");
const views = new Set(glob.sync(path.resolve(viewsPath, "**/*.cshtml")));
const entries = glob
  // all .js, .jsx, ts and .tsx files from ~/Views folder
  .sync(path.resolve(viewsPath, "**/*.@(js|ts)"))
  // that have .cshtml view with same name
  .filter(page => views.has(page.slice(0, -3) + ".cshtml"))
  .concat(
    glob
      .sync(path.resolve(viewsPath, "**/*.@(jsx|tsx)"))
      .filter(page => views.has(page.slice(0, -4) + ".cshtml"))
  )
  // grouped to dictionary by path relative to ~/Views folder
  .reduce((entries, page) => {
    const name = /(.*)\.(js|ts|jsx|tsx)$/.exec(page.slice(viewsPath.length))[1];
    entries[name] = page;
    return entries;
  }, {});

const assetProcessing = outDir => ({
  loader: "url-loader",
  options: {
    limit: 10000,
    fallback: "file-loader",
    outputPath: outDir
  }
});

module.exports = {
  entry: {
    "core-js/stable": require.resolve("core-js/stable"),
    "whatwg-fetch": require.resolve("whatwg-fetch"),
    ...entries
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
      ".svg"
    ],
    plugins: [
      new TsconfigPathsPlugin({
        extensions: [".js", ".ts", ".jsx", ".tsx"],
        configFile: path.resolve(__dirname, "tsconfig.json")
      })
    ]
    // alias: {
    //   app: path.resolve(__dirname, "src/app/"),
    //   assets: path.resolve(__dirname, "src/assets/")
    // }
  },
  output: {
    globalObject: "this",
    filename: "[name].js",
    path: outPath
  },
  optimization: {
    minimizer: [
      new TerserPlugin({
        cache: true,
        parallel: true,
        sourceMap: true,
        terserOptions: {
          compress: {
            // inline is buggy as of uglify-es 3.3.9
            // https://github.com/mishoo/UglifyJS2/issues/2842
            inline: 1
          }
        }
      })
    ]
  },
  module: {
    rules: [
      {
        test: /\.(jpg|jpeg|png|gif)?$/,
        use: assetProcessing("images")
      },
      // {
      //   test: /\.svg?$/,
      //   oneOf: [
      //     {
      //       issuer: /\.(scss|css)?$/,
      //       use: assetProcessing("images")
      //     },
      //     {
      //       issuer: /\.tsx?$/,
      //       use: "@svgr/webpack"
      //     }
      //   ]
      // },
      {
        test: /\.(woff|woff2|eot|ttf|otf)?$/,
        use: assetProcessing("fonts")
      }
    ]
  },
  plugins: [
    new CleanWebpackPlugin([outPath]),
    new ForkTsCheckerWebpackPlugin(),
    new ForkTsCheckerNotifierWebpackPlugin({
      title: "TypeScript",
      excludeWarnings: true,
      skipFirstNotification: false
    }),
    new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /ru|kk/),
    new webpack.DefinePlugin({
      "process.env.OUT_DIR": JSON.stringify(relOutDir)
    })
  ]
};
