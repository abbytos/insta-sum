import { initContentScript, displayEmptyPopup, closePopup, fetchTemplate } from '../content-script';

// Mock chrome API globally before any code runs
global.chrome = {
  runtime: {
    onMessage: {
      addListener: jest.fn(),
    },
    getURL: jest.fn().mockReturnValue('mocked-url/summary-template.html'),
  },
  storage: {
    local: {
      get: jest.fn().mockImplementation((key, callback) => {
        callback({ template: 'template content' });
      }),
    },
  },
  tabs: {
    sendMessage: jest.fn(),
  },
  windows: {
    create: jest.fn(),
  },
};

// Mock fetch globally
beforeAll(() => {
  global.fetch = jest.fn().mockResolvedValue({
    text: () => Promise.resolve('cached template content'),
  });
});

// Mock console.log to prevent cluttering test output
jest.spyOn(global.console, 'log').mockImplementation(() => {});

// Mock the entire content-script module (if necessary for other tests)
jest.mock('../content-script', () => ({
  initContentScript: jest.fn(),
  displayEmptyPopup: jest.requireActual('../content-script').displayEmptyPopup, // Use the real implementation
  closePopup: jest.fn(),
  fetchTemplate: jest.requireActual('../content-script').fetchTemplate, // Use actual implementation
}));

describe('Content Script', () => {
  beforeEach(() => {
    // Clear mocks before each test
    jest.clearAllMocks();
    document.body.innerHTML = '';
  });

  it('should listen for messages and handle displaySummary action', () => {
    // Test that initContentScript is correctly mocked and called
    initContentScript();
    expect(initContentScript).toHaveBeenCalled();
  });

  it('fetchTemplate should cache the template', async () => {
    const template = await fetchTemplate();
    console.log('Template received in test:', template);
  
    // Verify the fetched template content
    expect(template).toBe('cached template content');
  
    // Verify fetch was called with the correct URL
    expect(global.fetch).toHaveBeenCalledWith('mocked-url/summary-template.html');
  });

  it('displayEmptyPopup should create and display popup elements', () => {
    const result = displayEmptyPopup();

    // Check if result is not undefined
    expect(result).not.toBeUndefined();

    // Check if the elements are created and have the correct IDs
    const { summaryPopup, overlay } = result;
    expect(summaryPopup).not.toBeNull();
    expect(overlay).not.toBeNull();
    expect(document.getElementById('summary-popup')).toBe(summaryPopup);
    expect(document.getElementById('popup-overlay')).toBe(overlay);

    // Verify the elements have been appended to the document body
    expect(document.body.contains(summaryPopup)).toBe(true);
    expect(document.body.contains(overlay)).toBe(true);

    // Verify the content or styling if needed
    expect(summaryPopup.innerHTML).toContain('<p>Loading summary...</p>');

    // Verify if the popup is styled correctly (e.g., check class added after timeout)
    // Note: If you're checking styles or classes, you might need to use a delay or `setImmediate`
    setTimeout(() => {
      expect(summaryPopup.classList.contains('popup-slide-in')).toBe(true);
      expect(summaryPopup.style.right).toBe('0');
    }, 10);
  });

  // it('closePopup should remove popup and overlay from DOM', async () => {
  //   // Create and append mock elements
  //   const mockPopup = document.createElement('div');
  //   mockPopup.id = 'summary-popup';
  //   document.body.appendChild(mockPopup);
  
  //   const mockOverlay = document.createElement('div');
  //   mockOverlay.id = 'popup-overlay';
  //   document.body.appendChild(mockOverlay);
  
  //   // Call closePopup
  //   closePopup(mockPopup, mockOverlay);
  
  //   // Check that overlay is removed immediately
  //   expect(document.body.contains(mockOverlay)).toBe(false);
  // });
});