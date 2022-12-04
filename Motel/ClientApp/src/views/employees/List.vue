<template>
	<div>
		<h2 class="header">Сотрудники</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Логин</th>
				<th>ФИО</th>
				<th>Пасспорт</th>
				<th>Дата рождения</th>
				<th>Телефон</th>
				<th>Должность</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="employee in employees">
				<td>{{ employee.login }}</td>
				<td>{{ getFullname(employee.first_name, employee.last_name, employee.patronymic) }}</td>
				<td>{{ employee.passport_serial + ' ' +employee.passport_number }}</td>
				<td>{{ employee.birthday }}</td>
				<td>{{ employee.phone }}</td>
				<td>{{ employee.post ? roleNames[employee.post] : 'Уволен' }}</td>
				<td>
					<router-link :to="{ name: 'employees-edit', params: { id: employee.id }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteUser(employee.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'employees-new' }" class="btn btn-outline-primary">Добавить сотрудника</router-link>

	</div>
</template>

<script setup>
import {ref, reactive, onMounted, inject} from 'vue';
import { useStore } from 'vuex'
import { useRouter } from 'vue-router';

import { call_get, call_delete, call_post, call_patch } from '@/utils/api';
import { RequestError } from "@/exceptions";
import { getFullname } from "@/utils/stringUtils";
import { roles, roleNames } from "@/utils/roles";


const employees = inject('employees')

async function deleteUser(id) {
	await call_delete(`/api/employees/${ id }`);

	let index = employees.value.findIndex(u => u.id === id);
	employees.value.splice(index, 1);
}

</script>

<style>

</style>
