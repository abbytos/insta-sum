require('./src/__tests__/__mocks__/chrome.js'); // Mock chrome API if needed

const fetch = require('node-fetch');
global.fetch = fetch;

global.chrome = {
    contextMenus: {
      removeAll: jest.fn(),
      create: jest.fn(),
      onClicked: {
        addListener: jest.fn()
      }
    },
    runtime: {
      onMessage: {
        addListener: jest.fn()
      },
      getURL: jest.fn((path) => {
        if (path === 'config.json') {
          return 'mocked-url/config.json';
        }
        if (path === 'summary-template.html') {
          return 'mocked-url/summary-template.html';
        }
        return '';
      })
    },
    storage: {
      local: {
        get: jest.fn((key, callback) => {
          callback({ template: 'template content' });
        })
      }
    },
    tabs: {
      sendMessage: jest.fn()
    },
    windows: {
      create: jest.fn()
    }
  };
  