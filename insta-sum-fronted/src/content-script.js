// Content script to handle messages and display the summary popup

console.log('Content script loaded');

// Initialize content script by setting up a message listener
export function initContentScript() {
  chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    console.log('Registering message listener');
    console.log('Message received:', request);
  
    // Handle message action to display summary popup
    if (request.action === 'displaySummary' && request.summary) {
      console.log('Handling displaySummary'); // Log when displaySummary action is handled
      console.log('Summary:', request.summary);
      displaySummaryPopup(request.summary, request.keyHighlights, request.importantWords);
    } else {
      console.log('Invalid action or missing summary'); // Log if action is invalid or summary is missing
    }
  });
}

// Cached template to avoid redundant fetch requests
let cachedTemplate = null;

// Fetch the template from the extension's URL or return cached version
export const fetchTemplate = async () => {
  console.log('Fetching template...');
  
  // Return cached template if available
  if (cachedTemplate) {
    console.log('Returning cached template:', cachedTemplate);
    return Promise.resolve(cachedTemplate);
  }

  // Fetch template from the extension directory
  const templateUrl = await chrome.runtime.getURL('summary-template.html');
  console.log('Template URL:', templateUrl);

  return fetch(templateUrl)
    .then(response => {
      console.log('Fetch response:', response);
      return response.text();
    })
    .then(template => {
      cachedTemplate = template; // Cache the fetched template
      console.log('Template fetched and cached:', template);
      return template;
    })
    .catch(error => {
      console.error('Error fetching template:', error);
      return ''; // Return empty string if fetch fails
    });
};

// Format content using the provided template, summary, key highlights, and important words
function formatContentWithTemplate(summary, keyHighlights, importantWords, template) {
  // Create an array of important words from the comma-separated list
  const importantWordsArray = importantWords.split(', ').map(word => word.trim());

  // Function to bold important words in the text
  function boldImportantWords(text) {
    return importantWordsArray.reduce((acc, word) => {
      const regex = new RegExp(`\\b${word}\\b`, 'gi');
      return acc.replace(regex, `<strong>${word}</strong>`);
    }, text);
  }

  // Replace placeholders in the template with formatted summary and key highlights
  return template
    .replace('{{summary}}', boldImportantWords(summary
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>') // Convert **bold** to <strong>bold</strong>
      .replace(/^- (.*)$/gm, '<li>$1</li>') // Convert - item to <li>item</li>
      .replace(/(\n\s*\n)/g, '</p><p>'))) // Convert newlines to paragraphs
    .replace('{{keyHighlights}}', boldImportantWords(keyHighlights
      .split('\n')
      .map(line => {
        const formattedLine = line.replace(/^- /, ''); // Remove leading - for list items
        return `<li>${formattedLine}</li>`;
      })
      .join('')));
}

// Create and display an empty popup with a loading message
export const displayEmptyPopup = () => {
  console.log('Creating and displaying empty popup...');

  // Create and append the overlay element
  const overlay = document.createElement('div');
  overlay.id = 'popup-overlay';
  document.body.appendChild(overlay);

  // Create and append the summary popup element
  const summaryPopup = document.createElement('div');
  summaryPopup.id = 'summary-popup';
  summaryPopup.innerHTML = '<p>Loading summary...</p>';
  document.body.appendChild(summaryPopup);

  console.log('Overlay added:', overlay);
  console.log('Summary popup added:', summaryPopup);

  // Slide the popup into view after a short delay
  setTimeout(() => {
    summaryPopup.classList.add('popup-slide-in');
    summaryPopup.style.right = '0';  // Slide the popup into view
  }, 10);

  // Add event listener to close the popup when the overlay is clicked
  overlay.addEventListener('click', () => {
    console.log('Overlay clicked, closing popup...');
    closePopup(summaryPopup, overlay);
  });

  return { summaryPopup, overlay };
}

// Update the content of the popup with the formatted summary, key highlights, and important words
export function updatePopupContent(summaryPopup, summary, keyHighlights, importantWords) {
  if (!summaryPopup) {
    console.error('Summary popup element not found.');
    return;
  }

  fetchTemplate().then(template => {
    if (!template) {
      console.error('Failed to load template.');
      summaryPopup.innerHTML = '<p>Failed to load summary.</p>';
      return;
    }

    const formattedContent = formatContentWithTemplate(summary, keyHighlights, importantWords, template);
    if (!formattedContent) {
      console.error('Failed to format summary.');
      summaryPopup.innerHTML = '<p>Failed to format summary.</p>';
      return;
    }

    summaryPopup.innerHTML = formattedContent;
  });
}

// Display the summary popup with the provided data
function displaySummaryPopup(summary, keyHighlights, importantWords) {
  console.log('Displaying summary popup with data:', { summary, keyHighlights, importantWords });
  const { summaryPopup, overlay } = displayEmptyPopup();

  updatePopupContent(summaryPopup, summary, keyHighlights, importantWords);
}

// Close the popup and overlay elements
export const closePopup = (summaryPopup, overlay) => {
  console.log('Attempting to close popup...');

  // Remove the summary popup if it exists
  if (summaryPopup) {
    console.log('Removing summaryPopup...');
    if (document.body.contains(summaryPopup)) {
      summaryPopup.style.right = '-400px';
      setTimeout(() => {
        document.body.removeChild(summaryPopup);
      }, 500);
    } else {
      console.log('summaryPopup is not in the DOM.');
    }
  }

  // Remove the overlay if it exists
  if (overlay) {
    console.log('Removing overlay...');
    if (document.body.contains(overlay)) {
      document.body.removeChild(overlay);
    } else {
      console.log('overlay is not in the DOM.');
    }
  }
}

// Initialize the content script
initContentScript();
