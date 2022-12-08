<template>

	<form @submit.prevent="edit">
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.lastName">
			<label>Фамилия</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.firstName">
			<label>Имя</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.patronymic">
			<label>Отчество</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.passportSerial">
			<label>Серия паспорта</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="employee.passportNumber">
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

		<input type="submit" class="btn btn-outline-primary" value="Изменить">
		<router-link :to="{ name: 'employees' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref, watch} from "vue";
import {roleNames, roles} from "@/utils/roles";
import {RequestError} from "@/exceptions";
import {call_patch} from "@/utils/api";
import router from "@/router";
import {omit} from "@/utils/utils";

const props = defineProps({
	id: Number
});

const newEmployeeRoles = omit(roleNames, 'Admin')

const employees = inject('employees')

const employee = reactive({
	lastName: '',
	firstName: '',
	patronymic: '',
	passportSerial: '',
	passportNumber: '',
	birthday: new Date().toISOString().slice(0, 10),
	phone: '',
	post: roles.hr,
	login: '',
	password: ''
});

let employeeIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = employee;
		if (!employee.password) patchData = omit(patchData, 'password');

		await call_patch(`/api/employees/${props.id}`, patchData);

		if (employee.password) patchData = omit(patchData, 'password');

		Object.assign(employees.value[employeeIndex], patchData);
		error.value = '';
		await router.push({ name: 'employees' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => { console.log('Employees edit view id changed')
	employeeIndex = employees.value.findIndex(u => u.id === id);
	Object.assign(employee, employees.value[employeeIndex]);
}, { immediate: true })
</script>
