<template>
	<div>
		<h2 class="header">Сотрудники</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Логин</th>
				<th class="sortable" @click="sortBy='fullName'" :class="{sorted: sortBy==='fullName'}">ФИО</th>
				<th>Пасспорт</th>
				<th>Дата рождения</th>
				<th>Телефон</th>
				<th class="sortable" @click="sortBy='post'" :class="{sorted: sortBy==='post'}">Должность</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="employee in sortedEmployees">
				<td>{{ employee.login }}</td>
				<td>{{ getFullname(employee.firstName, employee.lastName, employee.patronymic) }}</td>
				<td>{{ employee.passportSerial + ' ' +employee.passportNumber }}</td>
				<td>{{ isoToDate(employee.birthday) }}</td>
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
import {computed, inject, ref} from 'vue';

import { call_delete} from '@/utils/api';
import { getFullname } from "@/utils/stringUtils";
import {  roleNames } from "@/utils/roles";
import { isoToDate } from '@/utils/timeUtils';

const employees = inject('employees')

const sortBy = ref('fullName')

const sortedEmployees = computed(() => {
	let data = employees.value.slice();
	if (sortBy.value === 'fullName') data.sort((a,b) =>
		getFullname(a.firstName, a.lastName, a.patronymic)
		.localeCompare(getFullname(b.firstName, b.lastName, b.patronymic))
	)
	else if (sortBy.value === 'post') {
		data.sort((a, b) => a.post - b.post)
		console.log(data)
	}

	return data;
})

async function deleteUser(id) {
	await call_delete(`/api/employees/${ id }`);

	let index = employees.value.findIndex(u => u.id === id);
	employees.value.splice(index, 1);
}

</script>

<style scoped>
.sorted {
	text-decoration: underline;
}
.sortable {
	cursor: pointer;
}
.sortable:hover {
	text-decoration: underline;
}
</style>
