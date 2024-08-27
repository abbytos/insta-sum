// Log that the background script has been loaded
console.log('Background script loaded.');

// Function to create a context menu item for summarizing selected text
export function createContextMenu() {
  console.log('createContextMenu called');
  
  // Remove all existing context menu items
  chrome.contextMenus.removeAll(() => {
    console.log('removeAll callback called');
    
    // Create a new context menu item
    chrome.contextMenus.create({
      id: 'summarize',
      title: 'Summarize selected text',
      contexts: ['selection']
    }, () => {
      if (chrome.runtime.lastError) {
        console.error('Error creating context menu item:', chrome.runtime.lastError.message);
      } else {
        console.log('Context menu item created.');
      }
    });
  });
}

// Call the function to create the context menu item
createContextMenu();

// Listener for context menu item clicks
chrome.contextMenus.onClicked.addListener((info, tab) => {
  console.log('Background script onClicked event triggered');

  if (info.menuItemId === 'summarize') {
    console.log("Selected text:", info.selectionText);
    
    // Fetch configuration and process the text
    fetchFromConfigAndProcess(info.selectionText, tab.id);
  }
});

// Function to fetch configuration and process the summarization
function fetchFromConfigAndProcess(text, tabId) {
  fetch(chrome.runtime.getURL('config.json'))
    .then(response => {
      if (!response.ok) {
        throw new Error(`Error loading config: ${response.statusText}`);
      }
      return response.json();
    })
    .then(config => {
      if (!config.apiEndpoint) {
        throw new Error('API endpoint is missing in the config.');
      }
      
      // Call APIs to get summary, key highlights, and important words
      const summaryPromise = fetchSummary(config.apiEndpoint, text);
      const keyHighlightsPromise = fetchKeyHighlights(config.apiEndpoint, text);
      const importantWordsPromise = fetchImportantWords(config.apiEndpoint, text);
      
      return Promise.all([summaryPromise, keyHighlightsPromise, importantWordsPromise]);
    })
    .then(([summary, keyHighlights, importantWords]) => {
      // Save results to local storage and send a message to the content script
      chrome.storage.local.set({ summary, keyHighlights, importantWords }, () => {
        chrome.tabs.sendMessage(tabId, { 
          action: 'displaySummary',
          summary,
          keyHighlights,
          importantWords
        });
      });
    })
    .catch(error => {
      console.error('Error fetching data:', error);
      chrome.tabs.sendMessage(tabId, { 
        action: 'displaySummary',
        summary: 'Failed to fetch data.'
      });
    });
}

// Function to fetch summary from the API
function fetchSummary(apiUrl, text) {
  return fetch(`${apiUrl}/summary`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ text: text })
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`Error fetching summary: ${response.statusText}`);
    }
    return response.json();
  })
  .then(data => {
    const summaryData = JSON.parse(data.summary.body);
    return summaryData.summary;
  })
  .catch(error => {
    console.error('Fetch error:', error);
    throw error;
  });
}

// Function to fetch key highlights from the API
function fetchKeyHighlights(apiUrl, text) {
  return fetch(`${apiUrl}/key-highlights`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ text: text })
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`Error fetching key highlights: ${response.statusText}`);
    }
    return response.json();
  })
  .then(data => {
    const keyHighlightsData = JSON.parse(data.summary.body);
    return keyHighlightsData.keyHighlights;
  })
  .catch(error => {
    console.error('Fetch error:', error);
    throw error;
  });
}

// Function to fetch important words from the API
function fetchImportantWords(apiUrl, text) {
  return fetch(`${apiUrl}/important-words`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ text: text })
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`Error fetching important words: ${response.statusText}`);
    }
    return response.json();
  })
  .then(data => {
    if (data.summary && data.summary.body) {
      const importantWordsData = JSON.parse(data.summary.body);
      return importantWordsData.importantWords;
    } else {
      throw new Error('Invalid response format');
    }
  })
  .catch(error => {
    console.error('Fetch error:', error);
    throw error;
  });
}

// Listener for messages from other parts of the extension
chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === 'saveSummary') {
    chrome.storage.local.set({
      summary: message.summary,
      keyHighlights: message.keyHighlights,
      importantWords: message.importantWords
    }, () => {
      sendResponse({ status: 'success' });
    });
    return true;
  } else if (message.action === 'requestSummary') {
    chrome.storage.local.get(['summary', 'keyHighlights', 'importantWords'], (data) => {
      sendResponse({
        summary: data.summary || 'No summary available',
        keyHighlights: data.keyHighlights || 'No key highlights available',
        importantWords: data.importantWords || 'No important words available'
      });
    });
    return true;
  }
});
