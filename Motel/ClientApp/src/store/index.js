import { createStore } from 'vuex'
import auth from './modules/auth'
import createMultiTabState from 'vuex-multi-tab-state';

const store = createStore({
	strict: true,
	state: {},
	mutations: {},
	modules: {
		auth
	},
	plugins: [
		createMultiTabState({
			statesPaths: [ 'jwt' ],
		})
	],
});

store.dispatch('initAuth')
	.then(r => console.log('store init'));

export default store;
