<template>
	<div>
		<h2 class="header">Уборка номеров</h2>

		<table class="table mt-16">
			<thead><tr>
				<th>Номер</th>
				<th>Дата и время</th>
				<th>Сотрудник</th>
				<th>Действия</th>
			</tr></thead>
			<tbody><tr v-for="roomCleaning in roomCleanings">
				<td>{{ roomCleaning.room.number }} ({{ roomCleaning.room.roomType.name }})</td>
				<td>{{ isoToDateTime(roomCleaning.datetime) }}</td>
				<td>{{ getFullname(roomCleaning.employee.firstName, roomCleaning.employee.lastName, roomCleaning.employee.patronymic) }}</td>
				<td>
					<button type="button" class="btn btn-outline-danger ms-2" @click="deleteRoomCleaning(roomCleaning.id)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'rooms' }" class="btn btn-outline-primary">Перейти к списку номеров</router-link>

	</div>
</template>

<script setup>
import {inject} from 'vue';

import {call_delete} from '@/utils/api';
import { isoToDateTime } from '@/utils/timeUtils';
import { getFullname } from '@/utils/stringUtils';


const roomCleanings = inject('roomCleanings')

async function deleteRoomCleaning(id) {
	await call_delete(`/api/room-cleanings/${ id }`);

	let index = roomCleanings.value.findIndex(o => o.id === id);
	roomCleanings.value.splice(index, 1);
}

</script>
