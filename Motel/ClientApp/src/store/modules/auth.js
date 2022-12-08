import { postj } from '@/utils/fetch'
import jwt_decode from "jwt-decode"
import {call_get, call_post} from '@/utils/api';
import { getFullname } from '@/utils/stringUtils';

export default {
	state: {
		jwt: null,
		jwtData: null,
		me: null
	},
	getters: {
		isAuth: state => !!state.jwt,
		jwtData: state => state.jwtData,
		role: state => state.jwtData?.role,

		fullName: state => state.me ? getFullname(state.me.firstName, state.me.lastName, state.me.patronymic) : null,
	},
	mutations: {
		auth(state, value) {
			console.log('auth, ', value)
			state.jwt = value;
			if (!!value) {
				state.jwtData = jwt_decode(value);
				localStorage.setItem('jwt', value);
			} else
				localStorage.removeItem('jwt');
		},
		logout(state) {
			state.jwt = state.jwtData = null;
			localStorage.removeItem('jwt');
		},

		setMe(state, value) { state.me = value; },
	},
	actions: {
		initAuth(ctx) {
			ctx.commit('auth', localStorage.getItem('jwt'))
		},
		async login(ctx, { login, password }) {
			let res = await postj('/login', {
				login: login,
				password: password
			});

			let jwt = res.accessToken;
			console.log('jwt: ', jwt);
			console.assert(!!jwt, "JWT must be not null here");
			ctx.commit('auth', jwt);
		},
		async logout({ commit }) {
			try { await call_post('/logout'); }
			finally { commit('logout'); }
		},

		async getMe(ctx) {
			let res = await call_get('/api/me');
			ctx.commit('setMe', res);
		},
	}
}
