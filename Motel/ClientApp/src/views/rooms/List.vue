<template>
	<div>
		<h2 class="header">Номера</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Номер</th>
				<th>Тип номера</th>
				<th>Необходима уборка</th>
				<th>Готовность</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="room in rooms">
				<td>{{ room.number }}</td>
				<td>{{ room.roomType.name }} ({{ room.roomType.pricePerHour }} руб./час, {{ room.roomType.capacity }} чел.)</td>
				<td><input class="form-check-input" type="checkbox" disabled :checked="room.isCleaningNeeded"></td>
				<td><input class="form-check-input" type="checkbox" disabled :checked="room.isReady"></td>
				<td>
					<router-link :to="{ name: 'rooms-edit', params: { id: room.number }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteRoom(room.number)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'rooms-new' }" class="btn btn-outline-primary">Добавить номер</router-link>

	</div>
</template>

<script setup>
import {inject} from 'vue';

import { call_delete } from '@/utils/api';


const rooms = inject('rooms')

async function deleteRoom(id) {
	await call_delete(`/api/rooms/${ id }`);

	let index = rooms.value.findIndex(u => u.number === id);
	rooms.value.splice(index, 1);
}

</script>
