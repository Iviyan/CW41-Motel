<template>

	<form @submit.prevent="add">
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


		<input type="submit" class="btn btn-outline-primary" value="Добавить тип номера">
		<router-link :to="{ name: 'room-types' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref} from "vue";
import {RequestError} from "@/exceptions";
import {call_post} from "@/utils/api";
import router from "@/router";

const roomTypes = inject('roomTypes')

const roomType = reactive({
	name: '',
	pricePerHour: '',
	capacity: '',
});

const error = ref('');

async function add() {
	try {
		let newRoomType = await call_post('/api/room-types', roomType);
		roomTypes.value.push(newRoomType);
		error.value = '';
		await router.push({ name: 'room-types' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

</script>
