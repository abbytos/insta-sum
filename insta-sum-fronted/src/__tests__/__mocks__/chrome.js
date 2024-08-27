// Mocking chrome directly in the test file
const chrome = {
    contextMenus: {
      create: jest.fn(),
      removeAll: jest.fn(),
      onClicked: {
        addListener: jest.fn()
      }
    },
    runtime: {
      getURL: jest.fn().mockReturnValue('mockURL')
    },
    storage: {
      local: {
        set: jest.fn(),
        get: jest.fn()
      }
    },
    tabs: {
      sendMessage: jest.fn()
    }
  };
  
  global.chrome = chrome;
  module.exports = chrome;
