<template>
	<div>
		<h2 class="header">Типы номеров</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Название</th>
				<th>Цена в час</th>
				<th>Вместимость</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="roomType in roomTypes">
				<td>{{ roomType.name }}</td>
				<td>{{ roomType.pricePerHour }}</td>
				<td>{{ roomType.capacity }}</td>
				<td>
					<router-link :to="{ name: 'room-types-edit', params: { id: roomType.id }}" type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteRoomType(roomType.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'room-types-new' }" class="btn btn-outline-primary">Добавить тип номера</router-link>

	</div>
</template>

<script setup>
import {inject} from 'vue';

import { call_delete } from '@/utils/api';


const roomTypes = inject('roomTypes')

async function deleteRoomType(id) {
	await call_delete(`/api/room-types/${ id }`);

	let index = roomTypes.value.findIndex(u => u.id === id);
	roomTypes.value.splice(index, 1);
}

</script>
