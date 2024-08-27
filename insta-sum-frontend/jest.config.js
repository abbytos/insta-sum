const { defaults } = require('jest-config');

module.exports = {
  ...defaults,
  preset: '@vue/cli-plugin-unit-jest/presets/no-babel',
  moduleDirectories: ['node_modules', 'src'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
    // '^chrome$': '<rootDir>/src/__tests__/__mocks__/chrome.js',
  },
  transform: {
    '^.+\\.vue$': '@vue/vue3-jest',
    '^.+\\.(js|jsx|ts|tsx)$': 'babel-jest',
  },
  transformIgnorePatterns: [
    "/node_modules/(?!my-module-to-transform).+\\.js$",
  ],
  moduleFileExtensions: ['js', 'vue', 'json'],
  setupFiles: ['<rootDir>/jest.setup.js'],
  testEnvironment: 'jsdom',
  testPathIgnorePatterns: ['<rootDir>/src/__tests__/__mocks__/'], // Ignore mocks directory for test discovery
};
