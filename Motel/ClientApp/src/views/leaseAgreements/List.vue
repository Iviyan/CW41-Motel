<template>
	<div>
		<h2 class="header">Договора аренды</h2>

		<div class="my-3 d-flex">
			<div class="form-floating d-inline-block">
				<select class="form-select" v-model="filters.room">
					<option :value="null">-</option>
					<option v-for="room in rooms" :value="room.number">
						{{ room.number }} ({{ room.roomType.name }})
					</option>
				</select>
				<label>Номер</label>
			</div>

			<router-link :to="{ name: 'lease-agreements-graph' }" class="btn btn-outline-primary ms-auto align-self-center">Статистика аренды комнат</router-link>
		</div>

		<table class="table mt-16">
			<thead><tr>
				<th>Клиент</th>
				<th>Период</th>
				<th>Комнаты</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="leaseAgreement in leaseAgreementsFiltered">
				<td>{{ leaseAgreement.clientName }}</td>
				<td>{{ isoToDateTime(leaseAgreement.startAt) }} - {{ isoToDateTime(leaseAgreement.endAt) }}</td>
				<td>{{ leaseAgreement.rooms.join(', ') }}</td>
				<td>
					<router-link :to="{ name: 'lease-agreements-edit', params: { id: leaseAgreement.id }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteLeaseAgreement(leaseAgreement.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'lease-agreements-new' }" class="btn btn-outline-primary">Добавить договор аренды</router-link>

	</div>
</template>

<script setup>
import {computed, inject, reactive} from 'vue';

import { call_delete } from '@/utils/api';
import { isoToDateTime } from '@/utils/timeUtils';

const rooms = inject('rooms')
const leaseAgreements = inject('leaseAgreements')

const filters = reactive({
	room: null
})

const leaseAgreementsFiltered = computed(() => {
	let result = leaseAgreements.value;
	if (filters.room) result = result.filter(a => a.rooms.includes(filters.room));
	return result;
})

async function deleteLeaseAgreement(id) {
	await call_delete(`/api/lease-agreements/${ id }`);

	let index = leaseAgreements.value.findIndex(o => o.id === id);
	leaseAgreements.value.splice(index, 1);
}

</script>
