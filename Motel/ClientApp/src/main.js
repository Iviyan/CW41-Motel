import '@/utils/prototypeModifications' // https://mathiasbynens.be/notes/prototypes

import { createApp } from 'vue'
import App from './App.vue'
import store from './store'
import router from './router'

import "bootstrap/dist/css/bootstrap.min.css"
import "izitoast/dist/css/iziToast.min.css"

//import "bootstrap"

createApp(App)
	.use(store)
	.use(router)
	.mount('#app')
