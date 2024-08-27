import '../background'; // Adjust the path if necessary

describe('Background Script', () => {
  beforeEach(() => {
    chrome.contextMenus.removeAll.mockClear();
    chrome.contextMenus.create.mockClear();
    
    console.log('Running test setup');
    
  });

  test('should fetch correct URL for config.json', () => {
    const url = chrome.runtime.getURL('config.json');
    expect(url).toBe('mocked-url/config.json');
  });

  test('should fetch correct URL for summary-template.html', () => {
    const url = chrome.runtime.getURL('summary-template.html');
    expect(url).toBe('mocked-url/summary-template.html');
  });
});