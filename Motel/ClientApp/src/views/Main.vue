<template>
	<nav class="main-menu">

		<div class="scrollbar">
			<p class="user-name">{{fullName}}</p>
			<p class="user-role">{{role}}</p>
			<hr>

			<ul>
				<li class="darker-shadow-down pd-b" :class="{active: router.currentRoute.value.name === 'home'}">
					<router-link :to="{ name: 'home' }">
						<span class="nav-text">Главная</span>
					</router-link>
				</li>


				<li class="dark" v-if="isRole(roles.admin, roles.hr)" :class="{active: router.currentRoute.value.name === 'employees'}">
					<router-link :to="{ name: 'employees' }">
						<span class="nav-text">Сотрудники</span>
					</router-link>
				</li>

				<li class="darker-shadow pd-t">
					<a href="#" @click="logout">
						<span class="nav-text">Выход</span>
					</a>
				</li>
				<li class="dark">
					<router-link :to="{ name: 'changePassword' }">
						<span class="nav-text">Изменить пароль</span>
					</router-link>
				</li>
			</ul>
		</div>
	</nav>

	<main>
		<router-view v-if="isReady" />
	</main>
</template>

<script setup>
import { onMounted, onUnmounted, computed, ref, provide, shallowRef, watch, reactive } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'

import iziToast from 'izitoast'
import { call_post } from '@/utils/api';
import { RequestError } from "@/exceptions";
import { roles, roleNames } from "@/utils/roles";

const store = useStore();
const router = useRouter();

const isReady = ref(false);

const fullName = computed(() => store.getters.fullName);
const role = computed(() => roleNames[store.getters.role]);

const isRole = (...roles) => roles.includes(store.getters.role);

onMounted(async () => {
	await store.dispatch('getMe');

	isReady.value = true;
})

onUnmounted(async () => {
	for (let toast of document.querySelectorAll('.iziToast'))
		iziToast.hide({}, toast);
})

async function logout() {
	await store.dispatch('logout');
}

// Change password

</script>

<style>
main {
	margin-left: 220px;
	padding: 16px 10px;
	position: relative;
	min-height: 100vh;
	box-sizing: border-box;
}

@media (max-width: 700px) {
	main {
		margin-left: 0;
	}
}

/* --- sidebar --- */

.scrollbar {
	height: 90%;
	width: 100%;
	overflow-y: scroll;
	overflow-x: hidden;
	margin-top: 20px;
}

/* Scrollbar Style */

.scrollbar::-webkit-scrollbar {
	width: 5px;
	background-color: #F7F7F7;
}

.scrollbar::-webkit-scrollbar-thumb {
	border-radius: 10px;
	box-shadow: inset 0 0 6px rgba(0, 0, 0, .3);
	background-color: #BFBFBF;
}

/* Scrollbar End */

.main-menu {
	background: #F7F7F7;
	position: fixed;
	top: 0;
	bottom: 0;
	height: 100%;
	left: 0;
	width: 220px;
	overflow: hidden;
	transition: width .2s linear;
	box-shadow: 1px 0 5px rgb(0 0 0 / 20%)
}

@media (max-width: 700px) {
	.main-menu {
		position: static;
		max-height: 100%;
		height: auto;
		width: 100%;
	}
}

.main-menu li {
	position: relative;
	display: block;
	width: 100%;
}

.main-menu li.pd-t {
	padding-top: 16px;
}

.main-menu li.pd-b {
	padding-bottom: 16px;
}

.main-menu li > a > * {
	padding-left: 12px;
}

.main-menu li > a {
	position: relative;
	width: 100%;
	height: 30px;
	display: table;
	border-collapse: collapse;
	border-spacing: 0;
	color: #444;
	font-size: 14px;
	text-decoration: none;
	transition: all .14s linear;
	font-family: 'Strait', sans-serif;
	border-top: 1px solid #f2f2f2;
	text-shadow: 1px 1px 1px #fff;
}

.main-menu .nav-text {
	position: relative;
	display: table-cell;
	vertical-align: middle;
	font-family: 'Titillium Web', sans-serif;
}

nav {
	user-select: none;
}

nav ul, nav li {
	outline: 0;
	margin: 0;
	padding: 0;
}


/* Darker element side menu Start*/


.main-menu li.darker {
	background-color: #ededed;
}

.main-menu li.dark {
	background-color: #f3f3f3;
}

.main-menu li.darker-shadow {
	background-color: #f3f3f3;
	box-shadow: inset 0px 5px 5px -4px rgba(50, 50, 50, 0.55);
}

.main-menu li.darker-shadow-down {
	background-color: #f3f3f3;
	box-shadow: inset 0px -4px 5px -4px rgba(50, 50, 50, 0.55);
}

/* Darker element side menu End*/


.main-menu li:hover > a {
	color: #fff;
	background-color: #54afaf;
	text-shadow: 0px 0px 0px;
}

.main-menu li.active > a {
	color: #fff;
	background-color: #9dbdbd;
	text-shadow: 0px 0px 0px;
}


.user-name {
	margin: 0;
	text-align: center;
	font-weight: 500;
}
.user-role {
	margin: 0;
	text-align: center;
}

</style>

