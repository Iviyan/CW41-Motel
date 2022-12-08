<template>
	<div>
		<h2 class="header">Услуги</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Услуга</th>
				<th>Номер</th>
				<th>Дата и время</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="serviceOrder in serviceOrders">
				<td>{{ serviceOrder.service.name }} ({{serviceOrder.service.price}} руб.)</td>
				<td>{{ serviceOrder.roomNumber }}</td>
				<td>{{ isoToDateTime(serviceOrder.datetime) }}</td>
				<td>
					<router-link :to="{ name: 'service-orders-edit', params: { id: serviceOrder.id }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteServiceOrder(serviceOrder.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'service-orders-new' }" class="btn btn-outline-primary">Добавить заказ</router-link>
		<button @click="loadCsv" class="btn btn-outline-primary ms-2">Загрузить csv</button>

	</div>
</template>

<script setup>
import {inject} from 'vue';

import {call_delete, downloadFile} from '@/utils/api';
import { isoToDateTime } from '@/utils/timeUtils';


const serviceOrders = inject('serviceOrders')

async function deleteServiceOrder(id) {
	await call_delete(`/api/service-orders/${ id }`);

	let index = serviceOrders.value.findIndex(o => o.id === id);
	serviceOrders.value.splice(index, 1);
}

function loadCsv() {
	downloadFile(`/api/service-orders/csv`);
}

</script>
