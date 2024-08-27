/**
 * Main entry point for the Vue.js application.
 * 
 * This script sets up and mounts the Vue.js application by importing the root component
 * (`App.vue`) and the Vuex store (`store.js`). It initializes the app, applies the store,
 * and mounts the application to the DOM element with the id 'app'.
 */

import { createApp } from 'vue';
import App from './App.vue';
import store from './store';

// Create and mount the Vue.js application
createApp(App)
  .use(store) // Apply the Vuex store for state management
  .mount('#app'); // Mount the application to the DOM element with id 'app'
