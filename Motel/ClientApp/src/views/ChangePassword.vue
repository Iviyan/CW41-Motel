<template>
	<h3 class="fs-3 text-center">Изменение пароля</h3>
	<form class="s-form">
		<p class="fieldset">
			<input v-model="changePasswordModal.oldPassword"
				   class="full-width has-padding has-border"
				   :type="changePasswordModal.oldPasswordShow ? 'text' : 'password'"
				   placeholder="Старый пароль">
			<a class="hide-password"
			   @click="changePasswordModal.oldPasswordShow = !changePasswordModal.oldPasswordShow">{{
					changePasswordModal.oldPasswordShow ? 'Hide' : 'Show'
				}}</a>
		</p>
		<p class="fieldset">
			<input v-model="changePasswordModal.newPassword"
				   class="full-width has-padding has-border"
				   :type="changePasswordModal.newPasswordShow ? 'text' : 'password'" placeholder="Новый пароль">
			<a class="hide-password"
			   @click="changePasswordModal.newPasswordShow = !changePasswordModal.newPasswordShow">{{
					changePasswordModal.newPasswordShow ? 'Hide' : 'Show'
				}}</a>
		</p>

		<p class="error-message">{{ changePasswordModal.error }}</p>
		<button class="btn btn-outline-primary" type="submit">Изменить</button>
	</form>
</template>

<script setup>
import {reactive} from "vue";
import {call_post} from "@/utils/api";
import {RequestError} from "@/exceptions";

const changePasswordModal = reactive({
	oldPassword: '',
	oldPasswordShow: false,
	newPassword: '',
	newPasswordShow: false,
	error: ''
});

async function tryChangePassword() {
	try {
		await call_post('/change-password', {
			oldPassword: changePasswordModal.oldPassword,
			newPassword: changePasswordModal.newPassword,
		});
		changePasswordModal.show = false;
		changePasswordModal.error = changePasswordModal.newPassword = changePasswordModal.oldPassword = ``;
	} catch (err) {
		if (err instanceof RequestError)
			changePasswordModal.error = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			changePasswordModal.error = 'Неизвестная ошибка';
	}
}
</script>

<style scoped>


.s-form {
	padding: 6px;
}

.s-form input {
	box-sizing: border-box;
}

.s-form .fieldset {
	position: relative;
	margin: 8px 0;
}

.s-form .fieldset:first-child {
	margin-top: 0;
}

.s-form .fieldset:last-child {
	margin-bottom: 0;
}

.s-form label {
	font-size: 14px;
}

.s-form input {
	margin: 0;
	padding: 0;
	border-radius: 2px;
}

.s-form input.full-width {
	width: 100%;
}

.s-form input.has-padding {
	padding: 8px 20px 8px 20px;
}

.s-form input.has-border {
	border: 1px solid #d2d8d8;
	appearance: none;
}

.s-form input.has-border:focus {
	border-color: #343642;
	box-shadow: 0 0 5px rgba(52, 54, 66, 0.1);
	outline: none;
}

.s-form input[type=password] {
	/* space left for the HIDE button */
	padding-right: 65px;
}

.s-form input[type=submit] {
	padding: 16px 0;
	cursor: pointer;
	background: #2f889a;
	color: #FFF;
	font-weight: bold;
	border: none;
	appearance: none;
}

.s-form .hide-password {
	display: inline-block;
	position: absolute;
	right: 0;
	top: 50%;
	padding: 6px 15px;
	border-left: 1px solid #d2d8d8;
	bottom: auto;
	cursor: pointer;
	transform: translateY(-50%);
	font-size: 14px;
	color: #343642;
}

.s-form .error-message {
	margin: 8px 0;
	color: #c00;
	text-align: center;
	word-break: break-word;
}

</style>
