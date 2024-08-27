module.exports = {
  presets: [
    '@vue/cli-plugin-babel/preset',
    '@babel/preset-env',
    '@babel/preset-react',
  ],
  plugins: [
    ['module-resolver', {
      alias: {
        '@': './src',
      },
    }],
  ],
};
