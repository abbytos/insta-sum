import { createStore } from 'vuex';

const store = createStore({
  state() {
    return {
      // State variables
      summarizedText: '',
      hoverText: '',
    };
  },
  mutations: {
    // Mutations to update state
    setSummarizedText(state, text) {
      state.summarizedText = text;
    },
    setHoverText(state, text) {
      state.hoverText = text;
    },
  },
  actions: {
    // Actions to commit mutations
    updateSummarizedText({ commit }, text) {
      commit('setSummarizedText', text);
    },
    updateHoverText({ commit }, text) {
      commit('setHoverText', text);
    },
  },
  getters: {
    // Getters to access state
    summarizedText(state) {
      return state.summarizedText;
    },
    hoverText(state) {
      return state.hoverText;
    },
  },
});

export default store;
