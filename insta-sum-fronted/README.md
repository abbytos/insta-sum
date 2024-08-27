# insta-sum-frontend
Insta-Sum is a Chrome extension built using Vue.js that provides text summarization capabilities by interacting with a backend API. The frontend uses Webpack for bundling and development.

## Technologies Used
- **Vue.js:** The framework used for building the frontend.
- **Webpack:** Module bundler to bundle and optimize the codebase for development and production.
- **Chrome Extension APIs:** To handle the extension's browser interactions.
- **Node.js:** For managing dependencies and build scripts.

## Project setup

To set up the project and run the development environment locally, follow these steps:

Install Dependencies

```
npm install
```

### Webpack Configuration
Webpack is used for bundling the assets and optimizing the codebase. It handles the following tasks:

Bundling JavaScript: All JavaScript files are bundled together.
Handling Vue Files: Webpack processes .vue files and resolves the Vue.js components.
CSS Preprocessing: Styles are processed and extracted into separate files.
The Webpack configuration is defined in webpack.config.js. You can customize it as needed for additional loaders or plugins.



### Compiles and minifies for production
```
npm run build
```

### Lints and fixes files
```
npm run lint
```

## Usage
Install the Extension: Once the project is built using Webpack, load the unpacked extension into Chrome from the chrome://extensions page.
Select Text: Right-click on selected text and choose "Summarize selected text" from the context menu.
View Summary: The popup will display the summarized version of the selected text.

### Customize configuration
See [Configuration Reference](https://cli.vuejs.org/config/).
