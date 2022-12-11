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
					<router-link :to="{ name: 'rooms-edit', params: { id: room.number }}"
								 v-if="isRole(roles.admin, roles.salesman)"
								 type="button" class="btn btn-outline-primary">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-edit" /></svg>
					</router-link>
					<button type="button" class="btn btn-outline-danger ms-2"
							v-if="isRole(roles.admin, roles.salesman)"
							@click="deleteRoom(room.number)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
					</button>
					<button type="button" class="btn btn-outline-info ms-2"
							v-if="isRole(roles.admin, roles.maid) && room.isCleaningNeeded"
							@click="cleanRoom(room.number)">
						<svg fill="currentColor" width="16" height="16"><use href="#icon-bucket" /></svg>
					</button>

				</td>
			</tr></tbody>
		</table>

		<router-link :to="{ name: 'rooms-new' }"
					 v-if="isRole(roles.admin, roles.salesman)"
					 class="btn btn-outline-primary">Добавить номер</router-link>

	</div>
</template>

<script setup>
import {inject} from 'vue';
import store from "@/store";

import { call_delete, call_post } from '@/utils/api';
import { roles } from '@/utils/roles';

const isRole = (...roles) => roles.includes(store.getters.role);

const rooms = inject('rooms')

async function deleteRoom(id) {
	await call_delete(`/api/rooms/${ id }`);

	let index = rooms.value.findIndex(u => u.number === id);
	rooms.value.splice(index, 1);
}

async function cleanRoom(id) {
	await call_post(`/api/rooms/${ id }/clean`);

	let room = rooms.value.find(u => u.number === id);
	room.isCleaningNeeded = false;
}

</script>
