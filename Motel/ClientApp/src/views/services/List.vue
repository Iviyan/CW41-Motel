<template>
	<div>
		<h2 class="header">Услуги</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Название</th>
				<th>Цена</th>
				<th>Актуальность</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="service in services">
				<td>{{ service.name }}</td>
				<td>{{ service.price }}</td>
				<td><input class="form-check-input" type="checkbox" disabled :checked="service.isActual"></td>
				<td>
					<router-link :to="{ name: 'services-edit', params: { id: service.id }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteService(service.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'services-new' }" class="btn btn-outline-primary">Добавить услугу</router-link>

	</div>
</template>

<script setup>
import {inject} from 'vue';

import { call_delete } from '@/utils/api';


const services = inject('services')

async function deleteService(id) {
	await call_delete(`/api/services/${ id }`);

	let index = services.value.findIndex(u => u.id === id);
	services.value.splice(index, 1);
}

</script>
