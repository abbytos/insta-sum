const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const VueLoaderPlugin = require('vue-loader/dist/plugin').default;
const CopyWebpackPlugin = require('copy-webpack-plugin'); 

module.exports = {
  mode: 'development', // or 'production'
  entry: {
    popup: './src/main.js', 
    background: './src/background.js',
    content: './src/content-script.js'
  },
  devtool: 'source-map',
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: '[name].bundle.js', // Output based on entry point names
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: './public/summary-template.html', // Path to your summary-template.html
      filename: 'summary-template.html', // Output HTML file
      inject: false, // Do not inject scripts
    }),
    new VueLoaderPlugin(), // Required for Vue loader
    new CopyWebpackPlugin({
      patterns: [
        { from: './public/assets', to: 'assets' }, // Copy assets directory
        { from: './public/manifest.json', to: 'manifest.json' }, // Copy manifest.json
        { from: './public/config.json', to: 'config.json' } // Copy manifest.json
        // Add more patterns as needed
      ]
    })
  ],
  module: {
    rules: [
      {
        test: /\.vue$/,
        loader: 'vue-loader'
      },
      {
        test: /\.js$/,
        loader: 'babel-loader',
        exclude: /node_modules/
      },
      {
        test: /\.css$/,
        use: ['style-loader', 'css-loader']
      },
      // Add other loaders if necessary
    ]
  },
  resolve: {
    alias: {
      'vue$': 'vue/dist/vue.esm-bundler.js'
    },
    extensions: ['.js', '.vue', '.json']
  }
};
