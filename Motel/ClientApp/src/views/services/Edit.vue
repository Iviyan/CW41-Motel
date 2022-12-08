<template>

	<form @submit.prevent="edit">
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

		<input type="submit" class="btn btn-outline-primary" value="Изменить">
		<router-link :to="{ name: 'services' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
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

const services = inject('services')

const service = reactive({
	name: '',
	price: '',
	isActual: true,
});

let serviceIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = service;

		await call_patch(`/api/services/${props.id}`, patchData);

		Object.assign(services.value[serviceIndex], patchData);
		error.value = '';
		await router.push({ name: 'services' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => {
	serviceIndex = services.value.findIndex(u => u.id === id);
	Object.assign(service, services.value[serviceIndex]);
}, { immediate: true })
</script>
