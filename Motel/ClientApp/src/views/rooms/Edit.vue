<template>

	<form @submit.prevent="edit">
		<div class="form-floating mb-3">
			<input type="number" class="form-control" v-model="room.number">
			<label>Номер</label>
		</div>
		<div class="form-floating mb-3">
			<select class="form-select" v-model="room.roomTypeId">
				<option v-for="roomType in roomTypes" :value="roomType.id">
					{{ roomType.name }} ({{ roomType.pricePerHour }} руб./час, {{ roomType.capacity }} чел.)
				</option>
			</select>
			<label>Тип номера</label>
		</div>
		<div class="form-check mb-3">
			<input class="form-check-input" type="checkbox" v-model="room.isCleaningNeeded">
			<label class="form-check-label">Необходима уборка</label>
		</div>
		<div class="form-check mb-3">
			<input class="form-check-input" type="checkbox" v-model="room.isReady">
			<label class="form-check-label">Готовность</label>
		</div>

		<input type="submit" class="btn btn-outline-primary" value="Изменить" :disabled="roomTypes.length === 0">
		<router-link :to="{ name: 'rooms' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref, watch} from "vue";
import {RequestError} from "@/exceptions";
import {call_patch} from "@/utils/api";
import router from "@/router";

const props = defineProps({
	id: Number
});

const rooms = inject('rooms')
const roomTypes = inject('roomTypes')

const room = reactive({
	number: '',
	roomTypeId: '',
	isCleaningNeeded: false,
	isReady: true,
});

let roomIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = room;

		await call_patch(`/api/rooms/${props.id}`, patchData);

		Object.assign(rooms.value[roomIndex], patchData);
		rooms.value[roomIndex].roomType = roomTypes.value.find(r => r.id === room.roomTypeId)
		error.value = '';
		await router.push({ name: 'rooms' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => {
	roomIndex = rooms.value.findIndex(u => u.number === id);
	Object.assign(room, rooms.value[roomIndex]);
}, { immediate: true })
</script>
