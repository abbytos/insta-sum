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
  
  describe('Mock Chrome API', () => {
    test('should be defined', () => {
    expect(global.chrome).toBeDefined();
    });
  
    test('chrome.runtime.getURL should return mockURL', () => {
      expect(global.chrome.runtime.getURL()).toBe('mockURL');
    });
  
    test('chrome.contextMenus.create should be a function', () => {
        console.log('Type of create:', typeof global.chrome.contextMenus.create);
        expect(typeof global.chrome.contextMenus.create).toBe('function');
      });
  });
  