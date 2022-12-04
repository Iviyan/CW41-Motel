<template>

	<form @submit.prevent="add">
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.last_name">
			<label>Фамилия</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.first_name">
			<label>Имя</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.patronymic">
			<label>Отчество</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.passport_serial">
			<label>Серия паспорта</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.passport_number">
			<label>Номер паспорта</label>
		</div>
		<div class="form-floating mb-3">
			<input type="date" class="form-control" v-model="employee.birthday">
			<label>Дата рождения</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.phone">
			<label>Телефон</label>
		</div>
		<div class="form-floating mb-3">
			<select class="form-select" v-model="employee.post">
				<option selected :value="null">Уволен</option>
				<option v-for="(value, key) in newEmployeeRoles" :value="key">{{ value }}</option>
			</select>
			<label for="floatingSelect">Должность</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.login">
			<label>Логин</label>
		</div>
		<div class="form-floating mb-3">
			<input type="password" class="form-control" v-model="employee.password">
			<label>Пароль</label>
		</div>

		<input type="submit" class="btn btn-outline-primary" value="Добавить сотрудника">
		<router-link :to="{ name: 'employees' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref} from "vue";
import {roleNames, roles} from "@/utils/roles";
import {RequestError} from "@/exceptions";
import {call_post} from "@/utils/api";
import router from "@/router";

function omit(obj, key) {
	const { [key]: omitted, ...rest } = obj;
	return rest;
}

const newEmployeeRoles = omit(roleNames, 'Admin')

const employees = inject('employees')

const employee = reactive({
	last_name: '',
	first_name: '',
	patronymic: '',
	passport_serial: '',
	passport_number: '',
	birthday: new Date().toISOString().slice(0, 10),
	phone: '',
	post: roles.hr,
	login: '',
	password: ''
});

const error = ref('');

async function add() {
	try {
		let newEmployee = await call_post('/api/employees', employee);
		employees.value.push(newEmployee);
		error.value = '';
		await router.push({ name: 'employees' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

</script>

<style scoped>

</style>
