<template>

	<form @submit.prevent="add">
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="service.name">
			<label>Название</label>
		</div>
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="service.price">
			<label>Цена</label>
		</div>
		<div class="form-check mb-3">
			<input class="form-check-input" type="checkbox" v-model="service.isActual">
			<label class="form-check-label">Актуальная услуга</label>
		</div>


		<input type="submit" class="btn btn-outline-primary" value="Добавить услугу">
		<router-link :to="{ name: 'services' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref} from "vue";
import {RequestError} from "@/exceptions";
import {call_post} from "@/utils/api";
import router from "@/router";

const services = inject('services')

const service = reactive({
	name: '',
	price: '',
	isActual: true,
});

const error = ref('');

async function add() {
	try {
		let newService = await call_post('/api/services', service);
		services.value.push(newService);
		error.value = '';
		await router.push({ name: 'services' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

</script>
