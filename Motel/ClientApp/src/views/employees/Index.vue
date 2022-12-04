<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, reactive, onMounted, provide} from 'vue';
import { useStore } from 'vuex'
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get, call_delete, call_post, call_patch } from '@/utils/api';
import { RequestError } from "@/exceptions";

const store = useStore();
const router = useRouter();

const employees = ref([]);
provide('employees', employees);

const isLoading = ref(false);

onMounted(async () => {
	console.log('Employees main view mounted')

	isLoading.value = true;
	employees.value = await call_get(`/api/employees`);
	console.log(employees.value);
	isLoading.value = false;
});
</script>

<style>

.header {
	margin: 0 0 10px 0;
	text-align: center;
	font-weight: 500;
}

.new-user-form {
	display: grid;
	grid-auto-flow: column;
	grid-template-columns: 1fr 1fr;
	grid-template-rows: auto auto;
	grid-column-gap: 8px;
	grid-row-gap: 8px;
	justify-content: start;
}

@media (max-width: 700px) {
	.new-user-form {
		grid-template-columns: 1fr;
		grid-template-rows: auto auto auto auto;
	}
}

.new-user-form input:not([type]), .new-user-form input[type=text], .new-user-form input[type=password] {
	box-sizing: border-box;
	width: 100%;
	padding: 4px 16px;
	margin: 0;
	border-radius: 2px;
	border: 1px solid #d2d8d8;
	appearance: none;
}

.new-user-form input:focus {
	border-color: #343642;
	box-shadow: 0 0 5px rgba(52, 54, 66, 0.1);
	outline: none;
}

.new-user-form .fieldset {
	position: relative;
	margin: 0px 0;
}

.new-user-form input[type=password] { padding-right: 65px; }

.new-user-form .fieldset:first-child { margin-top: 0; }

.new-user-form .fieldset:last-child { margin-bottom: 0; }

.new-user-form input[type=submit] {
	padding: 4px;
	cursor: pointer;
	background: #2f889a;
	color: #FFF;
	border: none;
	appearance: none;
}

.new-user-form .hide-password {
	display: inline-block;
	position: absolute;
	right: 0;
	top: 50%;
	padding: 2px 15px;
	border-left: 1px solid #d2d8d8;
	bottom: auto;
	cursor: pointer;
	transform: translateY(-50%);
	font-size: 14px;
	color: #343642;
}

.error-message {
	margin: 8px 0;
	color: #c00;
	text-align: center;
	word-break: break-word;
}

.users-table {
	text-align: center;
	border-collapse: collapse;
	border: 0px solid #bbb;
	width: 100%;
}

.users-table th, .users-table td {
	border: 0.1px solid #aaa;
	padding: 4px;
}

.users-table th {
	font-weight: 500;
	padding: 4px 6px;
}

.users-table tr:not(:first-child):hover {
	background-color: rgba(0, 0, 0, 0.05);
	cursor: pointer;
}

.users-table .telegram-bot-users-list > p {
	margin: 0;
	overflow-wrap: anywhere;
}

.users-table .is-admin { background-color: rgba(157, 24, 46, 0.03); }

.telegram-bot-users {

}

.telegram-bot-users > h6 {
	all: unset;
	display: block;
	margin: 0 0 8px 0;
	text-align: center;
	font-size: 1.08rem;
}

.telegram-bot-users > div {
	display: grid;
	grid-template-columns: 1fr auto;
	column-gap: 8px;
	align-items: center;
	padding: 4px;
	border: solid 1px #aaa;
	border-radius: 4px;
}

.telegram-bot-users > div:not(:first-child) {
	margin-top: 6px;
}

.telegram-bot-users > div > p {
	margin: 0;
}

</style>

<style scoped>
hr {
	width: 100%;
	background-color: #bbb;
	height: 1px;
	border: none;
}
</style>
