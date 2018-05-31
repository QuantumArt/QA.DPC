const path = require("path");
const glob = require("glob");
const webpack = require("webpack");
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const TsconfigPathsPlugin = require("tsconfig-paths-webpack-plugin");

const viewsDir = path.resolve(__dirname, "./Views");

const views = new Set(glob.sync(path.resolve(viewsDir, "**/*.cshtml")));

const entries = glob
  // all .js, .jsx, ts and .tsx files from ~/Views folder
  .sync(path.resolve(viewsDir, "**/*.@(js|ts)"))
  // that have .cshtml view with same name
  .filter(page => views.has(page.slice(0, -3) + ".cshtml"))
  .concat(
    glob
      .sync(path.resolve(viewsDir, "**/*.@(jsx|tsx)"))
      .filter(page => views.has(page.slice(0, -4) + ".cshtml"))
  )
  // grouped to dictionary by path relative to ~/Views folder
  .reduce((entries, page) => {
    const name = /(.*)\.(js|ts|jsx|tsx)$/.exec(page.slice(viewsDir.length))[1];
    entries[name] = page;
    return entries;
  }, {});

module.exports = (env, argv) => ({
  entry: entries,
  resolve: {
    extensions: [".js", ".ts", ".jsx", ".tsx"],
    plugins: [
      new TsconfigPathsPlugin({
        configFile: path.resolve(__dirname, "tsconfig.json")
      })
    ]
  },
  output: {
    filename: "[name].js",
    path: path.resolve(__dirname, "./Scripts/Bundles")
  },
  module: {
    rules: [
      {
        test: /\.(tsx?|jsx?)$/,
        include: /(ClientApp|Views)/,
        use: [
          {
            loader: "ts-loader",
            options: {
              // transpileOnly: argv.mode !== "production",
              configFile: path.resolve(__dirname, "tsconfig.json")
            }
          }
        ]
      },
      {
        test: /\.(scss|css)$/,
        use: [
          "style-loader",
          {
            loader: "css-loader",
            options: { minimize: argv.mode === "production" }
          },
          {
            loader: "postcss-loader",
            options: {
              plugins: [require("autoprefixer")]
            }
          },
          "sass-loader"
        ]
      },
      { test: /\.(png|jpg|jpeg|gif|svg)$/, use: "url-loader?limit=10000" }
    ]
  },
  plugins: [
    new webpack.DefinePlugin({
      "process.env.NODE_ENV": JSON.stringify(argv.mode)
    })
  ],
  optimization: {
    minimizer: [
      new UglifyJsPlugin({
        cache: true,
        parallel: true,
        sourceMap: true,
        uglifyOptions: {
          compress: {
            // inline is buggy as of uglify-es 3.3.9
            // https://github.com/mishoo/UglifyJS2/issues/2842
            inline: 1
          }
        }
      })
    ]
  }
});
