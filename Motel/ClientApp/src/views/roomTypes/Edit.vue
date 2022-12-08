<template>

	<form @submit.prevent="edit">
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="roomType.name">
			<label>Название</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="roomType.pricePerHour">
			<label>Цена за час</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="roomType.capacity">
			<label>Вместимость</label>
		</div>

		<input type="submit" class="btn btn-outline-primary" value="Изменить">
		<router-link :to="{ name: 'room-types' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
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

const roomTypes = inject('roomTypes')

const roomType = reactive({
	name: '',
	pricePerHour: '',
	capacity: '',
});

let roomTypeIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = roomType;

		await call_patch(`/api/room-types/${props.id}`, patchData);

		Object.assign(roomTypes.value[roomTypeIndex], patchData);
		error.value = '';
		await router.push({ name: 'room-types' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => {
	roomTypeIndex = roomTypes.value.findIndex(u => u.id === id);
	Object.assign(roomType, roomTypes.value[roomTypeIndex]);
}, { immediate: true })
</script>
